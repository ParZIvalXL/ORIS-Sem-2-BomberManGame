using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NetCode.Packages;
using Newtonsoft.Json;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace NetCode
{
    public class GameClientScript : MonoBehaviour
    {
        public string playerName;
        private Socket _client;
        private string _host = "127.0.0.1";
        private int _port = 8888;
        public static GameClientScript Instance; 
        public GameObject[] BombsList;

        void Awake()
        {
            Instance = this;
        }
        public async Task ConnectingPlayer(string name)
        {
            UIConnectScript.Instance._connectButton.interactable = false;
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                UIManager.Instance.loadingUI.SetText("Пытаемся подключиться к серверу, подождите чуть-чуть...");
                UIManager.Instance.loadingUI.Show(true);
                await ConnectToServer();
            }
            catch (Exception e)
            {
                Debug.Log(e);
                UIManager.Instance.loadingUI.SetText("Кажется, что-то пошло не так, пробуем снова...");
                try
                {
                    await ConnectToServer();
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                    UIManager.Instance.loadingUI.SetText("Не удалось установить соединение с сервером, попробуйте позже...");
                    UIManager.Instance.loadingUI.HideLoader(true);
                    UIConnectScript.Instance._connectButton.interactable = true;
                    return;
                }
            }

            // Проверяем, действительно ли клиент подключен
            if (!_client.Connected)
            {
                UIManager.Instance.loadingUI.SetText("Не удалось установить соединение с сервером.");
                UIManager.Instance.loadingUI.HideLoader(true);
                return;
            }

            UIManager.Instance.loadingUI.SetText("Успешно подключились! Отправляем ваши данные...");
            playerName = name;
            await SendName(playerName);
            try
            {
                UIManager.Instance.loadingUI.SetText("Зарегистрировались! Ждем данные и игровой сессии...");
                _ = Task.Run(() => ReceiveMessagesAsync(_client));
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            
        }

        public async Task ConnectToServer()
        {
            try
            {
                Debug.Log("Connecting...");

                // Устанавливаем таймаут для подключения (например, 5 секунд)
                var connectTask = _client.ConnectAsync(_host, _port);
                var timeoutTask = Task.Delay(5000); // 5 секунд

                var completedTask = await Task.WhenAny(connectTask, timeoutTask);

                if (completedTask == timeoutTask || !_client.Connected) // Добавляем проверку _client.Connected
                {
                    throw new TimeoutException("Не удалось подключиться к серверу за отведенное время или сервер недоступен.");
                }

                // Если подключение успешно
                Debug.Log("Connected!");
                UIManager.Instance.CloseInterface(true);
                UIConnectScript.Instance.uiLogin.Close();
            }
            catch (Exception e)
            {
                throw new Exception("Не удалось подключиться к серверу: " + e.Message); 
            }
        }


        public async Task SendMessagesAsync(string message)
        {
            byte[] nameData = Encoding.UTF8.GetBytes(message + "\n");
            _client.Send(nameData);
        }
        
        public async Task SendName(string message)
        {
            byte[] nameData = Encoding.UTF8.GetBytes(playerName + "\n");
            _client.Send(nameData);
        }
        
        async Task ReceiveMessagesAsync(Socket clientSocket)
        {
            try
            {
                byte[] buffer = new byte[4096];
                
                while (true)
                {
                    int receivedBytes = clientSocket.Receive(buffer);
                    if (receivedBytes > 0)
                    {
                        string encodedMessage = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                        string[] messages = encodedMessage.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                        Debug.Log(encodedMessage);
                        foreach (var message in messages)
                        {
                            var messageObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);
                            string messType = messageObject["Type"].ToString();
                            switch (messType)
                            {
                                case "MessagePackage":
                                {
                                    MessagePackage messagePackage = JsonConvert.DeserializeObject<MessagePackage>(message);
                                    ChatHolder.AddMessage(messagePackage);
                                    GameController.Instance.AddAction(() => { SendMessage(messagePackage);});
                                    break;
                                }
                                case "PlayerPackage":
                                {
                                    PlayerPackage playerPackage = JsonConvert.DeserializeObject<PlayerPackage>(message);
                                    // Debug.Log($"{playerPackage.Nickname} переместился на координаты: {playerPackage.PositionX}, {playerPackage.PositionY}");
                                    
                                    // какой то код
                                    break;
                                }
                                case "CurrentSession":
                                {
                                    CurrentSession currentSession =
                                        JsonConvert.DeserializeObject<CurrentSession>(message);
                                    ChatHolder.AddMessage(new MessagePackage
                                    {
                                        Sender = "Server",
                                        Content = "Карта загружена",
                                    });
                                    GameController.Instance.AddAction(() =>
                                    {
                                        UIManager.Instance.loadingUI.SetText("Карта загружена! Сейчас все расставим и можно играть!");
                                        
                                        MapBuilder.Instance.GenerateMap(currentSession);
                                        
                                        UIManager.Instance.loadingUI.HideLoader(true);
                                    });
                                    
                                    // код для конвертации карты
                                    break;
                                }

                                case "SpawnPlayer":
                                {
                                    PlayerPackage playerPackage = JsonConvert.DeserializeObject<PlayerPackage>(message);
                                    Debug.Log($"Игрок {playerPackage.Nickname} появился на позиции X: {playerPackage.SpawnPositionX}, Y:{playerPackage.SpawnPositionY}");
                                    ChatHolder.AddMessage(new MessagePackage
                                    {
                                        Sender = "Server",
                                        Content = $"Игрок {playerPackage.Nickname} появился на позиции X: {playerPackage.SpawnPositionX}, Y:{playerPackage.SpawnPositionY}"
                                    });
                                    GameController.Instance.AddAction(() =>
                                    {
                                        MapBuilder.Instance.CreatePlayer(playerPackage);
                                    });
                                    break;
                                }

                                case "ConnectionStatusPackage":
                                {
                                    var connectionStatusPackage = JsonConvert.DeserializeObject<ConnectionStatusPackage>(message);
                                    GameController.Instance.AddAction(() =>
                                    {
                                        if (connectionStatusPackage.ConnectionState == 200)
                                        {
                                            UIManager.Instance.loadingUI.SetText("Успешно подключились!");
                                        }
                                        else if (connectionStatusPackage.ConnectionState == 400 || 
                                                 connectionStatusPackage.ConnectionState == 404)
                                        {
                                            UIConnectScript.Instance.ShowErrorText(connectionStatusPackage
                                                .ConnectionDescription);
                                        }
                                    });
                                        break;
                                }
                                case "BombPackage":
                                {
                                    var bombPackage = JsonConvert.DeserializeObject<BombPackage>(message);
                                    Debug.Log($"Бомба появилась на позиции X: {bombPackage.PositionX}, Y:{bombPackage.PositionY}");
                                    GameController.Instance.AddAction(() => {
                                        var bomb = Instantiate(
                                            BombsList[0],
                                            new Vector3(bombPackage.PositionX, bombPackage.PositionY, 0),
                                            Quaternion.identity);
                                        if (bomb.TryGetComponent<BombScript>(out var bombScript))
                                        {
                                            bombScript.GetComponent<BombScript>().playerName = bombPackage.playerNickname;
                                        }
                                    });

                                    break;
                                }
                                case "PlayersList":
                                {
                                    var package = JsonConvert.DeserializeObject<PlayerListPackage>(message);
                                    foreach (var player in package.List)
                                    {
                                        var playerObj = JsonConvert.DeserializeObject<PlayerPackage>(player);
                                        Debug.Log("player: " + player);
                                        Debug.Log(playerObj.Nickname + " " + playerObj.PositionX + " " + playerObj.PositionY);
                                        GameController.Instance.AddAction(() =>
                                        {
                                            if(GameController.Instance.GetPlayer(playerObj.Nickname) == null)
                                                GameController.Instance.AddPlayer(playerObj);
                                            else
                                                GameController.Instance.UpdatePlayer(playerObj);
                                        });
                                    }
                                    break;
                                }
                                default:
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"Ошибка получения сообщений: {ex.Message}");
            }
        }
        
        private static void SendMessage(MessagePackage messagePackage)
        {
            var prefix = messagePackage.Sender == null ? messagePackage.Sender + ": " : "";
            string message = prefix + messagePackage.Content;
            Debug.Log(message);
            if(ChatScript.Instance != null)
                ChatScript.Instance.CreateNewMessage(message);
        }
        
        public async void SendBombPackage(Vector2 position)
        {
            var bombPackage = JsonConvert.SerializeObject(new BombPackage
            {
                playerNickname = playerName,
                BombType = BombType.Classic.ToString(),
                PositionX = Mathf.RoundToInt(position.x),
                PositionY = Mathf.RoundToInt(position.y),
                Type = nameof(BombPackage)
            });

            await SendMessagesAsync(bombPackage);
        }

        public async void SendPlayerPackage(PlayerPackage player)
        {
            var playerPackage = JsonConvert.SerializeObject(player , new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            await SendMessagesAsync(playerPackage);
        }
    }
}
