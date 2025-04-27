using TMPro;
using UnityEngine;
using static UnityEditor.Rendering.CameraUI;

public class UIHitResultPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI damageText;

    public void SetResultType(string typeText)
    {
        this.typeText.text = typeText;
    }

    public void SetDamageResult(float directDamage, float explosionDamage, ProjectileType projectileType)
    {
        if (directDamage > 0 || explosionDamage > 0)
        {
            float totalDamage = directDamage + explosionDamage;
            damageText.text = "-" + Mathf.RoundToInt(totalDamage).ToString();
        }
        else
        {
            damageText.text = "";
        }
    }
}
