using UnityEngine;
using Mirror;

public class Vehicle : Destructible
{
    [SerializeField] protected float maxLinearVelocity;

    [Header("Engine SFX")]
    [SerializeField] private AudioSource engineSound;
    [SerializeField] private float enginePitchModifier;

    [Header("Vehicle")]
    [SerializeField] protected Transform zoomOpticPosition;
    public Transform ZoomOpticPosition => zoomOpticPosition;

    public virtual float LinearVelocity => 0;

    protected float syncLinearVelocity;
    public float NormalizedLinearVelocity
    {
        get
        {
            if (Mathf.Approximately(0, syncLinearVelocity) == true) return 0;

            return Mathf.Clamp01(syncLinearVelocity / maxLinearVelocity);
        }
    }

    public Turret Turret;
    public int TeamID;
    public VehicleViewer Viewer;

    [SyncVar(hook = nameof(OnNetAimPointChanged))]
    private Vector3 netAimPoint;

    public Vector3 NetAimPoint
    {
        get => netAimPoint;
        set
        {
            if (isServer)
            {
                netAimPoint = value;
            }
            else if (isClient && isOwned)
            {
                CmdSetNetAimPoint(value);
            }
        }
    }

    [Command]
    public void CmdSetNetAimPoint(Vector3 point)
    {
        netAimPoint = point;
    }

    private void OnNetAimPointChanged(Vector3 oldValue, Vector3 newValue)
    {
        netAimPoint = newValue;
    }

    protected Vector3 targetInputControl;

    public void SetTargetControl(Vector3 control)
    {
        targetInputControl = control.normalized;
    }

    protected virtual void Update()
    {
        UpdateEngineSFX();
    }

    private void UpdateEngineSFX()
    {
        if (engineSound != null)
        {
            engineSound.pitch = 1.0f + NormalizedLinearVelocity * enginePitchModifier;
            engineSound.volume = 0.5f + NormalizedLinearVelocity;
        }
    }

    public void SetVisible(bool visible)
    {
        if (visible)
        {
            if (gameObject.layer == LayerMask.NameToLayer("Default")) return;
            SetLayerToAll("Default");
        }
        else
        {
            if (gameObject.layer == LayerMask.NameToLayer("Ignore Main Camera")) return;
            SetLayerToAll("Ignore Main Camera");
        }
    }

    private void SetLayerToAll(string layerName)
    {
        gameObject.layer = LayerMask.NameToLayer(layerName);

        foreach (Transform t in transform.GetComponentsInChildren<Transform>())
        {
            t.gameObject.layer = LayerMask.NameToLayer(layerName);
        }
    }

    public void Fire()
    {
        Turret.Fire();
    }

    [SyncVar(hook = "T")]
    public NetworkIdentity Owner;

    private void T(NetworkIdentity oldValue, NetworkIdentity newValue) { }
}
