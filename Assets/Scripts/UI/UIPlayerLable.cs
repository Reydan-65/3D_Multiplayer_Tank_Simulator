using TMPro;
using UnityEngine;

public class UIPlayerLable : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI fragText;
    [SerializeField] private Color selfColor;

    private int netID;
    public int NetID => netID;

    public void Init(int netID, string nickname)
    {
        this.netID = netID;
        nicknameText.text = nickname;
        fragText.text = "0";

        if (netID == Player.Local.netId)
            nicknameText.color = selfColor;
        else
            nicknameText.color = Color.white;
    }

    public void UpdateFrags(int frag)
    {
        fragText.text = frag.ToString();
    }
}
