using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GameServer;
using GameServer.Packages;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

class Server
{
    private Socket listenerSocket;
    public List<ClientHandler> clients = new List<ClientHandler>();
    private readonly int port;
    public TileType[,]? _map;
    private bool isRunning = true;
    public List<PlayerPackage> playersInGame = new List<PlayerPackage>();
    public List<PlayerPackage> _playersListPackage = new List<PlayerPackage>();
    public  List<PlayerPackage> _allPlayersList = new List<PlayerPackage>();
    public bool IsStarted { get; private set; } = false;

    public Server(int port)
    {
        this.port = port;
        listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public void Start()
    {
        try
        {
            listenerSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            listenerSocket.Listen(10);
            Console.WriteLine($"Сервер запущен на {IPAddress.Any}:{port}. Ожидание подключений...");
            _map = MapsReader.MirrorMapX(MapsReader.GetMap(File.ReadAllText("Maps.json")));

            while (isRunning) // Используем флаг вместо while(true)
            {
                var clientSocket = listenerSocket.Accept();
                var clientHandler = new ClientHandler(clientSocket, this);
                clients.Add(clientHandler);

                if (clients.Count >= 2)
                {
                    IsStarted = true;
                    var answerStartedGame = new ConnectionStatusPackage
                    {
                        ConnectionState = 100,
                        ConnectionDescription = "Игра началась"
                    }; 
                    BroadcastPackageAll(answerStartedGame);

                    if (clients.Count > 4)
                    {
                        var answer = new ConnectionStatusPackage
                        {
                            ConnectionState = 400,
                            ConnectionDescription = "Полное лобби"
                        };
                        BroadcastPackageSingle(answer, clientHandler);
                        clients.Remove(clientHandler);
                        clientHandler.Disconnect(clientHandler);
                    }
                }

                Task.Run(() => clientHandler.HandleClient());
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Ошибка сокета: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сервера: {ex.Message}");
        }   
        finally
        {
            Stop();
        }
    }

    public void BroadcastPackageSingle(object? obj, ClientHandler clientHandler)
    {
        var message = JsonConvert.SerializeObject(obj);
        clientHandler.SendMessage(message);
        Console.WriteLine(message);
    }

    public void BroadcastPackageAll(object? obj)
    {
        try
        {
            var message = JsonConvert.SerializeObject(obj);
            foreach (var client in clients.ToList())
            {
                try
                {
                    client.SendMessage(message);
                }
                catch
                {
                    RemoveClient(client);
                    client.Disconnect(client);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e + " хуесосы");
            throw;
        }
    }
    

    public void RemoveClient(ClientHandler client)
    {
        clients.Remove(client);
    }


    public void Stop()
    {
        isRunning = false;
    
        try
        {
            listenerSocket.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при закрытии сокета: {ex.Message}");
        }

        foreach (var client in clients)
        {
            client.Disconnect(client);
        }
        clients.Clear();
    
        Console.WriteLine("Сервер остановлен.");
    }
}