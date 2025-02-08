using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
    public static List<string> _playersListPackage = new List<string>();
    public bool timeOut = false;

    public ClientHandler(Socket socket, Server server)
    {
        clientSocket = socket;
        this.server = server;
        connected = true;
    }

    public static List<PlayerPackage> GetPlayersList()
    {
        var result = new List<PlayerPackage>();
        foreach (var playerSer in _playersListPackage)
        {
            var player = JsonConvert.DeserializeObject<PlayerPackage>(playerSer);
            result.Add(player);
        }

        return result;
    }

    [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: System.Byte[]; size: 1903MB")]
    public void HandleClient()
    {
        try
        {
            var buff = new byte[1024];
            int recB = clientSocket.Receive(buff);
            clientName = Encoding.UTF8.GetString(buff, 0, recB).Trim();
            Console.WriteLine($"{clientName} подключился к игре.");
            bool playerExists = _playersListPackage.Any(p =>
            {
                var existingPlayer = JsonConvert.DeserializeObject<PlayerPackage>(p);
                return existingPlayer != null && existingPlayer.Nickname == clientName;
            });

            if (!playerExists)
            {
                _playersListPackage.Add(JsonConvert.SerializeObject(new PlayerPackage
                {
                    Nickname = clientName,
                }));
            }
            else
            {
                var answer = new ConnectionStatusPackage
                {
                    ConnectionState = 404,
                    ConnectionDescription = "Такой игрок уже есть"
                };
                server.BroadcastPackage(answer, this);
                Disconnect();
            }

            var playerConnected = new PlayerConnectionPackage
            {
                PlayerName = clientName,
                CodeConnection = 1,
                Type = nameof(PlayerConnectionPackage)
            };
            var newClient = new MessagePackage
            {
                Sender = null,
                Content = $"{clientName} подключился к игре",
                Type = "MessagePackage"
            };
            var spawnPlayerPackage = new PlayerPackage
            {
                Nickname = clientName,
                SpawnPositionX = MapUpdater.SpawnPlayer(server._map).Item1,
                SpawnPositionY = MapUpdater.SpawnPlayer(server._map).Item2,
                Type = "SpawnPlayer"
            };
            var curentSession = new CurrentSession
            {
                grid = server._map,
                Type = nameof(CurrentSession)
            };
            server.BroadcastPackage(newClient, this);
            server.BroadcastPackage(spawnPlayerPackage, this);
            server.BroadcastPackage(curentSession, this);

            var connectionStatusPackage = new ConnectionStatusPackage
            {
                ConnectionState = (int)ConnectionState.Successful,
                ConnectionDescription = ConnectionState.Successful.ToString()
            };
            server.BroadcastPackage(connectionStatusPackage, this);
            server.BroadcastPackage(playerConnected, this);

            while (true)
            {
                try
                {
                    var buffer = new byte[2048];
                    int receivedBytes = clientSocket.Receive(buffer);
                    if (receivedBytes == 0)
                    {
                        Console.WriteLine($"Клиент {clientName} отключился");
                        Disconnect();
                        break;
                    }
                    string mess = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                    receivedData.Append(mess);
                    while (receivedData.ToString().Contains("\n"))
                    {
                        string[] messages = receivedData.ToString().Split(new[] { '\n' }, 2);
                        string json = messages[0].Trim();
                        receivedData.Clear();
                        if (messages.Length > 1)
                            receivedData.Append(messages[1]);
                        var messageObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                        string typeObj = messageObject["Type"];
                        switch (typeObj)
                        {
                            case "PlayerPackage":
                            {
                                PlayerPackage? package = JsonConvert.DeserializeObject<PlayerPackage>(json);
                                for (int i = 0; i < _playersListPackage.Count; i++)
                                {
                                    var playerObj =
                                        JsonConvert.DeserializeObject<PlayerPackage>(_playersListPackage[i]);
                                    if (playerObj != null && playerObj.Nickname == package.Nickname)
                                    {
                                        playerObj.PositionX = package.PositionX;
                                        playerObj.PositionY = package.PositionY;

                                        _playersListPackage[i] = JsonConvert.SerializeObject(playerObj);
                                        break;
                                    }
                                }

                                if (!timeOut)
                                {
                                    var playerListPackage = new PlayerListPackage
                                    {
                                        Type = "PlayersList",
                                        List = _playersListPackage
                                    };
                                    server.BroadcastPackage(playerListPackage, this);
                                    StartTimeOut();
                                }

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
                                server.BroadcastPackage(curentSession, this);
                                server.BroadcastPackage(package, this);
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Игрок {clientName} покинул игру");
                }
            }
        }
        finally
        {
            server.RemoveClient(this);
        }
    }

    public async Task StartTimeOut()
    {
        timeOut = true;
        await Task.Delay(250);
        timeOut = false;
    }

    public void SendMessage(string message)
    {
        try
        {
            var data = Encoding.UTF8.GetBytes(message + "\n");
            clientSocket.Send(data);
        }
        catch 
        {
            server.RemoveClient(this);
        }
    }

    public void Disconnect()
    {
        try
        {
            var answer = new PlayerConnectionPackage
            {
                PlayerName = clientName,
                CodeConnection = 0,
                Type = nameof(PlayerConnectionPackage)
            };

            server.BroadcastPackage(answer, this);
            connected = false;

            if (clientSocket.Connected)
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }

            server.RemoveClient(this);
            Console.WriteLine($"Клиент {clientName} успешно отключен");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при отключении {clientName}: {ex.Message}");
        }
    }

}
