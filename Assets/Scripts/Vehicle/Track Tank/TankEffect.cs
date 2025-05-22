using UnityEngine;

public class TankEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] exhaust;
    [SerializeField] private ParticleSystem[] exhaustAtMovementStart;
    [SerializeField] private Vector2 minMaxExhaustEmission;

    private TrackVehicle tank;
    private bool isStoped;

    private void Start()
    {
        tank = GetComponent<TrackVehicle>();
    }

    private void Update()
    {
        float exhaustEmission = Mathf.Lerp(minMaxExhaustEmission.x, minMaxExhaustEmission.y, tank.NormalizedLinearVelocity);

        for (int i = 0; i < exhaust.Length; i++)
        {
            ParticleSystem.EmissionModule emission = exhaust[i].emission;
            emission.rateOverTime = exhaustEmission;
        }

        if (tank.LinearVelocity < 0.1f)
        {
            isStoped = true;
        }

        if (tank.LinearVelocity > 1f)
        {
            if (isStoped)
            {
                for (int i = 0; i < exhaust.Length; i++)
                    exhaustAtMovementStart[i].Play();
            }

            isStoped = false;
        }
    }
}
