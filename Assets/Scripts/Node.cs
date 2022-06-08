using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteAlways]
public class Node : MonoBehaviour
{
    public List<Node> ConnectedTo = new List<Node>();
    [SerializeField] private List<Node> nodeToJump = new List<Node>();
    public bool RepositionNode = true;

    private List<Node> AllNodes;

    private Tilemap tile;
    private Vector3 posAux;
    private AIPath PathController;

    private bool mouseDown = false;
    private bool conection = false;
    private Vector3 mousePos;

    public bool hasGround;

    public List<Node> NodeToJump { get => nodeToJump; }

    private void Awake()
    {
        PathController = FindObjectOfType<AIPath>();
        tile = PathController != null ? PathController.Tile : null;
    }
    private void Start()
    {
        if (tile != null && RepositionNode)
            Reposition();


        if (Application.IsPlaying(gameObject))
        {
            hasGround = Physics2D.OverlapCircle(new Vector2(transform.position.x, transform.position.y - .7f), .1f);
        }
    }

    void Update()
    {
        if (conection && mouseDown)
        {
            if (AllNodes.Count > 0)
            {
                foreach (var node in AllNodes)
                {
                    if (node == this || !node.enabled) continue;
                    if (ConnectedTo.Contains(node)) continue;
                    float x = node.transform.position.x - mousePos.x;
                    float y = node.transform.position.y - mousePos.y;

                    if (x > -0.5f && x < 0.5f
                        && y > -0.5f && y < 0.5f)
                    {
                        ConnectedTo.Add(node);
                        AddToJump(node);
                        break;
                    }
                }
            }
            conection = false;
        }

        ConnectedTo.RemoveAll(delegate (Node o) { return o == null; });
        nodeToJump.RemoveAll(delegate (Node o) { return !ConnectedTo.Contains(o); });

        if (transform.position != posAux && !mouseDown && tile != null && RepositionNode)
            Reposition();

    }

    [MenuItem("GameObject/AIPath/Node")]
    public static GameObject InstantiateNode()
    {
        List<Node> AllNodes = FindObjectsOfType<Node>().ToList();
        GameObject ob = new GameObject("emptyGO");
        ob.AddComponent<Node>();
        string name = "Node (";

        ob.name = AllNodes.Count > 0 ? name + AllNodes.Count + ")" : name + "0)";
        return ob;
    }

    [ContextMenu("Make Conection")]
    void MakeConection()
    {
        conection = true;
        AllNodes = new List<Node>();
        AllNodes = FindObjectsOfType<Node>().ToList();
    }
    public void AddToJump(Node node)
    {
        if (nodeToJump.Contains(node))
            return;

        if (node.transform.position.y - transform.position.y > .1f)
        {
            nodeToJump.Add(node);
            return;
        }

        if (Mathf.Abs(node.transform.position.x - transform.position.x) > 2.1f && node.transform.position.y - transform.position.y < -.1f)
        {
            nodeToJump.Add(node);
            return;
        }

        if (tile != null)
        {
            if (Mathf.Abs(node.transform.position.x - transform.position.x) > 1.5f)
            {
                bool ground = false;

                int x = Mathf.Abs((int)(node.transform.position.x - transform.position.x));

                int signal = transform.position.x < node.transform.position.x ? 1 : -1;

                Vector3 tilePos = transform.position;
                tilePos.x -= tile.cellSize.x / 2;
                tilePos.y -= tile.cellSize.y / 2;

                for (int i = 1; i < x; i++)
                {
                    int variationX = i * signal;
                    if (tile.HasTile(new Vector3Int((int)tilePos.x + variationX, (int)tilePos.y - 1, (int)tilePos.z)))
                    {
                        ground = true;
                        break;
                    }
                }
                if (!ground)
                    nodeToJump.Add(node);
            }
        }
    }

    void DrawName(Vector3 worldPos, Color? colour = null)
    {
        UnityEditor.Handles.BeginGUI();
        if (colour.HasValue) GUI.color = colour.Value;
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
        Vector2 size = GUI.skin.label.CalcSize(new GUIContent(name.ToString()));
        GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), name.ToString());
        UnityEditor.Handles.EndGUI();
    }
    void DrawArrow(Vector3 pos, Vector3 direction, Color color)
    {
        float arrowHeadLength = 0.25f;
        float arrowHeadAngle = 20.0f;
        Gizmos.color = color;
        Gizmos.DrawLine(pos, direction);

        Vector3 right = Quaternion.LookRotation(pos - direction) * Quaternion.Euler(arrowHeadAngle, arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(pos - direction) * Quaternion.Euler(-arrowHeadAngle, -arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay((pos + direction) / 2, right * arrowHeadLength);
        Gizmos.DrawRay((pos + direction) / 2, left * arrowHeadLength);
    }

    void CheckNodes(Node node)
    {
        foreach (var n in node.ConnectedTo)
        {
            if (n == this)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, node.transform.position);
                return;
            }
        }
        DrawArrow(transform.position, node.transform.position, Color.white);
    }

    void Reposition()
    {
        Vector3 origin = tile.origin;
        origin.x += tile.cellSize.x / 2;
        origin.y += tile.cellSize.y / 2;
        if ((transform.position.x - origin.x) % 1 != 0 || (transform.position.y - origin.y) % 1 != 0)
        {
            if ((transform.position.x - origin.x) % 1 != 0 || (transform.position.y - origin.y) % 1 != 0)
            {
                float x = Mathf.Round(transform.position.x - origin.x) + origin.x;
                float y = Mathf.Round(transform.position.y - origin.y) + origin.y;

                Vector3 aux = new Vector3(x, y, transform.position.z);
                transform.position = posAux = aux;
            }
        }
    }

    public void OnDrawGizmos()
    {
        DrawName(transform.position, Color.black);
        Gizmos.DrawIcon(transform.position, "NodeIcon");

        foreach (var node in ConnectedTo)
        {
            if (node == null)
                return;

            CheckNodes(node);
        }
    }
    void OnEnable()
    {
        SceneView.duringSceneGui += SceneGUI;
    }

    void SceneGUI(SceneView sceneView)
    {
        Event cur = Event.current;

        if (cur.type == EventType.MouseDown || cur.type == EventType.MouseDrag)
        {
            mouseDown = true;
        }

        if (cur.type == EventType.MouseUp || cur.type == EventType.MouseLeaveWindow)
        {
            mouseDown = false;
        }

        if (conection)
        {
            HandleUtility.Repaint();
            mousePos = new Vector3(cur.mousePosition.x, cur.mousePosition.y, 0);
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
            mousePos = ray.origin;
            Handles.DrawLine(transform.position, mousePos);
        }
    }

}
