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
        [SerializeField] private Button _connectButton;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private UILogin uiLogin;
        private async void Connect()
        {
            string playerName = _inputField.text;
            await _gameClientScript.ConnectingPlayer(playerName); 
            UIManager.Instance.CloseInterface(true);
            uiLogin.Close();
        }

        private void Start()
        {
            _connectButton.onClick.AddListener(Connect);
        }
    }
}
