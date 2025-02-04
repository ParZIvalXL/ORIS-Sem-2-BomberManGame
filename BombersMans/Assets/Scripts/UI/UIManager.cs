using System;
using NetCode;
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
    public IInterface CurrentInterface => _currentInterface;
    public bool HasCurrentInterface => _currentInterface != null;

    public static UIManager Instance;
    public void OpenChatWindow()
    {
        InputManager.Instance.ResetPlayerMovement();
        OpenInterface(chatWindow);
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
        if (HasCurrentInterface)
        {
            if(_currentInterface is UILogin) 
                return;
            
            CloseInterface();
        }
        _currentInterface = newInterface;
        newInterface.Open();
        InputManager.Instance.ClientPlayerController.ControlLocked = true;
    }

    public void CloseInterface(bool force = false)
    {
        if(_currentInterface is UILogin && !force) 
            return;
        
        _currentInterface.Close();
        _currentInterface = null;
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
    }
}
