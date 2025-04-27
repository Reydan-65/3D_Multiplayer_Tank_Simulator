using UnityEngine;

[RequireComponent (typeof(Projectile))]
public class ProjectileMovement : MonoBehaviour
{
    private Projectile projectile;
    private Vector3 step;

    private void Awake()
    {
        projectile = GetComponent<Projectile>();
        step = new Vector3();
    }

    public void Move()
    {
        transform.forward = Vector3.Lerp(transform.forward, -Vector3.up,
                                         Mathf.Clamp01(Time.deltaTime * projectile.Properties.Mass)).normalized;

        step = transform.forward * projectile.Properties.Velocity * Time.deltaTime;

        transform.position += step;
    }
}
