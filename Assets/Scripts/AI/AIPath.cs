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

    [SerializeField] private Transform[] invaderBaseRedPoints;
    [SerializeField] private Transform[] invaderBaseBluePoints;

    private int currentRedPathIndex = 0;
    private int currentBluePathIndex = 0;

    private void Awake()
    {
        Instance = this;
    }

    public Vector3 GetTeamBasePoint(int teamID)
    {
        if (teamID == TeamSide.TeamRed)
        {
            return baseRedPoint.position;
        }

        if (teamID == TeamSide.TeamBlue)
        {
            return baseBluePoint.position;
        }

        return Vector3.zero;
    }

    public Vector3 GetRandomFirePoint(int teamID)
    {
        if (teamID == TeamSide.TeamRed)
        {
            return fireRedPoints[Random.Range(0, fireRedPoints.Length)].position;
        }

        if (teamID == TeamSide.TeamBlue)
        {
            return fireBluePoints[Random.Range(0, fireBluePoints.Length)].position;
        }

        return Vector3.zero;
    }

    public Vector3 GetRandomPatrolPoint(int teamID)
    {
        if (teamID == TeamSide.TeamRed)
        {
            return patrolRedPoints[Random.Range(0, patrolRedPoints.Length)].position;
        }

        if (teamID == TeamSide.TeamBlue)
        {
            return patrolBluePoints[Random.Range(0, patrolBluePoints.Length)].position;
        }

        return Vector3.zero;
    }

    public Vector3 GetInvaderPoint(int teamID)
    {
        Vector3 point = Vector3.zero;

        if (teamID == TeamSide.TeamRed)
        {
            if (invaderBaseRedPoints.Length > 0)
            {
                if (currentRedPathIndex < invaderBaseRedPoints.Length)
                    point = invaderBaseRedPoints[currentRedPathIndex].position;
                else
                    point = baseBluePoint.position;
            }
        }
        
        if (teamID == TeamSide.TeamBlue)
        {
            if (invaderBaseBluePoints.Length > 0)
            {
                if (currentBluePathIndex < invaderBaseBluePoints.Length)
                    point = invaderBaseBluePoints[currentBluePathIndex].position;
                else
                    point = baseRedPoint.position;
            }
        }

        return point;
    }

    public void NextPathIndex(int teamID)
    {
        if (teamID == TeamSide.TeamRed)
        {
            if (currentRedPathIndex < invaderBaseRedPoints.Length)
            {
                currentRedPathIndex++;
            }
        }
        else if (teamID == TeamSide.TeamBlue)
        {
            if (currentBluePathIndex < invaderBaseBluePoints.Length)
            {
                currentBluePathIndex++;
            }
        }
    }
}
