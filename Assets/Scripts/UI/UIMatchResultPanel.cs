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

        IsVisible(false);
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
        IsVisible(false);
        text.text = "";
    }

    private void OnMatchEnd()
    {
        IsVisible(true);

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

    public void IsVisible(bool isVisible)
    {
        if (resultPanel == null) return;

        resultPanel.SetActive(isVisible);
    }
}
