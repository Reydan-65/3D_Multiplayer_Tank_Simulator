using UnityEngine;
using UnityEngine.UI;

public class UIVehicleMark : MonoBehaviour
{
    [SerializeField] private Image image;

    [SerializeField] private Color[] teamColors;

    [SerializeField] private Color bgDestroyedColors;
    [SerializeField] private Color[] teamDestroyedColors;

    private Image bgImage;

    private void Start()
    {
        bgImage = GetComponent<Image>();
    }

    public void SetColor(int colorIndex)
    {
        image.color = teamColors[colorIndex];
    }

    public void SetDestroyedColors(int colorIndex)
    {
        bgImage.color = bgDestroyedColors;
        image.color = teamDestroyedColors[colorIndex];
    }
}
