using NetCode;
using UnityEngine;
using UnityEngine.Serialization;

public class InputManager : MonoBehaviour
{
    
    [FormerlySerializedAs("_uiManager")] [SerializeField] private UIManager uiManager;
    [FormerlySerializedAs("_chatScript")] [SerializeField] private ChatScript chatScript;
    [SerializeField] private PlayerController _clientPlayerController;
    public static InputManager Instance;
    [SerializeField] public PlayerController ClientPlayerController => _clientPlayerController;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;
        if (!_clientPlayerController)
            _clientPlayerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!uiManager) return;
            if (uiManager.IsChatOpen)
            {
                if (chatScript.Input.text != "")
                {
                    chatScript.SendChatMessage();
                }
                else
                {
                    uiManager.OpenChatWindow();
                }
            }
            else
            {
                uiManager.OpenChatWindow();
                chatScript.Input.ActivateInputField();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (uiManager.HasCurrentInterface)
            {
                uiManager.CloseInterface();
            }
        }

        var horInp = Input.GetAxis("Horizontal");
        var verInp = Input.GetAxis("Vertical");
        if ((horInp != 0 || verInp != 0) && !_clientPlayerController.ControlLocked) 
            _clientPlayerController.SetDirection(Vector2.up * verInp + Vector2.right * horInp);
        else
            _clientPlayerController.SetDirection(Vector2.zero);

        if (Input.GetKeyDown(KeyCode.Space)) _clientPlayerController.SpawnBomb(0);
    }
}
