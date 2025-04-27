using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class Ammunition : NetworkBehaviour
{
    public event UnityAction<int> AmmoCountChanged;

    [SerializeField] private ProjectileProperties projectileProperties;

    [SyncVar(hook = nameof(SyncAmmoCount))]
    [SerializeField] protected int syncAmmoCount;

    public ProjectileProperties ProjectileProperties => projectileProperties;
    public int AmmoCount => syncAmmoCount;

    #region Server

    [Server]
    public void SvAddAmmo(int count)
    {
        syncAmmoCount += count;
    }

    [Server]
    public bool SvDrawAmmo(int count)
    {
        if (syncAmmoCount == 0) return false;

        if (syncAmmoCount >= count)
        {
            syncAmmoCount -= count;

            return true;
        }

        return false;
    }

    #endregion

    #region Client

    private void SyncAmmoCount(int oldCount, int newCount)
    {
        AmmoCountChanged?.Invoke(newCount);
    }

    #endregion
}
