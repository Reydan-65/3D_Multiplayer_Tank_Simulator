using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class Turret : NetworkBehaviour
{
    public event UnityAction<int> UpdateSelectedAmmunition;
    public event UnityAction Shot;

    [SerializeField] protected Transform launchPoint;
    public Transform LaunchPoint => launchPoint;

    [SerializeField] private float fireRate;
    [SerializeField] protected Ammunition[] ammunition;
    public Ammunition[] Ammunition => ammunition;

    [Header("Shot Spread")]
    [SerializeField] protected float maxSpreadAngle = 1.0f;

    [SyncVar]
    private float fireTimer;
    public float FireTimer => fireTimer;
    public float FireRate => fireRate;
    public float FireTimerNormolize => fireTimer / fireRate;
    public ProjectileProperties SelectedProjectileProperties => ammunition[syncSelectedAmmunitionIndex].ProjectileProperties;
    public int SelectedAmmunitionIndex => syncSelectedAmmunitionIndex;

    [SyncVar]
    private int syncSelectedAmmunitionIndex = 0;


    private void Awake()
    {
        fireTimer = fireRate;
    }

    public void SetSelectedProperties(int index)
    {
        if (isOwned == false) return;

        if (index < 0 || index >= ammunition.Length) return;

        syncSelectedAmmunitionIndex = index;

        if (isClient)
            CmdReloadAmmunition();

        UpdateSelectedAmmunition?.Invoke(index);
    }

    [Command]
    private void CmdReloadAmmunition()
    {
        fireTimer = fireRate;
    }

    // Fire
    protected virtual void OnFire() { }

    public void Fire()
    {
        if (!isOwned) return;

        if (isClient)
            CmdFire();
    }

    [Server]
    public void SvFire()
    {
        if (fireTimer > 0) return;

        if (ammunition[syncSelectedAmmunitionIndex].SvDrawAmmo(1) == false) return;

        OnFire();

        fireTimer = fireRate;

        RpcFire();
        Shot?.Invoke();
    }

    [Command]
    private void CmdFire()
    {
        SvFire();
    }

    [ClientRpc]
    private void RpcFire()
    {
        if (isServer) return;

        fireTimer = fireRate;

        OnFire();
        Shot?.Invoke();
    }

    protected virtual void LateUpdate()
    {
        if (fireTimer > 0)
            fireTimer -= Time.deltaTime;
    }
}
