using UnityEngine;
using UnityEngine.Events;
using Mirror;

public interface IMatchCondition
{
    bool IsTriggered { get; }

    void OnServerMatchStart(MatchController controller);
    void OnServerMatchEnd(MatchController controller);
}

public class MatchController : NetworkBehaviour
{
    public UnityAction MatchStart;
    public UnityAction MatchEnd;

    public UnityAction SvMatchStart;
    public UnityAction SvMatchEnd;

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
        if (isServer)
        {
            if (matchActive == true)
            {
                foreach (var c in matchConditions)
                {
                    if (c.IsTriggered)
                    {
                        SvEndMatch();
                        break;
                    }
                }
            }
        }
    }

    [Server]
    public void SvRestartMatch()
    {
        if (matchActive) return;

        matchActive = true;

        var players = FindObjectsByType<Player>(FindObjectsSortMode.None);

        foreach (var p in players)
        {
            if (p.ActiveVehicle != null)
            {
                if (p.ActiveVehicle.gameObject.scene.IsValid())
                {
                    NetworkServer.UnSpawn(p.ActiveVehicle.gameObject);
                    Destroy(p.ActiveVehicle.gameObject);
                    p.ActiveVehicle = null;
                }
            }
        }

        foreach (var p in players)
        {
            p.SvSpawnClientVehicle();
        }

        foreach (var c in matchConditions)
        {
            c.OnServerMatchStart(this);
        }

        SvMatchStart?.Invoke();

        RpcMatchStart();
    }

    [Server]
    public void SvEndMatch()
    {
        foreach (var c in matchConditions)
        {
            c.OnServerMatchEnd(this);

            if (c is ConditionTeamDeathmatch)
            {
                WinTeamID = (c as ConditionTeamDeathmatch).WinTeamID;
            }

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
