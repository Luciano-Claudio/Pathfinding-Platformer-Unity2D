using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BotAI : MonoBehaviour
{
    public float minDist;
    public float maxDist;

    public List<Node> AllNodes = new List<Node>();

    public Node ClosestNode;
    public Node TargetNode;

    public Transform Target;

    public List<Node> Path;
    public List<Node> Lines;

    public BotMovimentController controllerMovement;

    [SerializeField] private float speed;

    [SerializeField] private float[] JumpForces;
    public float jumpForce;
    public bool jump = false;

    private Rigidbody2D m_Rigidbody2D;
    private bool tryAgain = false;
    private Node removeLine = null;

    void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        controllerMovement = GetComponent<BotMovimentController>();
        AllNodes = FindObjectsOfType<Node>().ToList();
    }

    void Update()
    {
        if(m_Rigidbody2D.velocity == Vector2.zero)
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
    }

    Node GetClosestNodeTo(Transform t)
    {
        Node fNode = null;
        float minDistance = Mathf.Infinity;
        foreach (var node in AllNodes)
        {
            float distance = (node.transform.position - t.position).sqrMagnitude;
            if(distance < minDistance)
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
            foreach(var node in n.ConnectedTo)
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
        RemoveLines();
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
        Lines = new List<Node>(Path);
        for (int i = 0; i < Lines.Count - 1; i++)
        {
            Path[i].Line.SetPosition(0, Path[i].transform.position);
            Path[i].Line.SetPosition(1, Path[i+1].transform.position);
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
                    if (removeLine != null)
                    {
                        removeLine.Line.SetPosition(0, Vector3.zero);
                        removeLine.Line.SetPosition(1, Vector3.zero);
                    }
                    removeLine = Path.First();
                    Path.Remove(Path.First());
                }

                if (Path.First() == TargetNode && Vector2.Distance(pos.position, transform.position) < minDist)
                {
                    if (removeLine != null)
                    {
                        removeLine.Line.SetPosition(0, Vector3.zero);
                        removeLine.Line.SetPosition(1, Vector3.zero);
                    }
                    Path.Clear();
                    Target = null;
                }
            }
        }
    }

    void RemoveLines()
    {
        foreach (var node in Lines)
        {
            node.Line.SetPosition(0, Vector3.zero);
            node.Line.SetPosition(1, Vector3.zero);
        }
        Lines.Clear();
    }
    IEnumerator IsStopped()
    {
        yield return new WaitForSeconds(1);
        if (m_Rigidbody2D.velocity == Vector2.zero) tryAgain = true;
    }
}
