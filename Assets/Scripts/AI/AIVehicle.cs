using UnityEngine;
using Mirror;

public enum AIBehaviourType
{
    Patrol,
    Support,
    InvaderBase
}

public class AIVehicle : NetworkBehaviour
{
    [SerializeField] private AIBehaviourType behaviourType;

    [Range(0f, 1f)]
    [SerializeField] private float patrolChance;
    [Range(0f, 1f)]
    [SerializeField] private float supportChance;
    [Range(0f, 1f)]
    [SerializeField] private float invaderBaseChance;

    [SerializeField] private Vehicle vehicle;
    [SerializeField] private AIMovement movement;
    [SerializeField] private AIShooter shooter;

    private Vehicle fireTarget;
    private Vector3 movementTarget;
    private int startCountTeamMember;
    private int countCountTeamMember;

    private void Start()
    {
        NetworkSessionManager.Match.MatchStart += OnMatchStart;

        if (vehicle != null)
            vehicle.Destroyed += OnVehicleDestroyed;

        movement.enabled = false;
        shooter.enabled = false;

        CalcTeamMember();
        SetStartBehaviour();
    }

    private void OnDestroy()
    {
        if (NetworkSessionManager.Instance != null)
            NetworkSessionManager.Match.MatchStart -= OnMatchStart;

        if (vehicle != null)
            vehicle.Destroyed -= OnVehicleDestroyed;
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
        Vehicle[] v = FindObjectsByType<Vehicle>(FindObjectsSortMode.None);

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

    private void StartBehaviour(AIBehaviourType type)
    {
        behaviourType = type;

        if (behaviourType == AIBehaviourType.InvaderBase)
        {
            movementTarget = AIPath.Instance.GetBasePoint(vehicle.TeamID);
        }

        if (behaviourType == AIBehaviourType.Patrol)
        {
            movementTarget = AIPath.Instance.GetRandomPatrolPoint(vehicle.TeamID);
        }

        if (behaviourType == AIBehaviourType.Support)
        {
            movementTarget = AIPath.Instance.GetRandomFirePoint(vehicle.TeamID);
        }

        movement.ResetPath();
    }

    private void OnReachDestination()
    {
        if (behaviourType != AIBehaviourType.Patrol)
        {
            movementTarget = AIPath.Instance.GetRandomPatrolPoint(vehicle.TeamID);
        }

        movement.ResetPath();
    }


    private void OnTeamMemberDestroyed(Destructible dest)
    {
        countCountTeamMember--;

        dest.Destroyed -= OnTeamMemberDestroyed;

        if ((float)countCountTeamMember / (float)startCountTeamMember < 0.4f)
        {
            StartBehaviour(AIBehaviourType.InvaderBase);
        }

        if (countCountTeamMember <= 2)
        {
            StartBehaviour(AIBehaviourType.InvaderBase);
        }
    }

    private void UpdateBehaviuor()
    {
        shooter.FindTarget();

        if (movement.ReachDestination == true)
        {
            OnReachDestination();
        }

        if (movement.HasPath == false)
        {
            movement.SetDestination(movementTarget);
        }
    }

    #endregion
}
