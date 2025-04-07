using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICannonAim : MonoBehaviour
{
    [SerializeField] private Image aim;
    [SerializeField] private Image reloadSlider;
    [SerializeField] private TextMeshProUGUI reloadTimer;

    private Vector3 aimPosition;

    private void Update()
    {
        if (Player.Local == null) return;
        if (Player.Local.ActiveVehicle == null) return;

        Vehicle v = Player.Local.ActiveVehicle;

        reloadSlider.fillAmount = v.Turret.FireTimerNormolize;

        if (v.Turret.FireTimer <= 0.1f)
        {
            reloadTimer.color = Color.green;
            reloadTimer.text = "Ready";
        }
        else
        {
            reloadTimer.color = Color.red;
            reloadTimer.text = (Mathf.Round(v.Turret.FireTimer * 10f) / 10f).ToString("F1");
        }

        aimPosition = VehicleInputController.TraceAimPointWithoutPlayerVehicle(v.Turret.LaunchPoint.position, v.Turret.LaunchPoint.forward);

        Vector3 result = Camera.main.WorldToScreenPoint(aimPosition);

        if (result.z > 0)
        {
            result.z = 0;
            aim.transform.position = result;
        }
    }
}
