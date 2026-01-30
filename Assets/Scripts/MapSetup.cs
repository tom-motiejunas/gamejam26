using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapSetup : MonoBehaviour
{
    // This script is mainly for Editor use to quickly setup the scene
#if UNITY_EDITOR
    [MenuItem("Tools/Setup Pacman Map")]
    public static void SetupMap()
    {
        GameObject gridGo = GameObject.Find("Grid");
        if (gridGo == null)
        {
            gridGo = new GameObject("Grid");
            gridGo.AddComponent<Grid>();
        }

        CreateTilemap(gridGo, "Floor", 0);
        CreateTilemap(gridGo, "Walls", 1, true);

        Debug.Log("Map setup complete.");
    }

    static void CreateTilemap(GameObject parent, string name, int sortingOrder, bool needsCollision = false)
    {
        Transform child = parent.transform.Find(name);
        GameObject go;
        if (child == null)
        {
            go = new GameObject(name);
            go.transform.SetParent(parent.transform);
        }
        else
        {
            go = child.gameObject;
        }

        if (!go.GetComponent<Tilemap>()) go.AddComponent<Tilemap>();
        
        TilemapRenderer tr = go.GetComponent<TilemapRenderer>();
        if (!tr) tr = go.AddComponent<TilemapRenderer>();
        tr.sortingOrder = sortingOrder;

        if (needsCollision)
        {
            if (!go.GetComponent<TilemapCollider2D>()) go.AddComponent<TilemapCollider2D>();
            if (!go.GetComponent<CompositeCollider2D>())
            {
                CompositeCollider2D cc = go.AddComponent<CompositeCollider2D>();
                cc.geometryType = CompositeCollider2D.GeometryType.Polygons; // Better for walls
                go.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            }
            // Ensure TilemapCollider uses Composite
            go.GetComponent<TilemapCollider2D>().usedByComposite = true;
            
            // Set layer to something detectable if needed, e.g., Default for now
        }
    }
#endif
}
