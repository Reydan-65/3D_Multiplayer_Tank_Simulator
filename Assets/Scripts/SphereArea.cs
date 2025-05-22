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

    public Vector3 RandomPointInside
    {
        get
        {
            Transform[] childrensTransform = GetComponentsInChildren<Transform>();
            Transform[] childrens = new Transform[childrensTransform.Length - 1];
            System.Array.Copy(childrensTransform, 1, childrens, 0, childrensTransform.Length - 1);

            Vector3 randomPosition = Vector3.zero;
            bool positionFound = false;

            for (int i = 0; i < childrens.Length; i++)
            {
                randomPosition = childrens[i].position;

                Collider[] colliders = Physics.OverlapSphere(randomPosition, 1.0f);
                bool hadVehicle = false;

                foreach (Collider collider in colliders)
                {
                    if (collider.transform.root.GetComponent<Vehicle>() != null)
                    {
                        hadVehicle = true;
                        break;
                    }
                }

                if (!hadVehicle)
                {
                    positionFound = true;
                    break;
                }
            }

            if (!positionFound)
            {
                Debug.LogWarning("No free spawn point found!");
            }

            return randomPosition;
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
