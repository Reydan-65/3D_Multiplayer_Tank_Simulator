using Mirror;
using UnityEngine;

public enum ArmorType
{
    Vehicle,
    Module,
    None
}

public class Armor : MonoBehaviour
{
    [SerializeField] private Transform parent;

    [SerializeField] private ArmorType type;
    [SerializeField] private Destructible destructible;
    [SerializeField] private int thickness;

    public ArmorType Type => type;
    public Destructible Destructible => destructible;
    public int Thickness => thickness;

    public NetworkIdentity Owner { get; set; } // Player

    private void Awake()
    {
        transform.SetParent(parent);
    }

    public void SetDestructible(Destructible destructible)
    {
        this.destructible = destructible;
    }
}
