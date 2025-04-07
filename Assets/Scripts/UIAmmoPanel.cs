using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAmmoPanel : MonoBehaviour
{
    [SerializeField] private Image ammoIcon;

    private Vector2 panelBasePosition;

    private void Start()
    {
        panelBasePosition = transform.position;

        float offset = ammoIcon.GetComponent<RectTransform>().sizeDelta.x / 2;

        if (transform.childCount > 1)
        {
            transform.position = new Vector2(panelBasePosition.x - offset * (transform.childCount / 2), panelBasePosition.y);
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform ammoIcon = transform.GetChild(i);

            if (ammoIcon != null)
            {
                Transform targetChild = ammoIcon.GetChild(2);

                if (targetChild != null)
                {
                    TextMeshProUGUI text = targetChild.GetComponent<TextMeshProUGUI>();

                    if (text != null)
                        text.text = (i + 1).ToString();
                }
            }
        }
    }
}
