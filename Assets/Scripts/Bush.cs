using Mirror;
using UnityEngine;

public class Bush : MonoBehaviour
{
    private Collider _collider;

    private void Start()
    {
        _collider = GetComponentInChildren<Collider>();
        _collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!NetworkServer.active) return;

        if (other != null && other.transform.root.TryGetComponent(out VehicleViewer vv))
        {
            vv.SvSetHidden(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!NetworkServer.active) return;

        if (other != null && other.transform.root.TryGetComponent(out VehicleViewer vv))
        {
            vv.SvSetHidden(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!NetworkServer.active) return;

        if (other != null && other.transform.root.TryGetComponent(out VehicleViewer vv))
        {
            vv.SvSetHidden(false);
        }
    }
}
