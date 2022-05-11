using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AIPath : MonoBehaviour
{
    [SerializeField] private Tilemap tile;


    [ContextMenu("Create Nodes")]
    void CreateNodes()
    {
        tile = FindObjectOfType<Tilemap>();
        if (tile == null)
        {
            Debug.Log("You must have a tilemap in your project");
            return;
        }

        Vector3 origin = tile.origin;
        origin.x += tile.cellSize.x / 2;
        origin.y += tile.cellSize.y / 2;

        //Debug.Log(origin);
        Vector3 bounds = tile.size;
        bounds.x -= Mathf.Abs(origin.x);
        bounds.y -= Mathf.Abs(origin.y);
        //Debug.Log(bounds);

        for (int u = 0; u < Mathf.Abs(origin.y) + Mathf.Abs(bounds.y); u++)
        {
            for (int i = 0; i < Mathf.Abs(origin.x) + Mathf.Abs(bounds.x); i++) {
                Vector3 pos = new Vector3(origin.x + i, origin.y + u, 0);
                if (!tile.HasTile(new Vector3Int(tile.origin.x + i, tile.origin.y + u, 0)) && tile.HasTile(new Vector3Int(tile.origin.x + i, tile.origin.y + u - 1, 0)))
                {
                    InstantiateNodes(pos);
                }
            }
        }
    }

    void InstantiateNodes(Vector3 position)
    {
        List<Node> AllNodes = FindObjectsOfType<Node>().ToList();

        foreach (var n in AllNodes)
        {
            if (n.transform.position == position)
            {
                return;
            }
        }

        GameObject node = Node.InstantiateNode();
        node.transform.parent = transform;
        node.transform.position = position;
    }
}
