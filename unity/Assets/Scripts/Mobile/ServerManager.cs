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


public class ServerManager : MonoBehaviour
{
    public bool useRemote = true;
    private string hostId;

    private WebSocket httpTunnel;
    private WebSocket wsTunnel;

    private HttpListener httpListener;
    private Thread httpThread;
    private WebSocketServer wsServer;

    public RawImage targetRenderer;
    public QRCodeGenerator QRCodeGenerator;

    private ConcurrentQueue<(string payloadJson, string senderId)> commandQueue =
        new ConcurrentQueue<(string, string)>();

    // Map of remoteId (IP or tunnel‐clientId) → VirtualController
    public static Dictionary<string, VirtualController> allControllers = new Dictionary<string, VirtualController>();


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

    private static ServerManager instance;

    void Start()
    {
        hostId = SystemInfo.deviceUniqueIdentifier;
        InputSystem.RegisterLayout<VirtualController>();

        if (useRemote)
        {
            StartRemoteServers();
        }
        else
        {
            StartHttpServer();
            StartWebSocketServer();
        }
    }

    void Update()
    {
        // Handle queued WebSocket commands safely on the main thread
        while (commandQueue.TryDequeue(out var item))
        {
            HandleCommandOnMainThread(item.payloadJson, item.senderId);
        }

        // pump the NativeWebSocket queues so OnMessage can fire:
        if (useRemote)
        {
            if (httpTunnel != null)
                httpTunnel.DispatchMessageQueue();
            if (wsTunnel != null)
                wsTunnel.DispatchMessageQueue();
        }
    }


    void StartHttpServer()
    {
        httpListener = new HttpListener();
        string ip = ChooseIP() ?? "localhost";
        string prefix = $"http://{ip}:8080/";
        httpListener.Prefixes.Add("http://"+ ip +":8080/"); // Localhost server on port 8080
        QRCodeGenerator.GenerateQRCode("http://" + ip + ":8080/", targetRenderer);
        Debug.Log(ip);
        httpListener.Start();

        httpThread = new Thread(() =>
        {
            while (httpListener.IsListening)
            {
                try
                {
                    var context = httpListener.GetContext();
                    string urlPath = context.Request.Url.AbsolutePath.TrimStart('/');
                    if (string.IsNullOrEmpty(urlPath)) urlPath = "og.html"; // Controller HTML

                    string filePath = Path.Combine(Application.streamingAssetsPath, urlPath);
                    Debug.Log("[Local][HTTP] Looking for file at: " + Path.GetFullPath(filePath));

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
                        using var writer = new StreamWriter(context.Response.OutputStream);
                        writer.Write("404 - File Not Found");
                    }

                    context.Response.OutputStream.Close();
                }
                catch (Exception ex)
                {
                    Debug.LogError("[Local][HTTP] Server error: " + ex.Message);
                }
            }
        })
        {
            IsBackground = true
        };
        httpThread.Start();

