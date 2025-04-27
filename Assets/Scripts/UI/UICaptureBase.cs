using UnityEngine;
using UnityEngine.UI;

public class UICaptureBase : MonoBehaviour
{
    [SerializeField] private ConditionCaptureBase condition;

    [SerializeField] private Slider localTeamSlider;
    [SerializeField] private Slider otherTeamSlider;

    private void Update()
    {
        if (Player.Local == null) return;

        if (Player.Local.TeamID == TeamSide.TeamRed)
        {
            UpdateSlider(localTeamSlider, condition.RedBaseCaptureLevel);
            UpdateSlider(otherTeamSlider, condition.BlueBaseCaptureLevel);
        }

        if (Player.Local.TeamID == TeamSide.TeamBlue)
        {
            UpdateSlider(localTeamSlider, condition.BlueBaseCaptureLevel);
            UpdateSlider(otherTeamSlider, condition.RedBaseCaptureLevel);
        }
    }

    private void UpdateSlider(Slider slider, float value)
    {
        if (value == 0)
            slider.gameObject.SetActive(false);
        else
        {
            if (slider.gameObject.activeSelf == false)
                slider.gameObject.SetActive(true);

            slider.value = value / 100;
        }
    }
}
