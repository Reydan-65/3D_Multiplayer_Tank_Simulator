using Mirror;
using UnityEngine;
public class ProjectileHitResult
{
    public ProjectileHitType Type;
    public float Damage;
    public float ExplosionDamage;
    public Vector3 Point;
    public Armor Armor;
    public ProjectileType ProjectileType;

    public ProjectileHitResult(ProjectileHitType type, float damage, float explosionDamage,
                             Vector3 point, Armor armor, ProjectileType projectileType)
    {
        Type = type;
        Damage = damage;
        ExplosionDamage = explosionDamage;
        Point = point;
        Armor = armor;
        ProjectileType = projectileType;
    }
}

public static class ProjectileHitResultWriteRead
{
    public static void WriteHitResult(this NetworkWriter writer, ProjectileHitResult hitResult)
    {
        writer.WriteInt((int)hitResult.Type);
        writer.WriteFloat(hitResult.Damage);
        writer.WriteFloat(hitResult.ExplosionDamage);
        writer.WriteVector3(hitResult.Point);
        writer.Write(hitResult.Armor);
        writer.WriteInt((int)hitResult.ProjectileType);
    }

    public static ProjectileHitResult ReadHitResult(this NetworkReader reader)
    {
        return new ProjectileHitResult(
            (ProjectileHitType)reader.ReadInt(),
            reader.ReadFloat(),
            reader.ReadFloat(),
            reader.ReadVector3(),
            reader.Read<Armor>(),
            (ProjectileType)reader.ReadInt()
        );
    }
}