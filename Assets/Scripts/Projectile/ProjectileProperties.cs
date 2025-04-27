using UnityEngine;

public enum ProjectileType
{
    ArmorPiercing,
    HighExplosion,
    Subcaliber
}

[CreateAssetMenu]
public class ProjectileProperties : ScriptableObject
{
    [SerializeField] private ProjectileType type;

    [Header("Common")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Sprite icon;

    [Header("Movement")]
    [SerializeField] private float velocity;
    [SerializeField] private float mass;
    [SerializeField] private float impactForce;

    [Header("Damage")]
    [SerializeField] private float damage;
    [SerializeField] private float expDamage;
    [Range(0f, 1f)]
    [SerializeField] private float damageSpread;

    [Header("Caliber")]
    [SerializeField] private float caliber;

    [Header("Armor Penetration")]
    [SerializeField] private float armorPenetration;
    [Range (0f, 1f)]
    [SerializeField] private float armorPenetrationSpread;
    [Range (0f, 90f)]
    [SerializeField] private float normalizationAngle;
    [Range (0f, 90f)]
    [SerializeField] private float ricochetAngle;

    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 5f;

    [Header("Explosion Settings")]
    [SerializeField] private ImpactEffect impactEffetcPrefab;

    public float ExplosionRadius => explosionRadius;

    public ProjectileType Type => type;

    public ImpactEffect ImpactEffetcPrefab => impactEffetcPrefab;
    public Projectile ProjectilePrefab => projectilePrefab;
    public Sprite Icon => icon;
    public float Velocity => velocity;
    public float Mass => mass;
    public float ImpactForce => impactForce;

    public float Damage => damage;
    public float ExpDamage => expDamage;
    public float DamageSpread => damageSpread;
    public float Caliber => caliber;
    public float ArmorPenetration => armorPenetration;
    public float ArmorPenetrationSpread => ArmorPenetrationSpread;
    public float NormalizationAngle => normalizationAngle;
    public float RicochetAngle => ricochetAngle;

    public float GetSpreadDamage()
    {
        float spreadDamage = damage * Random.Range(1 - damageSpread, 1 + damageSpread);
        return spreadDamage;
    }

    public float GetSpreadExpDamage()
    {
        float spreadExpDamage = expDamage * Random.Range(1 - damageSpread, 1 + damageSpread);
        return spreadExpDamage;
    }

    public float GetSpreadArmorPenetration()
    {
        float spreadArmorPenetration = armorPenetration * Random.Range(1 - armorPenetrationSpread, 1 + armorPenetrationSpread);
        return spreadArmorPenetration;
    }

    public ProjectileType GetProjectileType()
    {
        return type;
    }
}
