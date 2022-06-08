using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AIPathController : MonoBehaviour
{
    [SerializeField] private bool DrawPath = false;


    [Tooltip("Speed to apply velocity")]
    [SerializeField] private float speed;

    private float minDist;

    [Tooltip("Max distance in blocks you can jump")]
    [SerializeField] private float maxDistanceToJump;
    private float maxYDist;
    private float maxXDist;
    private float maxHypotenuse;

    public Transform Target;

    [SerializeField] private List<Node> Path;

    private List<Node> AllNodes = new List<Node>();

    private Node ClosestNode;
    private Node TargetNode;

    private Rigidbody2D m_Rigidbody2D;

    private Collider2D m_Collider2D;

    private LineRenderer m_LineRenderer;

    [SerializeField] private Node LastNode;
    private Node auxLastNode;

    private bool canJump = true;
    private bool tryAgain = false;
    private float velocity;

    public bool IsJumping { get; private set; }
    public float HorizontalMove { get; private set; }
    public Vector2 JumpForce { get; private set; }

    void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        if (m_Rigidbody2D == null)
        {
            m_Rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
            m_Rigidbody2D.freezeRotation = true;
        }
        m_Collider2D = GetComponent<Collider2D>();

        AllNodes = FindObjectsOfType<Node>().ToList();
        m_LineRenderer = GetComponent<LineRenderer>();

    }
    private void Start()
    {
        minDist = m_Collider2D.bounds.size.x / 2;
        maxYDist = maxDistanceToJump + .1f;
        maxXDist = maxYDist * 2;
        maxHypotenuse = Mathf.Sqrt(2 * Mathf.Pow(maxYDist, 2));
        velocity = speed;
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
        if (Target == null || AllNodes.Count == 0)
            return;

        if ((!GetClosestNodeTo(Target).Equals(TargetNode) || tryAgain) && canJump)
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
            if (distance < minDistance && node.hasGround)
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
                if (!VisitedNodes.Contains(node)
                    && !((Mathf.Abs(n.transform.position.y - node.transform.position.y) > maxYDist || Mathf.Abs(n.transform.position.x - node.transform.position.x) > maxXDist
                    || Vector2.Distance(n.transform.position, node.transform.position) > maxHypotenuse) && node.transform.position.y - n.transform.position.y > 0))
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
                LastNode = auxLastNode = ClosestNode;
            }
        }
    }

    void MoveTowardsPath()
    {
        HorizontalMove = 0;
        IsJumping = false;

        if (Path.Count > 0)
        {
            Node currentNode = Path.First();
            Transform pos = currentNode.transform;

            if (LastNode != auxLastNode)
            {
                velocity = speed;
                canJump = true;
            }
            auxLastNode = LastNode;

            float xMag = Mathf.Abs(pos.position.x - transform.position.x);
            float yMag = pos.position.y - transform.position.y;

            if (!(currentNode && yMag <= maxYDist && (xMag >= minDist || Mathf.Abs(yMag) >= minDist)))
            {
                LastNode = Path.First();
                if (LastNode == TargetNode && Vector2.Distance(pos.position, transform.position) < minDist)
                {
                    Path.Clear();
                    Target = null;
                }

                if (Path.Count > 1)
                {
                    Path.Remove(LastNode);
                }

                return;
            }

            float playerRelativeNode = Mathf.Abs(transform.position.y - LastNode.transform.position.y);
            float yMagNode = LastNode != currentNode ? pos.position.y - LastNode.transform.position.y : 0;
            bool direction = canJump && m_Rigidbody2D.velocity.y < -1 ? false : true;

            if (direction)
            {
                if (transform.position.x > pos.position.x)
                    HorizontalMove = -1;
                if (transform.position.x < pos.position.x)
                    HorizontalMove = 1;
            }

            //Direction = Mathf.Abs(yMag) >= maxYDist && m_Rigidbody2D.velocity.y < -1 ? 0 : Direction;

            if (LastNode.NodeToJump.Contains(currentNode)
                && playerRelativeNode <= minDist && canJump)
            {
                IsJumping = true;
                float aux = (xMag + 1) * 5;
                velocity = aux > velocity ? aux : velocity;
                JumpForce = CalculateJumpForce(xMag, yMag);
                canJump = false;
            }
            HorizontalMove *= velocity * 10;
        }
    }
    private Vector2 CalculateJumpForce(float xDistance, float jumpHeight)
    {
        float distance = Mathf.Abs(jumpHeight);
        float extra = .5f;

        if (distance >= .5f && xDistance >= 1.5f)
        {
            if (jumpHeight > 0)
            {
                distance = distance >= xDistance ? distance : xDistance;
                extra = 0;
            }
            else
            {
                distance = xDistance > 2 ? (xDistance - 1) / 2 : 1;
                extra = 0;
            }
        }
        else if (distance < .5f)
            distance = (xDistance + 1) / 2;

        if (distance > maxYDist)
            distance = maxYDist;

        Vector2 force = new Vector2(0, (Mathf.Sqrt(2 * distance * Physics2D.gravity.magnitude * m_Rigidbody2D.gravityScale) * m_Rigidbody2D.mass) + extra);

        return force;
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
        if (m_Rigidbody2D.velocity == Vector2.zero)
        {
            canJump = true;
            tryAgain = true;
        }
    }
}