using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotMovimentController : MonoBehaviour
{
    //public float m_JumpForce = 400f;
    //public float runSpeed = 40f;


    [SerializeField] private LayerMask m_WhatIsGround;                      
    [SerializeField] private Transform m_GroundCheck; 

    private float m_MovementSmoothing = .05f;
    const float k_GroundedRadius = .2f; 
    private bool m_Grounded;       
    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true;  
    private Vector3 m_Velocity = Vector3.zero;
    private int jumpQuantity = 2;


    public float HorizontalMove { get; set; }
    public float m_JumpForce { get; set; }
    public bool IsJump { get; set; }



    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        HorizontalMove = 0;
        IsJump = false;
        /*
        m_JumpForce = 700;
        m_runSpeed = 25;*/
    }

    void Update()
    {
        /*
        HorizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;



        if (Input.GetButtonDown("Jump"))
        {
            _isJump = true;
        }*/
    }

    private void FixedUpdate()
    {
        m_Grounded = false;
        Collider2D[] colliders;
        colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_Grounded = true;
            }
        }

        Move(HorizontalMove * Time.deltaTime, IsJump);
        IsJump = false;

        if (m_Grounded) jumpQuantity = 2;
    }


    void Move(float move, bool jump)
    {

        Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
        m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

        if (move > 0 && !m_FacingRight)
        {
            Flip();
        }
        else if (move < 0 && m_FacingRight)
        {
            Flip();
        }
        if (jump && jumpQuantity > 0)
        {
            m_Rigidbody2D.velocity = Vector2.zero;
            m_Grounded = false;
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
            jumpQuantity--;
        }

    }


    private void Flip()
    {
        m_FacingRight = !m_FacingRight;

        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(m_GroundCheck.position, k_GroundedRadius);
    }
}
