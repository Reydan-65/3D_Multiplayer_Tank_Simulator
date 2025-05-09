using UnityEngine;

public class AIRaySensor : MonoBehaviour
{
    [SerializeField] private Transform[] rays;
    [SerializeField] private float raycastDistance;
    public float RaycastDistance => raycastDistance;

    public (bool, float) Raycast()
    {
        float dist = -1;

        foreach (var v in rays)
        {
            RaycastHit hit;

            if (Physics.Raycast(v.position, v.forward, out hit, raycastDistance))
            {
                if (hit.collider.isTrigger) continue;

                if (dist < 0 || hit.distance < dist)
                {
                    dist = hit.distance;
                }
            }
        }

        return (dist > 0, dist);
    }

    private void OnDrawGizmos()
    {
        foreach (var v in rays)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(v.position, v.position + v.forward * raycastDistance);
        }
    }
}
