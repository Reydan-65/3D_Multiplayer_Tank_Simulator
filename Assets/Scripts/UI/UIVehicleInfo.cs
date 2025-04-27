using UnityEngine;

public class UIVehicleInfo : MonoBehaviour
{
    [SerializeField] private UIHealthSlider slider;

    [SerializeField] private Vector3 worldOffset;
    public Vector3 WorldOffset => worldOffset;

    private Vehicle vehicle;
    public Vehicle Vehicle => vehicle;

    public void SetVehicle(Vehicle vehicle)
    {
        this.vehicle = vehicle;

        int[] teamIDs = new int[2];
        teamIDs[0] = vehicle.Owner.GetComponent<Player>().TeamID;
        teamIDs[1] = Player.Local.TeamID;

        slider.Init(vehicle, teamIDs);
    }
}
