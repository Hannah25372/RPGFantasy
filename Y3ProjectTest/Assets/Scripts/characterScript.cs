using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class characterScript : MonoBehaviour
{

    
    public Animator animator;      

    public float speed = 2.0f;
    public float jumpHeight = 5.0f;
    private Rigidbody rb;
    public bool isGrounded;

    public State state;
    public Vector3 move;
    public float rotationSpeed;

    [SerializeField] Transform player;
    public Vector3 _velocity;
    [SerializeField] float castDistance;
    [SerializeField] Vector2 boxSize;
    [SerializeField] LayerMask groundLayer;

    float horizontalSpeed = 2.0f;
    bool mouseMovement;



    public enum State
    {
        IDLE, MOVING, JUMPING, CUTSCENE
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        state = State.IDLE;
    }


    void FixedUpdate()
    {

        HandleMovement();
    }


    public void HandleMovement()
    {

        //animator.SetBool("isWalking", true);

        //if state becomes CUTSCENE the players movement gets disabled
        if (state == State.CUTSCENE) return;


        //mouse turning
        if (Input.GetKey("m"))
        {
            mouseMovement = true;
            CursorOff();
        }
        if (Input.GetKey("n"))
        {
            mouseMovement = false;
            CursorOn();
        }
        if (mouseMovement)
        {
            float h = horizontalSpeed * Input.GetAxis("Mouse X");
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + h, 0);
        }

        move = rb.velocity;

        //movement for left and right, and changing state between moving and idle

        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        var move1 = (transform.forward * vertical * speed) + (transform.right * horizontal * speed);
        move.x = move1.x;
        move.z = move1.z;
        state = move.x == 0 && move.z == 0 ? State.IDLE : State.MOVING;


        //jump
        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            move.y = jumpHeight;
            state = State.JUMPING;
            //jumpSound.Play();
        }

        //if (Input.GetAxis("Jump") > 0 && isGrounded)
        //{         
        //        move.y = jumpHeight;
        //        state = State.JUMPING;
        //        //jumpSound.Play();
        //}

        rb.velocity = move;


    }

    // This function is a callback for when an object with a collider collides with this objects collider.
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == ("Ground"))
        {
            isGrounded = true;
        }
    }
    // This function is a callback for when the collider is no longer in contact with a previously collided object.
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == ("Ground"))
        {
            isGrounded = false;
        }
    }

    public void CursorOn()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CursorOff()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


}
