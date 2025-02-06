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
        [SerializeField] public Button helpButton;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] public UILogin uiLogin;
        [SerializeField] public TMP_Text errorText;
        public static UIConnectScript Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private async void Connect()
        {
            string playerName = _inputField.text;
            if (playerName.Length == 0)
            {
                ShowErrorText("Введи никнейм!");
                return;
            }

            HideErrorText();
            await _gameClientScript.ConnectingPlayer(playerName);
        }

        private void Start()
        {
            _connectButton.onClick.AddListener(Connect);
        }
        
        public void OpenHelpWindow()
        {
            helpButton.interactable = false;
            UIManager.Instance.OpenHelpWindow();
        }
        
        public void OnHelpWindowClose()
        {
            helpButton.interactable = true;
        }

        public void HideErrorText()
        {
            errorText.gameObject.SetActive(false);
        }

        public void ShowErrorText(string text)
        {
            errorText.text = text;
            errorText.gameObject.SetActive(true);
        }
    }
}
