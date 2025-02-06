using NetCode;
using UnityEngine;

public class UIHelp : MonoBehaviour, IInterface
{
    public void Close()
    {
        gameObject.SetActive(false);
        Cursor.visible = false;
    }

    public void Open()
    {
        gameObject.SetActive(true);
        Cursor.visible = true;
    }
}
