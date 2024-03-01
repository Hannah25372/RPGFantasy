using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class npcScript : MonoBehaviour, InteractableInterface
{

    simulation sim;

    public string name;

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

    float deathTimer;

    //character attributes
    [SerializeField] private int ID;
    [SerializeField] private List<Traits> traits;
    [SerializeField] private int ATK;
    [SerializeField] private int AC;
    [SerializeField] private int HP;
    [SerializeField] private Town currentTown;
    [SerializeField] private Relationship[] relationships;

    private ParticleSystem ps;

    private npcScript lastInteractedPlayer;

    public bool dead;

    
    public int HPController { get => HP; set
        {
            HP = Mathf.Clamp(value, 0, 100);
            if (HP == 0)
            {
                if (lastInteractedPlayer == null) {
                    simulation.AddToLog("kill", "0", ID.ToString());
                } 
                else
                {
                    simulation.AddToLog("kill", lastInteractedPlayer.IDController.ToString(), ID.ToString());
                }
                simulation.deadNPCs.Add(ID.ToString());
                dead = true;
                state = State.DEAD;
                animator.StopPlayback();
                ps.Stop();
                transform.Rotate(90, 0, 0);
                //gameObject.GetComponent<CapsuleCollider>().enabled = false;
                //gameObject.GetComponent<npcScript>().enabled = false;

                //Destroy(gameObject);

            }
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

    public int GetID()
    {
        return ID;
    }


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        ps = gameObject.transform.Find("fightParticles").GetComponent<ParticleSystem>();
        //traits = new List<Traits>();
        //relationships = new Relationship[8];

        changeTownBounds(Town.Village1);
        speed = 2;
        rotateDirection = 180f;

        duration = 3f;
        deathTimer = 0f;

        dead = false;

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
            timer(3);
        }

        if (state == State.DEAD || dead)
        {
            state = State.DEAD;
            

        }



    }

   

    void moveToLocation()
    {

    }

    //they attempt, and the sucess or fail determined by other NPC
    void AttemptSteal(npcScript otherNpc)
    {
        state = State.MOVING;
        animator.SetBool("isWalking", true);
    }


    //the other is the one who steals
    void NoticeSteal(npcScript otherNpc)
    {
        int rand = Random.Range(1,3); //either 1 or 2
        if (rand == 1) //success
        {
            simulation.AddToLog("steal_success", otherNpc.IDController.ToString(), ID.ToString(), "obj1");
            state = State.MOVING;
            animator.SetBool("isWalking", true);
            //swap the object in question
        } else //fail
        {
            simulation.AddToLog("steal_fail", otherNpc.IDController.ToString(), ID.ToString(), "obj1");
            state = State.MOVING;
            animator.SetBool("isWalking", true);
        }      
    }

    //the other is the one who steals
    void NoticeSteal()
    {
        int rand = Random.Range(1, 3); //either 1 or 2
        if (rand == 1) //success
        {
            simulation.AddToLog("steal_success", "0", ID.ToString(), "obj1");
            state = State.MOVING;
            animator.SetBool("isWalking", true);
            //swap the object in question
        }
        else //fail
        {
            simulation.AddToLog("steal_fail", "0", ID.ToString(), "obj1");
            timer(2);
            state = State.MOVING;
            animator.SetBool("isWalking", true);
            //make them stop and look at you.
        }
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

    void timer(float duration)
    {
        if (time < duration)
        {
            time += Time.deltaTime;
        }
        else
        {
            time = 0f;
            animator.SetBool("isWalking", true);
            ps.Stop();
            state = State.MOVING;
            rotates(150, 210);
            moves();
        }
    }

    void Fight(npcScript otherNPC)
    {
        //start fighting ideas, and give them options to mercy or flee. if notice another character has done one of these options, then respond to that
        

        ps.Play();
        if(ATK > otherNPC.ACControllor)
        {
            otherNPC.HPController =- Random.Range(0,20);
        } else
        {
            //HPController -= 2;
        }


    }

    public void escape()
    {
        animator.SetBool("isWalking", true);
        state = State.MOVING;
        rotates(150, 210);
        moves();
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
        if (!dead)
        {

        
        switch (collision.gameObject.tag)
        {
            case "Player":
                Debug.Log(ID.ToString() + " crashed into Player");
                lastInteractedPlayer = null;

                switch (Random.Range(0, 15))
                {
                    case < 3:
                        animator.SetBool("isWalking", false);
                        state = State.TALKING;
                        simulation.AddToLog("talk", ID.ToString(), "0");
                        break;
                    case < 8:
                        ps.Play();
                        animator.SetBool("isWalking", false);
                        state = State.FIGHTING;
                        simulation.AddToLog("fight", ID.ToString(), "0");
                        collision.gameObject.GetComponent<mainPlayer>().ReduceHP(10);
                        break;
                    case < 12:
                        if (Random.Range(0, 2) == 1)
                        {
                            simulation.AddToLog("steal_fail", ID.ToString(), "0");
                        } else
                        {
                            simulation.AddToLog("steal_success", ID.ToString(), "0");
                        }
                        break;
                    case < 15:
                        simulation.AddToLog("give", ID.ToString(), "0");
                        break;

                }
                break;

            case "NPC":
                var otherNPC = collision.gameObject.GetComponent<npcScript>();
                lastInteractedPlayer = otherNPC;
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

                if (otherNPC.dead)
                {
                    if (Random.Range(0, 2) == 0)
                    {
                        simulation.AddToLog("loot", ID.ToString(), otherID.ToString());
                        state = State.TALKING;
                    }
                    rotates(150, 210);

                    } else {
                    animator.SetBool("isWalking", false);
                    var stateAgainstNPC = simulation.NPCInteraction(this, collision.collider.GetComponent<npcScript>());
                    state = stateAgainstNPC[0];

                    //the interactions in here are just of this format, don't wanna add the steal tho
                    simulation.AddToLog(state, ID.ToString(), otherID.ToString());

                    if (state == State.FIGHTING) Fight(otherNPC);
                    if (state == State.STEALING) AttemptSteal(otherNPC);
                    if (state == State.STOLENFROM) NoticeSteal(otherNPC);
                }

                break;
            default:
                rotates(150, 210);
                break;
        }
    }

        
    }

    public void Interact(string interaction)
    {
        //what it does when it interacts
        //handle the fight either just here or as player, and then tell them the result
        state = State.IDLE;
        animator.SetBool("isWalking", false);
        lastInteractedPlayer = null;
        transform.LookAt(GameObject.FindGameObjectWithTag("Player").transform);

        switch (interaction)
        {
            case "fight":
                ps.Play();
                HPController -= 10;
                state = State.FIGHTING;
                break;
            case "give":
                state = State.RECIEVE;
                break;
            case "steal":
                NoticeSteal();
                break;
            case "talk":
                state = State.TALKING;
                break;
            default:
                break;
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public string GetInteractText()
    {
        return name;
    }
}


public enum Relationship
{
    Neutral, Enemy, Friend
}

public enum State
{
    IDLE, MOVING, TALKING, FIGHTING, STEALING, STOLENFROM, STARTFIGHT, JOINFIGHT, ESCAPE, KILL, STARTTALK, JOINTALK, GIVE, RECIEVE, DEAD
}

public enum Town
{
    Village1, Village2, Lake, Mountain, Dessert
}

public enum Traits
{
    Aggressive, Friendly, Loyal, Selfish
}