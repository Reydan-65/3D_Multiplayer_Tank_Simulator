using UnityEngine;
using UnityEngine.UI;

public class UIMinimap : MonoBehaviour
{
    [SerializeField] private SizeMap sizeMap;
    [SerializeField] private UIVehicleMark vehicleMarkPrefab;
    [SerializeField] private Image minimap;

    private UIVehicleMark[] vehicleMarks;
    private Player[] players;

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
        players = FindObjectsByType<Player>(FindObjectsSortMode.None);

        vehicleMarks = new UIVehicleMark[players.Length];

        for (int i = 0; i < vehicleMarks.Length; i++)
        {
            vehicleMarks[i] = Instantiate(vehicleMarkPrefab);

            if (players[i].TeamID == Player.Local.TeamID)
                vehicleMarks[i].SetColor(0);
            else
                vehicleMarks[i].SetColor(1);

            vehicleMarks[i].transform.SetParent(minimap.transform);
        }
    }

    private void OnMatchEnd()
    {
        for (int i = 0; i < minimap.transform.childCount; i++)
        {
            Destroy(minimap.transform.GetChild(i).gameObject);
        }

        vehicleMarks = null;
    }

    private void Update()
    {
        if (vehicleMarks == null) return;

        for (int i = 0; i < vehicleMarks.Length; i++)
        {
            if (players[i] == null) continue;
            if (players[i].ActiveVehicle == null) continue;

            Vector3 normPos = sizeMap.GetNormalizePosition(players[i].ActiveVehicle.transform.position);
            Vector3 posInMinimap = new Vector3(normPos.x * minimap.rectTransform.sizeDelta.x * 0.5f, normPos.z * minimap.rectTransform.sizeDelta.y * 0.5f, 0);

            vehicleMarks[i].transform.position = minimap.transform.position + posInMinimap;
        }
    }
}
