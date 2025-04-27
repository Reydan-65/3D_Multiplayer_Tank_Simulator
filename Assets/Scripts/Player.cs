using UnityEngine;
using Mirror;
using UnityEngine.Events;

[System.Serializable]
public class PlayerData
{
    public int ID;
    public int TeamID;
    public string Nickname;

    public PlayerData(int id, int teamID, string nickname)
    {
        ID = id;
        TeamID = teamID;
        Nickname = nickname;
    }
}

public static class PlayerDataReaderWriter
{
    public static void WritePlayerData(this NetworkWriter writer, PlayerData playerData)
    {
        writer.WriteInt(playerData.ID);
        writer.WriteInt(playerData.TeamID);
        writer.WriteString(playerData.Nickname);
    }

    public static PlayerData ReadPlayerData(this NetworkReader reader)
    {
        int id = reader.ReadInt();
        int teamID = reader.ReadInt();
        string nickname = reader.ReadString();

        return new PlayerData(id, teamID, nickname);
    }
}

public class Player : NetworkBehaviour
{
    private static int TeamIDCounter;

    public static UnityAction<int, int> ChangeFrags;

    public event UnityAction<Vehicle> VehicleSpawned;
    public event UnityAction<ProjectileHitResult> ProjectileHit;


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

    public Vehicle ActiveVehicle { get; set; }

    [Header("Player")]
    [SyncVar(hook = nameof(OnNicknameChanged))]
    public string Nickname;

    private UIAmmunitionPanel ammunitionPanel;
    private PlayerData data;
    public PlayerData Data => data;

    [SyncVar]
    [SerializeField] private int teamID;
    public int TeamID => teamID;

    [SyncVar(hook = nameof(OnFragsChanged))]
    private int frags;
    public int Frags
    {
        set
        {
            frags = value;
            // Server
            ChangeFrags?.Invoke((int)netId, frags);
        }
        get { return frags; }
    }

    [Command]
    public void CmdRegisterProjectileHit(
    ProjectileType projectileType,
    float directDamage,
    float explosionDamage,
    Vector3 hitPoint,
    Destructible target,
    ArmorType armorType,
    ProjectileHitType hitType)
    {
        if (target == null) return;

        RpcInvokeProjectileHit(hitType, directDamage, explosionDamage, hitPoint, projectileType);
        
        if (armorType == ArmorType.None) return;

        SvProcessHit(target, directDamage, explosionDamage, armorType);
    }

    [Server]
    private void SvProcessHit(Destructible target, float directDamage, float explosionDamage, ArmorType armorType)
    {
        if (target == null)
            return;

        Destructible rootDestructible = target.transform.root.GetComponent<Destructible>();
        if (rootDestructible == null) return;

        float totalDamage = directDamage + explosionDamage;

        target.SvApplyDamage((int)totalDamage);

        if (armorType == ArmorType.Module)
            rootDestructible.SvApplyDamage((int)totalDamage);

        if (rootDestructible.HitPoint <= 0) 
            frags++;
    }

    // Client
    [ClientRpc]
    private void RpcInvokeProjectileHit(
    ProjectileHitType hitType,
    float directDamage,
    float explosionDamage,
    Vector3 point,
    ProjectileType projectileType)
    {
        //Debug.Log($"RpcInvokeProjectileHit: type={hitType}, directDamage={directDamage}, explosionDamage={explosionDamage}, projectileType={projectileType}");

        ProjectileHitResult hitResult = new ProjectileHitResult(
            hitType,
            directDamage,
            explosionDamage,
            point,
            null,
            projectileType
        );

        ProjectileHit?.Invoke(hitResult);
    }

    [Command]
    public void CmdSpawnImpactEffect(Vector3 position, Quaternion rotation, uint prefabId)
    {
        RpcSpawnImpactEffect(position, rotation, prefabId);
    }

    [ClientRpc]
    private void RpcSpawnImpactEffect(Vector3 position, Quaternion rotation, uint prefabId)
    {
        if (NetworkClient.prefabs.TryGetValue(prefabId, out GameObject prefab))
        {
            Instantiate(prefab, position, rotation);
        }
    }

    private void OnFragsChanged(int oldValue, int newValue)
    {
        ChangeFrags?.Invoke((int)netId, newValue);
    }

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
                m_SpawnPoints[i] = spawnPointObjects[i].transform;
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isOwned)
        {
            string nickname = NetworkSessionManager.Instance.GetComponent<NetworkManagerHUD>().PlayerName;

            CmdSetName(nickname);

            NetworkSessionManager.Match.MatchEnd += OnMatchEnd;

            data = new PlayerData((int)netId, teamID, nickname);

            CmdAddPlayer(data);
            CmdUpdateData(data);
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        PlayerList.Instance.SvRemovePlayer(data);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        if (isLocalPlayer && ActiveVehicle != null)
        {
            var destructible = ActiveVehicle.GetComponent<Destructible>();
        }
    }

    private void OnMatchEnd()
    {
        if (ActiveVehicle != null)
        {
            ActiveVehicle.SetTargetControl(Vector3.zero);
            vehicleInputController.enabled = false;
        }
    }

    private void OnNicknameChanged(string oldName, string newName)
    {
        gameObject.name = "Player_" + newName; // on Client
    }

    [Command]
    private void CmdAddPlayer(PlayerData data)
    {
        PlayerList.Instance.SvAddPlayer(data);
    }

    [Command]
    private void CmdUpdateData(PlayerData data)
    {
        this.data = data;
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

        RpcSetClientActiveVehicle(ActiveVehicle.netIdentity);
    }

    [ClientRpc]
    private void RpcSetClientActiveVehicle(NetworkIdentity vehicle)
    {
        if (vehicle == null) return;

        ActiveVehicle = vehicle.GetComponent<Vehicle>();
        ActiveVehicle.Owner = netIdentity;

        if (ActiveVehicle != null && isLocalPlayer)
        {
            var destructible = ActiveVehicle.GetComponent<Destructible>();

            if (destructible != null)
            {
                if (destructible.HitPoint <= 0) HandleVehicleDeath();
            }

            if (VehicleCamera.Instance != null)
                VehicleCamera.Instance.SetTarget(ActiveVehicle);
        }

        vehicleInputController.enabled = true;

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