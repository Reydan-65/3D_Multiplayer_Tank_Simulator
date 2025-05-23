using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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
    // Движение
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

    // Поиск укрытий
    [SerializeField] private float coverSearchTime = 15f;

    private Vehicle attackVehicle;
    private Vector3 previousTarget;
    private bool hasPreviousTarget;
    private float coverSearchTimer;
    private bool isSearchingCover;

    // Застревание
    private bool isStuck;
    private Vector3 lastPosition;
    private float distanceCheckTimer;
    private float distanceCheckDuration = 10f;
    private float minDistanceThreshold = 1f;

    private void Awake()
    {
        vehicle = GetComponent<Vehicle>();
        path = new NavMeshPath();
        lastPosition = transform.position;
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

        if (!isStuck)
            MoveToTarget();

        UpdateCoverSearch();
        CheckDistanceTraveled();
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

    private void UpdateCoverSearch()
    {
        if (!isSearchingCover) return;

        coverSearchTimer -= Time.deltaTime;

        if (coverSearchTimer <= 0)
            ReturnToPreviousTarget();
    }

    public void SetDestination(Vector3 target)
    {
        if (this.target == target && hasPath) return;

        if (!isSearchingCover)
        {
            previousTarget = this.target;
            hasPreviousTarget = true;
        }

        if (Physics.CheckSphere(target, 2f))
        {
            Vector3 alternativeTarget = FindAlternativeTarget(target);
            this.target = alternativeTarget;
        }
        else
        {
            this.target = target;
        }

        CalculatePath(this.target);
    }

    private Vector3 FindAlternativeTarget(Vector3 originalTarget)
    {
        float searchRadius = 5f;
        int maxAttempts = 20;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * searchRadius;
            randomDirection.y = 0;
            Vector3 alternativeTarget = originalTarget + randomDirection;

            if (!Physics.CheckSphere(alternativeTarget, 1f))
            {
                return alternativeTarget;
            }
        }

        return originalTarget;
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
        if (isSearchingCover)
        {
            SearchForCover(attackVehicle);
            return;
        }

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

        var forwardSensorState = sensorForward.Raycast();
        var leftSensorState = sensorLeft.Raycast();
        var rightSensorState = sensorRight.Raycast();
        var backwardSensorState = sensorBackward.Raycast();

        if (Mathf.Abs(angleToTarget) > 45f)
        {
            turnControl = Mathf.Clamp(angleToTarget / 45f, -1f, 1f);
            forwardThrust = 0f;
        }
        else
        {
            turnControl = Mathf.Clamp(angleToTarget / 45f, -1f, 1f);
            forwardThrust = 1f;

            if (forwardSensorState.Item1 == true)
            {
                if (!leftSensorState.Item1 && !rightSensorState.Item1)
                {
                    forwardThrust = -0.2f;
                    turnControl = Random.Range(0, 2) == 0 ? -1f : 1f;
                }
                else if (!leftSensorState.Item1)
                {
                    forwardThrust = -0.2f;
                    turnControl = -1f;
                }
                else if (!rightSensorState.Item1)
                {
                    forwardThrust = -0.2f;
                    turnControl = 1f;
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

                if (leftSensorState.Item1 && leftSensorState.Item2 < minSideDistance)
                    turnControl = Mathf.Clamp(turnControl + 0.5f, -1f, 1f);
                else if (rightSensorState.Item1 && rightSensorState.Item2 < minSideDistance)
                    turnControl = Mathf.Clamp(turnControl - 0.5f, -1f, 1f);

                if (forwardSensorState.Item1 && forwardSensorState.Item2 < 5f)
                    turnControl += Random.Range(-0.5f, 0.5f);

                if (backwardSensorState.Item1 && backwardSensorState.Item2 < 5f)
                {
                    forwardThrust = 1f;
                    turnControl = 0f;
                }
            }
        }

        vehicle.SetTargetControl(new Vector3(turnControl, 0, forwardThrust));
    }

    public void Stop()
    {
        vehicle.SetTargetControl(Vector3.zero);
    }

    private void CheckDistanceTraveled()
    {
        distanceCheckTimer += Time.deltaTime;

        if (distanceCheckTimer >= distanceCheckDuration)
        {
            float distanceTraveled = Vector3.Distance(transform.position, lastPosition);

            if (distanceTraveled < minDistanceThreshold)
            {
                isStuck = true;
                HandleStuckState();
            }

            lastPosition = transform.position;
            distanceCheckTimer = 0f;
        }
    }

    private void HandleStuckState()
    {
        float backwardThrust = -1f;
        vehicle.SetTargetControl(new Vector3(0, 0, backwardThrust));

        StartCoroutine(WaitAndRecalculatePath());
    }

    private IEnumerator WaitAndRecalculatePath()
    {
        yield return new WaitForSeconds(Random.Range(2f, 5f));

        CalculatePath(target);

        if (hasPath && path.corners.Length > 0)
        {
            Vector3 closestPoint = FindClosestPathPoint();
            Vector3 directionToPoint = (closestPoint - transform.position).normalized;

            float angleToPoint = Vector3.SignedAngle(vehicle.transform.forward, directionToPoint, Vector3.up);
            float turnControl = Mathf.Clamp(angleToPoint / 45f, -1f, 1f);

            float forwardThrust = 1f;

            vehicle.SetTargetControl(new Vector3(turnControl, 0, forwardThrust));
        }

        isStuck = false;
    }

    private Vector3 FindClosestPathPoint()
    {
        Vector3 closestPoint = path.corners[0];
        float minDistance = Vector3.Distance(transform.position, closestPoint);

        for (int i = 1; i < path.corners.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, path.corners[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = path.corners[i];
            }
        }

        return closestPoint;
    }

    // Поиск укрытий
    public void OnUnderFire(Projectile projectile)
    {
        if (!isSearchingCover)
        {
            previousTarget = target;
            hasPreviousTarget = true;
        }

        isSearchingCover = true;
        coverSearchTimer = coverSearchTime;
        attackVehicle = projectile.OwnerVehicle;
    }

    private void ReturnToPreviousTarget()
    {
        isSearchingCover = false;
        coverSearchTimer = 0;
        target = previousTarget;

        if (hasPreviousTarget)
        {
            SetDestination(target);
            hasPreviousTarget = false;
        }
    }

    private void SearchForCover(Vehicle vehicle)
    {
        if (!isSearchingCover) return;

        if (vehicle == null)
        {
            ReturnToPreviousTarget();
            return;
        }

        Vector3 coverPoint = FindNearestCoverPoint(vehicle);
        if (coverPoint != Vector3.zero)
            SetDestination(coverPoint);
    }

    private Vector3 FindNearestCoverPoint(Vehicle vehicle)
    {
        if (vehicle == null)
            return Vector3.zero;

        Vector3 shooterPosition = vehicle.transform.position;
        Vector3 currentPosition = transform.position;
        float coverSearchRadius = 20f;
        int maxAttempts = 20;
        float minPathDistance = 10f;
        float heightThreshold = 2f;

        NavMeshPath testPath = new NavMeshPath();

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 awayDirection = (currentPosition - shooterPosition).normalized;
            Vector3 randomDirection = Quaternion.Euler(0, Random.Range(-60f, 60f), 0) * awayDirection;
            Vector3 randomPoint = currentPosition + randomDirection * coverSearchRadius;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, coverSearchRadius, NavMesh.AllAreas))
            {
                if (Mathf.Abs(hit.position.y - currentPosition.y) > heightThreshold)
                    continue;

                if (NavMesh.CalculatePath(currentPosition, hit.position, NavMesh.AllAreas, testPath))
                {
                    float pathLength = CalculatePathLength(testPath);
                    if (pathLength >= minPathDistance && IsPointBehindCover(hit.position, attackVehicle) && !IsPathBlocked(testPath))
                        return hit.position;
                }
            }
        }

        return Vector3.zero;
    }

    private bool IsPathBlocked(NavMeshPath path)
    {
        if (path.corners.Length < 2) return false;

        for (int i = 1; i < path.corners.Length; i++)
        {
            Vector3 start = path.corners[i - 1];
            Vector3 end = path.corners[i];
            float distance = Vector3.Distance(start, end);

            if (Physics.Raycast(start, (end - start).normalized, distance))
                return true;
        }

        return false;
    }

    private float CalculatePathLength(NavMeshPath path)
    {
        if (path.corners.Length < 2) return 0f;

        float length = 0f;
        for (int i = 1; i < path.corners.Length; i++)
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);

        return length;
    }

    private bool IsPointBehindCover(Vector3 point, Vehicle vehicle)
    {
        if (vehicle == null)
            return false;

        Vector3 shooterPosition = vehicle.transform.position;
        Vector3 direction = (point - shooterPosition).normalized;
        float distance = Vector3.Distance(shooterPosition, point);

        if (Physics.Raycast(shooterPosition, direction, distance))
            return true;

        return false;
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
