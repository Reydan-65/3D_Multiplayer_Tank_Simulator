using UnityEngine;
using UnityEngine.UI;

public class UIMinimap : MonoBehaviour
{
    [SerializeField] private Transform mainCanvas;
    [SerializeField] private SizeMap sizeMap;
    [SerializeField] private UIVehicleMark vehicleMarkPrefab;
    [SerializeField] private Image minimap;

    private UIVehicleMark[] vehicleMarks;
    private Vehicle[] vehicles;

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
        vehicles = FindObjectsByType<Vehicle>(FindObjectsSortMode.None);

        vehicleMarks = new UIVehicleMark[vehicles.Length];

        for (int i = 0; i < vehicleMarks.Length; i++)
        {
            vehicleMarks[i] = Instantiate(vehicleMarkPrefab);

            if (vehicles[i].TeamID == Player.Local.TeamID)
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
            if (vehicles[i] == null) continue;

            if (vehicles[i].HitPoint <= 0)
            {
                vehicles[i].gameObject.SetActive(false);
                continue;
            }

            if (vehicles[i] != Player.Local.ActiveVehicle && vehicles[i].TeamID != Player.Local.ActiveVehicle.TeamID)
            {
                bool isVisible = Player.Local.ActiveVehicle.Viewer.IsVisible(vehicles[i].netIdentity);

                vehicleMarks[i].gameObject.SetActive(isVisible);
            }

            if (vehicleMarks[i].gameObject.activeSelf == false) continue;

            Vector3 normPos = sizeMap.GetNormalizePosition(vehicles[i].transform.position);
            Vector3 posInMinimap = new Vector3(normPos.x * minimap.rectTransform.sizeDelta.x * 0.5f, normPos.z * minimap.rectTransform.sizeDelta.y * 0.5f, 0);
            posInMinimap.x *= mainCanvas.localScale.x;
            posInMinimap.y *= mainCanvas.localScale.y;
            vehicleMarks[i].transform.position = minimap.transform.position + posInMinimap;
        }
    }
}
