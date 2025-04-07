using Mirror;
using UnityEngine;

public class NetworkSessionManager : NetworkManager
{
    [SerializeField] private SphereArea[] spawnZonesRed;
    [SerializeField] private SphereArea[] spawnZonesBlue;

    public Vector3 RandomSpawnPointRed => spawnZonesRed[Random.Range(0, spawnZonesRed.Length)].RandomInside;
    public Vector3 RandomSpawnPointBlue => spawnZonesBlue[Random.Range(0, spawnZonesBlue.Length)].RandomInside;

    public static NetworkSessionManager Instance => singleton as NetworkSessionManager;
    public static EventCollector Events => Instance.eventCollector;

    public bool IsServer => (mode == NetworkManagerMode.Host || mode == NetworkManagerMode.ServerOnly);
    public bool IsClient => (mode == NetworkManagerMode.Host || mode == NetworkManagerMode.ClientOnly);

    [SerializeField] private EventCollector eventCollector;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        eventCollector.SvOnAddplayer();
    }
}
