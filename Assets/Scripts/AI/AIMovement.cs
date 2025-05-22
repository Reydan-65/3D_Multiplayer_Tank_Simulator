using UnityEngine;
using UnityEngine.AI;

public static class TransformExtensions
{
    public static Vector3 GetPositionZX(this Transform t)
    {
        var x = t.position;
        x.y = 0;
        return x;
    }
}

public static class VectorExtensions
{
    public static Vector3 GetPositionZX(this Vector3 v)
    {
        var x = v;
        x.y = 0;
        return x;
    }
}

[RequireComponent(typeof(Vehicle))]
public class AIMovement : MonoBehaviour
{
    [SerializeField] private AIRaySensor sensorForward;
    [SerializeField] private AIRaySensor sensorBackward;
    [SerializeField] private AIRaySensor sensorLeft;
    [SerializeField] private AIRaySensor sensorRight;
    [SerializeField] private float stopDistance = 2;
    [SerializeField] private float pathUpdateRate = 0.33f;

    private Vehicle vehicle;
    private NavMeshPath path;
    private Vector3 target;
    private Vector3 nextPathPoint;
    private int cornerIndex;
    private bool hasPath;
    private bool reachDestination;
    private float timerUpdatePath;

    public bool HasPath => hasPath;
    public bool ReachDestination => reachDestination;

    private void Awake()
    {
        vehicle = GetComponent<Vehicle>();
        path = new NavMeshPath();
    }

    private void Update()
    {
        if (NetworkSessionManager.Instance != null && !NetworkSessionManager.Match.MatchActive)
        {
            vehicle.SetTargetControl(Vector3.zero);
            return;
        }

        UpdatePathTimer();
        UpdateTarget();
        MoveToTarget();
    }

    private void UpdatePathTimer()
    {
        if (pathUpdateRate > 0)
        {
            timerUpdatePath += Time.deltaTime;

            if (timerUpdatePath > pathUpdateRate)
            {
                CalculatePath(target);
                timerUpdatePath = 0;
            }
        }
    }

    public void SetDestination(Vector3 target)
    {
        if (this.target == target && hasPath) return;

        this.target = target;

        CalculatePath(target);
    }

    public void ResetPath()
    {
        hasPath = false;
        reachDestination = false;
    }

    private void UpdateTarget()
    {
        if (!hasPath || path.corners.Length == 0) return;

        if (cornerIndex >= path.corners.Length)
            cornerIndex = path.corners.Length - 1;

        nextPathPoint = path.corners[cornerIndex];

        if (Vector3.Distance(transform.position, nextPathPoint) < stopDistance)
        {
            if (path.corners.Length - 1 > cornerIndex)
            {
                cornerIndex++;
                nextPathPoint = path.corners[cornerIndex];
            }
            else
            {
                hasPath = false;
                reachDestination = true;
            }
        }

        for (int i = 0; i < path.corners.Length - 1; i++)
            Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
    }

    private void CalculatePath(Vector3 target)
    {
        NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path);

        hasPath = path.corners.Length > 0;
        reachDestination = false;

        cornerIndex = 1;
    }

    // Движение к пункту назначения
    private void MoveToTarget()
    {
        if (nextPathPoint == null) return;

        if (reachDestination == true)
        {
            vehicle.SetTargetControl(new Vector3(0, 1, 0));
            return;
        }

        float turnControl = 0f;
        float forwardThrust = 0f;

        Vector3 referenceDirection = GetReferenceMovementDirectionZX();
        Vector3 vehicleDirection = GetVehicleDirectionZX();

        float angleToTarget = Vector3.SignedAngle(vehicleDirection, referenceDirection, Vector3.up);

        if (Mathf.Abs(angleToTarget) > 45f)
        {
            turnControl = Mathf.Clamp(angleToTarget / 45f, -1f, 1f);
            forwardThrust = 0f;
        }
        else
        {
            turnControl = Mathf.Clamp(angleToTarget / 45f, -1f, 1f);
            forwardThrust = 1f;

            var forwardSensorState = sensorForward.Raycast();
            var leftSensorState = sensorLeft.Raycast();
            var rightSensorState = sensorRight.Raycast();
            var backwardSensorState = sensorBackward.Raycast();

            if (forwardSensorState.Item1 == true)
            {
                if (!leftSensorState.Item1 && !rightSensorState.Item1)
                {
                    turnControl = Random.Range(0, 2) == 0 ? -1f : 1f;
                    forwardThrust = -0.5f;
                }
                else if (!leftSensorState.Item1)
                {
                    turnControl = -1f;
                    forwardThrust = -0.5f;
                }
                else if (!rightSensorState.Item1)
                {
                    turnControl = 1f;
                    forwardThrust = -0.5f;
                }
                else
                {
                    forwardThrust = -1f;
                    turnControl = Random.Range(-0.5f, 0.5f);
                }
            }
            else
            {
                turnControl = Mathf.Clamp(Vector3.SignedAngle(vehicleDirection, referenceDirection, Vector3.up), -55.0f, 55.0f) / 55.0f; // от -1 до 1

                float minSideDistance = 1;

                if (leftSensorState.Item1 == true && leftSensorState.Item2 < minSideDistance && turnControl < minSideDistance) turnControl = 1;
                if (rightSensorState.Item1 == true && rightSensorState.Item2 < minSideDistance && turnControl > minSideDistance) turnControl = -1;

                if (forwardSensorState.Item1 == true && forwardSensorState.Item2 < 5f)
                    turnControl += Random.Range(-0.5f, 0.5f);

                if (backwardSensorState.Item1 == true && backwardSensorState.Item2 < 5f)
                {
                    forwardThrust = 1f;
                    turnControl = 0f;
                }
            }
        }

        vehicle.SetTargetControl(new Vector3(turnControl, 0, forwardThrust));
    }

    // Направление объекта
    private Vector3 GetVehicleDirectionZX()
    {
        var vehicleDir = vehicle.transform.forward.GetPositionZX();
        vehicleDir.Normalize();
        return vehicleDir;
    }

    // Направление от объекта к цели
    private Vector3 GetReferenceMovementDirectionZX()
    {
        var vehiclePos = vehicle.transform.GetPositionZX();
        var targetPos = nextPathPoint.GetPositionZX();
        return (targetPos - vehiclePos).normalized;
    }
}
