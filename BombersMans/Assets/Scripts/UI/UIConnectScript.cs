using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace NetCode
{
    public class UIConnectScript : MonoBehaviour
    {
        [SerializeField] private GameClientScript _gameClientScript;
        [SerializeField] public Button _connectButton;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] public UILogin uiLogin;
        public static UIConnectScript Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private async void Connect()
        {
            string playerName = _inputField.text;
            await _gameClientScript.ConnectingPlayer(playerName);
        }

        private void Start()
        {
            _connectButton.onClick.AddListener(Connect);
        }
    }
}
