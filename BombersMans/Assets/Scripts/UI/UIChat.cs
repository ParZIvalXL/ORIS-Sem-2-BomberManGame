using NetCode;
using UnityEngine;

public class UIChat : MonoBehaviour, IInterface
{
    public void Open()
    {
        gameObject.SetActive(true);
        Cursor.visible = true;
        //UIManager.Instance.IsChatOpen = gameObject.gameObject.activeSelf;
    }
    
    public void Close()
    {
        gameObject.SetActive(false);
        Cursor.visible = false;
        //UIManager.Instance.IsChatOpen = gameObject.gameObject.activeSelf;
    }
}