        Debug.Log("[Local] HTTP server started on " + prefix);
    }

    void StartWebSocketServer()
    {
        FleckLog.Level = LogLevel.Debug;
        string ip = ChooseIP() ?? "0.0.0.0";
        string wsPrefix = $"ws://{ip}:8181";
        wsServer = new WebSocketServer(wsPrefix);
        wsServer.Start(socket =>
        {
            socket.OnOpen = () =>
            {
                Debug.Log($"[Local][WS] Client connected, IP: {socket.ConnectionInfo.ClientIpAddress}");

                MainThreadDispatcher.Enqueue(() =>
                {
                    // Making the controller
                    var device = InputSystem.AddDevice<VirtualController>();
                    device.remoteId = socket.ConnectionInfo.ClientIpAddress;
                    allControllers[socket.ConnectionInfo.ClientIpAddress] = device;
                    // Spawning when player connected
                    PlayerInputManager.instance.JoinPlayer(-1, -1, null, device);
                });
            };

            socket.OnClose = () =>
            {
                Debug.Log($"[Local][WS] Client disconnected, IP: {socket.ConnectionInfo.ClientIpAddress}");
                MainThreadDispatcher.Enqueue(() =>
                {
                    if (allControllers.TryGetValue(socket.ConnectionInfo.ClientIpAddress, out var dev))
                    {
                        foreach (var player in PlayerInput.all)
                        {
                            if (player.devices.Contains(dev))
                            {
                                Destroy(player.gameObject);
                                break;
                            }
                        }
                        allControllers.Remove(socket.ConnectionInfo.ClientIpAddress);
                    }
                });
            };

            socket.OnMessage = msg =>
            {
                Debug.Log($"[Local][WS] Input received from: {socket.ConnectionInfo.ClientIpAddress}");
                commandQueue.Enqueue((msg, socket.ConnectionInfo.ClientIpAddress));
            };
        });

        Debug.Log("[Local] Fleck WebSocket server started on " + wsPrefix);
    }

    string ChooseIP()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();
        }
        return null;
    }

    string GetContentType(string filePath)
    {
        string ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".html" => "text/html",
            ".js"   => "application/javascript",
            ".css"  => "text/css",
            ".png"  => "image/png",
            ".jpg"  => "image/jpeg",
            _       => "application/octet-stream"
        };
    }


    async void StartRemoteServers()
    {
        // DispatchLoop coroutine so incoming WS frames get delivered:
        StartCoroutine(RemoteDispatchLoop());

        // relay URLs:
        string relayBase     = "ws://178.128.247.108:5000";
        string httpTunnelUrl = $"{relayBase}/unity/{hostId}/http";
        string wsTunnelUrl   = $"{relayBase}/unity/{hostId}/ws";
        QRCodeGenerator.GenerateQRCode($"http://178.128.247.108:5000/host/{hostId}/http/og.html?hostId={hostId}", targetRenderer);
        Debug.Log($"[Remote] HTTP Tunnel URL = {httpTunnelUrl}");
        Debug.Log($"[Remote] WS Tunnel URL   = {wsTunnelUrl}");

        httpTunnel = new WebSocket(httpTunnelUrl);
        httpTunnel.OnMessage += async (bytes) =>
        {
            // Received a JSON‐wrapped HTTP request from the relay:
            string rawJson = Encoding.UTF8.GetString(bytes);
            Debug.Log($"[Remote][HTTP] Received request JSON: {rawJson}");

            try
            {
                var req = JsonUtility.FromJson<HttpTunnelRequest>(rawJson);
                if (req == null)
                {
                    Debug.LogError("[Remote][HTTP] Failed to parse HttpTunnelRequest!");
                    return;
                }

                // Determine which file to serve
                string relPath = req.url.TrimStart('/');
                if (string.IsNullOrEmpty(relPath)) relPath = "og.html";
                string filePath = Path.Combine(Application.streamingAssetsPath, relPath);

                bool exists = File.Exists(filePath);
                byte[] fileBytes;
                int statusCode = 200;
                string contentType = "application/octet-stream";

                if (!exists)
                {
                    statusCode = 404;
                    fileBytes = Encoding.UTF8.GetBytes("404 - Not Found");
                    contentType = "text/plain";
                    Debug.LogWarning($"[Remote][HTTP] File not found: {filePath}");
                }
                else
                {
                    try
                    {
                        fileBytes = File.ReadAllBytes(filePath);
                        contentType = GetContentType(filePath);
                    }
                    catch (Exception readEx)
                    {
                        statusCode = 500;
                        fileBytes = Encoding.UTF8.GetBytes("500 - Internal Server Error");
                        contentType = "text/plain";
                        Debug.LogError($"[Remote][HTTP] Exception reading file \"{filePath}\": {readEx}");
                    }
                }

                string base64Body = Convert.ToBase64String(fileBytes);
                var respObj = new HttpTunnelResponse
                {
                    requestId   = req.requestId,
                    status      = statusCode,
                    bodyBase64  = base64Body,
                    contentType = contentType
                };
                string respJson = JsonUtility.ToJson(respObj);

                Debug.Log($"[Remote][HTTP] Sending response (requestId={req.requestId}, status={statusCode})");
                await httpTunnel.SendText(respJson);
            }
            catch (Exception ex)
            {
                Debug.LogError("[Remote][HTTP] Exception in OnMessage: " + ex);
            }
        };

        // WS‐tunnel
        wsTunnel = new WebSocket(wsTunnelUrl);
        wsTunnel.OnMessage += (bytes) =>
        {
            string rawJson = Encoding.UTF8.GetString(bytes);
            Debug.Log($"[Remote][WS] OnMessage raw JSON: {rawJson}");

            try
            {
                // parse the wrapper (clientId + Base64 payload + optional event)
                var wrapper = JsonUtility.FromJson<WSTunnelRequest>(rawJson);
                if (wrapper == null)
                {
                    Debug.LogError("[Remote][WS] Failed to parse WSTunnelRequest!");
                    return;
                }

                // this is a disconnect event, tear down that VirtualController
                if (wrapper.payloadBase64 == null && wrapper.@event == "disconnect")
                {
                    Debug.Log($"[Remote][WS] Received disconnect for clientId={wrapper.clientId}");
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        if (allControllers.TryGetValue(wrapper.clientId, out var dev))
                        {
                            foreach (var player in PlayerInput.all)
                            {
                                if (player.devices.Contains(dev))
                                {
                                    Destroy(player.gameObject);
                                    break;
                                }
                            }
                            allControllers.Remove(wrapper.clientId);
                        }
                    });
                    return;
                }

                // Base64‐decode payload to get the actual JSON from client
                byte[] payloadBytes = Convert.FromBase64String(wrapper.payloadBase64);
                string payloadJson = Encoding.UTF8.GetString(payloadBytes);
                Debug.Log($"[Remote][WS] Decoded payload JSON: {payloadJson}");

                // new player player spawn
                if (!allControllers.ContainsKey(wrapper.clientId))
                {
                    Debug.Log($"[Remote][WS] New remote client: {wrapper.clientId}");
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        var device = InputSystem.AddDevice<VirtualController>();
                        device.remoteId = wrapper.clientId;
                        allControllers[wrapper.clientId] = device;
                        PlayerInputManager.instance.JoinPlayer(-1, -1, null, device);
                    });
                }

                // Enqueue the payload JSON along with sender=clientId
                commandQueue.Enqueue((payloadJson, wrapper.clientId));

                Debug.Log($"[Remote][WS] Enqueued command for clientId={wrapper.clientId}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Remote][WS] Exception in OnMessage: " + ex);
            }
        };

        Task httpConnectTask = null;
        Task wsConnectTask   = null;

        try
        {
            httpConnectTask = httpTunnel.Connect();
            wsConnectTask = wsTunnel.Connect();
            await Task.WhenAll(httpConnectTask, wsConnectTask);
        }
        catch
        {
            Debug.LogError("[Remote] Neither httpConnectTask nor wsConnectTask was created. Check relay URLs.");
        }

        Debug.Log($"[Remote] StartRemoteServers() complete. Final States → HTTP={(httpTunnel != null ? httpTunnel.State.ToString() : "null")}, WS={(wsTunnel != null ? wsTunnel.State.ToString() : "null")}");
    }


    // pump both NativeWebSocket clients once per frame,
    private System.Collections.IEnumerator RemoteDispatchLoop()
    {
        while (true)
        {
            httpTunnel?.DispatchMessageQueue();
            wsTunnel?.DispatchMessageQueue();
            yield return null;
        }
    }

    void HandleCommandOnMainThread(string json, string sender)
    {
        try
        {
            var cmd = JsonUtility.FromJson<CommandMessage>(json);
            var controller = allControllers[sender];
            var state = new GamepadState
            {
                leftStick = new UnityEngine.Vector2(cmd.x, cmd.y),
            };
            InputSystem.QueueStateEvent(controller, state);
            InputSystem.Update();
        }
        catch (Exception e)
        {
            Debug.LogWarning("Invalid command JSON: " + e.Message);
        }
    }

    [Serializable]
    public class CommandMessage
    {
        public string type;
        public float x;
        public float y;
    }

    void OnApplicationQuit()
    {
        if (!useRemote)
        {
            httpListener?.Stop();
            httpThread?.Abort();
            wsServer?.Dispose();
        }
        else
        {
            httpTunnel?.Close();
            wsTunnel?.Close();
        }
    }


    [Serializable]
    private class HttpTunnelRequest
    {
        public string requestId;
        public string method;
        public string url;
        public string bodyBase64;
        public string contentType;
    }

    [Serializable]
    private class HttpTunnelResponse
    {
        public string requestId;
        public int status;
        public string bodyBase64;
        public string contentType;
    }

    [Serializable]
    private class WSTunnelRequest
    {
        public string clientId;
        public string payloadBase64;
        public string @event;
    }
}
