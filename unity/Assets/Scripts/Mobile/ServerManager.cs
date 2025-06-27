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
using UnityEngine.SceneManagement;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

/**
 * @brief Manages HTTP/HTTPS and WebSocket servers (local or remote), handles client connections, command routing, and controller lifecycle.
 */
public class ServerManager : MonoBehaviour
{
    /**
     * @brief Singleton instance reference.
     */
    private static ServerManager instance;
    /**
     * @brief Currently selected controller identifier.
     */
    public static string CurrentController = "";

    [Header("Mode Settings")]
    /**
     * @brief Enable to use remote relay servers instead of local HTTP/WS servers.
     */
    [Tooltip("Enable to use remote relay servers instead of local HTTP/WS servers.")]
    public bool useRemote = true;
    /**
     * @brief Static mirror of useRemote for initialization.
     */
    public static bool useRemoteStatic = true;
    /**
     * @brief Toggle secure WebSocket (WSS) vs insecure (WS).
     */
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
    /**
     * @brief UI element to render generated QR codes.
     */
    public RawImage targetRenderer;

    /**
     * @brief Thread-safe queue of incoming commands (JSON payload, sender ID).
     */
    private ConcurrentQueue<(string payloadJson, string senderId)> commandQueue = new ConcurrentQueue<(string, string)>();

    /**
     * @brief Map of connection IDs to VirtualController devices.
     */
    public static Dictionary<string, VirtualController> allControllers = new Dictionary<string, VirtualController>();
    /**
     * @brief List of currently taken player color strings.
     */
    public static List<string> takenColors = new List<string>();
    /**
     * @brief Map of VirtualController to its WebSocket connection.
     */
    public static Dictionary<VirtualController, IWebSocketConnection> allSockets = new Dictionary<VirtualController, IWebSocketConnection>();
    private readonly ConcurrentDictionary<IWebSocketConnection, bool> awaitingPongs = new();

    // Newly added for HTTPS
    private Thread httpsThread;
    private TcpListener tcpListener;

    /**
     * @brief Unity Awake event; enforces singleton and persists across scenes.
     */
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

    /**
     * @brief Unity Start event; registers input layout, chooses remote/local mode, and starts appropriate servers.
     */
    void Start()
    {
        hostId = SystemInfo.deviceUniqueIdentifier;
        InputSystem.RegisterLayout<VirtualController>();

        useRemote = useRemoteStatic;
        if (useRemote)
            StartRemoteServers();
        else
        {
            StartHttpsServer();
            StartWebSocketServer();
        }
    }

    /**
     * @brief Unity Update event; processes queued commands and pumps message queues in remote mode.
     */
    void Update()
    {
        while (commandQueue.TryDequeue(out var item))
            HandleCommandOnMainThread(item.payloadJson, item.senderId);

        if (useRemote)
        {
            httpTunnel?.DispatchMessageQueue();
            wsTunnel?.DispatchMessageQueue();
        }
    }

    // ===================== Local Servers =====================

