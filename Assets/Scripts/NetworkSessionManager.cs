using Mirror;
using UnityEngine;

public class NetworkSessionManager : NetworkManager
{
    [SerializeField] private SphereArea[] spawnZonesRed;
    [SerializeField] private SphereArea[] spawnZonesBlue;

    public Vector3 RandomSpawnPointRed => spawnZonesRed[Random.Range(0, spawnZonesRed.Length)].RandomPointInside;
    public Quaternion RandomSpawnRotationRed => spawnZonesRed[Random.Range(0, spawnZonesRed.Length)].transform.rotation;

    public Vector3 RandomSpawnPointBlue => spawnZonesBlue[Random.Range(0, spawnZonesBlue.Length)].RandomPointInside;
    public Quaternion RandomSpawnRotationBlue => spawnZonesBlue[Random.Range(0, spawnZonesBlue.Length)].transform.rotation;


    public static NetworkSessionManager Instance => singleton as NetworkSessionManager;
    public static EventCollector Events => Instance.eventCollector;
    public static MatchController Match => Instance.matchCollector;

    public bool IsServer => (mode == NetworkManagerMode.Host || mode == NetworkManagerMode.ServerOnly);
    public bool IsClient => (mode == NetworkManagerMode.Host || mode == NetworkManagerMode.ClientOnly);

    [SerializeField] private EventCollector eventCollector;
    [SerializeField] private MatchController matchCollector;

    public Vector3 GetSpawnPointByTeam(int teamID) => teamID % 2 == 0 ?
        RandomSpawnPointRed :
        RandomSpawnPointBlue;

    public Quaternion GetSpawnRotationByTeam(int teamID) => teamID % 2 == 0 ?
        RandomSpawnRotationRed :
        RandomSpawnRotationBlue;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        if (eventCollector != null)
            eventCollector.SvOnAddplayer();
    }
}
