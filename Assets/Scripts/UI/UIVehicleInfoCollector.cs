using System.Collections.Generic;
using UnityEngine;

public class UIVehicleInfoCollector : MonoBehaviour
{
    [SerializeField] private Transform vehicleInfoPanel;
    [SerializeField] private UIVehicleInfo vehicleInfoPrefab;

    private UIVehicleInfo[] vehicleInfos;
    private List<Vehicle> vehiclesWithoutLocal;

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
        Vehicle[] vehicle = FindObjectsByType<Vehicle>(FindObjectsSortMode.None);

        vehiclesWithoutLocal = new List<Vehicle>(vehicle.Length - 1);

        for (int i = 0; i < vehicle.Length; i++)
        {
            if (vehicle[i] == Player.Local.ActiveVehicle) continue;

            vehiclesWithoutLocal.Add(vehicle[i]);
        }

        vehicleInfos = new UIVehicleInfo[vehiclesWithoutLocal.Count];

        for (int i = 0; i < vehicleInfos.Length; i++)
        {
            vehicleInfos[i] = Instantiate(vehicleInfoPrefab);
            vehicleInfos[i].SetVehicle(vehiclesWithoutLocal[i]);
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

            if (vehicleInfos[i].Vehicle.HitPoint <= 0)
            {
                vehicleInfos[i].gameObject.SetActive(false);
                continue;
            }

            if (Player.Local == null || Player.Local.ActiveVehicle == null || Player.Local.ActiveVehicle.Viewer == null) continue;

            bool isVisible = Player.Local.ActiveVehicle.Viewer.IsVisible(vehicleInfos[i].Vehicle.netIdentity);
            vehicleInfos[i].gameObject.SetActive(isVisible);

            if (!vehicleInfos[i].gameObject.activeSelf) continue;

            if (mainCamera == null) return;

            Vector3 screenPos = mainCamera.WorldToScreenPoint(vehicleInfos[i].Vehicle.transform.position + vehicleInfos[i].WorldOffset);

            if (screenPos.z > 0)
            {
                vehicleInfos[i].transform.position = screenPos;
            }
        }
    }

}
