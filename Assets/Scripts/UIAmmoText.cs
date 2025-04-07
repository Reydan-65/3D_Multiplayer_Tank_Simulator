using TMPro;
using UnityEngine;

public class UIAmmoText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    private int ammoIndex;
    private Turret turret;

    private void Awake()
    {
        Transform parent = transform.parent;

        if (parent != null)
            ammoIndex = transform.GetSiblingIndex();
    }

    private void Start()
    {
        NetworkSessionManager.Events.PlayerVehicleSpawned += OnPlayerVehicleSpawned;
    }

    private void OnDestroy()
    {
        if (NetworkSessionManager.Instance != null)
            NetworkSessionManager.Events.PlayerVehicleSpawned -= OnPlayerVehicleSpawned;

        if (turret != null)
            turret.AmmoChanged -= OnAmmoChanged;
    }

    // Handle
    private void OnPlayerVehicleSpawned(Vehicle vehicle)
    {
        if (turret != null)
            turret.AmmoChanged -= OnAmmoChanged;

        if (vehicle.Turret == null)
        {
            text.text = "N/A";
            return;
        }

        turret = vehicle.Turret;

        turret.AmmoChanged += OnAmmoChanged;

        OnAmmoChanged(turret.AmmoCounts[ammoIndex]);
    }

    // Unity API
    private void OnAmmoChanged(int ammo)
    {
        if (turret == null || ammoIndex < 0 || ammoIndex >= turret.AmmoCounts.Length)
        {
            text.text = "ERR";
            text.color = Color.red;
            return;
        }

        text.text = turret.AmmoCounts[ammoIndex].ToString();
        text.color = turret.AmmoCounts[ammoIndex] == 0 ? Color.red : Color.yellow;

    }
}
