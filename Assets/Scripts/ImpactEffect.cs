using UnityEngine;

public class ImpactEffect : MonoBehaviour
{
    [SerializeField] private float lifeTime;

    private void Update()
    {
        Destroy(gameObject, lifeTime);
    }
}
