using Mirror;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class MatchMemberData
{
    public int ID;
    public int TeamID;
    public string Nickname;
    public NetworkIdentity Member;

    public MatchMemberData(int id, int teamID, string nickname, NetworkIdentity member)
    {
        ID = id;
        TeamID = teamID;
        Nickname = nickname;
        Member = member;
    }
}

public static class MatchMemberDataExtention
{
    public static void WriteMatchMemberData(this NetworkWriter writer, MatchMemberData memberData)
    {
        writer.WriteInt(memberData.ID);
        writer.WriteInt(memberData.TeamID);
        writer.WriteString(memberData.Nickname);
        writer.WriteNetworkIdentity(memberData.Member);
    }

    public static MatchMemberData ReadMemberData(this NetworkReader reader)
    {
        int id = reader.ReadInt();
        int teamID = reader.ReadInt();
        string nickname = reader.ReadString();
        NetworkIdentity member = reader.ReadNetworkIdentity();

        return new MatchMemberData(id, teamID, nickname, member);
    }
}

public class MatchMember : NetworkBehaviour
{
    public static UnityAction<MatchMember, int> ChangeFragsAmount;
    public event UnityAction<ProjectileHitResult> ProjectileHit;

    public Vehicle ActiveVehicle { get; set; }

    #region Data

    protected MatchMemberData data;
    public MatchMemberData Data => data;


    [Command]
    protected void CmdUpdateData(MatchMemberData data)
    {
        this.data = data;
    }

    #endregion

    #region Frags

    [SyncVar(hook = nameof(OnFragsAmountChanged))]
    protected int fragsAmount;
    
    protected void OnFragsAmountChanged(int oldValue, int newValue)
    {
        ChangeFragsAmount?.Invoke(this, newValue);
    }

    [Server]
    public void SvResetFragsAmount()
    {
        fragsAmount = 0;
    }

    [Server]
    public void SvAddFrags(int count)
    {
        fragsAmount += count;

        ChangeFragsAmount?.Invoke(this, fragsAmount);
    }

    #endregion

    #region Nickname

    [SyncVar(hook = nameof(OnNicknameChanged))]
    protected string nickname;

    protected void OnNicknameChanged(string oldName, string newName)
    {
        gameObject.name = "Player_" + newName; // on Client
    }


    [Command] // on Server
    public void CmdSetName(string name)
    {
        nickname = name;
        gameObject.name = name;
    }

    #endregion

    #region TeamID

    [SyncVar]
    protected int teamID;
    public int TeamID => teamID;

    #endregion

    #region ProjectileHit

    [Command]
    public void CmdRegisterProjectileHit(
    ProjectileType projectileType,
    float directDamage,
    float explosionDamage,
    Vector3 hitPoint,
    Destructible target,
    ProjectileHitType hitType)
    {
        // Сервер уже обработал урон в HandleServerHit,
        // поэтому здесь можно только нотифицировать клиентов
        bool isVisible = ActiveVehicle.transform.root.GetComponent<VehicleViewer>().IsVisible(target.netIdentity);
        RpcInvokeProjectileHit(hitType, directDamage, explosionDamage, hitPoint, isVisible, projectileType, target.netIdentity);
    }


    [ClientRpc]
    private void RpcInvokeProjectileHit(
    ProjectileHitType hitType,
    float directDamage,
    float explosionDamage,
    Vector3 point,
    bool isVisible,
    ProjectileType projectileType,
    NetworkIdentity identity)
    {
        //Debug.Log($"RpcInvokeProjectileHit: type={hitType}, directDamage={directDamage}, explosionDamage={explosionDamage}, projectileType={projectileType}");

        ProjectileHitResult hitResult = new ProjectileHitResult(
            hitType,
            directDamage,
            explosionDamage,
            point,
            isVisible,
            projectileType,
            identity
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
            Instantiate(prefab, position, rotation);
    }

    #endregion
}
