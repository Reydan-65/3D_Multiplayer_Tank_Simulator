using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Vehicle))]
public class AIShooter : MonoBehaviour
{
    [SerializeField] private VehicleViewer viewer;
    [SerializeField] private Transform firePosition;

    private Vehicle vehicle;
    private Vehicle target;
    private Transform lookTransform;

    public bool HasTarget => target != null;


    private void Awake()
    {
        vehicle = GetComponent<Vehicle>();
    }

    private void Update()
    {
        //FindTarget();
        LookOnTarget();
        TryFire();
    }

    public void FindTarget()
    {
        List<Vehicle> v = viewer.GetAllVisiblelVehicles();

        float minDist = float.MaxValue;
        int index = -1;

        for (int i = 0; i < v.Count; i++)
        {
            if (v[i].HitPoint == 0) continue;
            if (v[i].TeamID == vehicle.TeamID) continue;

            float dist = Vector3.Distance(transform.position, v[i].transform.position);

            if (dist < minDist)
            {
                minDist = dist;
                index = i;
            }
        }

        if (index != -1)
        {
            target = v[index];

            VehicleDimensions vd = target.GetComponent<VehicleDimensions>();

            lookTransform = vd.GetPriorityFirePoint();
        }
        else
        {
            target = null;
            lookTransform = null;
        }
    }

    private void LookOnTarget()
    {
        if (lookTransform == null) return;

        vehicle.NetAimPoint = lookTransform.position;
    }

    private void TryFire()
    {
        if (target == null) return;

        RaycastHit hit;

        if (Physics.Raycast(firePosition.position, firePosition.forward, out hit, 1000))
        {
            if (hit.collider.isTrigger)
            {
                if (Physics.Raycast(hit.point + firePosition.forward * 0.01f, firePosition.forward, out hit, 1000 - Vector3.Distance(firePosition.position, hit.point)))
                {
                    if (hit.collider.transform.root == target.transform.root)
                        vehicle.Turret.SvFire();
                }
            }
            else
            {
                if (hit.collider.transform.root == target.transform.root)
                    vehicle.Turret.SvFire();
            }
        }
    }
}
