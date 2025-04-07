using UnityEngine;
using Mirror;
using UnityEngine.Events;
using System;

public class Destructible : NetworkBehaviour
{
    public UnityAction<int> HitPointChange;
    public event UnityAction OnDeath;
    [SerializeField] private UnityEvent Destroyed;

    [SerializeField] private int m_MaxHitPoint;
    [SerializeField] private GameObject m_DestroySFX;

    public int MaxHitPoint => m_MaxHitPoint;
    public int HitPoint => currentHitPoint;
    private int currentHitPoint;

    [SyncVar(hook = nameof(ChangeHitPoint))]
    private int syncCurrentHitPoint;

    public override void OnStartServer()
    {
        base.OnStartServer();

        syncCurrentHitPoint = m_MaxHitPoint;
        currentHitPoint = m_MaxHitPoint;
    }

    [Server]
    public void SvApplyDamage(int damage)
    {
        syncCurrentHitPoint -= damage;

        if (syncCurrentHitPoint <= 0)
        {
            syncCurrentHitPoint = 0;
            RpcDeath();

            if (m_DestroySFX != null)
            {
                GameObject sfx = Instantiate(m_DestroySFX, transform.position, Quaternion.identity);
                NetworkServer.Spawn(sfx);
            }

            RpcDestroy();
        }
    }

    [ClientRpc]
    private void RpcDestroy()
    {
        OnDestructibleDestroy();
    }

    protected virtual void OnDestructibleDestroy()
    {
        Destroyed?.Invoke();
    }

    [ClientRpc]
    private void RpcDeath()
    {
        OnDeath?.Invoke();

        if (isOwned && isLocalPlayer && Owner != null)
        {
            Player player = Owner.GetComponent<Player>();
            player?.HandleVehicleDeath();
        }
    }

    private void ChangeHitPoint(int oldValue, int newValue)
    {
        currentHitPoint = newValue;
        HitPointChange?.Invoke(newValue);
    }

    [SyncVar(hook = "T")]
    public NetworkIdentity Owner;

    private void T(NetworkIdentity oldValue, NetworkIdentity newValue) { }
}
