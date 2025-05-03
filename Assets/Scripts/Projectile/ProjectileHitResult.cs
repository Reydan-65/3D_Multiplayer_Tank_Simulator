using Mirror;
using UnityEngine;
public class ProjectileHitResult
{
    public ProjectileHitType Type;
    public float Damage;
    public float ExplosionDamage;
    public Vector3 Point;
    public bool IsVisible;
    public ProjectileType ProjectileType;

    public ProjectileHitResult(ProjectileHitType type, float damage, float explosionDamage,
                             Vector3 point, bool isVisible, ProjectileType projectileType)
    {
        Type = type;
        Damage = damage;
        ExplosionDamage = explosionDamage;
        Point = point;
        IsVisible = isVisible;
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
        writer.Write(hitResult.IsVisible);
        writer.WriteInt((int)hitResult.ProjectileType);
    }

    public static ProjectileHitResult ReadHitResult(this NetworkReader reader)
    {
        return new ProjectileHitResult(
            (ProjectileHitType)reader.ReadInt(),
            reader.ReadFloat(),
            reader.ReadFloat(),
            reader.ReadVector3(),
            reader.ReadBool(),
            (ProjectileType)reader.ReadInt()
        );
    }
}