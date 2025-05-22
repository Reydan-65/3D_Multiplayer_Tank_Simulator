using UnityEngine;

[RequireComponent (typeof(TrackVehicle))]
public class TankTrackTextureMovement : MonoBehaviour
{
    [SerializeField] private Renderer leftTrackRenderer;
    [SerializeField] private Renderer rightTrackRenderer;

    [SerializeField] private Vector2 direction;
    [SerializeField] private float modefier;

    private TrackVehicle tank;

    private void Start()
    {
        tank = GetComponent<TrackVehicle>();
    }

    private void FixedUpdate()
    {
        float speed = tank.LeftWheelRMP / 60.0f * modefier * Time.fixedDeltaTime;
        leftTrackRenderer.material.SetTextureOffset("_BaseMap", leftTrackRenderer.material.GetTextureOffset("_BaseMap") + direction * speed);

        speed = tank.RightWheelRMP / 60.0f * modefier * Time.fixedDeltaTime;
        rightTrackRenderer.material.SetTextureOffset("_BaseMap", rightTrackRenderer.material.GetTextureOffset("_BaseMap") + direction * speed);
    }
}
