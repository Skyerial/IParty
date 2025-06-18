using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Fleck;
using NativeWebSocket;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.InputSystem.Controls;

// Newly added:
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using Mono.Cecil;

public class ServerManager : MonoBehaviour
{
    private static ServerManager instance;

    [Header("Mode Settings")]
    [Tooltip("Enable to use remote relay servers instead of local HTTP/WS servers.")]
    public bool useRemote = true;

    private string hostId;

    // Local server fields
    private HttpListener httpListener;
    private Thread httpThread;
    private WebSocketServer wsServer;

    // Remote tunnel fields
    private WebSocket httpTunnel;
    private WebSocket wsTunnel;

    [Header("UI & Utilities")]
    public RawImage targetRenderer;

    // Thread-safe command queue
    private ConcurrentQueue<(string payloadJson, string senderId)> commandQueue = new ConcurrentQueue<(string, string)>();

    // Map of unique connectionId (IP:Port or clientId:connectionId) → VirtualController
    public static Dictionary<string, VirtualController> allControllers = new Dictionary<string, VirtualController>();
    public static Dictionary<VirtualController, IWebSocketConnection> allSockets = new Dictionary<VirtualController, IWebSocketConnection>();

    // NEWLY ADDED:
    private Thread httpsThread;
    private TcpListener tcpListener;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        hostId = SystemInfo.deviceUniqueIdentifier;
        InputSystem.RegisterLayout<VirtualController>();

