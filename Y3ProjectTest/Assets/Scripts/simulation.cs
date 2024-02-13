using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class simulation : MonoBehaviour
{

    public static List<string> log;

    //public List<npcScript> characters;
    //they have: HP, AC, ATK, traits, relationships, objectsHolding, location
    
    //things NPC can do:
    ////talk to each other
    ////fight
    ////mercy
    ////escape
    ////chase
    ////catch
    ////kill
    ////loot 
    ////steal
    ////give



    // Start is called before the first frame update
    void Start()
    {

        log = new List<string>();
        //characters = new List<npcScript>();
        //int i = 0;
        //foreach (var npc in characters)
        //{
        //    npc.StateControllor = State.MOVING;
        //    npc.IDController = i;
        //    i++;
        //}



    }

    //both NPCs call this when they crash into each other
    public static State[] NPCInteraction(npcScript npc1, npcScript npc2) 
    {

        State state1;
        State state2;
        State[] state = new State[2];

        int id1 = npc1.IDController;
        int id2 = npc2.IDController;

        Relationship rel1 = npc1.RelationshipsControllor[id2];
        Relationship rel2 = npc2.RelationshipsControllor[id1];

        List<Traits> traits1 = npc1.getTraits;
        List<Traits> traits2 = npc2.getTraits;

        //friends talk
        if (rel1 == Relationship.Friend)
        {
            state1 = State.TALKING;
            state2 = State.TALKING;
        }

        //aggressive one fights
        else if (rel1 == Relationship.Neutral)
        {

            if (traits1.Contains(Traits.Friendly) && traits2.Contains(Traits.Friendly))
            {
                state1 = State.TALKING;
                state2 = State.TALKING;
            }
            else if (traits1.Contains(Traits.Aggressive) && traits2.Contains(Traits.Aggressive))
            {
                state1 = State.FIGHTING;
                state2 = State.FIGHTING;
            }
            else if (traits1.Contains(Traits.Selfish))
            {
                state1 = State.STEALING;
                state2 = State.STOLENFROM;
            }
            else if (traits2.Contains(Traits.Selfish))
            {
                state1 = State.STOLENFROM;
                state2 = State.STEALING;                
            } else
            {
                state1 = State.TALKING;
                state2 = State.TALKING;
            }
        }

        else //(rel1 == Relationship.Enemy)
        {
            if (traits1.Contains(Traits.Aggressive) || traits2.Contains(Traits.Aggressive))
            {
                state1 = State.FIGHTING;
                state2 = State.FIGHTING;
            }
            else if (traits1.Contains(Traits.Selfish))
            {
                state1 = State.STEALING;
                state2 = State.STOLENFROM;
            }
            else if (traits2.Contains(Traits.Selfish))
            {
                state1 = State.STOLENFROM;
                state2 = State.STEALING;
            }
            else
            {
                state1 = State.FIGHTING;
                state2 = State.FIGHTING;
            }
        }


        //change so only steals if the player has an object too

        state[0] = state1;
        state[1] = state2;
        return state;
    }

    // Update is called once per frame
    void Update()
    {
        


    }



    //either 2 characters, 1 character 1 object, 2 characters 1 object. Overloaded function so can do either
    public static void AddToLog(string action, string ent1, string ent2, string ent3)
    {
        log.Add(action + "." + ent1 + "." + ent2 + "." + ent3);
    }
    public static void AddToLog(string action, string ent1, string ent2)
    {
        log.Add(action + "." + ent1 + "." + ent2);
    }

    public static void AddToLog(State action, string ent1, string ent2)
    {
        switch (action)
        {
            case State.TALKING:
                simulation.AddToLog("talk", ent1, ent2);
                break;
            case State.FIGHTING:
                simulation.AddToLog("fight", ent1, ent2);
                break;
            case State.STEALING:
                simulation.AddToLog("steal", ent1, ent2);
                break;
        }
    }


}
