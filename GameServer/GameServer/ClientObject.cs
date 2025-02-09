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
    public static Server server;
    public string clientName = "Unknown";
    private bool connected;
    private StringBuilder receivedData = new StringBuilder();
    public bool timeOut = false;

    public ClientHandler(Socket socket, Server server)
    {
        clientSocket = socket;
        ClientHandler.server = server;
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
            bool playerExists = server._playersListPackage.Any(p => p != null && p.Nickname == clientName);
            if (playerExists)
            {
                var answer = new ConnectionStatusPackage
                {
                    ConnectionState = 404,
                    ConnectionDescription = "Такой игрок уже есть"
                };
                server.BroadcastPackageSingle(answer, this);
                Disconnect(this);
                return;
            }
            
            var existingPlayer = server._allPlayersList.FirstOrDefault(p => p.Nickname == clientName);
            PlayerPackage newPlayer;
            if (existingPlayer != null)
            {
                newPlayer = new PlayerPackage
                {
                    Nickname = existingPlayer.Nickname,
                    PositionX = existingPlayer.PositionX,
                    PositionY = existingPlayer.PositionY,
                    Health = existingPlayer.Health,
                };
            }
            else
            {
                newPlayer = new PlayerPackage
                {
                    Nickname = clientName,
                };
                server._allPlayersList.Add(newPlayer);
            }

            server._playersListPackage.Add(newPlayer);
            server.playersInGame.Add(newPlayer);

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
            server.BroadcastPackageAll(newClient);
            server.BroadcastPackageSingle(spawnPlayerPackage, this);
            server.BroadcastPackageSingle(curentSession, this);

            var connectionStatusPackage = new ConnectionStatusPackage
            {
                ConnectionState = (int)ConnectionState.Successful,
                ConnectionDescription = ConnectionState.Successful.ToString()
            };
            server.BroadcastPackageSingle(connectionStatusPackage, this);
            server.BroadcastPackageAll(playerConnected);

            while (true)
            {
                try
                {
                    var buffer = new byte[2048];
                    int receivedBytes = clientSocket.Receive(buffer);
                    if (receivedBytes == 0)
                    {
                        Disconnect(this);
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
                                for (int i = 0; i < server._playersListPackage.Count; i++)
                                {
                                    if (server._playersListPackage[i] != null && server._playersListPackage[i].Nickname == package.Nickname)
                                    {
                                        server._playersListPackage[i].PositionX = package.PositionX;
                                        server._playersListPackage[i].PositionY = package.PositionY;

                                        break;
                                    }
                                }
                                for (int i = 0; i < server._allPlayersList.Count; i++)
                                {
                                    if (server._allPlayersList[i] != null && server._allPlayersList[i].Nickname == package.Nickname)
                                    {
                                        server._allPlayersList[i].PositionX = package.PositionX;
                                        server._allPlayersList[i].PositionY = package.PositionY;

                                        break;
                                    }
                                }
                                
                                if (!timeOut)
                                {
                                    var playerListPackage = new PlayerListPackage
                                    {
                                        Type = "PlayersList",
                                        List = server._playersListPackage
                                    };
                                    server.BroadcastPackageAll(playerListPackage);
                                    StartTimeOut();
                                }

                                break;
                            }
                            case "MessagePackage":
                            {
                                MessagePackage? package = JsonConvert.DeserializeObject<MessagePackage>(json);
                                Console.WriteLine($"{package.Sender}: {package.Content}");
                                server.BroadcastPackageAll(package);
                                break;
                            }
                            case "BombPackage":
                            {
                                BombPackage? package = JsonConvert.DeserializeObject<BombPackage>(json);
                                Console.WriteLine(
                                    $"Игрок {package.playerNickname} поставил бомбу {package.BombType} " +
                                    $"по координатам X:{package.PositionX}, Y:{package.PositionY}");
                                MapUpdater.SetBomb(server._map, package);
                                server.BroadcastPackageAll(curentSession);
                                server.BroadcastPackageAll(package);
                                break;
                            }
                        }
                    }
                    if (server.playersInGame.Count == 1 && server.IsStarted)
                    {
                        var ans = new PlayerStatus
                        {
                            PlayerCode = 1,
                            PlayerNickname = clientName,
                            TextStatus = "Вы победили!!!"
                        };
                        SendPlayerWin(ans, clientName);
                        break;
                    }
                }
                catch (Exception e)
                {
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

    public void Disconnect(ClientHandler client)
    {
        try
        {
            var answer = new PlayerConnectionPackage
            {
                PlayerName = clientName,
                CodeConnection = 0,
                Type = nameof(PlayerConnectionPackage)
            };

            server.BroadcastPackageAll(answer);
            connected = false;
            foreach (var player in server._playersListPackage.ToList())
            {
                if (player.Nickname == clientName)
                {
                    server._playersListPackage.Remove(player);
                }
            }
            if (clientSocket.Connected)
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }

            server.RemoveClient(this);
            Console.WriteLine($"Клиент {clientName} отключился");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при отключении {clientName}: {ex.Message}");
        }
    }

    public static void SendPlayerStatus(PlayerStatus obj, string playerName)
    {
        foreach (var pl in server._playersListPackage)
        {
            if (pl.Nickname == playerName)
            {
                var client = server.clients.FirstOrDefault(p => p.clientName == playerName);
                server.BroadcastPackageSingle(obj, client);
            }
        }
    }

    public static void SendPlayerWin(PlayerStatus obj, string playerName)
    {
        foreach (var player in server.playersInGame)
        {
            if (player.Nickname == playerName)
            {
                var client = server.clients.FirstOrDefault(p => p.clientName == playerName);
                server.BroadcastPackageSingle(obj, client);
            }
        }
    }

}
