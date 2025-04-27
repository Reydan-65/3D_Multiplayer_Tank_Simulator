using UnityEngine;

public class UIHitResultPanel : MonoBehaviour
{
    [SerializeField] private Transform parent;
    [SerializeField] private UIHitResultPopup hitResultPopupPrefab;

    private void Start()
    {
        if (NetworkSessionManager.Instance != null)
            NetworkSessionManager.Match.MatchStart += OnMatchStart;
    }

    private void OnDestroy()
    {
        if (NetworkSessionManager.Instance != null)
            NetworkSessionManager.Match.MatchStart -= OnMatchStart;

        if (Player.Local != null)
            Player.Local.ProjectileHit -= OnProjectileHit;
    }

    private void OnMatchStart()
    {
        if (Player.Local != null)
            Player.Local.ProjectileHit += OnProjectileHit;
    }

    private void OnProjectileHit(ProjectileHitResult hitResult)
    {
        //print($"OnProjectileHit: hitResult.Type = {hitResult.Type}, hitResult.Damage = {hitResult.Damage}, hitResult.ExplosionDamage = {hitResult.ExplosionDamage}, hitResult.ProjectileType = {hitResult.ProjectileType}");

        UIHitResultPopup popup = Instantiate(hitResultPopupPrefab, parent);
        if (hitResult.Type == ProjectileHitType.Environment && hitResult.Damage + hitResult.ExplosionDamage <= 0) return;
        popup.SetResultType(GetHitResultText(hitResult));
        popup.SetDamageResult(hitResult.Damage, hitResult.ExplosionDamage, hitResult.ProjectileType);
    }

    private string GetHitResultText(ProjectileHitResult hitResult)
    {
        switch (hitResult.Type)
        {
            case ProjectileHitType.Ricochet:
                return "�������!";
            case ProjectileHitType.Penetration:
            case ProjectileHitType.ModulePenetration:
                return "��������!";
            case ProjectileHitType.ModuleNoPenetration:
            case ProjectileHitType.NoPenetration:
                return "����� �� �������!";
            case ProjectileHitType.Environment:
                if (hitResult.ExplosionDamage > 0)
                {
                    return "�����!";
                }
                return "";
            default:
                return "";
        }
    }
}