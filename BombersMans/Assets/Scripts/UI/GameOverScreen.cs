using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class GameOverScreen : MonoBehaviour
{
    public TMP_Text Title; 
    public TMP_Text Description;
    public Button SpectatorButton;

    public void Show(string description, bool winner)
    {
        if (winner)
        {
            Title.text = "Victory!";
            Title.color = new Color(198, 255, 219);
        }

        Description.text = description;
    }
}
