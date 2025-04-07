using Mirror;
using UnityEngine.Events;

public class EventCollector : NetworkBehaviour
{
    public UnityAction<Vehicle> PlayerVehicleSpawned;

    [Server]
    public void SvOnAddplayer()
    {
        RpcOnAddplayer();
    }

    [ClientRpc]
    private void RpcOnAddplayer()
    {
        Player.Local.VehicleSpawned += OnPlayerVehicleSpawned; 
    }

    private void OnPlayerVehicleSpawned(Vehicle vehicle)
    {
        PlayerVehicleSpawned?.Invoke(vehicle);
    }
}
