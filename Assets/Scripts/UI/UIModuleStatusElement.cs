using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIModuleStatusElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Slider slider;

    private float recoveryTime;
    private float currentTime;
    private VehicleModule module;

    public VehicleModule Module => module;
    private void Start()
    {
        currentTime = 0;
    }

    public void SetModule(VehicleModule module)
    {
        this.module = module;
    }

    public void SetModuleName(string moduleName)
    {
        text.text = moduleName;
    }

    public void SetModuleRecoveryTime(float time)
    {
        recoveryTime = time;
    }
    public void ResetModuleRecoveryTime(float time)
    {
        currentTime = time;
    }

    private void Update()
    {
        if (Player.Local == null) return;
        if (Player.Local.ActiveVehicle == null) return;

        currentTime += Time.deltaTime;

        if (currentTime >= recoveryTime)
            currentTime = recoveryTime;

        UpdateSlider(slider, currentTime);
    }

    private void UpdateSlider(Slider slider, float value)
    {
        if (value == 0)
            slider.gameObject.SetActive(false);
        else
        {
            if (slider.gameObject.activeSelf == false)
                slider.gameObject.SetActive(true);

            slider.value = value / recoveryTime;
        }
    }
}
