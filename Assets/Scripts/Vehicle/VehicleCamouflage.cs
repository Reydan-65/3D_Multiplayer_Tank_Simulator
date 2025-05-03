using UnityEngine;

[RequireComponent(typeof(Vehicle))]
public class VehicleCamouflage : MonoBehaviour
{
    private const float CAMOUFLAGE_AFTER_SHOT = 0.07f;
    private const float CAMOUFLAGE_IN_MOVING = 0.75f;
    private const float CAMOUFLAGE_IN_IDLE = 1f;

    [SerializeField] private float baseDistance;
    [Range(0f, 1f)]
    [SerializeField] private float percent;
    [SerializeField] private float percentLerpRate;

    private float targetPercent;
    private float ñurrentDistance;
    private Vehicle vehicle;

    public float CurrentDistance => ñurrentDistance;

    // Unity API
    private void Start()
    {
        if (NetworkSessionManager.Instance == null) return;
        if (NetworkSessionManager.Instance.IsServer == false) return;

        vehicle = GetComponent<Vehicle>();
        vehicle.Turret.Shot += OnShot;
    }

    private void OnDestroy()
    {
        if (NetworkSessionManager.Instance == null) return;
        if (NetworkSessionManager.Instance.IsServer == false) return;

        vehicle.Turret.Shot -= OnShot;
    }

    private void Update()
    {
        if (NetworkSessionManager.Instance == null) return;
        if (NetworkSessionManager.Instance.IsServer == false) return;
        if (vehicle == null) return;


        targetPercent = vehicle.NormalizedLinearVelocity > 0.01f
            ? CAMOUFLAGE_IN_MOVING
            : CAMOUFLAGE_IN_IDLE;

        percent = Mathf.MoveTowards(percent, targetPercent, Time.deltaTime * percentLerpRate);
        percent = Mathf.Clamp01(percent);

        ñurrentDistance = baseDistance * percent;
    }

    // Handlers
    private void OnShot()
    {
        percent = CAMOUFLAGE_AFTER_SHOT;
    }
}
