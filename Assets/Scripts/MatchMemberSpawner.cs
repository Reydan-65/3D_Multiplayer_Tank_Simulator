using UnityEngine;
using Mirror;

public class MatchMemberSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject botPrefab;
    [Range(0, 15)]
    [SerializeField] private int targetAmountMemberTeam;

    [Server]
    public void SvRespawnVehicleAllMember()
    {
        SvRespawnPlayerVehicle();
        SvRespawnBotVehicle();
    }

    [Server]
    private void SvRespawnPlayerVehicle()
    {
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
    }

    [Server]
    private void SvRespawnBotVehicle()
    {
        var bots = FindObjectsByType<Bot>(FindObjectsSortMode.None);

        foreach (var b in bots)
        {
            NetworkServer.UnSpawn(b.gameObject);
            Destroy(b.gameObject);
        }

        int botAmount = targetAmountMemberTeam * 2 - MatchMemberList.Instance.MemberDataCount;

        for (int i = 0; i < botAmount; i++)
        {
            GameObject b = Instantiate(botPrefab);
            NetworkServer.Spawn(b);
        }
    }
}
