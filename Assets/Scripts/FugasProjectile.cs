using UnityEngine;

public class FugasProjectile : Projectile
{
    [SerializeField] private GameObject impactPrefab;
    [SerializeField] private float explosionRadius;

    protected override void UpdateProjectile()
    {
        transform.forward = Vector3.Lerp(transform.forward, -Vector3.up, Mathf.Clamp01(Time.deltaTime * mass)).normalized;

        Vector3 step = transform.forward * velocity * Time.deltaTime;

        transform.position += step;

        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, velocity * Time.deltaTime * RayAdvance))
        {
            transform.position = hit.point;

            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

            foreach (var nearby in colliders)
            {
                var destructible = nearby.transform.root.GetComponent<Destructible>();

                if (destructible)
                {
                    if (NetworkSessionManager.Instance.IsServer)
                    {
                        float distance = Vector3.Distance(transform.position, nearby.transform.position);

                        float damageMultiplier = Mathf.Clamp01(1.0f - (distance / explosionRadius));
                        float dmg = Mathf.Max(0, damage * damageMultiplier + Random.Range(-damageScatter, damageScatter) * damage);

                        destructible.SvApplyDamage((int)dmg);
                    }
                }
            }

            OnProjectileLifeEnd(hit.collider, hit.point, hit.normal);

            return;
        }
    }

    protected override void OnProjectileLifeEnd(Collider collider, Vector3 point, Vector3 normal)
    {
        base.OnProjectileLifeEnd(collider, point, normal);

        Instantiate(impactPrefab, transform.position, Quaternion.identity);
    }
}
