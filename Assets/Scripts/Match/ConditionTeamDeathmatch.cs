using Mirror;
using System;
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

        var players = FindObjectsByType<Player>(FindObjectsSortMode.None);

        foreach (var p in players)
        {
            if (p.ActiveVehicle != null)
            {
                p.ActiveVehicle.Destroyed += OnVehicleDestroyed;

                if (p.TeamID == TeamSide.TeamRed) red++;
                else if (p.TeamID == TeamSide.TeamBlue) blue++;
            }
        }
    }

    public void OnServerMatchEnd(MatchController controller) { }

    private void OnVehicleDestroyed(Destructible dest)
    {
        Vehicle v = (dest as Vehicle);

        if (v == null) return;

        var ownerPlayer = v.Owner?.GetComponent<Player>();

        if (ownerPlayer == null) return;

        switch (ownerPlayer.TeamID)
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

    private void Reset()
    {
        red = 0;
        blue = 0;
        triggered = false;
    }
}
