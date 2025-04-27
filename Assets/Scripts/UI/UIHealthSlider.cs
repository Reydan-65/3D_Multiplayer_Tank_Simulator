using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image sliderImage;
    [SerializeField] private Color[] teamColors;

    private Destructible destructible;

    public void Init(Destructible destructible, int[] teamIds)
    {
        this.destructible = destructible;

        destructible.HitPointChanged += OnHitPointChange;
        slider.maxValue = destructible.MaxHitPoint;
        slider.value = slider.maxValue;
        text.text = destructible.MaxHitPoint.ToString() + " / " + destructible.MaxHitPoint.ToString();

        if (teamIds[0] == teamIds[1])
            SetColor(0);
        else
            SetColor(1);
    }

    private void OnDestroy()
    {
        if (destructible == null) return;

        destructible.HitPointChanged -= OnHitPointChange;
    }

    private void OnHitPointChange(int hitPoint)
    {
        slider.value = (float)hitPoint;
        text.text = hitPoint.ToString() + " / " + destructible.MaxHitPoint.ToString();
    }

    private void SetColor(int colorIndex)
    {
        sliderImage.color = teamColors[colorIndex];
    }
}
