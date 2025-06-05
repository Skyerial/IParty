using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using UnityEngine;
using Fleck;
using System.Collections.Concurrent;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class ServerManager : MonoBehaviour
{
    private static ServerManager instance;
    private HttpListener httpListener;
    private Thread httpThread;
    private WebSocketServer wsServer;
    public static Dictionary<string, VirtualController> allControllers = new();
    public RawImage targetRenderer;

    // Command queue for thread-safe message handling
    private ConcurrentQueue<(string, string)> commandQueue = new ConcurrentQueue<(string, string)>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist through scenes
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }
    }

    void Start()
    {
        InputSystem.RegisterLayout<VirtualController>();
        StartHttpServer();
        StartWebSocketServer();
    }

    void Update()
    {
        // Handle queued WebSocket commands safely on the main thread
        while (commandQueue.TryDequeue(out (string, string) jsonSender))
        {
            var (json, sender) = jsonSender;
            HandleCommandOnMainThread(json, sender);
        }
    }

    string ChooseIP()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        return "*";
    }
    void StartHttpServer()
    {
        httpListener = new HttpListener();
        string ip = ChooseIP();
        // string ip = "localhost"; // Local debugging
        httpListener.Prefixes.Add("http://"+ ip +":8080/"); // Localhost server on port 8080
        QRCodeGenerator.GenerateQRCode("http://" + ip + ":8080/", targetRenderer);
        // QRGen?.GenerateQRCode("http://"+ ip +":8080/");
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

                    // If we want to change the directories.
                    // string rootPath = Path.Combine(Application.streamingAssetsPath, "/");
                    string filePath = Path.Combine(Application.streamingAssetsPath, urlPath);
                    Debug.Log("Looking for file at: " + Path.GetFullPath(filePath));
                    if (File.Exists(filePath))
                    {
                        if (filePath.EndsWith(".js"))
                        {
                            context.Response.ContentType = "application/javascript";
                        }
                        else if (filePath.EndsWith(".css"))
                        {
                            context.Response.ContentType = "text/css";
                        }
                        else if (filePath.EndsWith(".html"))
                        {
                            context.Response.ContentType = "text/html";
                        }

                        byte[] content = File.ReadAllBytes(filePath);
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
                    Debug.LogError("HTTP Server error: " + ex.Message);
                }
            }
        });
        httpThread.Start();

        Debug.Log("HTTP server started on port 8080");
    }

    void StartWebSocketServer()
    {
        FleckLog.Level = LogLevel.Debug;
        wsServer = new WebSocketServer("ws://0.0.0.0:8181"); // Initializing websocket for controller data.
        wsServer.Start(socket =>
        {
            socket.OnOpen = () =>
            {
                Debug.Log("WS: Client connected, IP:" + socket.ConnectionInfo.ClientIpAddress); // Debug
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

            socket.OnClose = () => Debug.Log("WS: Client disconnected, IP:" + socket.ConnectionInfo.ClientIpAddress); // Debug
            socket.OnMessage = msg =>
            {
                Debug.Log("WS: Input received from:" + socket.ConnectionInfo.ClientIpAddress);
                commandQueue.Enqueue((msg, socket.ConnectionInfo.ClientIpAddress)); // Main thread queue
            };
        });

        Debug.Log("WebSocket server started on port 8181");
    }

    void HandleCommandOnMainThread(string json, string sender)
    {
        try
        {   
            var cmd = JsonUtility.FromJson<CommandMessage>(json);
            var controller = allControllers[sender];
            var state = new GamepadState
                    {
                        leftStick = new Vector2(cmd.x, cmd.y),
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
        httpListener?.Stop();
        httpThread?.Abort();
        wsServer?.Dispose();
    }
}
