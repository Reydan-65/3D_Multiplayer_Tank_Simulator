using UnityEngine;
using Mirror;
using System.Linq;

public class Player : NetworkBehaviour
{
    private static int TeamIDCounter;

    public static Player Local
    {
        get
        {
            var x = NetworkClient.localPlayer;
            if (x != null)
                return x.GetComponent<Player>();
            return null;
        }
    }

    [SerializeField] private GameObject[] m_VehiclePrefab;
    [SerializeField] private float m_SpawnSpace;

    private Transform[] m_SpawnPoints;

    public Vehicle ActiveVehicle { get; set; }

    [Header("Player")]
    [SyncVar(hook = nameof(OnNicknameChanged))]
    public string Nickname;

    [SyncVar]
    [SerializeField] private int teamID;
    public int TeamID => teamID;

    public override void OnStartServer()
    {
        base.OnStartServer();

        teamID = TeamIDCounter % 2;
        TeamIDCounter++;

        GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("Spawn Point");

        if (spawnPointObjects.Length > 0)
        {
            m_SpawnPoints = new Transform[spawnPointObjects.Length];

            for (int i = 0; i < spawnPointObjects.Length; i++)
            {
                m_SpawnPoints[i] = spawnPointObjects[i].transform;
            }
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isOwned)
        {
            CmdSetName(NetworkSessionManager.Instance.GetComponent<NetworkManagerHUD>().PlayerName);
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        if (isLocalPlayer && ActiveVehicle != null)
        {
            var destructible = ActiveVehicle.GetComponent<Destructible>();
            if (destructible != null)
            {
                destructible.OnDeath -= HandleVehicleDeath;
            }
        }
    }

    private void OnNicknameChanged(string oldName, string newName)
    {
        gameObject.name = "Player_" + newName; // on Client
    }

    [Command] // on Server
    public void CmdSetName(string name)
    {
        Nickname = name;
        gameObject.name = "Player_" + name;
    }

    [Command]
    public void CmdSetTeamID(int teamID)
    {
        this.teamID = teamID;
    }

    [System.Obsolete]
    private void Update()
    {
        if (isServer)
        {
            if (Input.GetKeyDown(KeyCode.F3))
            {
                foreach (var p in Object.FindObjectsOfType<Player>())
                {
                    if (p.ActiveVehicle != null)
                    {
                        NetworkServer.UnSpawn(p.ActiveVehicle.gameObject);
                        Destroy(p.ActiveVehicle.gameObject);

                        p.ActiveVehicle = null;
                    }
                }

                foreach (var p in FindObjectsOfType<Player>())
                {
                    p.SvSpawnClientVehicle();
                }
            }
        }

        if (isLocalPlayer)
        {
            if (Input.GetKey(KeyCode.V))
            {
                if (Cursor.lockState != CursorLockMode.Locked)
                    Cursor.lockState = CursorLockMode.Locked;
                else
                    Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    [Server]
    private void SvSpawnClientVehicle()
    {
        if (ActiveVehicle != null) return;

        /*
        if (m_SpawnPoints == null || m_SpawnPoints.Length == 0) return;

        Vector2 spawnPosition = Vector2.zero;
        bool foundFreePosition = false;

        var shuffledSpawnPoints = m_SpawnPoints.OrderBy(x => Random.value).ToArray();

        foreach (var spawnPoint in shuffledSpawnPoints)
        {
            if (spawnPoint == null) continue;

            if (Physics2D.OverlapCircle(spawnPoint.position, m_SpawnSpace) == null)
            {
                spawnPosition = spawnPoint.position;
                foundFreePosition = true;
                break;
            }
        }

        if (!foundFreePosition) return;
        */

        GameObject playerVehicle = Instantiate(m_VehiclePrefab[Random.Range(0, m_VehiclePrefab.Length)], /*spawnPosition*/ transform.position, Quaternion.identity);
        if (playerVehicle == null) return;

        playerVehicle.transform.position = teamID % 2 == 0 ?
            NetworkSessionManager.Instance.RandomSpawnPointRed :
            NetworkSessionManager.Instance.RandomSpawnPointBlue;

        playerVehicle.transform.rotation = teamID % 2 == 0 ?
            Quaternion.Euler(0, 180.0f, 0) :
            Quaternion.identity;

        NetworkServer.Spawn(playerVehicle, netIdentity.connectionToClient);

        ActiveVehicle = playerVehicle.GetComponent<Vehicle>();
        if (ActiveVehicle == null) return;

        ActiveVehicle.Owner = netIdentity;

        // Для хоста не подписываемся на сервере
        if (!isLocalPlayer)
        {
            var destructible = playerVehicle.GetComponent<Destructible>();
            if (destructible != null)
            {
                destructible.OnDeath += HandleVehicleDeath;
            }
        }

        RpcSetClientActiveVehicle(ActiveVehicle.netIdentity);
    }

    [ClientRpc]
    private void RpcSetClientActiveVehicle(NetworkIdentity vehicle)
    {
        if (vehicle == null) return;

        ActiveVehicle = vehicle.GetComponent<Vehicle>();

        if (ActiveVehicle != null && isLocalPlayer)
        {
            var destructible = ActiveVehicle.GetComponent<Destructible>();
            if (destructible != null)
            {
                destructible.OnDeath += HandleVehicleDeath;

                if (destructible.HitPoint <= 0)
                {
                    HandleVehicleDeath();
                }
            }

            if (VehicleCamera.Instance != null)
            {
                VehicleCamera.Instance.SetTarget(ActiveVehicle);
            }
        }
    }

    public void HandleVehicleDeath()
    {
        if (!isLocalPlayer) return;

        if (VehicleCamera.Instance != null)
        {
            VehicleCamera.Instance.SetTarget(null);
        }

        //if (UIHUD.Instance != null)
        //{
        //    UIHUD.Instance.SetOnMenuPanel();
        //}
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (m_SpawnSpace <= 0) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_SpawnSpace);
    }
#endif
}