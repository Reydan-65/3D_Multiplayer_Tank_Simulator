using UnityEngine;

public class AIPath : MonoBehaviour
{
    public static AIPath Instance;

    [SerializeField] private Transform baseRedPoint;
    [SerializeField] private Transform baseBluePoint;

    [SerializeField] private Transform[] fireRedPoints;
    [SerializeField] private Transform[] fireBluePoints;

    [SerializeField] private Transform[] patrolRedPoints;
    [SerializeField] private Transform[] patrolBluePoints;

    private void Awake()
    {
        Instance = this;
    }

    public Vector3 GetBasePoint(int teamID)
    {
        if (teamID == TeamSide.TeamRed)
        {
            return baseBluePoint.position;
        }

        if (teamID == TeamSide.TeamBlue)
        { 
            return baseRedPoint.position; 
        }

        return Vector3.zero;
    }

    public Vector3 GetRandomFirePoint(int teamID)
    {
        if (teamID == TeamSide.TeamRed)
        {
            return fireBluePoints[Random.Range(0, fireBluePoints.Length)].position;
        }

        if (teamID == TeamSide.TeamBlue)
        {
            return fireRedPoints[Random.Range(0, fireRedPoints.Length)].position;
        }

        return Vector3.zero;
    }

    public Vector3 GetRandomPatrolPoint(int teamID)
    {
        if (teamID == TeamSide.TeamRed)
        {
            return patrolBluePoints[Random.Range(0, patrolBluePoints.Length)].position;
        }

        if (teamID == TeamSide.TeamBlue)
        {
            return patrolRedPoints[Random.Range(0, patrolRedPoints.Length)].position;
        }

        return Vector3.zero;
    }
}
