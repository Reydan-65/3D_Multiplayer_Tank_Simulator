using UnityEngine;
using Mirror;
using System.Collections.Generic;

[RequireComponent(typeof(Vehicle))]
public class VehicleViewer : NetworkBehaviour
{
    private const float UPDATE_INTERVAL = 0.33f;
    private const float X_RAY_DISTANCE = 50f;
    private const float CAMOUFLAGE_DISTANCE = 150f;
    private const float TIME_TO_EXIT_DISCOVERY = 10f;

    [SerializeField] private float viewDistance;
    [SerializeField] private Transform[] viewPoints;

    private Vehicle vehicle;
    private float remainingTimeLastUpdate;

    private List<VehicleDimensions> allVehicleDimensions = new List<VehicleDimensions>();

    private readonly SyncList<NetworkIdentity> visibleVehicles = new SyncList<NetworkIdentity>();
    private readonly SyncList<float> remainingTime = new SyncList<float>();

    [SyncVar(hook = nameof(OnHiddenChanged))]
    private bool isHidden = false;

    public bool IsHidden => isHidden;

    private void OnHiddenChanged(bool oldValue, bool newValue)
    {
        isHidden = newValue;
    }

    [SyncVar]
    private bool isDetected = false;
    public bool IsDetected { get => isDetected; set => isDetected = value; }

    // Debug
    [SerializeField] private Color color;

    public override void OnStartServer()
    {
        base.OnStartServer();

        vehicle = GetComponent<Vehicle>();

        if (NetworkSessionManager.Instance != null)
            NetworkSessionManager.Match.SvMatchStart += OnSvMatchStart;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        if (NetworkSessionManager.Instance != null)
            NetworkSessionManager.Match.SvMatchStart -= OnSvMatchStart;
    }

    private void OnDestroy()
    {
        if (NetworkSessionManager.Instance != null)
            NetworkSessionManager.Match.SvMatchStart -= OnSvMatchStart;
    }

    private void OnSvMatchStart()
    {
        color = Random.ColorHSV();

        Vehicle[] allVehicles = FindObjectsByType<Vehicle>(0);

        for (int i = 0; i < allVehicles.Length; i++)
        {
            if (vehicle == allVehicles[i]) continue;

            VehicleDimensions vd = allVehicles[i].GetComponent<VehicleDimensions>();

            if (vd == null) continue;

            if (vehicle.TeamID != allVehicles[i].TeamID)
            {
                allVehicleDimensions.Add(vd);
            }
            else
            {
                visibleVehicles.Add(vd.Vehicle.netIdentity);
                remainingTime.Add(-1);
            }
        }
    }

    private void Update()
    {
        if (isServer == false) return;

        remainingTimeLastUpdate += Time.deltaTime;
        if (remainingTimeLastUpdate >= UPDATE_INTERVAL)
        {
            for (int i = 0; i < allVehicleDimensions.Count; i++)
            {
                if (allVehicleDimensions[i].Vehicle == null) continue;

                bool isVisible = false;

                if (allVehicleDimensions[i].Vehicle.HitPoint <= 0)
                {
                    if (!visibleVehicles.Contains(allVehicleDimensions[i].Vehicle.netIdentity))
                    {
                        visibleVehicles.Add(allVehicleDimensions[i].Vehicle.netIdentity);
                        remainingTime.Add(-1);
                    }
                    continue;
                }

                foreach (Transform viewPoint in viewPoints)
                {
                    isVisible = allVehicleDimensions[i].IsVisibleFromPoint(viewPoint,
                                                                           viewPoint.position,
                                                                           color,
                                                                           viewDistance,
                                                                           X_RAY_DISTANCE,
                                                                           CAMOUFLAGE_DISTANCE);

                    if (isVisible) break;
                }

                if (isVisible)
                {
                    allVehicleDimensions[i].Vehicle.netIdentity.transform.root.GetComponent<VehicleViewer>().IsDetected = true;

                    int index = visibleVehicles.IndexOf(allVehicleDimensions[i].Vehicle.netIdentity);
                    if (index == -1)
                    {
                        visibleVehicles.Add(allVehicleDimensions[i].Vehicle.netIdentity);
                        remainingTime.Add(-1);
                    }
                    else
                    {
                        remainingTime[index] = -1;
                    }
                }
                else if (!isVisible && visibleVehicles.Contains(allVehicleDimensions[i].Vehicle.netIdentity))
                {
                    int index = visibleVehicles.IndexOf(allVehicleDimensions[i].Vehicle.netIdentity);
                    if (remainingTime[index] == -1)
                        remainingTime[index] = TIME_TO_EXIT_DISCOVERY;
                }
            }

            for (int i = remainingTime.Count - 1; i >= 0; i--)
            {
                if (remainingTime[i] > 0)
                {
                    remainingTime[i] -= (Time.deltaTime + UPDATE_INTERVAL);
                    if (remainingTime[i] <= 0)
                        remainingTime[i] = 0;
                }

                if (remainingTime[i] == 0)
                {
                    if (i < visibleVehicles.Count)
                    {
                        var vehicleViewer = visibleVehicles[i].transform.root.GetComponent<VehicleViewer>();
                        if (vehicleViewer != null)
                            vehicleViewer.IsDetected = false;
                    }

                    remainingTime.RemoveAt(i);
                    visibleVehicles.RemoveAt(i);
                }
            }

            remainingTimeLastUpdate = 0;
        }
    }

    public bool IsVisible(NetworkIdentity identity)
    {
        return visibleVehicles.Contains(identity);
    }

    [Server]
    public void SvSetHidden(bool hidden)
    {
        isHidden = hidden;
    }

    public List<Vehicle> GetAllVehicles()
    {
        List<Vehicle> allVehicles = new List<Vehicle>(allVehicleDimensions.Count);

        for (int i = 0; i < allVehicleDimensions.Count; i++)
            allVehicles.Add(allVehicleDimensions[i].Vehicle);

        return allVehicles;
    }

    public List<Vehicle> GetAllVisiblelVehicles()
    {
        List<Vehicle> allVehicles = new List<Vehicle>(visibleVehicles.Count);

        for (int i = 0; i < visibleVehicles.Count; i++)
            allVehicles.Add(visibleVehicles[i].GetComponent<Vehicle>());

        return allVehicles;
    }
}
