using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotMovimentController : MonoBehaviour
{

    private float m_MovementSmoothing = .05f;
    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true;  
    private Vector3 m_Velocity = Vector3.zero;

    private AIController Control;

    public float HorizontalMove { get; set; }
    public Vector2 m_JumpForce { get; set; }
    public bool IsJump { get; set; }



    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        Control = GetComponent<AIController>();
    }
    private void Start()
    {
        HorizontalMove = 0;
        IsJump = false;
    }

    private void FixedUpdate()
    {
        Move(HorizontalMove * Time.deltaTime, IsJump);
        IsJump = false;

    }


    void Move(float move, bool jump)
    {
        m_Rigidbody2D.velocity = new Vector3(move, m_Rigidbody2D.velocity.y);
        //Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
        //m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

        if (move > 0 && !m_FacingRight)
        {
            Flip();
        }
        else if (move < 0 && m_FacingRight)
        {
            Flip();
        }
        if (jump)
        {
            m_Rigidbody2D.velocity = Vector2.zero;
            m_Rigidbody2D.AddForceAI(m_JumpForce);
        }

    }


    private void Flip()
    {
        m_FacingRight = !m_FacingRight;

        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }


}
