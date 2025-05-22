using System;
using System.Collections.Generic;
using UnityEngine;

public class UIMatchMemberList : MonoBehaviour
{
    [SerializeField] private Transform localTeamPanel;
    [SerializeField] private Transform otherTeamPanel;

    [SerializeField] private UIMatchMemberLable[] matchMemberLablePrefab;

    private List<UIMatchMemberLable> allMatchMemberLable = new List<UIMatchMemberLable>();

    private void Start()
    {
        MatchMemberList.UpdateList += OnUpdateMatchMemberList;
        MatchMember.ChangeFragsAmount += OnChangeFragsAmount;
    }

    private void OnDestroy()
    {
        MatchMemberList.UpdateList -= OnUpdateMatchMemberList;
        MatchMember.ChangeFragsAmount -= OnChangeFragsAmount;
    }

    private void OnChangeFragsAmount(MatchMember member, int frags)
    {
        for (int i = 0; i < allMatchMemberLable.Count; i++)
        {
            if (allMatchMemberLable[i].NetID == member.netId)
            {
                allMatchMemberLable[i].UpdateFrags(frags);
            }
        }
    }

    private void OnUpdateMatchMemberList(List<MatchMemberData> matchMemberData)
    {
        for (int i = 0; i < localTeamPanel.childCount; i++)
        {
            Destroy(localTeamPanel.GetChild(i).gameObject);
        }

        for (int i = 0; i < otherTeamPanel.childCount; i++)
        {
            Destroy(otherTeamPanel.GetChild(i).gameObject);
        }

        allMatchMemberLable.Clear();

        for (int i = 0; i < matchMemberData.Count; i++)
        {
            if (matchMemberData[i].TeamID == Player.Local.TeamID)
            {
                AddMatchMemberLable(matchMemberData[i], matchMemberLablePrefab[0], localTeamPanel);
            }

            if (matchMemberData[i].TeamID != Player.Local.TeamID)
            {
                AddMatchMemberLable(matchMemberData[i], matchMemberLablePrefab[1], otherTeamPanel);
            }
        }
    }

    private void AddMatchMemberLable(MatchMemberData data, UIMatchMemberLable matchMemberLable, Transform parent)
    {
        UIMatchMemberLable l = Instantiate(matchMemberLable);

        l.transform.SetParent(parent);
        l.Init(data.ID, data.Nickname, data.Member);

        if (data.Member != null)
        {
            MatchMember m = data.Member.GetComponent<MatchMember>();
            if (m != null)
            {
                if (m.ActiveVehicle != null)
                    m.ActiveVehicle.Destroyed += OnVehicleDestroyed;
            }
        }

        allMatchMemberLable.Add(l);
    }

    private void OnVehicleDestroyed(Destructible destructible)
    {
        for (int i = 0; i < allMatchMemberLable.Count; i++)
        {
            if (allMatchMemberLable[i].MatchMember != null &&
                allMatchMemberLable[i].MatchMember.ActiveVehicle != null &&
                allMatchMemberLable[i].MatchMember.ActiveVehicle == destructible)
            {
                allMatchMemberLable[i].UpdateTextColor();
            }
        }
    }
}
