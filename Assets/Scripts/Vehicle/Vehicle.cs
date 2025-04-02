using UnityEngine;

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
}
