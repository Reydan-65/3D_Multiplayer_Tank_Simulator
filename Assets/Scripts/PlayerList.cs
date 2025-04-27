using Mirror;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class PlayerList : NetworkBehaviour
{
    public static PlayerList Instance;

    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private List<PlayerData> allPlayerData = new List<PlayerData>();
    public static UnityAction<List<PlayerData>> UpdatePlayerList;

    public override void OnStartClient()
    {
        base.OnStartClient();
        allPlayerData.Clear();
    }

    [Server]
    public void SvAddPlayer(PlayerData data)
    {
        if (!allPlayerData.Exists(p => p.ID == data.ID))
        {
            allPlayerData.Add(data);
            RpcClearPlayerDataList();

            for (int i = 0; i < allPlayerData.Count; i++)
                RpcAddPlayer(allPlayerData[i]);
        }
    }

    [Server]
    public void SvRemovePlayer(PlayerData data)
    {
        for (int i = 0; i < allPlayerData.Count; i++)
        {
            if (allPlayerData[i].ID == data.ID)
            {
                allPlayerData.RemoveAt(i);
                break;
            }
        }

        RpcRemovePlayer(data);
    }

    [ClientRpc]
    private void RpcClearPlayerDataList()
    {
        // Check host
        if (isServer) return;

        allPlayerData.Clear();
    }

    [ClientRpc]
    private void RpcAddPlayer(PlayerData data)
    {
        if (!allPlayerData.Exists(p => p.ID == data.ID))
        {
            allPlayerData.Add(data);
        }

        UpdatePlayerList?.Invoke(allPlayerData);
    }

    [ClientRpc]
    private void RpcRemovePlayer(PlayerData data)
    {
        for (int i = 0; i < allPlayerData.Count; i++)
        {
            if (allPlayerData[i].ID == data.ID)
            {
                allPlayerData.RemoveAt(i);
                break;
            }
        }

        UpdatePlayerList?.Invoke(allPlayerData);
    }

    /*
    // Public Methods
    public Player GetUserByNickname(PlayerData data)
    {
        if (AllPlayerData == null || AllPlayerData.Count == 0)
        {
            Debug.LogError("UserList is empty or not initialized.");
            return null;
        }

        foreach (var userData in AllPlayerData)
        {
            Debug.Log($"Checking user: {userData.Nickname} (ID: {userData.ID})");
            if (userData.Nickname.Equals(data.Nickname, StringComparison.OrdinalIgnoreCase))
            {
                if (NetworkServer.spawned.TryGetValue((uint)userData.ID, out NetworkIdentity identity))
                {
                    Player player = identity.GetComponent<Player>();
                    if (player != null)
                    {
                        Debug.Log($"User found: {player.Data.Nickname} (ID: {player.Data.ID})");
                        return player;
                    }
                    else
                        Debug.LogError($"User component not found for ID: {userData.ID}");
                }
                else
                    Debug.LogError($"NetworkIdentity not found for ID: {userData.ID}");
            }
        }

        Debug.LogError($"User with nickname {data.Nickname} not found.");
        return null;
    }

    public Player GetUserById(int userId)
    {
        foreach (var userData in AllPlayerData)
        {
            if (NetworkServer.spawned.TryGetValue((uint)userData.ID, out NetworkIdentity identity))
            {
                Player player = NetworkServer.spawned[(uint)userData.ID].GetComponent<Player>();

                if (player != null && player.Data.Id == userId)
                    return player;
            }
        }
        return null;
    }*/
}
