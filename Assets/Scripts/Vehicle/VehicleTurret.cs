using UnityEngine;

[RequireComponent (typeof(TrackVehicle))]
public class VehicleTurret : Turret
{
    [SerializeField] private Transform tower;
    [SerializeField] private Transform mask;

    [SerializeField] private float horizontalRotationSpeed;
    [SerializeField] private float verticalRotationSpeed;

    [SerializeField] private float maxTopAngle;
    [SerializeField] private float maxBottomAngle;

    [Header("SFX")]
    [SerializeField] private AudioSource fireSound;
    [SerializeField] private ParticleSystem mazzel;
    [SerializeField] private float forceRecoil;

    private TrackVehicle tank;
    private float currentMaskAngle;
    private Rigidbody tankRigidbody;

    private void Start()
    {
        tank = GetComponent<TrackVehicle>();
        tankRigidbody = GetComponent<Rigidbody>();

        maxTopAngle = -maxTopAngle;
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        ControlTurretAim();
    }

    protected override void OnFire()
    {
        base.OnFire();

        Projectile proj = Instantiate(SelectedProjectileProperties.ProjectilePrefab);

        proj.Owner = tank.Owner;
        proj.OwnerVehicle = tank;
        proj.SetProperties(SelectedProjectileProperties);

        proj.transform.position = launchPoint.position;

        Vector3 fireDirection = launchPoint.forward;

        if (maxSpreadAngle > 0)
            fireDirection = ApplyRandomSpread(fireDirection, maxSpreadAngle);

        proj.transform.forward = fireDirection;
        FireSFX();
    }

    private Vector3 ApplyRandomSpread(Vector3 direction, float maxAngle)
    {
        float spreadX = Random.Range(-maxAngle, maxAngle);
        float spreadY = Random.Range(-maxAngle, maxAngle);

        Quaternion spreadRotation = Quaternion.Euler(spreadX, spreadY, 0);

        return spreadRotation * direction;
    }

    private void FireSFX()
    {
        fireSound.Play();
        mazzel.Play();

        tankRigidbody.AddForceAtPosition(-mask.forward * forceRecoil, mask.position, ForceMode.Impulse);
    }

    private void ControlTurretAim()
    {
        // Tower
        Vector3 lp = tower.InverseTransformPoint(tank.NetAimPoint);
        lp.y = 0;
        Vector3 lpg = tower.TransformPoint(lp);
        tower.rotation = Quaternion.RotateTowards(tower.rotation, Quaternion.LookRotation((lpg - tower.position).normalized, tower.up), horizontalRotationSpeed * Time.deltaTime);

        // Mask
        mask.localRotation = Quaternion.identity;

        lp = mask.InverseTransformPoint(tank.NetAimPoint);
        lp.x = 0;
        lpg = mask.TransformPoint(lp);

        float targetAngle = Vector3.SignedAngle((lpg - mask.position).normalized, mask.forward, mask.right);
        targetAngle = Mathf.Clamp(targetAngle, maxTopAngle, maxBottomAngle);

        currentMaskAngle = Mathf.MoveTowards(currentMaskAngle, -targetAngle, Time.deltaTime * verticalRotationSpeed);
        mask.localRotation = Quaternion.Euler(currentMaskAngle, 0, 0);
    }
}
