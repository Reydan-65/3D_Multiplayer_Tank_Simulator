using UnityEngine;

public class Bot : MatchMember
{
    [SerializeField] private Vehicle vehicle;

    public override void OnStartServer()
    {
        base.OnStartServer();

        teamID = MatchController.GetNextTeam();

        nickname = "b_" + GetRandomName();

        data = new MatchMemberData((int)netId, teamID, nickname, netIdentity);

        transform.position = NetworkSessionManager.Instance.GetSpawnPointByTeam(teamID);

        ActiveVehicle = vehicle;
        ActiveVehicle.TeamID = teamID;
        ActiveVehicle.Owner = netIdentity;
        ActiveVehicle.name = nickname;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        if (MatchMemberList.Instance != null)
            MatchMemberList.Instance.SvRemoveMember(data);
    }

    private void Start()
    {
        if (isServer)
        {
            if (MatchMemberList.Instance != null)
                MatchMemberList.Instance.SvAddMember(data);
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();

        ActiveVehicle = vehicle;
        ActiveVehicle.TeamID = teamID;
        ActiveVehicle.Owner = netIdentity;
        ActiveVehicle.name = nickname;
    }

    private string GetRandomName()
    {
        string[] names = { "�������", "����", "�����", "����", "������",
            "������", "����", "�����", "�����", "�������", "���������",
            "�����", "����", "�����", "������", "������", "������",
            "������", "�������", "������", "�����", "������",
            "�������", "�������", "������", "�������", "������",
            "����", "�����", "�����", "������", "�����", "��������",
            "������", "�����", "�������", "�����", "����", "���", "�������" };

        return names[Random.Range(0, names.Length)];
    }
}
