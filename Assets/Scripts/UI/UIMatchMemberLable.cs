using Mirror;
using TMPro;
using UnityEngine;

public class UIMatchMemberLable : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI fragText;
    [SerializeField] private Color selfColor;
    [SerializeField] private Color selfDestroyedColor;

    private int netID;
    public int NetID => netID;
    private MatchMember matchMember;
    public MatchMember MatchMember => matchMember;

    public void Init(int netID, string nickname, NetworkIdentity identity)
    {
        this.netID = netID;
        nicknameText.text = nickname;
        fragText.text = "0";

        if (netID == Player.Local.netId)
            nicknameText.color = selfColor;
        else
            nicknameText.color = Color.white;

        matchMember = identity.GetComponent<MatchMember>();
    }

    public void UpdateTextColor()
    {
        if (netID == Player.Local.netId)
            nicknameText.color = selfDestroyedColor;
        else
            nicknameText.color = Color.gray;
    }

    public void UpdateFrags(int frag)
    {
        fragText.text = frag.ToString();
    }
}
