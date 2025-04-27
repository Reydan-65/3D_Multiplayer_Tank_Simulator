using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAmmunitionElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Image image;
    [SerializeField] private GameObject selectionBorder;

    public void SetAmmunition(Ammunition ammunition)
    {
        image.sprite = ammunition.ProjectileProperties.Icon;

        UpdateAmmoCount(ammunition.AmmoCount);
    }

    public void UpdateAmmoCount(int count)
    {
        text.text = count.ToString();
    }

    public void UpdateButtonText(int index)
    {
        buttonText.text = index.ToString();
    }

    public void Select()
    {
        selectionBorder.SetActive(true);
    }

    public void UnSelect()
    {
        selectionBorder.SetActive(false);
    }
}
