using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
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

    public ClientHandler(Socket socket, Server server)
    {
        clientSocket = socket;
        this.server = server;
        connected = true;
    }
    
    public async Task HandleClient()
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

            while (true)
            {
                try
                {
                    var buffer = new byte[1024];
                    int receivedBytes = clientSocket.Receive(buffer);
                    var mess = Encoding.UTF8.GetString(buffer, 0, receivedBytes).Trim();
                    try
                    {
                        var messageObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(mess);
                        string typeObj = messageObject["Type"];
                        switch (typeObj)
                        {
                            case "PlayerPackage":
                            {
                                PlayerPackage? package = JsonConvert.DeserializeObject<PlayerPackage>(mess);
                                Console.WriteLine($"Игрок передвинулся: X={package.PositionX}, Y={package.PositionY}");
                                server.BroadcastPackage(package, this);
                                break;
                            }
                            case "MessagePackage":
                            {
                                MessagePackage? package = JsonConvert.DeserializeObject<MessagePackage>(mess);
                                Console.WriteLine($"{package.Sender}: {package.Content}");
                                server.BroadcastPackage(package, this);
                                break;
                            }
                            case "BombPackage":
                            {
                                BombPackage? package = JsonConvert.DeserializeObject<BombPackage>(mess);
                                Console.WriteLine($"Игрок {package.playerNickname} поставил бомбу {package.BombType} " +
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
                    catch (Exception e)
                    {
                        Console.WriteLine(e +  e.Message);
                        throw;
                    }
                }
                catch  
                {
                    Console.WriteLine($"{clientName} отключился.");
                    server.BroadcastPackage($"Игрок {clientName} отключился от игры", this);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка клиента: {ex.Message}");
        }
        finally
        {
            Disconnect();
        }
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
            Disconnect();
        }
    }

    public void Disconnect()
    {
        connected = false;
        clientSocket.Close();
        server.RemoveClient(this);
    }
}
