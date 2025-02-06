using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using GameServer;
using GameServer.Packages;
using Newtonsoft.Json;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

class ClientHandler
{
    private readonly Socket clientSocket;
    private readonly Server server;
    public string clientName = "Unknown";
    private bool connected;
    private StringBuilder receivedData = new StringBuilder();

    public ClientHandler(Socket socket, Server server)
    {
        clientSocket = socket;
        this.server = server;
        connected = true;
    }

    public void HandleClient()
    {
        try
        {
            var buff = new byte[1024];
            int recB = clientSocket.Receive(buff);
            clientName = Encoding.UTF8.GetString(buff, 0, recB).Trim();
            Console.WriteLine($"{clientName} подключился к игре.");
            var newClient = new MessagePackage
            {
                Sender = null,
                Content = $"{clientName} подключился к игре",
                Type = "MessagePackage"
            };
            server.BroadcastPackage(newClient, this);

            var spawnPlayerPackage = new PlayerPackage
            {
                Nickname = clientName,
                SpawnPositionX = MapUpdater.SpawnPlayer(server._map).Item1,
                SpawnPositionY = MapUpdater.SpawnPlayer(server._map).Item2,
                Type = "SpawnPlayer"
            };

            server.BroadcastPackage(spawnPlayerPackage, this);

            var curentSession = new CurrentSession
            {
                grid = server._map,
                Type = nameof(CurrentSession)
            };

            server.BroadcastPackage(curentSession, this);

            var connectionStatusPackage = new ConnectionStatusPackage
            {
                ConnectionState = (int)ConnectionState.Successful,
                ConnectionDescription = ConnectionState.Successful.ToString()
            };

            server.BroadcastPackage(connectionStatusPackage, this);
            
            while (true)
            {
                try
                {
                    var buffer = new byte[1024];
                    int receivedBytes = clientSocket.Receive(buffer);
                    if (receivedBytes == 0) break;

                    string mess = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                    receivedData.Append(mess);

                    while (receivedData.ToString().Contains("\n"))
                    {
                        string[] messages = receivedData.ToString().Split(new[] { '\n' }, 2);
                        string json = messages[0].Trim();
                        receivedData.Clear();
                        if (messages.Length > 1)
                            receivedData.Append(messages[1]);

                        try
                        {
                            var messageObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                            string typeObj = messageObject["Type"];

                            switch (typeObj)
                            {
                                case "PlayerPackage":
                                {
                                    PlayerPackage? package = JsonConvert.DeserializeObject<PlayerPackage>(json);
                                    server.BroadcastPackage(package, this);
                                    break;
                                }
                                case "MessagePackage":
                                {
                                    MessagePackage? package = JsonConvert.DeserializeObject<MessagePackage>(json);
                                    Console.WriteLine($"{package.Sender}: {package.Content}");
                                    server.BroadcastPackage(package, this);
                                    break;
                                }
                                case "BombPackage":
                                {
                                    BombPackage? package = JsonConvert.DeserializeObject<BombPackage>(json);
                                    Console.WriteLine(
                                        $"Игрок {package.playerNickname} поставил бомбу {package.BombType} " +
                                        $"по координатам X:{package.PositionX}, Y:{package.PositionY}");
                                    MapUpdater.SetBomb(server._map, package);
                                    server.BroadcastPackage(package, this);
                                    break;
                                }
                                case "CurrentPackage":
                                {
                                    break;
                                }
                            }
                        }
                        catch (JsonException e)
                        {
                            Console.WriteLine($"Ошибка десериализации: {e.Message}");
                        }
                    }
                }
                catch (SocketException)
                {
                    Console.WriteLine($"{clientName} отключился.");
                    server.BroadcastPackage($"Игрок {clientName} отключился от игры", this);
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Ошибка: {e.Message}");
                }
            }
        }
        finally{}
    }

    public void SendMessage(string message)
    {
        try
        {
            var data = Encoding.UTF8.GetBytes(message + "\n");
            clientSocket.Send(data);
        }
        catch 
        { Disconnect();
        }
    }

    public void Disconnect()
    {
        connected = false;
        clientSocket.Close();
        server.RemoveClient(this);
    }
}
