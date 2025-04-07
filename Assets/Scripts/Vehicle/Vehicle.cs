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

    public float NormalizedLinearVelocity
    {
        get
        {
            if (Mathf.Approximately(0, LinearVelocity) == true) return 0;

            return Mathf.Clamp01(LinearVelocity / maxLinearVelocity);
        }
    }

    public Turret Turret;

    [SyncVar]
    private Vector3 netAimPoint;

    public Vector3 NetAimPoint
    {
        get => netAimPoint;

        set
        {
            netAimPoint = value;        // Client
            CmdSetNetAimPoint(value);   // Server
        }
    }

    [Command]
    private void CmdSetNetAimPoint(Vector3 point)
    {
        netAimPoint = point;
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
            SetLayerToAll("Default");
        else
            SetLayerToAll("Ignore Main Camera");
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
}
