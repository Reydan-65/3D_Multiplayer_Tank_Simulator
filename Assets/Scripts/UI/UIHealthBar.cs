using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI text;

    private Destructible destructible;

    private void Start()
    {
        if (NetworkSessionManager.Instance != null)
            NetworkSessionManager.Events.PlayerVehicleSpawned += OnPlayerVehicleSpawned;

    }

    private void OnDestroy()
    {
        if (NetworkSessionManager.Instance != null)
            NetworkSessionManager.Events.PlayerVehicleSpawned -= OnPlayerVehicleSpawned;

        if (destructible != null)
            destructible.HitPointChanged -= OnHitPointChange;
    }

    private void OnPlayerVehicleSpawned(Vehicle vehicle)
    {
        destructible = vehicle;

        if (destructible == null) return;

        slider.value = destructible.MaxHitPoint;
        fillImage.color = Color.green;

        destructible.HitPointChanged += OnHitPointChange;

        text.text = destructible.MaxHitPoint.ToString() + " / " + destructible.MaxHitPoint.ToString();
    }

    private void OnHitPointChange(int hitPoint)
    {
        text.text = hitPoint.ToString() + " / " + destructible.MaxHitPoint.ToString();

        if (destructible.MaxHitPoint > 0)
            slider.value = (float)hitPoint / destructible.MaxHitPoint;
        else
            slider.value = 0;

        if (hitPoint <= 0.2f * destructible.MaxHitPoint)
            fillImage.color = Color.red;
        else if (hitPoint > 0.2f)
            fillImage.color = new Color(1f, 0.6f, 0f);
        else if (hitPoint > 0.75f)
            fillImage.color = Color.green;
    }
}
