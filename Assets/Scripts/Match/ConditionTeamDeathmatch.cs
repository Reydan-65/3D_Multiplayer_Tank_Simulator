using UnityEngine;

public class ConditionTeamDeathmatch : MonoBehaviour, IMatchCondition
{
    private int red;
    private int blue;

    private int winTeamID = -1;
    public int WinTeamID => winTeamID;

    private bool triggered;

    public bool IsTriggered => triggered;

    public void OnServerMatchStart(MatchController controller)
    {
        Reset();

        var matchMember = FindObjectsByType<MatchMember>(0);

        foreach (var m in matchMember)
        {
            if (m.ActiveVehicle != null)
            {
                m.ActiveVehicle.Destroyed += OnVehicleDestroyed;

                if (m.TeamID == TeamSide.TeamRed) red++;
                else if (m.TeamID == TeamSide.TeamBlue) blue++;
            }
        }
    }

    public void OnServerMatchEnd(MatchController controller) { }

    private void OnVehicleDestroyed(Destructible dest)
    {
        Vehicle v = (dest as Vehicle);

        if (v == null) return;

        var ownerMatchMember = v.Owner?.GetComponent<MatchMember>();

        if (ownerMatchMember == null) return;

        switch (ownerMatchMember.TeamID)
        {
            case TeamSide.TeamRed: { red--; break; }
            case TeamSide.TeamBlue: { blue--; break; }
        }

        if (red == 0)
        {
            winTeamID = 1;
            triggered = true;
        }

        if (blue == 0)
        {
            winTeamID = 0;
            triggered = true;
        }
    }

    public void Reset()
    {
        red = 0;
        blue = 0;
        winTeamID = -1;

        triggered = false;
    }
}
