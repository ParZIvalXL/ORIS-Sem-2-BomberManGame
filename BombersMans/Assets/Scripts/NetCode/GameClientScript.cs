using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
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
                byte[] buffer = new byte[1024];
                
                while (true)
                {
                    int receivedBytes = clientSocket.Receive(buffer);
                    if (receivedBytes > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
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
    }
}
