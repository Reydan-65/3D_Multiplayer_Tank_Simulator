using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class Player : MatchMember
{
    public event UnityAction<Vehicle> VehicleSpawned;

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
    [SerializeField] private VehicleInputController vehicleInputController;
    [SerializeField] private float m_SpawnSpace;

    private Transform[] m_SpawnPoints;
    private UIAmmunitionPanel ammunitionPanel;

    public override void OnStartServer()
    {
        base.OnStartServer();

        teamID = MatchController.GetNextTeam();

        GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("Spawn Point");

        if (spawnPointObjects.Length > 0)
        {
            m_SpawnPoints = new Transform[spawnPointObjects.Length];

            for (int i = 0; i < spawnPointObjects.Length; i++)
                m_SpawnPoints[i] = spawnPointObjects[i].transform;
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        MatchMemberList.Instance.SvRemoveMember(data);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isOwned)
        {
            string nickname = NetworkSessionManager.Instance.GetComponent<NetworkManagerHUD>().PlayerName;

            CmdSetName(nickname);

            NetworkSessionManager.Match.MatchStart += OnMatchStart;
            NetworkSessionManager.Match.MatchEnd += OnMatchEnd;

            data = new MatchMemberData((int)netId, teamID, nickname, netIdentity);

            CmdAddPlayer(data);
            CmdUpdateData(data);
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        if (isLocalPlayer && ActiveVehicle != null)
        {
            var destructible = ActiveVehicle.GetComponent<Destructible>();
        }

        if (NetworkSessionManager.Match != null)
        {
            NetworkSessionManager.Match.MatchStart -= OnMatchStart;
            NetworkSessionManager.Match.MatchEnd -= OnMatchEnd;
        }
    }

    private void OnMatchStart()
    {
        vehicleInputController.enabled = true;
    }

    private void OnMatchEnd()
    {
        if (ActiveVehicle != null)
        {
            ActiveVehicle.SetTargetControl(Vector3.zero);
            vehicleInputController.enabled = false;
        }
    }

    

    [Command]
    private void CmdAddPlayer(MatchMemberData data)
    {
        MatchMemberList.Instance.SvAddMember(data);
    }

    private void Update()
    {
        if (isLocalPlayer)
        {
            if (ActiveVehicle != null)
                ActiveVehicle.SetVisible(!VehicleCamera.Instance.IsZoom);
        }

        if (isServer)
        {
            if (Input.GetKeyDown(KeyCode.F3))
                NetworkSessionManager.Match.SvRestartMatch();
        }

        if (isLocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                if (Cursor.lockState != CursorLockMode.Locked)
                    Cursor.lockState = CursorLockMode.Locked;
                else
                    Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    [Server]
    public void SvSpawnClientVehicle()
    {
        if (ActiveVehicle != null) return;

        GameObject playerVehicle = Instantiate(m_VehiclePrefab[Random.Range(0, m_VehiclePrefab.Length)], transform.position, Quaternion.identity);

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
        ActiveVehicle.TeamID = teamID;
        vehicleInputController.enabled = false;

        RpcSetClientActiveVehicle(ActiveVehicle.netIdentity);
    }

    [ClientRpc]
    private void RpcSetClientActiveVehicle(NetworkIdentity vehicle)
    {
        if (vehicle == null) return;

        ActiveVehicle = vehicle.GetComponent<Vehicle>();

        if (ActiveVehicle == null) return;

        ActiveVehicle.Owner = netIdentity;
        ActiveVehicle.TeamID = teamID;

        if (ActiveVehicle != null && isLocalPlayer)
        {
            var destructible = ActiveVehicle.GetComponent<Destructible>();

            if (destructible != null)
            {
                if (destructible.HitPoint <= 0) HandleVehicleDeath();
            }

            if (VehicleCamera.Instance != null)
                VehicleCamera.Instance.SetTarget(ActiveVehicle);

            UIVisibilityStatus vs = FindAnyObjectByType<UIVisibilityStatus>();
            vs.SetTarget(ActiveVehicle);
        }

        vehicleInputController.enabled = false;

        VehicleSpawned?.Invoke(ActiveVehicle);
    }

    public void HandleVehicleDeath()
    {
        if (!isLocalPlayer) return;

        if (VehicleCamera.Instance != null)
            VehicleCamera.Instance.SetTarget(null);
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