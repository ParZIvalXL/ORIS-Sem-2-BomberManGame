using System;
using NetCode;
using UnityEngine;

public class UILogin : MonoBehaviour, IInterface
{
    private void Start()
    {
        UIManager.Instance.OpenInterface(this);
    }

    public void Open()
    {
        gameObject.SetActive(true);
        Cursor.visible = true;
        UIManager.Instance.Blur();
    }
    
    public void Close()
    {
        gameObject.SetActive(false);
        Cursor.visible = false;
        UIManager.Instance.UnBlur();
    }
}
