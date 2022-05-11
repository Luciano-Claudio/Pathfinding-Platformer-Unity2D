using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BotAI : MonoBehaviour
{

    [SerializeField] private bool DrawPath = false;


    [Tooltip("Speed to apply velocity")]
    [SerializeField] private float speed;

    private float minDist;
    private float minYDist;

    [Tooltip("Max distance in blocks you can jump")]
    [SerializeField] private float maxDistanceToJump;
    private float maxDist;

    public Transform Target;

    [SerializeField] private List<Node> Path;

    private List<Node> AllNodes = new List<Node>();

    private Node ClosestNode;
    private Node TargetNode;

    private BotMovimentController controllerMovement;

    private Rigidbody2D m_Rigidbody2D;

    private LineRenderer m_LineRenderer;

    private AIPath PathController;
    private Tilemap tile;

    private bool tryAgain = false;
    private bool canJump = true;

    void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        if (m_Rigidbody2D == null)
        {
            m_Rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
            m_Rigidbody2D.freezeRotation = true;
        }
        controllerMovement = GetComponent<BotMovimentController>();
        PathController = FindObjectOfType<AIPath>();
        AllNodes = FindObjectsOfType<Node>().ToList();
        m_LineRenderer = GetComponent<LineRenderer>();

        tile = PathController != null ? PathController.Tile : null;
    }
    private void Start()
    {
        minDist = .1f;
        minYDist = tile != null ? tile.cellSize.y : 1f;
        maxDist = maxDistanceToJump + .1f;
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
        RenderLines();
        if (Target == null)
            return;

        if (!GetClosestNodeTo(Target).Equals(TargetNode) || tryAgain)
        {
            FindPath();
            tryAgain = false;
        }
        MoveTowardsPath();
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
                if (!VisitedNodes.Contains(node) && Mathf.Abs(n.transform.position.y - node.transform.position.y) <= maxDist)
                {
                    VisitedNodes.Add(node);
                    nodeAndParent.Add(node, n);
                    Q.Enqueue(node);
                }
            }
        }
        Target = null;
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

        if (Path.Count > 0)
        {
            var currentNode = Path.First();
            Transform pos = currentNode.transform;

            var xMag = Mathf.Abs(pos.position.x - transform.position.x);
            var yMag = pos.position.y - transform.position.y;



            if (currentNode && yMag <= maxDist && (xMag >= minDist || Mathf.Abs(yMag) >= minYDist))
            {
                int direction = 0;
                if (transform.position.x > pos.position.x)
                    direction = -1;
                if (transform.position.x < pos.position.x)
                    direction = 1;

                controllerMovement.HorizontalMove = direction * speed;

                controllerMovement.m_JumpForce = CalculateJumpForce(yMag, direction, pos.position);

                if (transform.position.y + .1f < pos.position.y && yMag > minDist && canJump)
                {
                    controllerMovement.IsJump = true;
                    canJump = false;
                }
            }
            else
            {
                if (Path.First() == TargetNode && Vector2.Distance(pos.position, transform.position) < minDist)
                {
                    Path.Clear();
                    Target = null;
                    canJump = true;
                }

                if (Path.Count > 1)
                {
                    Path.Remove(Path.First());
                    canJump = true;
                }
            }
        }
    }
    private Vector2 CalculateJumpForce(float jumpHeight, int direction, Vector2 target)
    {

        Vector2 forces;
        float yForce = Mathf.Sqrt(2 * jumpHeight * Physics2D.gravity.magnitude * m_Rigidbody2D.gravityScale) + .5f;

        forces = new Vector2(direction * 1, yForce);
        return forces;
    }

    void RenderLines()
    {
        if (!DrawPath || Target == null || Path.Count <= 0)
        {
            if (m_LineRenderer != null) m_LineRenderer.positionCount = 0;
            return;
        }

        if (m_LineRenderer == null)
        {
            m_LineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        m_LineRenderer.startWidth = m_LineRenderer.endWidth = .1f;
        m_LineRenderer.startColor = m_LineRenderer.endColor = Color.red;
        if (m_LineRenderer.materials.Count() != 0)
        {
            m_LineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        }

        m_LineRenderer.positionCount = Path.Count + 1;

        m_LineRenderer.SetPosition(0, transform.position);

        for (int i = 0; i < Path.Count; i++)
        {
            m_LineRenderer.SetPosition(i + 1, Path[i].transform.position);
        }
    }

    IEnumerator IsStopped()
    {
        yield return new WaitForSeconds(1);
        if (m_Rigidbody2D.velocity == Vector2.zero) tryAgain = true;
    }
}