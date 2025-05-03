using UnityEngine;

public class SizeMap : MonoBehaviour
{
    [SerializeField] private Vector2 size;
    public Vector2 Size { get { return size; } }

    public Vector3 GetNormalizePosition(Vector3 position)
    {
        return new Vector3(position.x / (size.x * 0.5f), 0, position.z / (size.y * 0.5f));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(size.x, 0, size.y));
    }
}
