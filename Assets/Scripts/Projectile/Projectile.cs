using Mirror;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private ProjectileProperties properties;
    [SerializeField] private ProjectileMovement movement;
    [SerializeField] private ProjectileHit hit;
    [SerializeField] private GameObject visualModel;
    [SerializeField] private float delayBeforeDestroy;
    [SerializeField] private float lifeTime;

    public NetworkIdentity Owner { get; set; }
    public ProjectileProperties Properties => properties;

    private void Start()
    {
        Destroy(gameObject, lifeTime);

        if (properties == null)
        {
            Debug.LogError("ProjectileProperties is not assigned!");
        }
    }

    private void Update()
    {
        hit.Check();
        movement.Move();

        if (hit.IsHit) OnHit();
    }

    private void OnHit()
    {
        transform.position = hit.RaycastHit.point;
        ProjectileHitResult hitResult = hit.GetHitResult();

        if (NetworkSessionManager.Instance.IsClient)
        {
            if (Properties.ImpactEffetcPrefab != null && Owner != null)
            {
                NetworkIdentity effectIdentity = Properties.ImpactEffetcPrefab.GetComponent<NetworkIdentity>();
                if (effectIdentity == null)
                {
                    return;
                }

                Player ownerPlayer = Owner.GetComponent<Player>();
                if (ownerPlayer != null && ownerPlayer.isOwned)
                {
                    // Вызываем RPC только если есть активный сервер
                    ownerPlayer.CmdSpawnImpactEffect(
                        hitResult.Point,
                        Quaternion.LookRotation(hit.RaycastHit.normal),
                        effectIdentity.assetId
                    );
                }
            }
        }

        Destroy();
    }

    private void Destroy()
    {
        visualModel.SetActive(false);
        enabled = false;
        Destroy(gameObject, delayBeforeDestroy);
    }

    public void SetProperties(ProjectileProperties props)
    {
        properties = props;
    }
}