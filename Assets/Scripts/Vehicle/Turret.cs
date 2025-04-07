using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class Turret : NetworkBehaviour
{
    [SerializeField] protected Transform launchPoint;
    public Transform LaunchPoint => launchPoint;

    [SerializeField] private float fireRate;
    [SerializeField] protected Projectile[] projectilePrefabs;
    public Projectile[] ProjectilePrefabs => projectilePrefabs;

    [Header("Shot Spread")]
    [SerializeField] protected float maxSpreadAngle = 1.0f;

    private float fireTimer;
    public float FireTimer => fireTimer;
    public float FireRate => fireRate;
    public float FireTimerNormolize => fireTimer / fireRate;

    [SyncVar(hook = nameof(OnProjectileTypeChanged))]
    protected int currentProjectileIndex = 0;

    [SyncVar(hook = nameof(OnAmmoCountsChanged))]
    [SerializeField] private int[] ammoCounts;
    public int[] AmmoCounts => ammoCounts;

    private void OnProjectileTypeChanged(int oldIndex, int newIndex)
    {
        currentProjectileIndex = newIndex;
        ProjectileTypeChanged?.Invoke(newIndex);
    }
    private void OnAmmoCountsChanged(int[] oldCounts, int[] newCounts)
    {
        ammoCounts = newCounts;
        AmmoChanged?.Invoke(ammoCounts[currentProjectileIndex]);
    }

    public UnityAction<int> AmmoChanged;
    public UnityAction<int> ProjectileTypeChanged;

    private void Awake()
    {
        fireTimer = fireRate;
    }

    // Change ammo count
    [Server]
    public void SvAddAmmo(int[] counts)
    {
        for (int i = 0; i < ammoCounts.Length; i++)
        {
            ammoCounts[i] += counts[i];
        }
    }

    [Server]
    protected virtual bool SvDrawAmmo(int[] counts)
    {
        if (ammoCounts[currentProjectileIndex] >= counts[currentProjectileIndex])
        {
            ammoCounts[currentProjectileIndex] -= counts[currentProjectileIndex];
            SvSyncAmmo();
            return true;
        }

        return false;
    }

    [Server]
    private void SvSyncAmmo()
    {
        RpcSyncAmmo(ammoCounts);
    }

    [ClientRpc]
    private void RpcSyncAmmo(int[] newAmmo)
    {
        ammoCounts = newAmmo;
        AmmoChanged?.Invoke(ammoCounts[currentProjectileIndex]);
    }

    public void ChangeProjectileType(int index)
    {
        if (!isOwned) return;

        if (isClient)
            CmdChangeProjectileType(index);
    }

    [Command]
    private void CmdChangeProjectileType(int newIndex)
    {
        if (newIndex >= 0 && newIndex < projectilePrefabs.Length)
        {
            currentProjectileIndex = newIndex;

            ProjectileTypeChanged?.Invoke(newIndex);
        }
    }

    [ClientRpc]
    private void RpcChangeProjectileType(int newIndex)
    {
        if (newIndex >= 0 && newIndex < projectilePrefabs.Length)
        {
            currentProjectileIndex = newIndex;
            RpcProjectileTypeChanged(newIndex);
        }
    }

    [ClientRpc]
    private void RpcProjectileTypeChanged(int newIndex)
    {
        currentProjectileIndex = newIndex;
        ProjectileTypeChanged?.Invoke(newIndex);
    }

    // Fire
    protected virtual void OnFire() { }

    public void Fire()
    {
        if (!isOwned) return;

        if (isClient)
            CmdFire();
    }

    [Command]
    private void CmdFire()
    {
        if (fireTimer > 0) return;

        int[] ammoToDraw = new int[ammoCounts.Length];
        ammoToDraw[currentProjectileIndex] = 1;

        if (SvDrawAmmo(ammoToDraw) == false) return;

        OnFire();

        fireTimer = fireRate;

        RpcFire();
    }

    [ClientRpc]
    private void RpcFire()
    {
        if (isServer) return;

        fireTimer = fireRate;

        OnFire();
    }

    protected virtual void Update()
    {
        if (fireTimer > 0)
            fireTimer -= Time.deltaTime;
    }
}
