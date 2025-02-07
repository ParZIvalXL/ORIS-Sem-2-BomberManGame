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
    private List<ClientHandler> clients = new List<ClientHandler>();
    private readonly int port;
    public TileType[,]? _map;

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
            _map = MapsReader.GetMap(File.ReadAllText("Maps.json"));
            while (true)
            {
                var clientSocket = listenerSocket.Accept();
                if (clients.Count >= 4)
                {
                    
                }
                var clientHandler = new ClientHandler(clientSocket, this);
                clients.Add(clientHandler);
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

    public void BroadcastPackage(object? obj, ClientHandler sender)
    {
        var message = JsonConvert.SerializeObject(obj);
        if (typeof(CurrentSession) == obj.GetType() && sender == clients[clients.Count - 1])
        {
            sender.SendMessage(message);
            return;
        }

        foreach (var client in clients)
        {
            try
            {
                client.SendMessage(message);
            }
            catch
            {
                clients.Remove(client);
                client.Disconnect();
            }
        }
    }

    public void RemoveClient(ClientHandler client)
    {
        clients.Remove(client);
    }

    public void Stop()
    {
        foreach (var client in clients)
        {
            client.Disconnect();
        }
        listenerSocket.Close();
        Console.WriteLine("Сервер остановлен.");
    }
}