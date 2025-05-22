using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class Destructible : NetworkBehaviour
{
    public event UnityAction<int> HitPointChanged;
    public event UnityAction<Destructible> Destroyed;
    public event UnityAction<Destructible> Recovered;

    [SerializeField] private int maxHitPoint;
    [SerializeField] private UnityEvent EventDestroyed;
    [SerializeField] private UnityEvent EventRecovered;

    [SerializeField] private GameObject destroySFX;

    [SerializeField] private int currentHitPoint; // debug

    public int MaxHitPoint => maxHitPoint;
    public int HitPoint => currentHitPoint;

    [SyncVar(hook = nameof(SyncHitPoint))]
    private int syncCurrentHitPoint;

    #region Server

    public override void OnStartServer()
    {
        base.OnStartServer();

        syncCurrentHitPoint = maxHitPoint;
        currentHitPoint = maxHitPoint;
    }

    [Server]
    protected void SvRecovery()
    {
        syncCurrentHitPoint = maxHitPoint;
        currentHitPoint = maxHitPoint;

        RpcRecovery();
    }

    [Server]
    public void SvApplyDamage(int damage)
    {
        if (!NetworkServer.active)
        {
            Debug.LogWarning("SvApplyDamage called when server was not active");
            return;
        }
       
        syncCurrentHitPoint -= damage;

        if (syncCurrentHitPoint <= 0)
        {
            syncCurrentHitPoint = 0;
            RpcDestroy();
        }
    }

    #endregion

    #region Client
    private void SyncHitPoint(int oldValue, int newValue)
    {
        currentHitPoint = newValue;
        HitPointChanged?.Invoke(newValue);
    }

    [ClientRpc]
    private void RpcDestroy()
    {
        if (destroySFX != null)
        {
            GameObject sfx = Instantiate(destroySFX);

            sfx.transform.position = transform.position;
            sfx.transform.rotation = transform.rotation;
        }

        Destroyed?.Invoke(this);
        EventDestroyed?.Invoke();
    }

    [ClientRpc]
    private void RpcRecovery()
    {
        Recovered?.Invoke(this);
        EventRecovered?.Invoke();
    }

    #endregion
}
