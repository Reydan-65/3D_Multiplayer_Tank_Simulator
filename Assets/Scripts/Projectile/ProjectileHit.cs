using UnityEngine;

public enum ProjectileHitType
{
    Penetration,
    NoPenetration,
    Ricochet,
    ModulePenetration,
    ModuleNoPenetration,
    Environment,
    HighExplosionImpact
}

[RequireComponent(typeof(Projectile))]
public class ProjectileHit : MonoBehaviour
{
    private const float RAY_ADVANCE = 1.1f;

    private bool isHit = false;
    private Armor hitArmor;
    private ArmorType hitArmorType;
    private Destructible target;
    private RaycastHit raycastHit;
    private Projectile projectile;

    public bool IsHit => isHit;
    public Armor HitArmor => hitArmor;
    public Destructible Target => target;
    public RaycastHit RaycastHit => raycastHit;
    public Projectile Projectile => projectile;


    private void Awake()
    {
        projectile = GetComponent<Projectile>();
        hitArmorType = ArmorType.None;
    }

    public void Check()
    {
        if (isHit) return;

        if (Physics.Raycast(transform.position, transform.forward, out raycastHit,
            projectile.Properties.Velocity * Time.deltaTime * RAY_ADVANCE))
        {
            if (raycastHit.collider.isTrigger && !raycastHit.collider.GetComponent<Armor>()) return;

            hitArmor = raycastHit.collider.GetComponent<Armor>();

            if (hitArmor != null)
                hitArmorType = hitArmor.Type;

            isHit = true;
        }
    }

    public ProjectileHitResult GetHitResult()
    {
        float directDamage = CalculateDirectDamage();
        float explosionDamage = CalculateExplosionDamage();
        ProjectileHitType hitType = DetermineHitType(directDamage, explosionDamage);

        bool isTargetVisible = false;

        if (target != null)
        {
            VehicleViewer viewer = null;

            if (Player.Local != null && Player.Local.ActiveVehicle != null)
                viewer = Player.Local.ActiveVehicle.GetComponentInParent<VehicleViewer>();

            isTargetVisible = viewer != null && viewer.IsVisible(target.netIdentity);
        }

        AIMovement ai = hitArmor?.transform.root.GetComponent<AIMovement>();

        if (ai != null)
            ai.OnUnderFire(projectile);

        return new ProjectileHitResult(
            hitType,
            directDamage,
            explosionDamage,
            isHit ? raycastHit.point : transform.position,
            isTargetVisible,
            projectile?.Properties?.Type ?? ProjectileType.ArmorPiercing,
            target?.netIdentity
        );
    }

    private float CalculateDirectDamage()
    {
        if (hitArmor == null)
            return 0f;

        float normalization = GetAdjustedNormalizationAngle();
        float angle = GetImpactAngle(normalization);
        float effectiveArmor = hitArmor.Thickness / Mathf.Cos(angle * Mathf.Deg2Rad);
        float penetration = projectile.Properties.GetSpreadArmorPenetration();

        target = hitArmor.Destructible;

        if (IsRicochet(angle))
            return 0f;

        if (penetration >= effectiveArmor)
        {
            float directDamage = projectile.Properties.GetSpreadDamage();
            return directDamage;
        }

        return 0f;
    }


    private float CalculateExplosionDamage()
    {
        if (projectile?.Properties == null || projectile.Properties.ExplosionRadius <= 0)
            return 0f;

        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            projectile.Properties.ExplosionRadius
        );

        float closestDistance = float.MaxValue;
        float maxDamage = projectile.Properties.GetSpreadExpDamage();
        Armor closestArmor = null;

        foreach (var collider in colliders)
        {
            if (!collider.TryGetComponent<Armor>(out var armor) ||
                armor.Destructible == null ||
                armor.Destructible.netId == 0)
                continue;

            float distance = Vector3.Distance(transform.position, collider.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                target = armor.Destructible;

                closestArmor = armor;
                hitArmor = closestArmor;
            }
        }

        if (target == null)
            return 0f;

        target = closestArmor.Destructible;
        hitArmorType = closestArmor.Type;

        float damageMultiplier = 1f - Mathf.Clamp01(closestDistance / projectile.Properties.ExplosionRadius);

        return maxDamage * damageMultiplier;
    }

    private ProjectileHitType DetermineHitType(float directDamage, float explosionDamage)
    {
        if (directDamage == 0 && explosionDamage == 0 && hitArmor != null &&
            IsRicochet(GetImpactAngle(GetAdjustedNormalizationAngle())))
            return ProjectileHitType.Ricochet;

        if (explosionDamage > 0)
        {
            if (directDamage > 0) return GetPenetrationType();
            return ProjectileHitType.HighExplosionImpact;
        }

        if (directDamage > 0) return GetPenetrationType();
        if (hitArmor != null) return GetNoPenetrationType();

        return ProjectileHitType.Environment;
    }

    private ProjectileHitType GetPenetrationType() =>
        hitArmor.Type == ArmorType.Module ?
            ProjectileHitType.ModulePenetration :
            ProjectileHitType.Penetration;

    private ProjectileHitType GetNoPenetrationType() =>
        hitArmor.Type == ArmorType.Module ?
            ProjectileHitType.ModuleNoPenetration :
            ProjectileHitType.NoPenetration;

    private float GetAdjustedNormalizationAngle()
    {
        float normalization = projectile.Properties.NormalizationAngle;
        if (projectile.Properties.Caliber > hitArmor.Thickness * 2)
            normalization = (projectile.Properties.NormalizationAngle * 1.4f *
                           projectile.Properties.Caliber) / hitArmor.Thickness;

        return normalization;
    }

    private float GetImpactAngle(float normalization) =>
        Mathf.Abs(Vector3.SignedAngle(-projectile.transform.forward,
                raycastHit.normal, projectile.transform.right)) - normalization;

    private bool IsRicochet(float angle) =>
        angle > projectile.Properties.RicochetAngle &&
        projectile.Properties.Caliber < hitArmor.Thickness * 3 &&
        hitArmor.Type == ArmorType.Vehicle;

    //private void SendDamageToServer(
    //ProjectileType projectileType,
    //float directDamage,
    //float explosionDamage,
    //Vector3 hitPoint,
    //ProjectileHitType hitType)
    //{
    //    if (projectile.Owner == null) return;

    //    MatchMember shooter = projectile.Owner.GetComponent<MatchMember>();

    //    if (!shooter.isOwned) return;

    //    //Debug.Log($"Sending damage to server: Shooter = {shooter.name}, ShooterNetId = {shooter.netId}, damage = {directDamage}, target = {targetNetId}");

    //    shooter.CmdRegisterProjectileHit(
    //        projectileType,
    //        directDamage,
    //        explosionDamage,
    //        hitPoint,
    //        target,
    //        hitType
    //    );
    //}
}
