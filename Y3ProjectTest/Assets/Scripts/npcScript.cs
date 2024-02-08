using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class npcScript : MonoBehaviour
{

    simulation sim;

    //bounds of walking
    public int maxX;
    public int minX;
    public int maxZ;
    public int minZ;

    public int speed;
    private float rotateDirection;
    //private bool rotating;

    public float walkingTime;

    public Vector3 movement;
    Rigidbody rb;
    public Animator animator;
    public State state;
    public float time;
    public float duration;

    //character attributes
    public int ID;
    public int[] friendLevels;
    public List<Traits> traits;
    public int ATK;
    public int AC;
    public int HP;
    public Town currentTown;
    public Relationship[] relationships;

    public int getHP() { return HP; }
    public int getAC() { return AC; }
    public int getATK() { return ATK; }
    public int getID() { return ID; }
    public List<Traits> getTraits() { return traits; }
    public int[] getFriendLevels() { return friendLevels; }
    public State getState() { return state; }
    public Town getTown() { return currentTown; }
    public void setHP(int num) { HP = num; }
    public void setAC(int num) { AC = num; }
    public void setATK(int num) { ATK = num; }
    public void setID(int num) { ID = num; }
    public void setFriendLevel(int num, int index) { friendLevels[index] = num; }
    public void setState(State _state) { state = _state; }
    public void setTown(Town _town) { currentTown = _town; }



    public enum State
    {
        IDLE, MOVING, TALKING, FIGHTING, STEALING
    }

    public enum Town
    {
        Village1, Village2, Lake, Mountain, Dessert
    }

    public enum Traits
    {
        Aggressive, Friendly, Loyal, Selfish
    }

    public enum Relationship
    {
        Neutral, Enemy, Friend
    }


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        traits = new List<Traits>();
        friendLevels = new int[8];
        relationships = new Relationship[8];

        changeTownBounds(Town.Village1);

        speed = 2;
        rotateDirection = 180f;

        duration = 3f;

        state = State.MOVING;
        animator.SetBool("isWalking", true);
        


        

        
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

      

        if (state == State.MOVING)
        {
            randomMovementInTown();
        }

        if (state == State.TALKING)
        {
            talk();
        }
        


    }


    void moveToLocation()
    {

    }

    void steal()
    {

    }

    void give() 
    { }

    void pickUp()
    {

    }

    void drop()
    {

    }

    void loot()
    {

    }

    void kill()
    {

    }

    void talk()
    {
        if (time < duration)
        {
            time += Time.deltaTime;
        }
        else
        {
            time = 0f;
            animator.SetBool("isWalking", true);
            state = State.MOVING;
            rotates(150, 210);
            moves();
        }
    }

    void fight()
    {
        //start fighting ideas, and give them options to mercy or flee. if notice another character has done one of these options, then respond to that
    }

    void escape()
    {

    }

    void chase()
    {

    }

    void _catch() { 
    }




    void changeTownBounds(Town town)
    {
        switch (town)
        {
            case Town.Village1:
                maxX = 280;
                minX = 265;
                maxZ = 276;
                minZ = 242;
                break;
            case Town.Village2:
                maxX = 413;
                minX = 386;
                maxZ = 390;
                minZ = 377;
                break;
            case Town.Lake:
                maxX = 170;
                minX = 128;
                maxZ = 170;
                minZ = 145;
                break;
            case Town.Mountain:
                maxX = 492;
                minX = 416;
                maxZ = 185;
                minZ = 153;
                break;
            case Town.Dessert:
                maxX = 132;
                minX = 75;
                maxZ = 400;
                minZ = 353;
                break;
        }
    }

    void randomMovementInTown()
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
                int otherID = collision.gameObject.GetComponent<npcScript>().getID();

                Debug.Log(ID + " crashed into NPC " + otherID);
                animator.SetBool("isWalking", false);

             

                int friendLev = friendLevels[otherID];
                int boundary = friendLevels[otherID];


                if (friendLev > 8)
                {
                    state = State.TALKING;
                }
                if (friendLev < 3)
                {
                    if (traits.Contains(Traits.Aggressive))
                    {
                        state = State.FIGHTING;
                    } else
                    {
                        state = State.STEALING;
                    }
                }
                if (friendLev >=3 || friendLev <=8)
                {
                    if (traits.Contains(Traits.Friendly))
                    {
                        state = State.TALKING;
                    }
                    else if (traits.Contains(Traits.Aggressive) && traits.Contains(Traits.Selfish))
                    {
                        state = State.FIGHTING;
                    } else
                    {
                        //random chance, but for now talk
                        state = State.TALKING;
                    }
                }



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
