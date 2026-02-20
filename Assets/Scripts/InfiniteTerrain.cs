using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
    [Header("Terrain Generation parameters")]
    [SerializeField] private int tileStartCount = 10;
    [SerializeField] private int minimumStraightTiles = 3;
    [SerializeField] private int maximumStraightTiles = 15;


    [Header("Terrain prefabs")]
    [SerializeField] private GameObject startingTile;
    [SerializeField] private List<GameObject> turnTiles;
    [SerializeField] private List<GameObject> obstacles;

    
    private Vector3 currentTileLocation = Vector3.zero;
    private Vector3 currentTileDirection = Vector3.forward;
    private GameObject previousTile;

    private List<GameObject> activeTiles = new List<GameObject>();
    private List<GameObject> activeObstacles = new List<GameObject>();


    private void Start()
    {
        activeTiles = new List<GameObject>();
        activeObstacles = new List<GameObject>();

        Random.InitState(System.DateTime.Now.Millisecond);

        for (int i = 0; i < tileStartCount; i++)
        {
            SpawnTile(startingTile.GetComponent<Tile>(), false);
        }

        SpawnTile(SelectRandomGameObjectFromList(turnTiles).GetComponent<Tile>());
    }

    #region Tile management
    private void SpawnTile(Tile tile, bool spawnObstacles = false)
    {
        Quaternion newTileRotation = tile.gameObject.transform.rotation * Quaternion.LookRotation(currentTileDirection, Vector3.up);

        previousTile = Instantiate(tile.gameObject, currentTileLocation, newTileRotation);
        activeTiles.Add(previousTile);

        if(spawnObstacles) SpawnObstacle();

        if(tile.type == TileType.STRAIGHT)
            currentTileLocation += Vector3.Scale(previousTile.GetComponent<Renderer>().bounds.size, currentTileDirection);
    }

    private void DeletePreviousTile()
    {
        while (activeTiles.Count != 0)
        {
            GameObject tile = activeTiles[0];
            activeTiles.RemoveAt(0);
            Destroy(tile);
        }
        
        while (activeObstacles.Count != 0)
        {
            GameObject obstacle = activeObstacles[0];
            activeObstacles.RemoveAt(0);
            Destroy(obstacle);
        }
    }

    public void AddDirection(Vector3 direction)
    {
        currentTileDirection = direction;
        DeletePreviousTile();

        Vector3 tilePlacementScale;
        if(previousTile.GetComponent<Tile>().type == TileType.SIDEWAYS)
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
            SpawnTile(startingTile.GetComponent<Tile>(), i != 0);
        }

        SpawnTile(SelectRandomGameObjectFromList(turnTiles).GetComponent<Tile>());
    }
    #endregion

    #region Obstacle management
    private void SpawnObstacle()
    {
        if (Random.value > 0.2f) return; // 20% chance to spawn an obstacle

        GameObject obstaclePrefab = SelectRandomGameObjectFromList(obstacles);
        Quaternion newObjectRotation = obstaclePrefab.gameObject.transform.rotation * Quaternion.LookRotation(currentTileDirection, Vector3.up);

        GameObject obstacle = Instantiate(obstaclePrefab, currentTileLocation, newObjectRotation);
        activeObstacles.Add(obstacle);
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
