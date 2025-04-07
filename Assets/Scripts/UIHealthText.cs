using TMPro;
using UnityEngine;

public class UIHealthText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private Destructible destructible;

    private void Start()
    {
        NetworkSessionManager.Events.PlayerVehicleSpawned += OnPlayerVehicleSpawned;
    }

    private void OnDestroy()
    {
        if (NetworkSessionManager.Instance != null)
            NetworkSessionManager.Events.PlayerVehicleSpawned -= OnPlayerVehicleSpawned;

        if (destructible != null)
            destructible.HitPointChange -= OnHitPointChange;
    }

    private void OnPlayerVehicleSpawned(Vehicle vehicle)
    {
        destructible = vehicle;

        destructible.HitPointChange += OnHitPointChange;

        text.text = destructible.HitPoint.ToString();
        text.color = Color.green;
    }

    private void OnHitPointChange(int hitPoint)
    {
        text.text = hitPoint.ToString();

        if (hitPoint > destructible.MaxHitPoint * 0.5f)
            text.color = Color.yellow;
        else if (hitPoint > 0)
            text.color = new Color(1f, 0.6f, 0f);
        else
            text.color = Color.red;
    }
}
