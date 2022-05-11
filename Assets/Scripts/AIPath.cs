using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AIPath : MonoBehaviour
{
    [SerializeField] private Tilemap tile;

    public Tilemap Tile { get => tile; private set => tile = value; }

    [ContextMenu("Create Nodes")]
    void CreateNodes()
    {
        if (Tile == null)
        {
            Debug.Log("You must have a tilemap in your project");
            return;
        }

        Vector3 origin = Tile.origin;
        origin.x += Tile.cellSize.x / 2;
        origin.y += Tile.cellSize.y / 2;

        //Debug.Log(origin);
        Vector3 bounds = Tile.size;
        bounds.x -= Mathf.Abs(origin.x);
        bounds.y -= Mathf.Abs(origin.y);
        //Debug.Log(bounds);

        for (int u = 0; u < Mathf.Abs(origin.y) + Mathf.Abs(bounds.y); u++)
        {
            for (int i = 0; i < Mathf.Abs(origin.x) + Mathf.Abs(bounds.x); i++) {
                Vector3 pos = new Vector3(origin.x + i, origin.y + u, 0);
                if (!Tile.HasTile(new Vector3Int(Tile.origin.x + i, Tile.origin.y + u, 0)) && Tile.HasTile(new Vector3Int(Tile.origin.x + i, Tile.origin.y + u - 1, 0)))
                {
                    InstantiateNodes(pos);
                }
            }
        }
    }

    [ContextMenu("Make Conections")]
    void MakeConections()
    {
        List<Node> AllNodes = new List<Node>();
        AllNodes = GetComponentsInChildren<Node>().ToList();
        if (AllNodes.Count == 0)
        {
            Debug.Log("You must build your nodes and place them chiddren of this transform, or use \"Create Nodes\" in Context Menu");
            return;
        }
        foreach(var node in AllNodes)
        {
            node.MakeConections();
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
