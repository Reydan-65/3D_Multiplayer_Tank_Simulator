using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    [SerializeField] private float damage;
    [SerializeField] private float damageInterval = 2f;

    private List<int> damagedObjects = new List<int>();
    private Dictionary<int, Coroutine> damageCoroutines = new Dictionary<int, Coroutine>();

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (NetworkSessionManager.Instance.IsServer == false) return;

        Vehicle v = other.transform.root.GetComponent<Vehicle>();

        if (v == null) return;

        if (damagedObjects.Contains((int)v.netId))
            return;

        Destructible d = v.GetComponent<Destructible>();

            if (d != null)
            {
                if (d.HitPoint > 0)
                {
                    damagedObjects.Add((int)v.netId);
                    Coroutine damageCoroutine = StartCoroutine(DamageOverTime(v, d));
                    damageCoroutines[(int)v.netId] = damageCoroutine;
                }
            }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;
        if (NetworkSessionManager.Instance.IsServer == false) return;

        Vehicle v = other.transform.root.GetComponent<Vehicle>();

        if (v == null) return;

        StopDamageOverTime(v);
    }

    private void StopDamageOverTime(Vehicle vehicle)
    {
        if (damagedObjects.Contains((int)vehicle.netId))
        {
            damagedObjects.Remove((int)vehicle.netId);

            if (damageCoroutines.TryGetValue((int)vehicle.netId, out Coroutine damageCoroutine))
            {
                StopCoroutine(damageCoroutine);
                damageCoroutines.Remove((int)vehicle.netId);
            }
        }
    }

    private IEnumerator DamageOverTime(Vehicle v, Destructible d)
    {
        while (d.HitPoint > 0)
        {
            d.SvApplyDamage((int)damage);
            yield return new WaitForSeconds(damageInterval);
        }

        StopDamageOverTime(v);
    }
}
