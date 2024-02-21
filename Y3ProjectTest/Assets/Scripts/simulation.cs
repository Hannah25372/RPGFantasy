using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class simulation : MonoBehaviour
{

    public static List<string> log;
    public static int lastViewedLog;
    public static string currentLog;
    private List<Pattern> patterns;
    private List<Event> events;

    private List<PartialPatternBlock> PartialPatternPool;

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

        events = new List<Event>();
        PartialPatternPool = new List<PartialPatternBlock>();

        SetUpEventLogs();
        SetUpPatterns();

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

        if (lastViewedLog < log.Count - 1) //length = 3. last viewed is 2
        {
            lastViewedLog++;
            currentLog = log[lastViewedLog];
            string[] currentLogBreakdown = currentLog.Split(".");  //"fight" "1" "2"


            //does this log complete /advance any partial patterns?
            //loop through partial patterns, is it a point which would be the next action in any? If yes, increment them
            foreach (PartialPatternBlock partialPattern in PartialPatternPool)
            {
                //if the next log matches something in a partial pattern
                if (partialPattern.getNextEvent().action == currentLogBreakdown[1])
                {
                    //if ()
                    //{

                    //}
                }
            }

            //does this log start a pattern?

            //what events will be needed to advance any partial patterns? give suggestions
        }


    }



    //either 2 characters, 1 character 1 object, 2 characters 1 object. Overloaded function so can do either
    public static void AddToLog(string action, string ent1, string ent2, string ent3)
    {
        string gameLog = action + "." + ent1 + "." + ent2 + "." + ent3;
        log.Add(gameLog);
        Debug.Log("Added to game log: " + gameLog);
        Debug.Log("Check: " + log[log.Count -1]);
    }
    public static void AddToLog(string action, string ent1, string ent2)
    {
        string gameLog = action + "." + ent1 + "." + ent2;
        log.Add(gameLog);
        Debug.Log("Added to game log: " + gameLog);
        Debug.Log("Check: " + log[log.Count - 1]); 
    }

    public static void AddToLog(State action, string ent1, string ent2)
    {
        switch (action)
        {
            case State.TALKING:
                AddToLog("talk", ent1, ent2);
                break;
            case State.FIGHTING:
                AddToLog("fight", ent1, ent2);
                break;
            //case State.STEALING:
            //    AddToLog("steal", ent1, ent2);
            //    break;
        }
    }

    private void SetUpEventLogs()
    {
        events.Add(new Event("fight","A","B",null));
        events.Add(new Event("escape", "A", "B", null));
        events.Add(new Event("kill", "A", "B", null));
        events.Add(new Event("chase", "A", "B", null));
        events.Add(new Event("catch", "A", "B", null));
        events.Add(new Event("talk", "A", "B", null));

        events.Add(new Event("drop", "A", "B", "AA"));
        events.Add(new Event("pickup", "A", "B", "AA"));
        events.Add(new Event("give", "A", "B", "AA"));
        events.Add(new Event("steal_sucess", "A", "B", "AA"));
        events.Add(new Event("steal_fail", "A", "B", "AA"));

        

    }
    private void SetUpPatterns()
    {
        //vengence pattern
        List<Event> pat1 = new List<Event>();
        pat1.Add(new Event("fight", "A", "B", null));
        pat1.Add(new Event("escape", "B", "A", null));
        pat1.Add(new Event("fight", "B", "A", null));
        Pattern pattern1 = new(PatternName.vengence, pat1);

        //steal pattern
        List<Event> pat2 = new List<Event>();
        pat2.Add(new Event("steal_success", "A", "B", "AA"));
        pat2.Add(new Event("fight", "B", "A", null));
        pat2.Add(new Event("kill", "B", "A", null));
        pat2.Add(new Event("loot", "B", "A", "AA"));
        Pattern pattern2 = new(PatternName.revenge, pat2);

        patterns.Add(pattern1);
        patterns.Add(pattern2);
    }

}




//stores an individual event, with the parts that make up an event
class Event
{
    public string action;
    public string char1;
    public string char2;
    public string obj;

    public Event(string action, string char1, string char2, string obj)
    {
        this.action = action;
        this.char1 = char1;
        this.char2 = char2;
        this.obj = obj;
    }
}


//stores a story pattern, including the events and the nullifying events for a pattern
//the default character letter used denotes where conditions have to be met of the same character being used etc.
class Pattern {

    PatternName name;
    List<Event> events;

    public Pattern(PatternName name, List<Event> events)
    {
        this.name = name;
        this.events = events;
    }

    public Event getEvent(int no)
    {
        return events[no];
    }
    public int noEvents()
    {
        return events.Count;
    }

}

//holds the pattern being followed, where you are up to, which characters, objects end up getting used
//stick the actual characters being used in the pattern sections so they know which ones to look for
class PartialPatternBlock
{
    Pattern pattern; //add the pattern in question
    int nextEvent;
    string charA;
    string charB;
    string charC; //can end up having patterns with 3 characters (but an individual events would only have max 2 characters)
    string obj;

    public Event getNextEvent()
    {
       return pattern.getEvent(nextEvent);      
    }

}

//pattern names
enum PatternName
{
    revenge, betrayal, vengence, 
}

