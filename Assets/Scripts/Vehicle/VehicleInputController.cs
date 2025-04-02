using UnityEngine;

[RequireComponent(typeof(Player))]
public class VehicleInputController : MonoBehaviour
{
    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    protected virtual void Update()
    {
        if (player == null) return;
        if (player.ActiveVehicle == null) return;

        if (player.isLocalPlayer)
        {
            player.ActiveVehicle.SetTargetControl(new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Jump"), Input.GetAxis("Vertical")));
        }
    }
}
