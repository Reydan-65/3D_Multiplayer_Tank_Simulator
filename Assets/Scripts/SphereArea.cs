using UnityEngine;

public class SphereArea : MonoBehaviour
{
    [SerializeField] private float radius;
    [SerializeField] private Color color = Color.green;

    public Vector3 RandomInside
    {
        get
        {
            var pos = Random.insideUnitSphere * radius + transform.position;

            pos.y = transform.position.y;

            return pos;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
