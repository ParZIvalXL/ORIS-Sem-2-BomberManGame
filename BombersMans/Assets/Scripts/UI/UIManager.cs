using System;
using NetCode;
using UI;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public bool IsChatOpen = false;
    [SerializeField] private UILogin loginWindow;
    [SerializeField] private UIChat chatWindow;
    public GameOverScreen gameOverScreen;
    public UIGameOver gameEndScreen;
    private IInterface _currentInterface;
    [SerializeField] private GameObject blur;
    public LoadingUI loadingUI;
    public UIHelp help;
    public IInterface CurrentInterface => _currentInterface;
    [SerializeField] private GameUIScript gameUIScript;
    public bool HasCurrentInterface => _currentInterface != null;

    public static UIManager Instance;
    public void OpenChatWindow()
    {
        InputManager.Instance.ResetPlayerMovement();
        OpenInterface(chatWindow);
    }

    public void UpdateHealthBar()
    {
        gameUIScript.SetHealth(PlayerController.Instance.health);
    }

    public void Blur()
    {
        blur.SetActive(true);
    }

    public void UnBlur()
    {
        blur.SetActive(false);
    }
    public void OpenInterface(IInterface newInterface)
    {
        Debug.Log("OpenInterface " + newInterface);
        if (newInterface is UILogin)
        {
            gameUIScript.gameObject.SetActive(false);
        }
        
        if (HasCurrentInterface && newInterface is not UIHelp)
        {
            if(_currentInterface is UILogin) 
                return;
            
            CloseInterface();
        }
        _currentInterface = newInterface;
        newInterface.Open();
        InputManager.Instance.ClientPlayerController.ControlLocked = true;
    }

    public void OpenHelpWindow()
    {
        OpenInterface(help);
    }
    
    public void CloseHelpWindow()
    {
        if(_currentInterface is UIHelp)
            CloseInterface();
        else
            help.Close();
        UIConnectScript.Instance.OnHelpWindowClose();
    }
    
    public void CloseInterface(bool force = false)
    {
        if(_currentInterface is UILogin && !force) 
            return;
        else
            gameUIScript.gameObject.SetActive(false);
        
        _currentInterface.Close();
        _currentInterface = null;
        if (loginWindow.isActiveAndEnabled)
        {
            OpenInterface(loginWindow);
            return;
        }
        UnBlur();
        InputManager.Instance.ClientPlayerController.ControlLocked = false;
    }
    
    public void ShowGameOver(string message, PlayerGameEndReason reason)
    {
        gameOverScreen.Show(message, reason == PlayerGameEndReason.Winner);
        gameEndScreen.Open();
    }

    private void Awake()
    {
        Instance = this;
        if(loadingUI.animator is null || LoadingUI.Instance is null) loadingUI.SetUp();
    }
}
