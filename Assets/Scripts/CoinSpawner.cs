using UnityEngine;
using UnityEngine.Tilemaps;

public class CoinSpawner : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase coinTile;
    public GameObject coinPrefab;

    void Start()
    {
        if (tilemap == null || coinTile == null || coinPrefab == null)
        {
            Debug.LogError("CoinSpawner: Missing references!");
            return;
        }

        SpawnCoins();
    }

    void SpawnCoins()
    {
        BoundsInt bounds = tilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(cellPos);

                if (tile == coinTile)
                {
                    // Convert cell position to world position (center of tile)
                    Vector3 worldPos = tilemap.GetCellCenterWorld(cellPos);

                    // Instantiate coin
                    Instantiate(coinPrefab, worldPos, Quaternion.identity, transform);

                    // Remove the static tile
                    tilemap.SetTile(cellPos, null);
                }
            }
        }
    }
}
