using System.Collections.Generic;
using UnityEngine;

public class VehicleVisibleInCamera : MonoBehaviour
{
    private List<Vehicle> vehicles = new List<Vehicle>();

    private void Start()
    {
        if (NetworkSessionManager.Instance != null)
            NetworkSessionManager.Match.MatchStart += OnMatchStart;
    }


    private void OnDestroy()
    {
        if (NetworkSessionManager.Instance != null)
            NetworkSessionManager.Match.MatchStart -= OnMatchStart;
    }

    private void OnMatchStart()
    {
        vehicles.Clear();

        Vehicle[] allVehicles = FindObjectsByType<Vehicle>(0);

        for (int i = 0; i < allVehicles.Length; i++)
        {
            if (allVehicles[i] == Player.Local.ActiveVehicle) continue;

            vehicles.Add(allVehicles[i]);
        }
    }

    private void Update()
    {
        for (int i = 0;i < vehicles.Count;i++)
        {
            if (Player.Local == null) return;
            if (Player.Local.ActiveVehicle == null) return;
            if (Player.Local.ActiveVehicle.Viewer == null) return;
            if (vehicles[i] == null) return;

            bool isVisible = Player.Local.ActiveVehicle.Viewer.IsVisible(vehicles[i].netIdentity);

            vehicles[i].SetVisible(isVisible);
        }
    }
}
