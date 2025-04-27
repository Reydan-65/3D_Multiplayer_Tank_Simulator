using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICannonAim : MonoBehaviour
{
    [SerializeField] private Image aim;
    [SerializeField] private Image reloadSlider;
    [SerializeField] private TextMeshProUGUI reloadTimer;

    [Header("Sensitivity Thresholds")]
    [SerializeField] private float positionThreshold = 0.01f;
    [SerializeField] private float rotationThreshold = 0.01f;

    private Vector3 _lastLaunchPosition;
    private Quaternion _lastLaunchRotation;
    private Vector3 _cachedAimPosition;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

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

        // Проверяем, изменились ли позиция или поворот за пределами порога
        bool positionChanged = Vector3.Distance(_lastLaunchPosition, launchPoint.position) > positionThreshold;
        bool rotationChanged = Quaternion.Angle(_lastLaunchRotation, launchPoint.rotation) > rotationThreshold;

        if (positionChanged || rotationChanged)
        {
            _lastLaunchPosition = launchPoint.position;
            _lastLaunchRotation = launchPoint.rotation;
            _cachedAimPosition = VehicleInputController.TraceAimPointWithoutPlayerVehicle(
                launchPoint.position,
                launchPoint.forward
            );
        }

        if (mainCamera == null) return;

        // Обновляем позицию прицела на экране
        Vector3 screenPos = mainCamera.WorldToScreenPoint(_cachedAimPosition);
        if (screenPos.z > 0)
        {
            screenPos.z = 0;
            aim.transform.position = screenPos;
        }
    }
}