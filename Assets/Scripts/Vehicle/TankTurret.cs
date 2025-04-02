using UnityEngine;

[RequireComponent (typeof(TrackTank))]
public class TankTurret : MonoBehaviour
{
    [SerializeField] private Transform aim;

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

    private TrackTank tank;
    private float currentMaskAngle;
    private Rigidbody tankRigidbody;

    private void Start()
    {
        tank = GetComponent<TrackTank>();
        tankRigidbody = GetComponent<Rigidbody>();

        maxTopAngle = -maxTopAngle;
    }

    private void Update()
    {
        // Temp
        if (Input.GetMouseButtonDown(0))
        {
            Fire();
        }

        ControlTurretAim();
    }

    public void Fire()
    {
        FireSFX();
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
        Vector3 lp = tower.InverseTransformPoint(aim.position);
        lp.y = 0;
        Vector3 lpg = tower.TransformPoint(lp);
        tower.rotation = Quaternion.RotateTowards(tower.rotation, Quaternion.LookRotation((lpg - tower.position).normalized, tower.up), horizontalRotationSpeed * Time.deltaTime);

        // Mask
        mask.localRotation = Quaternion.identity;

        lp = mask.InverseTransformPoint(aim.position);
        lp.x = 0;
        lpg = mask.TransformPoint(lp);

        float targetAngle = Vector3.SignedAngle((lpg - mask.position).normalized, mask.forward, mask.right);
        targetAngle = Mathf.Clamp(targetAngle, maxTopAngle, maxBottomAngle);

        currentMaskAngle = Mathf.MoveTowards(currentMaskAngle, targetAngle, Time.deltaTime * verticalRotationSpeed);
        mask.localRotation = Quaternion.Euler(currentMaskAngle, 0, 0);
    }
}
