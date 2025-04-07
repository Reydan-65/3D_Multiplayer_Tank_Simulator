using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] protected GameObject visualModel;

    [SerializeField] protected float velocity;
    [SerializeField] protected float lifeTime;
    [SerializeField] protected float mass;

    [SerializeField] protected float damage;
    [Range(0.0f, 1.0f)]
    [SerializeField] protected float damageScatter;

    [SerializeField] protected float impactForce;

    protected const float RayAdvance = 1.1f;

    protected virtual void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    protected void Update()
    {
        UpdateProjectile();
    }

    protected virtual void UpdateProjectile()
    {
        transform.forward = Vector3.Lerp(transform.forward, -Vector3.up, Mathf.Clamp01(Time.deltaTime * mass)).normalized;

        Vector3 step = transform.forward * velocity * Time.deltaTime;
        
        transform.position += step;

        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, velocity * Time.deltaTime * RayAdvance))
        {
            transform.position = hit.point;

            var destructible = hit.collider.transform.root.GetComponent<Destructible>();

            if (destructible)
            {
                // is your projectile
                // if yes send command to server

                if (NetworkSessionManager.Instance.IsServer)
                {
                    float dmg = damage + Random.Range(-damageScatter, damageScatter) * damage;

                    destructible.SvApplyDamage((int)dmg);
                }
            }

            OnProjectileLifeEnd(hit.collider, hit.point, hit.normal);

            return;
        }
    }

    protected virtual void OnProjectileLifeEnd(Collider collider, Vector3 point, Vector3 normal)
    {
        visualModel.SetActive(false);
        enabled = false;
    }
}
