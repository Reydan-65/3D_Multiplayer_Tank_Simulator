using UnityEngine;
using Mirror;

public enum AIBehaviourType
{
    Patrol,
    Support,
    InvaderBase,
    DefenderBase
}

public class AIVehicle : NetworkBehaviour
{
    [SerializeField] private AIBehaviourType behaviourType;
    public AIBehaviourType BehaviourType => behaviourType;

    [Range(0f, 1f)]
    [SerializeField] private float patrolChance;
    [Range(0f, 1f)]
    [SerializeField] private float supportChance;
    [Range(0f, 1f)]
    [SerializeField] private float invaderBaseChance;

    [SerializeField] private Vehicle vehicle;
    [SerializeField] private AIMovement movement;
    [SerializeField] private AIShooter shooter;
    [SerializeField] private float supportWaitTime = 20f;

    private float supportTimer;
    private Vector3 movementTarget;
    private Vector3 previousMovementTarget;
    private bool isSupportTimerRunning = false;
    private AIBehaviourType previousBehaviourType;

    private int startCountTeamMember;
    private int countCountTeamMember;

    private TeamBase teamBase;
    private bool isBaseUnderAttack;

    private void Start()
    {
        NetworkSessionManager.Match.MatchStart += OnMatchStart;

        if (vehicle != null)
            vehicle.Destroyed += OnVehicleDestroyed;

        movement.enabled = false;
        shooter.enabled = false;

        CalcTeamMember();
        SetStartBehaviour();

        TeamBase[] bases = FindObjectsByType<TeamBase>(0);

        for (int i = 0; i < bases.Length; i++)
        {
            if (bases[i] != null)
            {
                if (vehicle.TeamID == bases[i].TeamID)
                {
                    teamBase = bases[i];
                    teamBase.BaseCaptureStarted += OnBaseCaptureStarted;
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (NetworkSessionManager.Instance != null)
            NetworkSessionManager.Match.MatchStart -= OnMatchStart;

        if (vehicle != null)
            vehicle.Destroyed -= OnVehicleDestroyed;

        if (teamBase != null)
            teamBase.BaseCaptureStarted -= OnBaseCaptureStarted;
    }

    private void Update()
    {
        if (isServer)
            UpdateBehaviuor();
    }

    private void OnMatchStart()
    {
        movement.enabled = true;
        shooter.enabled = true;
    }

    private void OnVehicleDestroyed(Destructible dest)
    {
        movement.enabled = false;
        shooter.enabled = false;
    }

    private void CalcTeamMember()
    {
        Vehicle[] v = FindObjectsByType<Vehicle>(0);

        for (int i = 0; i < v.Length; i++)
        {
            if (v[i].TeamID == vehicle.TeamID)
            {
                if (v[i] != vehicle)
                {
                    startCountTeamMember++;
                    v[i].Destroyed += OnTeamMemberDestroyed;
                }
            }
        }

        countCountTeamMember = startCountTeamMember;
    }

    private void SetStartBehaviour()
    {
        float chance = Random.Range(0.0f, patrolChance + supportChance + invaderBaseChance);

        if (chance >= 0.0f && chance <= patrolChance)
        {
            StartBehaviour(AIBehaviourType.Patrol);
            return;
        }

        if (chance >= patrolChance && chance <= patrolChance + supportChance)
        {
            StartBehaviour(AIBehaviourType.Support);
            return;
        }

        if (chance >= patrolChance + supportChance && chance <= patrolChance + supportChance + invaderBaseChance)
        {
            StartBehaviour(AIBehaviourType.InvaderBase);
            return;
        }
    }

    #region Behaviour

    public void StartBehaviour(AIBehaviourType type)
    {
        behaviourType = type;

        if (behaviourType == AIBehaviourType.InvaderBase)
            movementTarget = AIPath.Instance.GetInvaderPoint(vehicle.TeamID);

        if (behaviourType == AIBehaviourType.DefenderBase)
            movementTarget = AIPath.Instance.GetTeamBasePoint(vehicle.TeamID);

        if (behaviourType == AIBehaviourType.Patrol)
            movementTarget = AIPath.Instance.GetRandomPatrolPoint(vehicle.TeamID);

        if (behaviourType == AIBehaviourType.Support)
            movementTarget = AIPath.Instance.GetRandomFirePoint(vehicle.TeamID);

        movement.ResetPath();
    }

    private void OnReachDestination()
    {
        if (behaviourType == AIBehaviourType.DefenderBase && isBaseUnderAttack)
        {
            movement.Stop();
            return;
        }

        if (behaviourType == AIBehaviourType.Patrol)
            movementTarget = AIPath.Instance.GetRandomPatrolPoint(vehicle.TeamID);

        if (behaviourType == AIBehaviourType.InvaderBase)
        {
            AIPath.Instance.NextPathIndex(vehicle.TeamID);
            movementTarget = AIPath.Instance.GetInvaderPoint(vehicle.TeamID);
        }

        if (behaviourType == AIBehaviourType.DefenderBase)
        {
            behaviourType = previousBehaviourType;
            movementTarget = previousMovementTarget;
        }

        if (behaviourType == AIBehaviourType.Support)
        {
            if (!isSupportTimerRunning)
            {
                supportTimer = supportWaitTime;
                isSupportTimerRunning = true;
            }
        }

        movement.ResetPath();
    }

    private bool IsClosestToTeamBase()
    {
        Vector3 teamBase = AIPath.Instance.GetTeamBasePoint(vehicle.TeamID);
        float closestDistance = float.MaxValue;
        bool isClosest = false;

        Vehicle[] vehicles = FindObjectsByType<Vehicle>(0);

        foreach (Vehicle v in vehicles)
        {
            if (v.HitPoint > 0)
            {
                if (v.TeamID == vehicle.TeamID)
                {
                    float distance = Vector3.Distance(v.transform.position, teamBase);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        isClosest = (v == vehicle);
                    }
                }
            }
        }

        return isClosest;
    }

    private bool HasDefendersBaseAllies()
    {
        AIVehicle[] aiVehicles = FindObjectsByType<AIVehicle>(0);

        foreach (AIVehicle ai in aiVehicles)
        {
            Vehicle v = ai.GetComponent<Vehicle>();

            if (v != null)
            {
                if (v.TeamID == vehicle.TeamID && ai.behaviourType == AIBehaviourType.DefenderBase)
                    return true;
            }
        }

        return false;
    }

    private void OnTeamMemberDestroyed(Destructible dest)
    {
        countCountTeamMember--;

        dest.Destroyed -= OnTeamMemberDestroyed;

        if ((float)countCountTeamMember / (float)startCountTeamMember < 0.4f)
            StartBehaviour(AIBehaviourType.InvaderBase);

        if (countCountTeamMember <= 2)
            StartBehaviour(AIBehaviourType.InvaderBase);
    }

    private void OnBaseCaptureStarted()
    {
        if (IsClosestToTeamBase() && !HasDefendersBaseAllies() && behaviourType != AIBehaviourType.InvaderBase)
        {
            previousBehaviourType = behaviourType;
            previousMovementTarget = movementTarget;

            StartBehaviour(AIBehaviourType.DefenderBase);
        }
    }

    private void UpdateBehaviuor()
    {
        CheckBaseThreat();

        if (isBaseUnderAttack && IsClosestToTeamBase() && behaviourType != AIBehaviourType.DefenderBase)
        {
            previousBehaviourType = behaviourType;
            previousMovementTarget = movementTarget;
            StartBehaviour(AIBehaviourType.DefenderBase);
        }

        shooter.FindTarget();

        if (movement.ReachDestination)
            OnReachDestination();

        if (behaviourType == AIBehaviourType.DefenderBase && isBaseUnderAttack)
        {
            movementTarget = AIPath.Instance.GetTeamBasePoint(vehicle.TeamID);
            movement.ResetPath();
            return;
        }

        if (behaviourType == AIBehaviourType.Support && supportTimer > 0)
        {
            supportTimer -= Time.deltaTime;
            if (supportTimer <= 0)
            {
                supportTimer = 0;
                isSupportTimerRunning = false;
                movementTarget = AIPath.Instance.GetRandomFirePoint(vehicle.TeamID);
                movement.ResetPath();
            }
        }

        if (!movement.HasPath)
            movement.SetDestination(movementTarget);
    }

    #endregion

    private void CheckBaseThreat()
    {
        if (teamBase == null) return;

        isBaseUnderAttack = teamBase.IsCapturing || AreEnemiesNearBase();
    }

    private bool AreEnemiesNearBase()
    {
        if (teamBase == null) return false;

        Vehicle[] enemies = FindObjectsByType<Vehicle>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        float threatRadius = 30f;

        foreach (Vehicle enemy in enemies)
        {
            if (enemy.TeamID != vehicle.TeamID && enemy.HitPoint > 0)
            {
                float distance = Vector3.Distance(enemy.transform.position, teamBase.transform.position);
                if (distance <= threatRadius) return true;
            }
        }

        return false;
    }
}
