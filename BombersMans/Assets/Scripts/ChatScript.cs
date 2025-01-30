using System;
using System.Net.Sockets;
using NetCode;
using Newtonsoft.Json;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ChatScript : MonoBehaviour
{
        [SerializeField] private GameClientScript _gameClientScript;
        [SerializeField] private Button _connectButton;
        [SerializeField] private TMP_InputField _inputField;
        public TMP_InputField Input => _inputField;
        [SerializeField] private UIChat uiChat;
        [SerializeField] private GameObject chatPanel;
        [SerializeField] private TMP_Text textMessage;
        public static ChatScript Instance;
        
        public bool IsChatOpen;
        
        public async void SendChatMessage()
        {
            if (_inputField.text.Length == 0) return;
            
            var messageData = new MessagePackage
            {
                Sender = _gameClientScript.playerName,
                Content = _inputField.text,
                Type = nameof(MessagePackage)
            };
            var serializedMessage = JsonConvert.SerializeObject(messageData);
            
            await _gameClientScript.SendMessagesAsync(serializedMessage);
            
            _inputField.text = "";
        }

        private void Start()
        {
            _connectButton.onClick.AddListener(SendChatMessage);
        }

        public void CreateNewMessage(string message)
        {
            if(message.Length == 0) return;
            UpdateChat();
        }

        public void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            UpdateChat();
        }

        public void UpdateChat()
        {
            textMessage.text = ChatHolder.GetAllMessages();
        }
}