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
    [SerializeField] private int ID;
    [SerializeField] private List<Traits> traits;
    [SerializeField] private int ATK;
    [SerializeField] private int AC;
    [SerializeField] private int HP;
    [SerializeField] private Town currentTown;
    [SerializeField] private Relationship[] relationships;

    private ParticleSystem ps;

    
    public int HPController { get => HP; set
        {
            HP = Mathf.Clamp(value, 0, 10);
            if (HP == 0) Destroy(gameObject);
        }
    }

    public int IDController { get => ID; set { ID = value; } }
    public int ACControllor { get => AC; set { AC = value; } }
    public int ATKController { get => ATK; set { ATK = value; } }
    public Town TownController { get => currentTown; set { currentTown = value; } }
    public List<Traits> getTraits => traits;
    public State StateControllor { get => state; set { state = value; } }
    public Relationship[] RelationshipsControllor { get => relationships; }
    public void setNewRelationship(int index, Relationship rel) { relationships[index] = rel; }




    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        ps = gameObject.transform.Find("fightParticles").GetComponent<ParticleSystem>();
        //traits = new List<Traits>();
        //relationships = new Relationship[8];

        changeTownBounds(Town.Village1);
        Debug.Log(HPController);
        speed = 2;
        rotateDirection = 180f;

        duration = 3f;

        state = State.MOVING;
        animator.SetBool("isWalking", true);


        //npcScript[] npcScripts = GameObject.FindObjectsOfType<npcScript>();
        //foreach (var npc in npcScripts) 
        //{
        //    if(npc.clan == this.clan)
        //    npcRelationShip.Add(npc, Relationship.Friend);
        //    else
        //}
        

        
        
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
        
        if (state == State.FIGHTING)
        {
            fight();
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
        if (time < duration)
        {
            time += Time.deltaTime;
        }
        else
        {
            time = 0f;
            animator.SetBool("isWalking", true);
            //ps.Stop();
            state = State.MOVING;
            rotates(150, 210);
            moves();
        }
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


    private bool NotInteractingAlready(npcScript npc)
    {
        if((npc.StateControllor == State.MOVING || npc.StateControllor == State.IDLE) && (state == State.MOVING || state == State.IDLE))
        {
            return false;
        } else
        {
            return true;
        }
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
                var otherNPC = collision.gameObject.GetComponent<npcScript>();
                int otherID = otherNPC.IDController;

                Debug.Log(ID + " crashed into NPC " + otherID);

                //if (NotInteractingAlready(otherNPC))
                //{
                //    animator.SetBool("isWalking", false);
                //    var stateAgainstNPC = simulation.NPCInteraction(this, collision.collider.GetComponent<npcScript>());
                //    state = stateAgainstNPC[0];

                //}
                //else
                //{
                //    Debug.Log(ID + " or " + otherID + " already interacting");
                //}

                animator.SetBool("isWalking", false);
                var stateAgainstNPC = simulation.NPCInteraction(this, collision.collider.GetComponent<npcScript>());
                state = stateAgainstNPC[0];

                //the interactions in here are just of this format
                simulation.AddToLog(state, ID.ToString() , otherID.ToString());
                   


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

enum Clan 
{ 
 blue,
 red,
}

public enum Relationship
{
    Neutral, Enemy, Friend
}

public enum State
{
    IDLE, MOVING, TALKING, FIGHTING, STEALING, STOLENFROM
}

public enum Town
{
    Village1, Village2, Lake, Mountain, Dessert
}

public enum Traits
{
    Aggressive, Friendly, Loyal, Selfish
}