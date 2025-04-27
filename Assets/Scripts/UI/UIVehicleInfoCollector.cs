using System.Collections.Generic;
using UnityEngine;

public class UIVehicleInfoCollector : MonoBehaviour
{
    [SerializeField] private Transform vehicleInfoPanel;
    [SerializeField] private UIVehicleInfo vehicleInfoPrefab;

    private UIVehicleInfo[] vehicleInfos;
    private List<Player> playersWithoutLocal;

    private Camera mainCamera;
    private void Start()
    {
        if (NetworkSessionManager.Instance != null)
        {
            NetworkSessionManager.Match.MatchStart += OnMatchStart;
            NetworkSessionManager.Match.MatchEnd += OnMatchEnd;
        }

        mainCamera = Camera.main;
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
        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);

        playersWithoutLocal = new List<Player>(players.Length - 1);

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == Player.Local) continue;

            playersWithoutLocal.Add(players[i]);
        }

        vehicleInfos = new UIVehicleInfo[playersWithoutLocal.Count];

        for (int i = 0; i < vehicleInfos.Length; i++)
        {
            vehicleInfos[i] = Instantiate(vehicleInfoPrefab);
            vehicleInfos[i].SetVehicle(playersWithoutLocal[i].ActiveVehicle);
            vehicleInfos[i].transform.SetParent(vehicleInfoPanel);
        }
    }

    private void OnMatchEnd()
    {
        for (int i = 0; i < vehicleInfos.Length; i++)
            Destroy(vehicleInfoPanel.transform.GetChild(i).gameObject);

        vehicleInfos = null;
    }

    private void LateUpdate()
    {
        if (vehicleInfos == null) return;

        for (int i = 0; i < vehicleInfos.Length; i++)
        {
            if (vehicleInfos[i] == null) continue;
            if (vehicleInfos[i].Vehicle == null) continue;

            Vector3 screenPos = mainCamera.WorldToScreenPoint(vehicleInfos[i].Vehicle.transform.position + vehicleInfos[i].WorldOffset);

            if (screenPos.z > 0)
                vehicleInfos[i].transform.position = screenPos;
        }
    }
}
