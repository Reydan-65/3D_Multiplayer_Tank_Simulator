using UnityEngine;
using UnityEngine.UI;

public class UIVehicleMark : MonoBehaviour
{
    [SerializeField] private Image image;

    [SerializeField] private Color[] teamColors;

    public void SetColor(int colorIndex)
    {
        image.color = teamColors[colorIndex];
    }
}
