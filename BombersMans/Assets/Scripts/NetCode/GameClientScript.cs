using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NetCode.Packages;
using Newtonsoft.Json;
using UnityEngine;

namespace NetCode
{
    public class GameClientScript : MonoBehaviour
    {
        public string playerName;
        private Socket _client;
        private string _host = "127.0.0.1";
        private int _port = 8888;
        public static GameClientScript Instance;

        void Awake()
        {
            Instance = this;
        }
        public async Task ConnectingPlayer(string name)
        {
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _client.Connect(_host, _port);
            playerName = name;
            await SendName(playerName);
            try
            {
                _ = Task.Run(() => ReceiveMessagesAsync(_client));
            }
            catch (Exception e)
            {
                Debug.Log(e);
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
                byte[] buffer = new byte[2048];
                
                while (true)
                {
                    int receivedBytes = clientSocket.Receive(buffer);
                    if (receivedBytes > 0)
                    {
                        string encodedMessage = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                        string[] messages = encodedMessage.Split('\n', StringSplitOptions.RemoveEmptyEntries);
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
                                    Debug.Log($"{playerPackage.Nickname} переместился на координаты: {playerPackage.PositionX}, {playerPackage.PositionY}");
                                    
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
                                    
                                    // код для спавна игрока на карте
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
            var playerPackage = JsonConvert.SerializeObject(player);
            
            await SendMessagesAsync(playerPackage);

        }
    }
}
