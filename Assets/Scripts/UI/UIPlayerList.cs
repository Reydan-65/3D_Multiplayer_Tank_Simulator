using System.Collections.Generic;
using UnityEngine;

public class UIPlayerList : MonoBehaviour
{
    [SerializeField] private Transform localTeamPanel;
    [SerializeField] private Transform otherTeamPanel;

    [SerializeField] private UIPlayerLable[] playerLablePrefab;

    private List<UIPlayerLable> allPlayerLable = new List<UIPlayerLable>();

    private void Start()
    {
        PlayerList.UpdatePlayerList += OnUpdatePlayerList;
        Player.ChangeFrags += OnChangeFrags;
    }

    private void OnDestroy()
    {
        PlayerList.UpdatePlayerList -= OnUpdatePlayerList;
        Player.ChangeFrags -= OnChangeFrags;
    }

    private void OnChangeFrags(int playerNetId, int frags)
    {
        for (int i = 0; i < allPlayerLable.Count; i++)
        {
            if (allPlayerLable[i].NetID == playerNetId)
            {
                allPlayerLable[i].UpdateFrags(frags);
            }
        }
    }

    private void OnUpdatePlayerList(List<PlayerData> playerData)
    {
        for (int i = 0; i < localTeamPanel.childCount; i++)
        {
            Destroy(localTeamPanel.GetChild(i).gameObject);
        }

        for (int i = 0; i < otherTeamPanel.childCount; i++)
        {
            Destroy(otherTeamPanel.GetChild(i).gameObject);
        }

        allPlayerLable.Clear();

        for (int i = 0; i < playerData.Count; i++)
        {
            if (playerData[i].TeamID == Player.Local.TeamID)
            {
                AddPlayerLable(playerData[i], playerLablePrefab[0], localTeamPanel);
            }

            if (playerData[i].TeamID != Player.Local.TeamID)
            {
                AddPlayerLable(playerData[i], playerLablePrefab[1], otherTeamPanel);
            }
        }
    }

    private void AddPlayerLable(PlayerData data, UIPlayerLable playerLable, Transform parent)
    {
        UIPlayerLable l = Instantiate(playerLable);

        l.transform.SetParent(parent);
        l.Init(data.ID, data.Nickname);

        allPlayerLable.Add(l);
    }
}
