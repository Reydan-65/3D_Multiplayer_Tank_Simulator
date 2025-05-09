using UnityEngine;

[RequireComponent(typeof(Vehicle))]
public class VehicleDimensions : MonoBehaviour
{
    [SerializeField] private Transform[] points;
    [SerializeField] private Transform[] priorityFirePoints;

    private Vehicle vehicle;
    public Vehicle Vehicle => vehicle;

    //RaycastHit[] hits = new RaycastHit[10];

    private void Awake()
    {
        vehicle = GetComponent<Vehicle>();
    }

    public bool IsVisibleFromPoint(Transform source, Vector3 point, Color debugColor,
                 float baseViewDistance, float xrayDistance, float camouflageDistance)
    {
        float distance = Vector3.Distance(point, transform.position);

        if (distance <= xrayDistance)
            return true;

        bool hasBushInPath = CheckBushBetween(point);
        float effectiveDistance = CalculateEffectiveDistance(distance, baseViewDistance,
                                                         camouflageDistance, hasBushInPath);

        if (distance > effectiveDistance)
            return false;

        foreach (Transform targetPoint in points)
        {
            Vector3 direction = (targetPoint.position - point).normalized;
            float pointDistance = Vector3.Distance(point, targetPoint.position);

            RaycastHit[] hits = Physics.RaycastAll(point, direction, pointDistance);
            bool hasObstacle = false;
            bool hasBush = false;

            foreach (var hit in hits)
            {
                if (hit.collider.transform.root == source.root ||
                    hit.collider.transform.root == transform.root)
                    continue;

                if (hit.collider.GetComponentInParent<Bush>() != null)
                {
                    hasBush = true;
                }
                else
                {
                    hasObstacle = true;
                    break; 
                }
            }

            if (hasObstacle) return false;

            if (!hasObstacle && hasBush)
                return true;
        }

        return true;
    }

    private bool CheckBushBetween(Vector3 point)
    {
        Vector3 direction = (transform.position - point).normalized;
        float distance = Vector3.Distance(point, transform.position);

        RaycastHit[] hits = Physics.RaycastAll(point, direction, distance);
        foreach (var hit in hits)
        {
            if (hit.collider.transform.root != transform.root &&
                hit.collider.GetComponentInParent<Bush>() != null) return true;
        }
        return false;
    }

    private float CalculateEffectiveDistance(float distance, float baseViewDistance,
                                      float camouflageDistance, bool isTargetInBush)
    {
        VehicleCamouflage vc = Vehicle.GetComponent<VehicleCamouflage>();
        if (vc == null) return baseViewDistance;

        if (isTargetInBush)
        {
            return (baseViewDistance * 0.5f) - vc.CurrentDistance;
        }
        else if (distance > camouflageDistance)
        {
            return baseViewDistance - vc.CurrentDistance;
        }

        return baseViewDistance;
    }

    public Transform GetPriorityFirePoint()
    {

        return priorityFirePoints[0];
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (points == null) return;

        Gizmos.color = Color.blue;

        for (int i = 0; i < points.Length; i++)
        {
            Gizmos.DrawSphere(points[i].position, 0.2f);
        }
    }
#endif

}
