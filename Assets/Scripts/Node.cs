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
    private Tilemap tile;
    private Vector3 posAux;
    [SerializeField] private bool mouseDown = false;

    private void Awake()
    {
        tile = FindObjectOfType<Tilemap>();
    }
    private void Start()
    {
        Reposition();
    }

    void Update()
    {
        if (tile == null)
        {
            return;
        }

        if (transform.position != posAux && !mouseDown)
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

        Vector3 right = Quaternion.LookRotation(pos - direction) * Quaternion.Euler(arrowHeadAngle, 0, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(pos - direction) * Quaternion.Euler(-arrowHeadAngle, 0, 0) * new Vector3(0, 0, 1);
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
        if (cur.type == EventType.MouseDrag)
        {
            mouseDown = true;
        }

        if (cur.type == EventType.MouseUp)
        {
            mouseDown = false;
        }
    }

    IEnumerator RoundPosition(Vector3 origin)
    {
        yield return new WaitForSeconds(.1f);
    }
}
