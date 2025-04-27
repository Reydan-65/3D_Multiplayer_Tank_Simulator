using Mirror;
using UnityEngine.Events;

public class EventCollector : NetworkBehaviour
{
    public UnityAction<Vehicle> PlayerVehicleSpawned;

    [Server]
    public void SvOnAddplayer()
    {
        RpcOnAddPlayer();
    }

    [ClientRpc]
    private void RpcOnAddPlayer()
    {
        Player.Local.VehicleSpawned += OnPlayerVehicleSpawned; 
    }

    private void OnPlayerVehicleSpawned(Vehicle vehicle)
    {
        PlayerVehicleSpawned?.Invoke(vehicle);
    }
}