        if (useRemote)
            StartRemoteServers();
        else
        {
            // HTTP
            // StartHttpServer();

            StartHttpsServer();
            StartWebSocketServer(); // Uses WSS now
        }
    }

    void Update()
    {
        // Process incoming commands safely on main thread
        while (commandQueue.TryDequeue(out var item))
            HandleCommandOnMainThread(item.payloadJson, item.senderId);

        // Pump NativeWebSocket queues when using remote mode
        if (useRemote)
        {
            httpTunnel?.DispatchMessageQueue();
            wsTunnel?.DispatchMessageQueue();
        }
    }

    // ===================== Local Servers =====================
    void StartHttpsServer()
    {
        var ip = ChooseIP() ?? "0.0.0.0";
        TcpListener tcpListener = new TcpListener(IPAddress.Parse(ip), 8080);
        tcpListener.Start();
        Debug.Log($"[Local][HTTPS] Trying to run on https://{ip}:8080");
        QRCodeGenerator.GenerateQRCode("https://" + ip + ":8080", targetRenderer);
        httpsThread = new Thread(() =>
        {
            while (true)
            {
                try
                {
                    var client = tcpListener.AcceptTcpClient();
                    HandleClient(client);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[Local][HTTPS] " + ex.Message);
                }
            }
        })
        { IsBackground = true };
        httpsThread.Start();

        Debug.Log("[Local][HTTPS] Server started.");
    }

    void HandleClient(TcpClient client)
    {
        using var stream = client.GetStream();
        using var ssl = new SslStream(stream, false);
        string certPath = Path.Combine(Application.streamingAssetsPath, "iparty.pfx");
        Debug.Log("Looking for cert at:" + certPath);

        var cert = new X509Certificate2(certPath, "unity");
        ssl.AuthenticateAsServer(cert, false, SslProtocols.Tls12, false);

        using var reader = new StreamReader(ssl);
        using var writer = new StreamWriter(ssl) { AutoFlush = true };

        var requestLine = reader.ReadLine();
        if (string.IsNullOrEmpty(requestLine)) return;

        var tokens = requestLine.Split(' ');
        if (tokens.Length < 2) return;

        var path = tokens[1] == "/" ? "/index.html" : tokens[1];
        var filePath = Path.Combine(Application.streamingAssetsPath, path.TrimStart('/'));

        Debug.Log("Looking for index at:" + filePath);

        if (File.Exists(filePath))
        {
            var content = File.ReadAllBytes(filePath);
            var contentType = GetContentType(filePath);
            writer.WriteLine("HTTP/1.1 200 OK");
            writer.WriteLine($"Content-Length: {content.Length}");
            writer.WriteLine($"Content-Type: {contentType}");
            writer.WriteLine();
            ssl.Write(content, 0, content.Length);
        }
        else
        {
            writer.WriteLine("HTTP/1.1 404 Not Found");
            writer.WriteLine("Content-Length: 0");
            writer.WriteLine();
        }
    }

    void StartHttpServer()
    {
        httpListener = new HttpListener();
        string ip = ChooseIP() ?? "localhost";
        Debug.Log(ip);
        string prefix = $"http://{ip}:8080/";
        httpListener.Prefixes.Add(prefix);

        // Generate QR for clients
        QRCodeGenerator.GenerateQRCode("http://" + ip + ":8080/?hostId=8181", targetRenderer);
        Debug.Log($"[Local] HTTP server starting at {prefix}");

        httpListener.Start();
        httpThread = new Thread(() =>
        {
            while (httpListener.IsListening)
            {
                try
                {
                    var context = httpListener.GetContext();
                    string urlPath = context.Request.Url.AbsolutePath.TrimStart('/');
                    if (string.IsNullOrEmpty(urlPath)) urlPath = "index.html";
                    string filePath = Path.Combine(Application.streamingAssetsPath, urlPath);
                    Debug.Log(filePath);
                    if (File.Exists(filePath))
                    {
                        byte[] content = File.ReadAllBytes(filePath);
                        context.Response.ContentType = GetContentType(filePath);
                        context.Response.ContentLength64 = content.Length;
                        context.Response.OutputStream.Write(content, 0, content.Length);
                    }
                    else
                    {
                        context.Response.StatusCode = 404;
                        using var w = new StreamWriter(context.Response.OutputStream);
                        w.Write("404 - File Not Found");
                    }
                    context.Response.OutputStream.Close();
                }
                catch (Exception ex)
                {
                    Debug.LogError("[Local][HTTP] " + ex.Message);
                }
            }
        }) { IsBackground = true };
        httpThread.Start();

        Debug.Log("[Local][HTTP] Server started.");
    }

    void StartWebSocketServer()
    {
        FleckLog.Level = LogLevel.Debug;
        string ip = ChooseIP() ?? "0.0.0.0";
        Debug.Log(ip);
        string wsPrefix = $"wss://{ip}:8181";
        wsServer = new WebSocketServer(wsPrefix);
        string certPath = Path.Combine(Application.streamingAssetsPath, "iparty.pfx");
        wsServer.Certificate = new X509Certificate2(certPath, "unity");
        wsServer.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
        wsServer.Start(socket =>
        {
            socket.OnOpen = () =>
            {
                // Create unique connection ID using IP and port
                string connectionId = $"{socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort}";
                Debug.Log($"[Local][WS] Client connected: {connectionId}");
                MainThreadDispatcher.Enqueue(() =>
                {
                    var device = InputSystem.AddDevice<VirtualController>();
                    device.remoteId = connectionId;
                    allControllers[connectionId] = device;
                    allSockets[device] = socket;

                    // TESTING CHARACTER CREATION
                    // PlayerInputManager.instance.JoinPlayer(-1, -1, null, device);
                });
            };

            socket.OnClose = () =>
            {
                string connectionId = $"{socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort}";
                Debug.Log($"[Local][WS] Disconnected: {connectionId}");
                MainThreadDispatcher.Enqueue(() =>
                {
                    if (allControllers.TryGetValue(connectionId, out var dev))
                    {
                        foreach (var p in PlayerInput.all)
                        {
                            if (p.devices.Contains(dev)) { Destroy(p.gameObject); break; }
                        }
                        PlayerManager.RemovePlayer(allControllers[connectionId]);
                        allSockets.Remove(allControllers[connectionId]);
                        allControllers.Remove(connectionId);
                    }
                });
            };

            socket.OnMessage = msg =>
            {
                string connectionId = $"{socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort}";
                Debug.Log($"[Local][WS] Msg from {connectionId}");
                commandQueue.Enqueue((msg, connectionId));
            };
        });

        Debug.Log($"[Local] WS server started at {wsPrefix}");
    }

    // ===================== Remote Servers =====================

    async void StartRemoteServers()
    {
        StartCoroutine(RemoteDispatchLoop());

        string relayBase = "wss://iparty.duckdns.org:5001";
        string httpTunnelUrl = $"{relayBase}/unity/{hostId}/http";
        string wsTunnelUrl = $"{relayBase}/unity/{hostId}/ws";

        // Generate QR for remote URLs
        QRCodeGenerator.GenerateQRCode($"https://iparty.duckdns.org:5001/host/{hostId}/http/index.html?hostId={hostId}", targetRenderer);
        Debug.Log($"[Remote] HTTP Link = https://iparty.duckdns.org:5001/host/{hostId}/http/index.html?hostId={hostId}");
        Debug.Log($"[Remote] HTTP Tunnel = {httpTunnelUrl}");
        Debug.Log($"[Remote] WS Tunnel   = {wsTunnelUrl}");

        httpTunnel = new WebSocket(httpTunnelUrl);
        httpTunnel.OnMessage += async bytes =>
        {
            string rawJson = Encoding.UTF8.GetString(bytes);
            Debug.Log("[Remote][HTTP] Raw JSON: " + rawJson);
            try
            {
                var req = JsonUtility.FromJson<HttpTunnelRequest>(rawJson);
                string relPath = req.url.TrimStart('/');
                if (string.IsNullOrEmpty(relPath)) relPath = "og.html";
                string filePath = Path.Combine(Application.streamingAssetsPath, relPath);

                byte[] bodyBytes;
                int status = 200;
                string ct = GetContentType(filePath);

                if (!File.Exists(filePath))
                {
                    status = 404;
                    bodyBytes = Encoding.UTF8.GetBytes("404 - Not Found");
                    ct = "text/plain";
                }
                else
                {
                    try { bodyBytes = File.ReadAllBytes(filePath); }
                    catch { status = 500; bodyBytes = Encoding.UTF8.GetBytes("500 - Error"); ct = "text/plain"; }
                }

                string base64 = Convert.ToBase64String(bodyBytes);
                var resp = new HttpTunnelResponse { requestId = req.requestId, status = status, bodyBase64 = base64, contentType = ct };
                await httpTunnel.SendText(JsonUtility.ToJson(resp));
            }
            catch (Exception ex)
            {
                Debug.LogError("[Remote][HTTP] " + ex);
            }
        };

        wsTunnel = new WebSocket(wsTunnelUrl);
        wsTunnel.OnMessage += bytes =>
        {
            var rawJson = Encoding.UTF8.GetString(bytes);
            // Debug.Log($"[Remote][WS] Raw wrapper JSON: {rawJson}");

            var wrapper = JsonUtility.FromJson<WSTunnelRequest>(rawJson);

            // Disconnect-only
            if (wrapper.payloadBase64 == null && wrapper.@event == "disconnect")
            {
                Debug.Log($"[Remote][WS] DISCONNECT: {wrapper.clientId}");
                MainThreadDispatcher.Enqueue(() => CleanupController(wrapper.clientId));
                return;
            }

            // Payloads
            if (wrapper.payloadBase64 != null)
            {
                // Reserve the slot immediately
                if (!allControllers.ContainsKey(wrapper.clientId))
                {
                    Debug.Log($"[Remote][WS] First payload from {wrapper.clientId}, creating controller");
                    var device = InputSystem.AddDevice<VirtualController>();
                    device.remoteId = wrapper.clientId;
                    allControllers[wrapper.clientId] = device;
                }

                // Decode & debug log
                var payloadBytes = Convert.FromBase64String(wrapper.payloadBase64);
                var json = Encoding.UTF8.GetString(payloadBytes);
                // Debug.Log($"[Remote][WS] Decoded client JSON: {json}");

                // Enqueue for processing
                commandQueue.Enqueue((json, wrapper.clientId));
            }
        };

        // Connect tunnels
        try { await Task.WhenAll(httpTunnel.Connect(), wsTunnel.Connect()); }
        catch { Debug.LogError("[Remote] Connect failed"); }

        Debug.Log($"[Remote] HTTP={httpTunnel?.State}, WS={wsTunnel?.State}");
    }

    IEnumerator RemoteDispatchLoop()
    {
        while (true)
        {
            httpTunnel?.DispatchMessageQueue();
            wsTunnel?.DispatchMessageQueue();
            yield return null;
        }
    }

    // ===================== Shared Helpers =====================
    string ChooseIP()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        return host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString();
    }

    string GetContentType(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".html" => "text/html",
            ".js" => "application/javascript",
            ".css" => "text/css",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            _ => "application/octet-stream",
        };
    }

    public static void SendtoAllSocketsController(string controller)
    {
        var messageObject = new MessagePlayers
        {
            type = "controller",
            controller = controller,
            playerstats = PlayerManager.playerStats.Select(kvp => new PlayerConfig { name = kvp.Value.name, color = kvp.Value.color }).ToList()
        };

        string json = JsonUtility.ToJson(messageObject);

        SendSockets(json);
    }
    
    public static void SendSockets(string json)
    {
        if (instance.useRemote)
        {
            // Remote mode: send via wsTunnel
            if (instance.wsTunnel != null && instance.wsTunnel.State == WebSocketState.Open)
            {
                string base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
                var wrapper = new WSTunnelRequest
                {
                    clientId = "all",
                    payloadBase64 = base64Payload,
                    @event = null
                };
                string wrapperJson = JsonUtility.ToJson(wrapper);
                instance.wsTunnel.SendText(wrapperJson);
            }
        }
        else
        {
            // Local mode: send directly via IWebSocketConnection
            foreach (var sock in allSockets.Values.ToArray())
            {
                sock.Send(json);
            }
        }
    }
    void HandleCommandOnMainThread(string json, string sender)
    {
        try
        {
            var controller = allControllers[sender];
            if (PlayerManager.playerStats.ContainsKey(controller))
            {
                var cmd = JsonUtility.FromJson<CommandMessage>(json);
                var state = new GamepadState();
                Debug.Log(cmd.type);
                switch (cmd.type)
                {
                    case "analog":
                        state = new GamepadState
                        {
                            leftStick = new Vector2(cmd.x, cmd.y),
                            buttons =
                                (ushort)(
                                    (cmd.A ? (1 << (int)GamepadButton.South) : 0) |
                                    (cmd.D ? (1 << (int)GamepadButton.North) : 0) |
                                    (cmd.B ? (1 << (int)GamepadButton.East)  : 0) |
                                    (cmd.C ? (1 << (int)GamepadButton.West)  : 0) |
                                    (cmd.button ? (1 << (int)GamepadButton.LeftShoulder) : 0)
                                )
                        };
                        break;
                    case "dpad":
                        break;
                }
                InputSystem.QueueStateEvent(controller, state);
                InputSystem.Update();
            }
            else
            {
                var cmd = JsonUtility.FromJson<PlayerConfig>(json);
                byte[] face = Convert.FromBase64String(cmd.data);
                PlayerManager.RegisterPlayer(controller, cmd.color, cmd.name, face);
                PlayerInputManager.instance.JoinPlayer(-1, -1, null, controller);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Invalid command JSON: " + e.Message);
        }
    }

    // void parseImage(string image, string name)
    // {
    //     Debug.Log("Parsing image...");
    //     // var cmd = JsonUtility.FromJson<ImageJSON>(image);
    //     byte[] imageBytes = Convert.FromBase64String(image);
    //     Debug.Log(imageBytes);
    //     string path = Path.Combine(Application.dataPath, "Resources", "Images", "Faces", name + ".png");
    //     File.WriteAllBytes(path, imageBytes);
    //     Debug.Log("Saving file at " + path);
    // }

    void SpawnController(string clientId)
    {
        var device = InputSystem.AddDevice<VirtualController>();
        device.remoteId = clientId;
        allControllers[clientId] = device;

        // Spawned by HandleCommand
        // PlayerInputManager.instance.JoinPlayer(-1, -1, null, device);
    }

    void CleanupController(string clientId)
    {
        if (allControllers.TryGetValue(clientId, out var dev))
        {
            foreach (var p in PlayerInput.all)
                if (p.devices.Contains(dev)) { Destroy(p.gameObject); break; }
            // Removing Player from PlayerManager.
            PlayerManager.RemovePlayer(allControllers[clientId]);
            allControllers.Remove(clientId);
        }
    }

    void OnApplicationQuit()
    {
        if (useRemote)
        {
            httpTunnel?.Close();
            wsTunnel?.Close();
        }
        else
        {
            // httpListener?.Stop();
            // httpThread?.Abort();
            tcpListener?.Stop();
            httpsThread?.Abort();
            wsServer?.Dispose();
        }
    }

    [Serializable]
    public class CommandMessage
    {
        public string type;
        public float x;
        public float y;
        public bool A;
        public bool B;
        public bool C;
        public bool D;
        public bool button;
    }

    [Serializable]
    public class PlayerConfig { public string name; public string color; public string data; }

    [Serializable]
    public class MessagePlayers { public string type;  public string controller;  public List<PlayerConfig> playerstats; }

    [Serializable]
    private class HttpTunnelRequest { public string requestId; public string method; public string url; public string bodyBase64; public string contentType; }

    [Serializable]
    private class HttpTunnelResponse { public string requestId; public int status; public string bodyBase64; public string contentType; }

    [Serializable]
    private class WSTunnelRequest { public string clientId; public string payloadBase64; public string @event; }
}