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
        if (Player.Local == null || Player.Local.ActiveVehicle == null)
            return;

        Vehicle v = Player.Local.ActiveVehicle;
        Transform launchPoint = v.Turret.LaunchPoint;

        // Обновляем UI перезарядки (это можно оставить без изменений)
        reloadSlider.fillAmount = v.Turret.FireTimerNormolize;

        if (v.Turret.FireTimer <= 0.1f)
        {
            reloadTimer.color = Color.green;
            reloadTimer.text = "Готово!";
        }
        else
        {
            reloadTimer.color = Color.red;
            reloadTimer.text = v.Turret.FireTimer.ToString("F1");
        }

        aimPosition = VehicleInputController.TraceAimPointWithoutPlayerVehicle(launchPoint.position, launchPoint.forward);

        Vector3 screenPos = Camera.main.WorldToScreenPoint(aimPosition);

        if (screenPos.z > 0)
        {
            screenPos.z = 0;
            aim.transform.position = screenPos;
        }

    }
}
