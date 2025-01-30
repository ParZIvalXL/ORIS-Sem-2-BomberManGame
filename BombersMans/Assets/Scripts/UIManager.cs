using System;
using NetCode;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public bool IsChatOpen = false;
    [SerializeField] private UILogin loginWindow;
    [SerializeField] private UIChat chatWindow;
    private IInterface _currentInterface;
    public IInterface CurrentInterface => _currentInterface;
    public bool HasCurrentInterface => _currentInterface != null;

    public static UIManager Instance;
    public void OpenChatWindow()
    {
        InputManager.Instance.ResetPlayerMovement();
        OpenInterface(chatWindow);
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
        InputManager.Instance.ClientPlayerController.ControlLocked = false;
    }

    private void Awake()
    {
        Instance = this;
    }
}
