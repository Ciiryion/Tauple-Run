using UnityEngine;

public enum TileType
{
    STRAIGHT,
    LEFT,
    RIGHT,
    SIDEWAYS,

    OBSTACLE
}

public class Tile : MonoBehaviour
{
    public TileType type;
    public Transform pivot;
}
