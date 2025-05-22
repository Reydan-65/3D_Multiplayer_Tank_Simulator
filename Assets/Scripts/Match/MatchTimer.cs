using Mirror;
using UnityEngine;

public class MatchTimer : NetworkBehaviour, IMatchCondition
{
    [SerializeField] private float matchTime;

    [SyncVar]
    private float timeLeft;
    public float TimeLeft => timeLeft;

    private bool timerEnd = false;

    bool IMatchCondition.IsTriggered => timerEnd;

    public bool IsTriggered => throw new System.NotImplementedException();

    public void OnServerMatchStart(MatchController controller)
    {
        Reset();
    }

    public void OnServerMatchEnd(MatchController controller)
    {
        enabled = false;
    }

    private void Start()
    {
        if (isServer)
        {
            enabled = false;
        }
    }

    private void Update()
    {
        if (isServer)
        {
            timeLeft -= Time.deltaTime;

            if (timeLeft < 0)
            {
                timeLeft = 0;
                timerEnd = true;
            }
        }
    }

    public void Reset()
    {
        enabled = true;
        timeLeft = matchTime;
        timerEnd = false;
    }
}
