using UnityEngine;
using Mirror;

public class TrackModule : NetworkBehaviour
{
    [Header("Visual")]
    [SerializeField] private GameObject leftTrackMesh;
    [SerializeField] private GameObject leftTrackMeshDestroyed;
    [SerializeField] private GameObject rightTrackMesh;
    [SerializeField] private GameObject rightTrackMeshDestroyed;

    [Space(5)]
    [SerializeField] private VehicleModule leftTrackModule;
    [SerializeField] private VehicleModule rightTrackModule;

    private TrackTank tank;

    private void Start()
    {
        tank = GetComponent<TrackTank>();

        leftTrackModule.Destroyed += OnLeftTrackDestroyed;
        rightTrackModule.Destroyed += OnRightTrackDestroyed;

        leftTrackModule.Recovered += OnLeftTrackRecovered;
        rightTrackModule.Recovered += OnRightTrackRecovered;
    }

    private void OnDestroy()
    {
        leftTrackModule.Destroyed -= OnLeftTrackDestroyed;
        rightTrackModule.Destroyed -= OnRightTrackDestroyed;

        leftTrackModule.Recovered -= OnLeftTrackRecovered;
        rightTrackModule.Recovered -= OnRightTrackRecovered;
    }

    private void OnLeftTrackDestroyed(Destructible dest)
    {
        if (!leftTrackMeshDestroyed.activeSelf)
            ChangeActiveObject(leftTrackMesh, leftTrackMeshDestroyed);

        TakeAwayMobility();
    }

    private void OnRightTrackDestroyed(Destructible dest)
    {
        if (!rightTrackMeshDestroyed.activeSelf)
            ChangeActiveObject(rightTrackMesh, rightTrackMeshDestroyed);

        TakeAwayMobility();
    }

    private void OnLeftTrackRecovered(Destructible dest)
    {
        ChangeActiveObject(leftTrackMesh, leftTrackMeshDestroyed);

        if (rightTrackModule.HitPoint > 0) RegainMobility();
    }

    private void OnRightTrackRecovered(Destructible dest)
    {
        ChangeActiveObject(rightTrackMesh, rightTrackMeshDestroyed);

        if (leftTrackModule.HitPoint > 0) RegainMobility();
    }

    private void ChangeActiveObject(GameObject a, GameObject b)
    {
        a.SetActive(b.activeSelf);
        b.SetActive(!b.activeSelf);
    }

    private void TakeAwayMobility()
    {
        tank.enabled = false;
    }

    private void RegainMobility()
    {
        tank.enabled = true;
    }
}
