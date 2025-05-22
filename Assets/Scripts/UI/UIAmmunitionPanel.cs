using System;
using System.Collections.Generic;
using UnityEngine;

public class UIAmmunitionPanel : MonoBehaviour
{
    [SerializeField] private Transform parent;
    [SerializeField] private UIAmmunitionElement elementPrefab;

    private List<UIAmmunitionElement> allElements = new List<UIAmmunitionElement>();

    private Turret turret;
    private int lastSelectedAmunitionIndex;

    private void Start()
    {
        SubscribeToSessionEvents();
    }

    private void SubscribeToSessionEvents()
    {
        if (NetworkSessionManager.Instance != null)
        {
            NetworkSessionManager.Match.MatchStart -= OnMatchStarted;
            NetworkSessionManager.Match.MatchEnd -= OnMatchEnded;

            NetworkSessionManager.Match.MatchStart += OnMatchStarted;
            NetworkSessionManager.Match.MatchEnd += OnMatchEnded;
        }
    }

    private void OnDestroy()
    {
        UnSubscribeEvents();
    }

    private void OnMatchStarted()
    {
        ClearAllElements();

        if (Player.Local == null || Player.Local.ActiveVehicle == null) return;

        turret = Player.Local.ActiveVehicle.Turret;
        if (turret == null) return;
        turret.UpdateSelectedAmmunition += OnTurretUpdateSelectedAmmunition;

        CreateAmmunitionElements();
    }

    private void OnMatchEnded()
    {
        ClearAllElements();
        turret = null;
    }

    private void UnSubscribeEvents()
    {
        if (NetworkSessionManager.Instance != null)
        {
            NetworkSessionManager.Match.MatchStart -= OnMatchStarted;
            NetworkSessionManager.Match.MatchEnd -= OnMatchEnded;
        }

        if (turret != null)
        {
            turret.UpdateSelectedAmmunition -= OnTurretUpdateSelectedAmmunition;
        }
    }

    private void ClearAllElements()
    {
        if (turret != null && turret.Ammunition != null)
        {
            for (int i = 0; i < turret.Ammunition.Length; i++)
            {
                if (turret.Ammunition[i] != null)
                    turret.Ammunition[i].AmmoCountChanged -= OnAmmoCountChanged;
            }
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
        if (turret == null || turret.Ammunition == null) return;

        for (int i = 0; i < turret.Ammunition.Length; i++)
        {
            if (turret.Ammunition[i] == null) continue;

            UIAmmunitionElement element = Instantiate(elementPrefab, parent);
            element.transform.localScale = Vector3.one;

            element.UpdateButtonText(i + 1);
            element.SetAmmunition(turret.Ammunition[i]);

            turret.Ammunition[i].AmmoCountChanged -= OnAmmoCountChanged;
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