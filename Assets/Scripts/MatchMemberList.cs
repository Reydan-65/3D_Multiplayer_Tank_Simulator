using Mirror;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class MatchMemberList : NetworkBehaviour
{
    public static MatchMemberList Instance;

    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private List<MatchMemberData> allMemberData = new List<MatchMemberData>();
    public int MemberDataCount => allMemberData.Count;

    public static UnityAction<List<MatchMemberData>> UpdateList;

    public override void OnStartClient()
    {
        base.OnStartClient();
        allMemberData.Clear();
    }

    [Server]
    public void SvAddMember(MatchMemberData data)
    {
        if (!allMemberData.Exists(p => p.ID == data.ID))
        {
            allMemberData.Add(data);
            RpcClearList();

            for (int i = 0; i < allMemberData.Count; i++)
                RpcAddMember(allMemberData[i]);
        }
    }

    [Server]
    public void SvRemoveMember(MatchMemberData data)
    {
        for (int i = 0; i < allMemberData.Count; i++)
        {
            if (allMemberData[i].ID == data.ID)
            {
                allMemberData.RemoveAt(i);
                break;
            }
        }

        if (isClient)
            RpcRemoveMember(data);
    }

    [ClientRpc]
    private void RpcClearList()
    {
        // Check host
        if (isServer) return;

        allMemberData.Clear();
    }

    [ClientRpc]
    private void RpcAddMember(MatchMemberData data)
    {
        if (!allMemberData.Exists(p => p.ID == data.ID))
        {
            allMemberData.Add(data);
        }

        UpdateList?.Invoke(allMemberData);
    }

    [ClientRpc]
    private void RpcRemoveMember(MatchMemberData data)
    {
        if (!isClient) return;

        for (int i = 0; i < allMemberData.Count; i++)
        {
            if (allMemberData[i].ID == data.ID)
            {
                allMemberData.RemoveAt(i);
                break;
            }
        }

        UpdateList?.Invoke(allMemberData);
    }
}
