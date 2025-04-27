using TMPro;
using UnityEngine;

public class UIMatchResultPanel : MonoBehaviour
{
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI text;

    private void Start()
    {
        if (NetworkSessionManager.Instance != null)
        {
            NetworkSessionManager.Match.MatchStart += OnMatchStart;
            NetworkSessionManager.Match.MatchEnd += OnMatchEnd;
        }
    }

    private void OnDestroy()
    {
        if (NetworkSessionManager.Instance != null)
        {
            NetworkSessionManager.Match.MatchStart -= OnMatchStart;
            NetworkSessionManager.Match.MatchEnd -= OnMatchEnd;
        }
    }

    private void OnMatchStart()
    {
        resultPanel.SetActive(false);
    }

    private void OnMatchEnd()
    {
        resultPanel.SetActive(true);

        int winTeamID = NetworkSessionManager.Match.WinTeamID;

        if (winTeamID == Player.Local.TeamID)
        {
            text.color = Color.green;
            text.text = "Победа!";
        }
        else
        {
            text.color = Color.red;
            text.text = "Поражение!";
        }

        if (winTeamID == -1)
        {
            text.color = Color.yellow;
            text.text = "Ничья!";
        }
    }
}
