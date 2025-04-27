using System.Collections.Generic;
using UnityEngine;

public class UIAmmunitionPanel : MonoBehaviour
{
    [SerializeField] private Transform parent;
    [SerializeField] private UIAmmunitionElement elementPrafab;

    private List<UIAmmunitionElement> allElements = new List<UIAmmunitionElement>();
    private Turret turret;
    private int lastSelectedAmunitionIndex;

    private void Start()
    {
        if (NetworkSessionManager.Instance != null)
            NetworkSessionManager.Events.PlayerVehicleSpawned += OnPlayerVehicleSpawned;
    }

    private void OnDestroy()
    {
        if (NetworkSessionManager.Instance != null)
            NetworkSessionManager.Events.PlayerVehicleSpawned -= OnPlayerVehicleSpawned;

        if (turret != null)
        {
            turret.UpdateSelectedAmmunition -= OnTurretUpdateSelectedAmmunition;

            for (int i = 0; i < turret.Ammunition.Length; i++)
                turret.Ammunition[i].AmmoCountChanged -= OnAmmoCountChanged;
        }
    }

    private void OnPlayerVehicleSpawned(Vehicle vehicle)
    {
        ClearAllElements();

        turret = vehicle.Turret;
        turret.UpdateSelectedAmmunition += OnTurretUpdateSelectedAmmunition;

        CreateAmmunitionElements();
    }

    private void ClearAllElements()
    {
        if (turret != null)
        {
            for (int i = 0; i < turret.Ammunition.Length; i++)
                turret.Ammunition[i].AmmoCountChanged -= OnAmmoCountChanged;
        }

        foreach (var element in allElements)
        {
            if (element != null && element.gameObject != null)
                Destroy(element.gameObject);
        }

        allElements.Clear();
    }

    private void CreateAmmunitionElements()
    {
        if (turret == null) return;

        for (int i = 0; i < turret.Ammunition.Length; i++)
        {
            UIAmmunitionElement element = Instantiate(elementPrafab, parent);
            element.transform.localScale = Vector3.one;

            element.UpdateButtonText(i + 1);
            element.SetAmmunition(turret.Ammunition[i]);

            turret.Ammunition[i].AmmoCountChanged += OnAmmoCountChanged;

            allElements.Add(element);

            if (i == 0)
            {
                element.Select();
                lastSelectedAmunitionIndex = 0;
            }
        }
    }

    private void OnAmmoCountChanged(int count)
    {
        if (turret == null || allElements.Count <= turret.SelectedAmmunitionIndex) return;

        allElements[turret.SelectedAmmunitionIndex].UpdateAmmoCount(count);
    }

    private void OnTurretUpdateSelectedAmmunition(int index)
    {
        if (index < 0 || index >= allElements.Count) return;

        allElements[lastSelectedAmunitionIndex].UnSelect();
        allElements[index].Select();

        lastSelectedAmunitionIndex = index;
    }
}