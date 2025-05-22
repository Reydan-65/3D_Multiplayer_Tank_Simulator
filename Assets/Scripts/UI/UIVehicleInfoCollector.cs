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
        mainCamera = Camera.main;

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
        ClearVehicleInfos();

        Vehicle[] vehicle = FindObjectsByType<Vehicle>(0);
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
        ClearVehicleInfos();
    }

    private void ClearVehicleInfos()
    {
        if (vehicleInfos != null)
        {
            foreach (var vehicleInfo in vehicleInfos)
            {
                if (vehicleInfo != null)
                    Destroy(vehicleInfo.gameObject);
            }
            vehicleInfos = null;
        }

        vehiclesWithoutLocal?.Clear();
    }

    private void LateUpdate()
    {
        if (vehicleInfos == null || Player.Local == null) return;

        for (int i = 0; i < vehicleInfos.Length; i++)
        {
            if (vehicleInfos[i] == null || vehicleInfos[i].Vehicle == null)
            {
                if (vehicleInfos[i] != null)
                    vehicleInfos[i].gameObject.SetActive(false);
                continue;
            }

            bool shouldBeActive = vehicleInfos[i].Vehicle.HitPoint > 0 &&
                                Player.Local.ActiveVehicle != null &&
                                Player.Local.ActiveVehicle.Viewer != null &&
                                Player.Local.ActiveVehicle.Viewer.IsVisible(vehicleInfos[i].Vehicle.netIdentity);

            vehicleInfos[i].gameObject.SetActive(shouldBeActive);

            if (shouldBeActive && mainCamera != null)
            {
                Vector3 screenPos = mainCamera.WorldToScreenPoint(vehicleInfos[i].Vehicle.transform.position + vehicleInfos[i].WorldOffset);
                if (screenPos.z > 0)
                    vehicleInfos[i].transform.position = screenPos;
            }
        }
    }
}
