using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class BotAI : MonoBehaviour
{
    public bool DrawPath = false;
    public float minDist;
    public float maxDist;

    public List<Node> AllNodes = new List<Node>();

    public Node ClosestNode;
    public Node TargetNode;

    public Transform Target;

    public List<Node> Path;


    [SerializeField] private float speed;

    [SerializeField] private float[] JumpForces;
    public float jumpForce;
    public bool jump = false;

    private BotMovimentController controllerMovement;
    private Rigidbody2D m_Rigidbody2D;
    private LineRenderer line;
    private bool tryAgain = false;

    void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        controllerMovement = GetComponent<BotMovimentController>();
        AllNodes = FindObjectsOfType<Node>().ToList();
        line = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (m_Rigidbody2D.velocity == Vector2.zero)
        {
            StartCoroutine(IsStopped());
        }
    }

    void FixedUpdate()
    {
        if (Target == null)
            return;

        if (!GetClosestNodeTo(Target).Equals(TargetNode) || tryAgain)
        {
            FindPath();
            tryAgain = false;
        }
        MoveTowardsPath();
        RenderLines();
    }

    Node GetClosestNodeTo(Transform t)
    {
        Node fNode = null;
        float minDistance = Mathf.Infinity;
        foreach (var node in AllNodes)
        {
            float distance = (node.transform.position - t.position).sqrMagnitude;
            if (distance < minDistance)
            {
                minDistance = distance;
                fNode = node;
            }

        }
        return fNode;
    }

    void FindPath()
    {
        Path.Clear();

        TargetNode = GetClosestNodeTo(Target);
        ClosestNode = GetClosestNodeTo(transform);

        if (TargetNode == null || ClosestNode == null)
        {
            Debug.Log("Something went Wrong");
            return;
        }

        HashSet<Node> VisitedNodes = new HashSet<Node>();
        Queue<Node> Q = new Queue<Node>();
        Dictionary<Node, Node> nodeAndParent = new Dictionary<Node, Node>();

        Q.Enqueue(ClosestNode);

        while (Q.Count > 0)
        {
            Node n = Q.Dequeue();
            if (n.Equals(TargetNode))
            {
                MakePath(nodeAndParent);
                return;
            }
            foreach (var node in n.ConnectedTo)
            {
                if (!VisitedNodes.Contains(node))
                {
                    VisitedNodes.Add(node);
                    nodeAndParent.Add(node, n);
                    Q.Enqueue(node);
                }
            }
        }
    }

    void MakePath(Dictionary<Node, Node> nap)
    {
        if (nap.Count > 0)
        {
            if (nap.ContainsKey(TargetNode) && nap.ContainsValue(ClosestNode))
            {
                Node cNode = TargetNode;
                while (cNode != ClosestNode)
                {
                    Path.Add(cNode);
                    cNode = nap[cNode];
                }

                //Path.Add(ClosestNode);
                Path.Reverse();
            }
        }
    }

    void MoveTowardsPath()
    {
        controllerMovement.HorizontalMove = 0;
        jump = false;

        if (Path.Count > 0)
        {
            var currentNode = Path.First();
            Transform pos = currentNode.transform;
            /*
            if (Path.Count == 1)
                pos = Target;
            */
            var xMag = Mathf.Abs(pos.position.x - transform.position.x);
            var yMag = pos.position.y - transform.position.y;



            if (currentNode && xMag >= minDist && yMag <= maxDist)
            {
                jumpForce = yMag > 0 ? JumpForces[Mathf.RoundToInt(yMag)] : 0;
                //Debug.Log(jumpForce);
                controllerMovement.m_JumpForce = jumpForce;
                if (transform.position.x > pos.position.x)
                {
                    controllerMovement.HorizontalMove = -1 * speed;
                }
                if (transform.position.x < pos.position.x)
                {
                    controllerMovement.HorizontalMove = 1 * speed;
                }

                if (transform.position.y + .1f < pos.position.y && yMag > minDist)
                {
                    jump = true;
                    controllerMovement.IsJump = jump;
                }
            }
            else
            {
                if (Path.Count > 1)
                {
                    Path.Remove(Path.First());
                }

                if (Path.First() == TargetNode && Vector2.Distance(pos.position, transform.position) < minDist)
                {
                    Path.Clear();
                    Target = null;
                }
            }
        }
    }

    void RenderLines()
    {
        if (!(Path.Count > 0))
            return;

        if (!DrawPath)
        {
            if (line != null) line.positionCount = 0;
            return;
        }

        if (line == null)
        {
            line = gameObject.AddComponent<LineRenderer>();
        }
        line.startWidth = line.endWidth = .1f;
        line.startColor = line.endColor = Color.red;
        if (line.materials.Count() != 0)
        {
            line.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        }

        line.positionCount = Path.Count + 1;

        line.SetPosition(0, transform.position);

        for (int i = 0; i < Path.Count; i++)
        {
            line.SetPosition(i + 1, Path[i].transform.position);
        }
    }

    IEnumerator IsStopped()
    {
        yield return new WaitForSeconds(1);
        if (m_Rigidbody2D.velocity == Vector2.zero) tryAgain = true;
    }
}