using UnityEngine;

namespace NetCode
{
    public class UIGameOver : MonoBehaviour, IInterface
    {
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
}