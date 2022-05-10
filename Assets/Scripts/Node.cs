using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{

    public List<Node> ConnectedTo = new List<Node>();
    public LineRenderer Line;

    private void Awake()
    {
        Line = GetComponent<LineRenderer>();
    }
    private void Start()
    {
        Line.positionCount = 2;
        Line.SetPosition(0, Vector3.zero);
        Line.SetPosition(1, Vector3.zero);
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
}
