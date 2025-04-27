using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class UIModuleStatusPanel : MonoBehaviour
{
    [SerializeField] private GameObject header;
    [SerializeField] private Transform parent;
    [SerializeField] private UIModuleStatusElement elementPrefab;

    private VehicleModule leftTrack;
    private VehicleModule rightTrack;
    private TrackTank tank;
    private List<UIModuleStatusElement> activeElements = new List<UIModuleStatusElement>();

    private RectTransform rectTransform;
    private RectTransform parentRect;
    private Vector2 originalPosition;

    private void Start()
    {
        if (NetworkSessionManager.Instance != null)
        {
            NetworkSessionManager.Events.PlayerVehicleSpawned += OnPlayerVehicleSpawned;
        }

        rectTransform = GetComponent<RectTransform>();
        parentRect = parent.GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
        header.SetActive(false);

        UpdatePanelSize();
    }

    private void OnDestroy()
    {
        if (NetworkSessionManager.Instance != null)
        {
            NetworkSessionManager.Events.PlayerVehicleSpawned -= OnPlayerVehicleSpawned;
        }

        UnsubscribeFromModules();
        ClearAllElements();
    }

    private void UnsubscribeFromModules()
    {
        if (leftTrack != null)
        {
            leftTrack.Destroyed -= OnLeftTrackDestroyed;
            leftTrack.Recovered -= OnLeftTrackRecovered;
        }

        if (rightTrack != null)
        {
            rightTrack.Destroyed -= OnRightTrackDestroyed;
            rightTrack.Recovered -= OnRightTrackRecovered;
        }

        if (tank != null)
        {
            tank.Destroyed -= OnTankDestroyed;
        }
    }

    private void ClearAllElements()
    {
        foreach (var element in activeElements)
        {
            if (element != null)
            {
                Destroy(element.gameObject);
            }
        }

        activeElements.Clear();
        UpdatePanelSize();
    }

    private void OnPlayerVehicleSpawned(Vehicle v)
    {
        tank = v.GetComponent<TrackTank>();
        if (tank == null) return;

        UnsubscribeFromModules();

        var allModules = tank.GetComponentsInChildren<VehicleModule>();

        foreach (var module in allModules)
        {
            if (module.Title == "Left Track")
            {
                leftTrack = module;
                leftTrack.Destroyed += OnLeftTrackDestroyed;
                leftTrack.Recovered += OnLeftTrackRecovered;
            }
            else if (module.Title == "Right Track")
            {
                rightTrack = module;
                rightTrack.Destroyed += OnRightTrackDestroyed;
                rightTrack.Recovered += OnRightTrackRecovered;
            }
        }

        tank.Destroyed += OnTankDestroyed;

        UpdatePanelSize();
    }

    private void OnTankDestroyed(Destructible dest)
    {
        ClearAllElements();
        header.SetActive(false);
        UnsubscribeFromModules();
    }

    private void OnLeftTrackDestroyed(Destructible dest)
    {
        CreateStatusElement("Левый каток!", leftTrack);
    }

    private void OnRightTrackDestroyed(Destructible dest)
    {
        CreateStatusElement("Правый каток!", rightTrack);
    }

    private void OnLeftTrackRecovered(Destructible dest)
    {
        RemoveTrackElement(leftTrack);
        UpdateVisualViewState();
        UpdatePanelSize();
    }

    private void OnRightTrackRecovered(Destructible dest)
    {
        RemoveTrackElement(rightTrack);
        UpdateVisualViewState();
        UpdatePanelSize();
    }

    private void RemoveTrackElement(VehicleModule track)
    {
        if (track == null) return;

        for (int i = activeElements.Count - 1; i >= 0; i--)
        {
            if (activeElements[i] != null && activeElements[i].Module == track)
            {
                Destroy(activeElements[i].gameObject);
                activeElements.RemoveAt(i);
            }
        }
    }

    private void CreateStatusElement(string moduleName, VehicleModule module)
    {
        if (module == null) return;

        header.SetActive(true);

        foreach (var element in activeElements)
        {
            if (element != null && element.Module == module)
            {
                element.SetModuleName(moduleName);
                element.SetModuleRecoveryTime(module.RecoveredTime);
                return;
            }
        }

        var newElement = Instantiate(elementPrefab, parent);
        if (newElement != null)
        {
            newElement.SetModule(module);
            newElement.SetModuleName(moduleName);
            newElement.SetModuleRecoveryTime(module.RecoveredTime);
            activeElements.Add(newElement);
        }

        UpdatePanelSize();
    }

    private void UpdateVisualViewState()
    {
        header.SetActive(activeElements.Count > 0);
        rectTransform.anchoredPosition = originalPosition;
    }

    private void UpdatePanelSize()
    {
        RectTransform headerTextR = header.GetComponentInChildren<TextMeshProUGUI>().GetComponent<RectTransform>();
        RectTransform contentR = parentRect;

        int childCount = parent.transform.childCount - 2;

        float elementH = elementPrefab.GetComponent<RectTransform>().sizeDelta.y;
        float offset = elementH;

        float headerTextH = headerTextR.sizeDelta.y;
        float contentH = elementH * childCount + offset;

        float totalH = headerTextH + contentH;

        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, totalH + 5);
    }
}