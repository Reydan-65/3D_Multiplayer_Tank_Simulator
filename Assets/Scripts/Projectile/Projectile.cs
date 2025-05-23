using Mirror;
using System.Globalization;
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
    public Vehicle OwnerVehicle { get; set; }
    public ProjectileProperties Properties => properties;

    private void Start()
    {
        Destroy(gameObject, lifeTime);

        //if (properties == null)
        //Debug.LogError("ProjectileProperties is not assigned!");
    }

    private void Update()
    {
        hit.Check();

        if (!hit.IsHit)
            movement.Move();
        else
            OnHit();
    }

    private void OnHit()
    {
        transform.position = hit.RaycastHit.point;

        ProjectileHitResult hitResult = hit.GetHitResult();

        if (NetworkServer.active)
        {
            HandleServerHit(hitResult);
        }
        else if (NetworkSessionManager.Instance.IsClient && Owner.isLocalPlayer)
        {
            HandleClientHit(hitResult);
        }

        Destroy();
    }

    private void HandleServerHit(ProjectileHitResult hitResult)
    {
        if (hitResult == null) return;

        SpawnImpactEffect(hitResult);

        if (hit.HitArmor == null) return;

        Destructible target = hit.HitArmor.Destructible;

        if (target == null) return;

        ArmorType armorType = hit.HitArmor.Type;

        SvTakeDamage(target, hitResult.Damage, hitResult.ExplosionDamage, armorType);

        if (hitResult.Type != ProjectileHitType.Environment)
            SvAddFrags(target);


        MatchMember ownerMember = Owner.GetComponent<MatchMember>();
        if (ownerMember != null && ownerMember.isServer)
        {
            if (ownerMember.isOwned)
            {
                ownerMember.CmdRegisterProjectileHit(
                    hitResult.ProjectileType,
                    hitResult.Damage,
                    hitResult.ExplosionDamage,
                    hitResult.Point,
                    target,
                    hitResult.Type
                );
            }
        }
    }

    private void HandleClientHit(ProjectileHitResult hitResult)
    {
        if (hitResult == null) return;

        if (hitResult.Type == ProjectileHitType.Environment ||
            hitResult.Type == ProjectileHitType.HighExplosionImpact)
        {
            SpawnImpactEffect(hitResult);
        }

        VehicleViewer viewer = Player.Local?.ActiveVehicle?.GetComponentInParent<VehicleViewer>();
        if (viewer == null || viewer.IsVisible(hitResult.TargetIdentity))
        {
            SpawnImpactEffect(hitResult);
        }
        
        if (hit.HitArmor == null) return;

        Destructible target = hit.HitArmor.Destructible;

        if (target == null) return;

        MatchMember ownerMember = Owner.GetComponent<MatchMember>();
        if (ownerMember != null && ownerMember.isOwned)
        {
            ownerMember.CmdRegisterProjectileHit(
                hitResult.ProjectileType,
                hitResult.Damage,
                hitResult.ExplosionDamage,
                hitResult.Point,
                target,
                hitResult.Type
            );
        }
    }

    private void SpawnImpactEffect(ProjectileHitResult hitResult)
    {
        if (Properties.ImpactEffetcPrefab != null && Owner != null)
        {
            NetworkIdentity effectIdentity = Properties.ImpactEffetcPrefab.GetComponent<NetworkIdentity>();

            if (effectIdentity != null)
            {
                MatchMember ownerMember = Owner.GetComponent<MatchMember>();

                if (ownerMember != null && (ownerMember.isOwned || ownerMember.isServer))
                {
                    ownerMember.CmdSpawnImpactEffect(
                        hitResult.Point,
                        Quaternion.LookRotation(hit.RaycastHit.normal),
                        effectIdentity.assetId
                    );
                }
            }
        }
    }

    private void SvTakeDamage(Destructible target, float damage, float explosionDamage, ArmorType type)
    {
        if (target == null) return;

        float totalDamage = damage + explosionDamage;

        //Debug.Log($"Total Damage: {totalDamage}, Direct Damage: {damage}, Explosion Damage: {explosionDamage}");

        if (totalDamage > 0)
        {
            target.SvApplyDamage((int)totalDamage);

            if (type == ArmorType.Module)
            {
                Destructible rootDestructible = target.transform.root.GetComponent<Destructible>();

                if (rootDestructible != null)
                {
                    rootDestructible.SvApplyDamage((int)totalDamage);
                }
            }
        }
    }

    private void SvAddFrags(Destructible target)
    {
        if (target == null) return;

        Destructible rootDestructible = target.transform.root.GetComponent<Destructible>();

        if (rootDestructible != null)
        {
            MatchMember member = Owner.GetComponent<MatchMember>();

            if (member != null && rootDestructible.HitPoint <= 0)
            {
                if (target.GetComponent<Vehicle>().TeamID != member.TeamID)
                    member.SvAddFrags(1);
                else
                    member.SvAddFrags(-1);
            }
        }
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