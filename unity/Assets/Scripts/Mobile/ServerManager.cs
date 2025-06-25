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

public class ServerManager : MonoBehaviour
{
    private static ServerManager instance;

    [Header("Mode Settings")]
    [Tooltip("Enable to use remote relay servers instead of local HTTP/WS servers.")]
    public bool useRemote = true;
    public static bool useRemoteStatic = true;


    // Toggle secure websocket, insecure is used to connect testing script
    public bool useSecure = true;

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
    public static List<string> takenColors = new List<string>();

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

        useRemote = useRemoteStatic;
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
        })
        { IsBackground = true };
        httpThread.Start();

        Debug.Log("[Local][HTTP] Server started.");
    }

    void StartWebSocketServer()
    {
        FleckLog.Level = LogLevel.Debug;
        string ip = ChooseIP() ?? "0.0.0.0";
        Debug.Log(ip);

        // Select secure or insecure websocket
        string wsPrefix = useSecure ? $"wss://{ip}:8181" : $"ws://{ip}:8181";
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

                    // var message = new
                    // {
                    //     type = "clientinfo",
                    //     clientId = connectionId
                    // };
                    // string json = JsonUtility.ToJson(message);
                    // SendMessageToClient(connectionId, json);
                    // TESTING CHARACTER CREATION
                    // PlayerInputManager.instance.JoinPlayer(-1, -1, null, device);
                });
            };

            socket.OnClose = () =>
            {
                string connectionId = $"{socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort}";
                string port = socket.ConnectionInfo.ClientPort.ToString();
                Debug.Log($"[Local][WS] Disconnected: {connectionId}");
                MainThreadDispatcher.Enqueue(() =>
                {
                    // RECONNECT EVENT SLOT
                    Reconnect reconnectFunction = FindAnyObjectByType<Reconnect>();
                    if (reconnectFunction)
                    {
                        reconnectFunction.DisconnectEvent(connectionId, port);
                    }
                    else
                    {
                        // Old method
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

    public static void SendtoAllSockets(string controller)
    {
        var messageObject = new MessagePlayers
        {
            type = "controller",
            controller = controller
        };

        string json = JsonUtility.ToJson(messageObject);

        SendMessages(json);
    }

    public static void SendMessages(string json)
    {
        if (instance.useRemote)
        {
            // Remote mode: send via wsTunnel to each remote client
            if (instance.wsTunnel != null && instance.wsTunnel.State == WebSocketState.Open)
            {
                string base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

                foreach (var clientId in allControllers.Keys)
                {
                    var wrapper = new WSTunnelRequest
                    {
                        clientId = clientId,
                        payloadBase64 = base64Payload,
                        @event = null
                    };

                    string wrapperJson = JsonUtility.ToJson(wrapper);
                    instance.wsTunnel.SendText(wrapperJson);
                }
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

    public void HandleReconnect(string key, string ip, string port)
    {
        string oldKey = $"{ip}:{port}"; // reconnecting client
        if (!allControllers.ContainsKey(oldKey) || !allControllers.ContainsKey(key))
        {
            Debug.LogWarning($"[Reconnect] Missing key: {oldKey} or {key}");
            return;
        }

        var reconnectingDevice = allControllers[oldKey];
        var oldDevice = allControllers[key];


        if (!allSockets.TryGetValue(oldDevice, out var socket))
        {
            Debug.LogWarning($"[Reconnect] No socket found for device with key: {key}");
            return;
        }

        allSockets[reconnectingDevice] = socket;
        allSockets.Remove(oldDevice);
        InputSystem.RemoveDevice(oldDevice);

        // Replace controller in key map
        allControllers[key] = reconnectingDevice;
        allControllers.Remove(oldKey);

        Debug.Log($"[Reconnect] Swapped controller {oldKey} -> {key}");

        var reconnectFunction = FindAnyObjectByType<Reconnect>();
        reconnectFunction?.ReconnectEvent();
    }
    public static void SendMessageToClient(string clientId, string json)
    {
        if (instance.useRemote)
        {
            // Remote mode: send to one client via wsTunnel
            if (instance.wsTunnel != null && instance.wsTunnel.State == WebSocketState.Open)
            {
                string base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
                var wrapper = new WSTunnelRequest
                {
                    clientId = clientId,
                    payloadBase64 = base64Payload,
                    @event = null
                };
                string wrapperJson = JsonUtility.ToJson(wrapper);
                instance.wsTunnel.SendText(wrapperJson);
            }
        }
        else
        {
            // Local mode: find the socket and send directly
            if (allControllers.TryGetValue(clientId, out var device) && allSockets.TryGetValue(device, out var socket))
            {
                socket.Send(json);
            }
            else
            {
                Debug.LogWarning($"[Local][WS] No socket found for client: {clientId}");
            }
        }
    }

    void HandleCommandOnMainThread(string json, string sender)
    {
        // try
        // {
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
                                (cmd.B ? (1 << (int)GamepadButton.East) : 0) |
                                (cmd.C ? (1 << (int)GamepadButton.West) : 0) |
                                (cmd.button ? (1 << (int)GamepadButton.LeftShoulder) : 0)
                            )
                    };
                    break;
                case "dpad":
                    break;
                case "text":
                    Debug.Log("received text input");
                    Debug.Log(cmd.T);
                    // PlayerTypingController.HandleInput(cmd.T);
                    TMGameManager gameManager = FindAnyObjectByType<TMGameManager>();
                    gameManager.HandleMobileInput(controller, cmd.T);
                    break;
            }
            InputSystem.QueueStateEvent(controller, state);
            InputSystem.Update();
        }
        else
        {
            var cmd = JsonUtility.FromJson<PlayerConfig>(json);
            if (cmd.type == "reconnect")
            {
                string ip = sender.Split(":")[0];
                string port = sender.Split(":")[1];
                if (allControllers.ContainsKey($"{ip}:{cmd.code}"))
                {
                    HandleReconnect(sender, ip, cmd.code);
                    var messageObject = new ReconnectJSON
                    {
                        type = "reconnect-status",
                        approved = true
                    };

                    var data = JsonUtility.ToJson(messageObject);
                    SendMessageToClient(sender, data);
                }
                else
                {
                    var messageObject = new ReconnectJSON
                    {
                        type = "reconnect-status",
                        approved = false
                    };

                    var data = JsonUtility.ToJson(messageObject);
                    SendMessageToClient(sender, data);
                }
            }
            else
            {
                Debug.Log("Current color: " + cmd.color);
                Debug.Log(takenColors.Contains(cmd.color));
                if (takenColors.Contains(cmd.color))
                {
                    var messageObject = new CreatorJSON
                    {
                        type = "character-status",
                        approved = false,
                        name = cmd.name
                    };

                    var data = JsonUtility.ToJson(messageObject);
                    SendMessageToClient(sender, data);
                }
                else
                {
                    takenColors.Add(cmd.color);
                    byte[] face = Convert.FromBase64String(cmd.data);
                    PlayerManager.RegisterPlayer(controller, cmd.color, cmd.name, face);
                    PlayerInputManager.instance.JoinPlayer(-1, -1, null, controller);

                    var messageObject = new CreatorJSON
                    {
                        type = "character-status",
                        approved = true,
                        name = cmd.name
                    };

                    var data = JsonUtility.ToJson(messageObject);
                    SendMessageToClient(sender, data);
                }

            }
        }
        // }
        // catch (Exception e)
        // {
        //     Debug.LogWarning("Invalid command JSON: " + e.Message);
        // }
    }

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
        public string T;
        public bool button;
    }

    [Serializable]
    public class PlayerConfig { public string type; public string code; public string name; public string color; public string data; }

    [Serializable]
    public class MessagePlayers { public string type; public string controller;/* public List<PlayerConfig> playerstats; */ }

    [Serializable]
    public class ReconnectJSON { public string type; public bool approved; }
    [Serializable]
    public class CreatorJSON { public string type; public bool approved; public string name; }

    [Serializable]
    private class HttpTunnelRequest { public string requestId; public string method; public string url; public string bodyBase64; public string contentType; }

    [Serializable]
    private class HttpTunnelResponse { public string requestId; public int status; public string bodyBase64; public string contentType; }

    [Serializable]
    private class WSTunnelRequest { public string clientId; public string payloadBase64; public string @event; }
}
