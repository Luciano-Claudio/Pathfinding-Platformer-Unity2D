using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public int XMovement;
    public bool JumpPress;

    public Transform groundCheck;
    public LayerMask whatIsGround;
    const float k_GroundedRadius = .1f;

    public float lerpSpeed;

    public float MovementSpeed;
    public float JumpForce;

    private Rigidbody2D Rig;
    private bool canJump;
    // Start is called before the first frame update
    void Start()
    {
        Rig = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
      /*  XMovement = (int)Input.GetAxisRaw("Horizontal");
        if(Input.GetButtonDown("Jump"))
            JumpPress = true;
      */
    }

    private void FixedUpdate()
    {
        canJump = false;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, k_GroundedRadius, whatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                canJump = true;
            }
        }

        Rig.velocity = new Vector2(XMovement * MovementSpeed, Rig.velocity.y);

        if(JumpPress)
            Jump();
    }

    void Jump()
    {
        if (canJump)
            Rig.AddForce(new Vector2(0, JumpForce));
        JumpPress = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, k_GroundedRadius);
    }
}
