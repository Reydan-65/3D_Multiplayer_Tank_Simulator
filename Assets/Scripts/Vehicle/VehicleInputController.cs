using UnityEngine;

[RequireComponent(typeof(Player))]
public class VehicleInputController : MonoBehaviour
{
    public const float AimDistance = 1000.0f;
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
            if (player.ActiveVehicle.isOwned)
            {
                player.ActiveVehicle.SetTargetControl(new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Jump"), Input.GetAxis("Vertical")));
                player.ActiveVehicle.NetAimPoint = TraceAimPointWithoutPlayerVehicle(VehicleCamera.Instance.transform.position, VehicleCamera.Instance.transform.forward);

                if (Input.GetMouseButtonDown(0))
                    player.ActiveVehicle.Fire();

                var turret = player.ActiveVehicle.GetComponent<Turret>();

                if (turret != null && turret.isOwned)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1)) turret.SetSelectedProperties(0);
                    if (Input.GetKeyDown(KeyCode.Alpha2)) turret.SetSelectedProperties(1);
                    if (Input.GetKeyDown(KeyCode.Alpha3)) turret.SetSelectedProperties(2);
                }
            }
        }
    }

    public static Vector3 TraceAimPointWithoutPlayerVehicle(Vector3 start, Vector3 direction)
    {
        Ray ray = new Ray(start, direction);
        RaycastHit[] hits = Physics.RaycastAll(ray, AimDistance);

        if (Player.Local == null || Player.Local.ActiveVehicle == null)
            return ray.GetPoint(AimDistance);

        var m = Player.Local.ActiveVehicle.GetComponent<Rigidbody>();

        foreach (var hit in hits)
        {
            if (hit.rigidbody == m || hit.collider.isTrigger) continue;

            return hit.point;
        }

        return ray.GetPoint(AimDistance);
    }
}
