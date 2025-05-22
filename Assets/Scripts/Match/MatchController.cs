using UnityEngine;
using UnityEngine.Events;
using Mirror;
using System.Collections;

public interface IMatchCondition
{
    bool IsTriggered { get; }

    void OnServerMatchStart(MatchController controller);
    void OnServerMatchEnd(MatchController controller);
}

public class MatchController : NetworkBehaviour
{
    private static int TeamIDCounter;

    public static int GetNextTeam() => TeamIDCounter++ % 2;
    public static void ResetTeamCounter() => TeamIDCounter = 1;


    public event UnityAction MatchStart;
    public event UnityAction MatchEnd;

    public event UnityAction SvMatchStart;
    public event UnityAction SvMatchEnd;

    [SerializeField] private MatchMemberSpawner spawner;
    [SerializeField] private float delayAfterSpawnBeforeStartMatch = 0.5f;

    [SyncVar]
    private bool matchActive;
    public bool MatchActive => matchActive;

    private IMatchCondition[] matchConditions;

    public int WinTeamID = -1;

    private void Awake()
    {
        matchConditions = GetComponentsInChildren<IMatchCondition>();
    }

    private void Update()
    {
        if (isServer && matchActive)
        {
            foreach (var c in matchConditions)
            {
                if (c.IsTriggered && WinTeamID == -1)
                {
                    SvEndMatch();
                    break;
                }
            }
        }
    }

    [Server]
    public void SvRestartMatch()
    {
        if (matchActive) return;

        WinTeamID = -1;
        matchActive = true;

        spawner.SvRespawnVehicleAllMember();

        foreach (var c in matchConditions)
        {
            if (c is ConditionTeamDeathmatch teamDeathmatch)
                teamDeathmatch.Reset();
            if (c is ConditionCaptureBase captureBase)
                captureBase.Reset();
        }

        MatchTimer timer = FindFirstObjectByType<MatchTimer>();
        timer.Reset();

        StartCoroutine(StartEventMatchWithDelay(delayAfterSpawnBeforeStartMatch));
    }

    private IEnumerator StartEventMatchWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var c in matchConditions)
            c.OnServerMatchStart(this);

        SvMatchStart?.Invoke();

        RpcMatchStart();
    }

    [Server]
    public void SvEndMatch()
    {
        if (!matchActive) return;

        foreach (var c in matchConditions)
        {
            c.OnServerMatchEnd(this);

            if (c is ConditionTeamDeathmatch)
                WinTeamID = (c as ConditionTeamDeathmatch).WinTeamID;

            if (c is ConditionCaptureBase)
            {
                if ((c as ConditionCaptureBase).RedBaseCaptureLevel == 100)
                    WinTeamID = TeamSide.TeamBlue;

                if ((c as ConditionCaptureBase).BlueBaseCaptureLevel == 100)
                    WinTeamID = TeamSide.TeamRed;
            }
        }

        matchActive = false;

        SvMatchEnd?.Invoke();

        RpcMatchEnd(WinTeamID);
    }

    [ClientRpc]
    private void RpcMatchStart()
    {
        MatchStart?.Invoke();
    }

    [ClientRpc]
    private void RpcMatchEnd(int winTeamID)
    {
        WinTeamID = winTeamID;
        MatchEnd?.Invoke();
    }
}
