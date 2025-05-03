using UnityEngine;

public class UIVisibilityStatus : MonoBehaviour
{
    [SerializeField] private Transform view;
    [SerializeField] private float hideDelay = 10f;

    private Vehicle vehicle;
    private VehicleViewer viewer;
    private float visibilityTimer;
    private bool wasVisibleLastFrame;
    private bool isTimerExpired;

    private void Start()
    {
        ResetVisibility();
    }

    private void ResetVisibility()
    {
        visibilityTimer = 0f;
        wasVisibleLastFrame = false;
        isTimerExpired = true;
        view?.gameObject.SetActive(false);
    }

    private void Update()
    {
        UpdateVisibilityStatus();
    }

    private void UpdateVisibilityStatus()
    {
        if (vehicle == null || viewer == null || view == null)
            return;

        bool isVisibleNow = viewer.IsDetected;

        if (isVisibleNow && !wasVisibleLastFrame && isTimerExpired)
        {
            view.gameObject.SetActive(true);
            visibilityTimer = hideDelay;
            isTimerExpired = false; 
        }

        if (view.gameObject.activeSelf)
        {
            visibilityTimer -= Time.deltaTime;

            if (visibilityTimer <= 0f)
            {
                visibilityTimer = 0f;
                view.gameObject.SetActive(false);
                isTimerExpired = true;
            }
        }

        wasVisibleLastFrame = isVisibleNow;
    }

    public void SetTarget(Vehicle target)
    {
        vehicle = target;
        viewer = vehicle.Viewer;
        ResetVisibility();
    }
}