using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
    [Header("Terrain Generation Parameters")]
    [SerializeField] private int tileStartCount = 10;
    [SerializeField] private int minimumStraightTiles = 3;
    [SerializeField] private int maximumStraightTiles = 15;

    [Header("Obstacle Generation")]
    [Tooltip("Probabilité d'apparition d'une tuile obstacle au lieu d'une tuile normale.")]
    [Range(0f, 1f)] 
    [SerializeField] private float obstacleProbability = 0.2f;

    [Header("Terrain prefabs")]
    [SerializeField] private GameObject startingTile;
    [SerializeField] private List<GameObject> turnTiles;
    [SerializeField] private List<GameObject> obstacleTiles;
    
    private Vector3 currentTileLocation = Vector3.zero;
    private Vector3 currentTileDirection = Vector3.forward;
    private GameObject previousTile;

    // Plus besoin d'une liste séparée pour les obstacles, tout est une tuile !
    private List<GameObject> activeTiles = new List<GameObject>();

    private void Start()
    {
        activeTiles = new List<GameObject>();

        Random.InitState(System.DateTime.Now.Millisecond);

        // On génère le début du chemin avec des tuiles normales uniquement
        for (int i = 0; i < tileStartCount; i++)
        {
            SpawnTile(startingTile.GetComponent<Tile>());
        }

        SpawnTile(SelectRandomGameObjectFromList(turnTiles).GetComponent<Tile>());
    }

    #region Tile management
    private void SpawnTile(Tile tile)
    {
        Quaternion newTileRotation = tile.gameObject.transform.rotation * Quaternion.LookRotation(currentTileDirection, Vector3.up);

        previousTile = Instantiate(tile.gameObject, currentTileLocation.Snap(), newTileRotation);
        activeTiles.Add(previousTile);

        if (tile.type == TileType.STRAIGHT || tile.type == TileType.OBSTACLE)
        {
            currentTileLocation += Vector3.Scale(previousTile.GetComponent<Renderer>().bounds.size, currentTileDirection);
        }
    }

    private void DeletePreviousTile()
    {
        while (activeTiles.Count > 1)
        {
            GameObject tile = activeTiles[0];
            activeTiles.RemoveAt(0);
            Destroy(tile);
        }
    }

    public void AddDirection(Vector3 direction)
    {
        currentTileDirection = direction;
        DeletePreviousTile();

        Vector3 tilePlacementScale;
        if (previousTile.GetComponent<Tile>().type == TileType.SIDEWAYS) // Unused for now
        {
            tilePlacementScale = Vector3.Scale(previousTile.GetComponent<Renderer>().bounds.size / 2 +
                (Vector3.one * startingTile.GetComponent<BoxCollider>().size.z / 2), currentTileDirection);
        }
        else // Left or Right tiles
        {
            tilePlacementScale = Vector3.Scale(previousTile.GetComponent<Renderer>().bounds.size - (Vector3.one * 0.5f) +
                (Vector3.one * startingTile.GetComponent<BoxCollider>().size.z / 2), currentTileDirection);
        }

        currentTileLocation += tilePlacementScale;

        int currentPathLength = Random.Range(minimumStraightTiles, maximumStraightTiles);
        for (int i = 0; i < currentPathLength; i++)
        {
            GameObject tileToSpawn = startingTile;

            // Don't spawn an obstacle on first tile after turning
            if (i != 0 && obstacleTiles != null && obstacleTiles.Count > 0)
            {
                if (Random.value <= obstacleProbability)
                {
                    tileToSpawn = SelectRandomGameObjectFromList(obstacleTiles);
                }
            }

            SpawnTile(tileToSpawn.GetComponent<Tile>());
        }

        SpawnTile(SelectRandomGameObjectFromList(turnTiles).GetComponent<Tile>());
    }
    #endregion

    #region Utils
    private GameObject SelectRandomGameObjectFromList(List<GameObject> list)
    {
        if(list.Count == 0) return null;
        return list[Random.Range(0, list.Count)];
    }
    #endregion
}