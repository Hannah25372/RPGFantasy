using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkNPC : MonoBehaviour
{

    public int maxX;
    public int minX;
    public int maxZ;
    public int minZ;

    public int speed;
    public float rotateDirection;
    public bool rotating;

    public float walkingTime;

    public Vector3 movement;
    Rigidbody rb;
    public Animator animator;
    public State state;
    public float time;
    public float duration;

    public enum State
    {
        IDLE, MOVING, TALKING
    }


    // Start is called before the first frame update
    void Start()
    {
        animator.SetBool("isWalking", true);
        rb = GetComponent<Rigidbody>();

        maxX = 7;
        minX = -7;
        maxZ = 7;
        minZ = -7;
        speed = 1;
        rotateDirection = 180f;
        rotating = false;

        duration = 3f;

        state = State.MOVING;
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (state == State.MOVING)
        {
            //Moves
            moves();


            //If exceeds the boundary they can walk in, NPC turns
            if (transform.position.x > maxX || transform.position.x < minX || transform.position.z > maxZ || transform.position.z < minZ)
            {
                rotates(150, 210);
            }

            //Every second has a 1/200 chance of changing direction
            if (Random.Range(0, 201) == 0)
            {
                rotates(-90, 90);
            }
        }

        if (state == State.TALKING)
        {
            if (time < duration)
            {
                time += Time.deltaTime;
            } else
            {
                time = 0f;
                animator.SetBool("isWalking", true);
                state = State.MOVING;
                rotates(150, 210);
                moves();
            }
        }
        


    }

    void moves()
    {
        movement = rb.velocity;
        var move1 = (transform.forward * speed);
        movement.x = move1.x;
        movement.z = move1.z;
        rb.velocity = movement;
    }

    void rotates(int low, int high)
    {
        rotateDirection = Random.Range(low, high);
        transform.Rotate(new Vector3(0, rotateDirection, 0));
        moves();
    }

    private void OnCollisionEnter(Collision collision)
    {

        switch (collision.gameObject.tag)
        {
            case "Player":
                Debug.Log("Crashed into Player");
                animator.SetBool("isWalking", false);
                state = State.TALKING;

                break;
            case "NPC":
                Debug.Log("Crashed into NPC");
                animator.SetBool("isWalking", false);
                state = State.TALKING;


                break;
            default:
                rotates(150, 210);
                break;
        }

        
    }

    //private void OnCollisionExit(Collision collision)
    //{
    //    switch (collision.gameObject.tag)
    //    {
    //        case "player":

    //            break;
    //        case "NPC":
    //            state = State.MOVING;
                

    //            break;
    //        default:
    //            rotates(150, 210);
    //            break;
    //    }
    //}

}
