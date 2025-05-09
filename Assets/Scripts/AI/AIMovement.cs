using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.GridLayoutGroup;

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
    [SerializeField] private float stopDistance = 1;
    [SerializeField] private float pathUpdateRate = 0.33f;

    private Vector3 target;
    private Vector3 nextPathPoint;
    private Vehicle vehicle;
    private int cornerIndex;

    private NavMeshPath path;
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
        //SetDestination(GameObject.FindGameObjectWithTag("Bot Target").transform.position);

        if (pathUpdateRate > 0)
        {
            timerUpdatePath += Time.deltaTime;

            if (timerUpdatePath > pathUpdateRate)
            {
                CalculatePath(target);
                timerUpdatePath = 0;
            }
        }

        UpdateTarget();
        MoveToTarget();
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
        if (!hasPath) return;

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
        {
            Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
        }
    }

    private void CalculatePath(Vector3 target)
    {
        NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path);

        hasPath = path.corners.Length > 0;
        reachDestination = false;

        cornerIndex = 1;
    }

    private void MoveToTarget()
    {
        if (nextPathPoint == null) return;

        if (reachDestination == true)
        {
            vehicle.SetTargetControl(new Vector3(0, 1, 0));
            return;
        }

        float turnControl = 0;
        float forwardThrust = 1;

        var referenceDirection = GetReferenceMovementDirectionZX();
        var vehicleDirection = GetVehicleDirectionZX();

        var forwardSensorState = sensorForward.Raycast();
        var leftSensorState = sensorLeft.Raycast();
        var rightSensorState = sensorRight.Raycast();

        if (forwardSensorState.Item1 == true)
        {
            forwardThrust = 0;

            if (leftSensorState.Item1 == false)
            {
                turnControl = -1;
                forwardThrust = -0.2f;
            }
            else if (rightSensorState.Item1 == false)
            {
                turnControl = 1;
                forwardThrust = -0.2f;
            }
            else
            {
                forwardThrust = -1;
            }
        }
        else
        {
            turnControl = Mathf.Clamp(Vector3.SignedAngle(vehicleDirection, referenceDirection, Vector3.up), -45.0f, 45.0f) / 45.0f; // от -1 до 1

            // Не прижиматься к препятствиям
            float minSideDistance = 1;

            if (leftSensorState.Item1 == true && leftSensorState.Item2 < minSideDistance && turnControl < minSideDistance) turnControl = 1;
            if (rightSensorState.Item1 == true && rightSensorState.Item2 < minSideDistance && turnControl > minSideDistance) turnControl = -1;
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