    /**
     * @brief Starts a local HTTPS server on port 8080, generates QR code, and spawns handling thread.
     */
    void StartHttpsServer()
    {
        var ip = ChooseIP() ?? "0.0.0.0";
        TcpListener tcpListener = new TcpListener(IPAddress.Parse(ip), 8080);
        tcpListener.Start();
        Debug.Log($"[Local][HTTPS] Trying to run on https://{ip}:8080");
        MainThreadDispatcher.Enqueue(() => QRCodeGenerator.GenerateQRCode("https://" + ip + ":8080", targetRenderer));
        httpsThread = new Thread(() =>
        {
            while (true)
            {
                try
                {
                    var client = tcpListener.AcceptTcpClient();
                    MainThreadDispatcher.Enqueue(() => HandleClient(client));
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

    /**
     * @brief Handles an incoming HTTPS client: authenticates SSL, reads HTTP request, serves files or 404.
     * @param client The accepted TcpClient connection.
     */
    void HandleClient(TcpClient client)
    {
        using var stream = client.GetStream();
        using var ssl = new SslStream(stream, false);
        string certPath = Path.Combine(Application.streamingAssetsPath, "iparty.pfx");

        try
        {
            var cert = new X509Certificate2(certPath, "unity",
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
            Debug.Log("[Local][HTTPS] Certificate loaded.");
            ssl.AuthenticateAsServer(cert, false, SslProtocols.Tls12, false);
        }
        catch (Exception ex)
        {
            Debug.LogError("[Local][HTTPS] SSL Auth failed: " + ex.Message);
            if (ex.InnerException != null)
                Debug.LogError("Inner Exception: " + ex.InnerException.Message);
        }

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

    /**
     * @brief Starts a local HTTP server on port 8080, serves files from StreamingAssets, and spawns handling thread.
     */
    void StartHttpServer()
    {
        httpListener = new HttpListener();
        string ip = ChooseIP() ?? "localhost";
        Debug.Log(ip);
        string prefix = $"http://{ip}:8080/";
        httpListener.Prefixes.Add(prefix);

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

    /**
     * @brief Starts a local WebSocket server (secure or insecure) on port 8181, handles OnOpen, OnClose, and OnMessage.
     */
    void StartWebSocketServer()
    {
        FleckLog.Level = LogLevel.Debug;
        string ip = ChooseIP() ?? "0.0.0.0";
        Debug.Log(ip);

        string wsPrefix = useSecure ? $"wss://{ip}:8181" : $"ws://{ip}:8181";
        wsServer = new WebSocketServer(wsPrefix);
        string certPath = Path.Combine(Application.streamingAssetsPath, "iparty.pfx");
        wsServer.Certificate = new X509Certificate2(certPath, "unity",
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
        wsServer.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
        wsServer.Start(socket =>
        {
            socket.OnOpen = () =>
            {
                string connectionId = $"{socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort}";
                Debug.Log($"[Local][WS] Client connected: {connectionId}");
                MainThreadDispatcher.Enqueue(() =>
                {
                    var device = InputSystem.AddDevice<VirtualController>();
                    device.remoteId = connectionId;
                    allControllers[connectionId] = device;
                    allSockets[device] = socket;
                });
            };

            socket.OnClose = () =>
            {
                string connectionId = $"{socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort}";
                string port = socket.ConnectionInfo.ClientPort.ToString();
                Debug.Log($"[Local][WS] Disconnected: {connectionId}");
                MainThreadDispatcher.Enqueue(() =>
                {
                    Reconnect reconnectFunction = FindAnyObjectByType<Reconnect>();
                    if (SceneManager.GetActiveScene().name == "Lobby")
                    {
                        CleanupController(connectionId);
                    }
                    else if (reconnectFunction)
                    {
                        if (!reconnectFunction.disconnected)
                        {
                            reconnectFunction.DisconnectEvent(connectionId, socket.ConnectionInfo.ClientPort.ToString());
                        }
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

        _ = Task.Run(async () =>
        {
            while (true)
            {
                PingAllAsync();
                await Task.Delay(10000);
            }
        });

        Debug.Log($"[Local] WS server started at {wsPrefix}");
    }

    // ===================== Remote Servers =====================

    /**
     * @brief Initializes and connects remote HTTP and WS tunnels, sets up message handlers and dispatch loop.
     */
    async void StartRemoteServers()
    {
        StartCoroutine(RemoteDispatchLoop());

        string relayBase = "wss://iparty.duckdns.org:5001";
        string httpTunnelUrl = $"{relayBase}/unity/{hostId}/http";
        string wsTunnelUrl = $"{relayBase}/unity/{hostId}/ws";

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

            var wrapper = JsonUtility.FromJson<WSTunnelRequest>(rawJson);

            if (wrapper.payloadBase64 == null && wrapper.@event == "disconnect")
            {
                Debug.Log($"[Remote][WS] DISCONNECT: {wrapper.clientId}");
                MainThreadDispatcher.Enqueue(() => CleanupController(wrapper.clientId));
                return;
            }

            if (wrapper.payloadBase64 != null)
            {
                if (!allControllers.ContainsKey(wrapper.clientId))
                {
                    Debug.Log($"[Remote][WS] First payload from {wrapper.clientId}, creating controller");
                    var device = InputSystem.AddDevice<VirtualController>();
                    device.remoteId = wrapper.clientId;
                    allControllers[wrapper.clientId] = device;
                }

                var payloadBytes = Convert.FromBase64String(wrapper.payloadBase64);
                var json = Encoding.UTF8.GetString(payloadBytes);

                commandQueue.Enqueue((json, wrapper.clientId));
            }
        };

        try { await Task.WhenAll(httpTunnel.Connect(), wsTunnel.Connect()); }
        catch { Debug.LogError("[Remote] Connect failed"); }

        Debug.Log($"[Remote] HTTP={httpTunnel?.State}, WS={wsTunnel?.State}");
    }

    /**
     * @brief Dispatches pending messages for remote tunnels each frame.
     */
    IEnumerator RemoteDispatchLoop()
    {
        while (true)
        {
            httpTunnel?.DispatchMessageQueue();
            wsTunnel?.DispatchMessageQueue();
            yield return null;
        }
    }

    /**
     * @brief Chooses the first IPv4 address of this machine.
     * @return A string IP address or null if none found.
     */
    string ChooseIP()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        return host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString();
    }

    /**
     * @brief Determines the Content-Type header based on file extension.
     * @param path The file system path.
     * @return The MIME type string.
     */
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

    /**
     * @brief Sends a controller switch message to all clients.
     * @param controller The controller identifier to set.
     */
    public static void SendtoAllSockets(string controller)
    {
        CurrentController = controller;
        Debug.Log("Setting a new main controller");
        var messageObject = new MessagePlayers
        {
            type = "controller",
            controller = controller
        };

        string json = JsonUtility.ToJson(messageObject);

        SendMessages(json);
    }

    /**
     * @brief Closes all connections and clears internal state.
     */
    public void CloseConnections()
    {
        Debug.Log("[ServerManager] Closing WebSocket connections and clearing state...");

        if (useRemote)
        {
            httpTunnel?.Close();
            wsTunnel?.Close();
        }
        else
        {
            wsServer?.Dispose();
            httpsThread?.Abort();
            tcpListener?.Stop();
        }
        allControllers.Clear();
        allSockets.Clear();
        takenColors.Clear();
    }

    /**
     * @brief Sends a JSON string to all connected clients (remote or local).
     * @param json The JSON payload to send.
     */
    public static void SendMessages(string json)
    {
        if (instance.useRemote)
        {
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
            foreach (var sock in allSockets.Values.ToArray())
            {
                sock.Send(json);
            }
        }
    }

    /**
     * @brief Sends a control-clearing message to a specific client socket.
     * @param controller The VirtualController whose socket to send to.
     */
    public static void SendtoSocket(VirtualController controller)
    {
        var messageObject = new MessagePlayers
        {
            type = "clear-text",
            controller = controller.name
        };
        var sock = allSockets[controller];
        string json = JsonUtility.ToJson(messageObject);
        sock.Send(json);
    }

    /**
     * @brief Checks each WebSocket connection for availability and schedules reconnection or cleanup if needed.
     */
    public void CheckAllSockets()
    {
        foreach (var socket in allSockets.Values.ToArray())
        {
            if (!socket.IsAvailable)
            {
                MainThreadDispatcher.Enqueue(async () =>
                {
                    await Task.Yield();
                    Reconnect reconnectFunction = FindAnyObjectByType<Reconnect>();
                    string ip = socket.ConnectionInfo.ClientIpAddress;
                    int port = socket.ConnectionInfo.ClientPort;
                    string connectionId = $"{ip}:{port}";

                    if (SceneManager.GetActiveScene().name == "Lobby")
                    {
                        CleanupController(connectionId);
                    }
                    else if (reconnectFunction && !reconnectFunction.disconnected)
                    {
                        reconnectFunction.DisconnectEvent(connectionId, socket.ConnectionInfo.ClientPort.ToString());
                    }
                });
                Debug.Log($"The socket {socket.ConnectionInfo.ClientIpAddress} isn't available");
            }
        }
    }

    /**
     * @brief Logs a ping, clears awaiting pongs, and invokes socket checks.
     */
    public void PingAllAsync()
    {
        Debug.Log("Ping...");
        awaitingPongs.Clear();
        CheckAllSockets();
    }

    /**
     * @brief Handles a reconnection request by swapping devices and sockets when clients reconnect.
     * @param key The new controller key (clientId).
     * @param ip The IP part of the old controller key.
     * @param port The port part of the old controller key.
     */
    public void HandleReconnect(string key, string ip, string port)
    {
        string oldKey = $"{ip}:{port}";

        lock (controllerLock)
        {
            if (!allControllers.ContainsKey(oldKey) || !allControllers.ContainsKey(key)) return;

            var reconnectingDevice = allControllers[oldKey];
            var oldDevice = allControllers[key];

            if (!allSockets.TryGetValue(oldDevice, out var socket)) return;

            allSockets[reconnectingDevice] = socket;
            allSockets.Remove(oldDevice);

            InputSystem.RemoveDevice(oldDevice);
            allControllers[key] = reconnectingDevice;
            allControllers.Remove(oldKey);
        }

        Debug.Log($"[Reconnect] Swapped controller {oldKey} -> {key}");
        FindAnyObjectByType<Reconnect>()?.ReconnectEvent();
        CheckAllSockets();
    }

    /**
     * @brief Sends a JSON message to a single client, using remote or local transport as configured.
     * @param clientId The identifier of the target client.
     * @param json The JSON payload to send.
     */
    public static void SendMessageToClient(string clientId, string json)
    {
        if (instance.useRemote)
        {
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

    /**
     * @brief Processes a single command message JSON on the main Unity thread, updating input state or handling player/config commands.
     * @param json The command message payload.
     * @param sender The clientId of the sender.
     */
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
                                (cmd.B ? (1 << (int)GamepadButton.East) : 0) |
                                (cmd.C ? (1 << (int)GamepadButton.West) : 0) |
                                (cmd.button ? (1 << (int)GamepadButton.LeftShoulder) : 0)
                            )
                    };
                    break;
                case "dpad":
                    break;
                case "text":
                    cmd.T = new string(cmd.T.Where(c =>
                        char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());
                    if (cmd.T.Length < 200)
                    {
                        Debug.Log("received text input");
                        TMGameManager gameManager = FindAnyObjectByType<TMGameManager>();
                        gameManager.HandleMobileInput(controller, cmd.T);
                    }
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
                string controllerNow = "joystick-preset";
                if (allControllers.ContainsKey($"{ip}:{cmd.code}"))
                    {
                        HandleReconnect(sender, ip, cmd.code);
                        Debug.Log("The current controller is..." + CurrentController);
                        if (CurrentController != "")
                        {
                            Debug.Log("This point was reached.");
                            controllerNow = CurrentController;
                        }
                        var messageObject = new ReconnectJSON
                        {
                            type = "reconnect-status",
                            approved = true,
                            controller = controllerNow
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

                if (!string.IsNullOrEmpty(cmd.name) && cmd.name.Length < 24)
                {
                    cmd.name = new string(cmd.name.Where(c => char.IsLetterOrDigit(c) || c == ' ' || c == '_').ToArray());
                }
                else
                {
                    cmd.name = "Player";
                }


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
        }
        catch (Exception e)
        {
            Debug.LogWarning("Invalid command JSON: " + e.Message);
        }
    }

    /**
     * @brief Instantiates a new VirtualController for a given clientId and stores it.
     * @param clientId The unique identifier for the new controller.
     */
    void SpawnController(string clientId)
    {
        var device = InputSystem.AddDevice<VirtualController>();
        device.remoteId = clientId;
        allControllers[clientId] = device;
    }

    /**
     * @brief Cleans up and removes a client’s controller, UI, and resources on disconnect.
     * @param clientId The clientId whose controller to remove.
     */
    private readonly object controllerLock = new();

    void CleanupController(string clientId)
    {
        lock (controllerLock)
        {
            if (!allControllers.TryGetValue(clientId, out var dev)) return;

            foreach (var p in PlayerInput.all)
            {
                if (p.devices.Contains(dev)) { Destroy(p.gameObject); break; }
            }

            if (PlayerManager.playerStats.TryGetValue(dev, out var stats))
                takenColors.Remove(stats.color);

            PlayerSpawn playerSpawn = FindAnyObjectByType<PlayerSpawn>();
            playerSpawn?.RemoveFromLobby(dev);
            PlayerManager.RemovePlayer(dev);
            allSockets.Remove(dev);
            allControllers.Remove(clientId);
        }
    }

    /**
     * @brief Unity event called when the application quits; cancels tasks and closes all connections.
     */
    private CancellationTokenSource cancelSource = new();

    void OnApplicationQuit()
    {
        cancelSource.Cancel();

        if (useRemote)
        {
            httpTunnel?.Close();
            wsTunnel?.Close();
        }
        else
        {
            tcpListener?.Stop();
            wsServer?.Dispose();
        }
    }

    // ===================== Serializable Message Types =====================

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
    public class ReconnectJSON { public string type; public bool approved; public string controller; }
    [Serializable]
    public class CreatorJSON { public string type; public bool approved; public string name; }
    [Serializable]
    public class Ping { public string type; }
    [Serializable]
    private class HttpTunnelRequest { public string requestId; public string method; public string url; public string bodyBase64; public string contentType; }

    [Serializable]
    private class HttpTunnelResponse { public string requestId; public int status; public string bodyBase64; public string contentType; }

    [Serializable]
    private class WSTunnelRequest { public string clientId; public string payloadBase64; public string @event; }
}
