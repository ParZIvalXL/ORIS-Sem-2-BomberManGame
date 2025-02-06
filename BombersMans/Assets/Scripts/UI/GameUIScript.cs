using UnityEngine;
using UnityEngine.UI;

public class GameUIScript : MonoBehaviour
{
    public Slider healthSlider;
    
    public void SetHealth(float health)
    {
        healthSlider.value = health;
    }
}
