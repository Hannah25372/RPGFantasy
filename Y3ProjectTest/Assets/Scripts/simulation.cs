using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class simulation : MonoBehaviour
{

    public static List<string> log;
    public static int lastViewedLog = -1;
    public static string currentLog;
    private List<Pattern> patterns;
    private List<Event> events;

    private List<PartialBlock> PartialPatternPool;

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

    public static void WriteToLog(string text)
    {
        string path = "Assets/TextFiles/Log.txt";
        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(text);
        writer.Close();
        //StreamReader reader = new StreamReader(path);
        //Print the text from the file
        //Debug.Log(reader.ReadToEnd());
        //reader.Close();
    }

    public static void WritePartialPool(string text)
    {
        string path = "Assets/TextFiles/PartialPool.txt";
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(text);
        writer.Close();
    }

    public string getPartialPoolText()
    {
        string text = "";
        List<PartialBlock> blocks = PartialPatternPool;
        foreach (PartialBlock block in blocks)
        {
            //Pattern pattern;
            //int nextEvent;
            //npcScript charA;
            //npcScript charB;
            //npcScript charC;
            //string obj;
            //float started;
            //float duration;
           // text += block.GetStringVersion();

        }

        return text;
    }


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
        PartialPatternPool = new List<PartialBlock>();
        patterns = new List<Pattern>();

        //SetUpEventLogs();
        SetUpPatterns();

        WriteToLog("LOG FILE");
        WritePartialPool(getPartialPoolText());

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

        //GetNPCByID("1");

        if (lastViewedLog < log.Count - 1) //length = 3. last viewed is 2
        {
            bool addedToPartialPattern = false;
            bool createdNewPattern = false;
            lastViewedLog++;
            currentLog = log[lastViewedLog];
            string[] currentLogBreakdown = currentLog.Split(".");  //"fight" "1" "2"

            

            //does this log complete /advance any partial patterns?
            //loop through partial patterns, is it a point which would be the next action in any? If yes, increment them
            foreach (PartialBlock partialPattern in PartialPatternPool)
            {
                //if the next log matches something in a partial pattern
                if (partialPattern.GetCurrentEvent().action == currentLogBreakdown[0])
                {
                    //if ()
                    //{

                    //}
                }
            }

            //does this log start a pattern? Opens a PartialPattern and adds to the PartialPatternPool
            if (!addedToPartialPattern)
            {
                foreach (Pattern pat in patterns)
                {
                    if (pat.getEvent(0).action == currentLogBreakdown[0]) //theres a pattern for this
                    {
                        npcScript tempCharA = GetNPCByID(currentLogBreakdown[1]);
                        npcScript tempCharB = GetNPCByID(currentLogBreakdown[2]);
                        if (tempCharA == null || tempCharB == null)
                        {
                            Debug.Log("Character doesn't exist anymore cannot add to pool");
                        } else
                        {
                            PartialPatternPool.Add(new PartialBlock(pat, tempCharA, tempCharB));
                            createdNewPattern = true;
                            Debug.Log("Added to partial pool: " + pat.name.ToString());
                            WritePartialPool(pat.name.ToString());
                        }

                       
                    }
                }


            }
            
            //anything to remove from partial pool? pattern taking too long or a character dies.


            //what events will be needed to advance any partial patterns? give suggestions

        }


    }


    public npcScript GetNPCByID(string IDstring)
    {
        int ID = int.Parse(IDstring);
        GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");
        Debug.Log("NPCs found: " + npcs.Length);
        List<npcScript> npcScripts = new List<npcScript>();
        foreach (GameObject npc in npcs) {
            npcScripts.Add(npc.GetComponent<npcScript>());      
        }
        foreach(npcScript npc in npcScripts)
        {
            Debug.Log("NPC test: " + npc);
            Debug.Log("ID searching: " + ID.ToString() + ". NPC: " + npc.GetID().ToString());
            if (npc.GetID() == ID)
            {
                return npc;
            }
        }
        return null;          
    }

    //either 2 characters, 1 character 1 object, 2 characters 1 object. Overloaded function so can do either
    public static void AddToLog(string action, string ent1, string ent2, string ent3)
    {
        string gameLog = action + "." + ent1 + "." + ent2 + "." + ent3;
        log.Add(gameLog);
        Debug.Log("Added to game log: " + gameLog);
        WriteToLog(gameLog);
        //Debug.Log("Check: " + log[log.Count -1]);
    }
    public static void AddToLog(string action, string ent1, string ent2)
    {
        string gameLog = action + "." + ent1 + "." + ent2;
        log.Add(gameLog);
        Debug.Log("Added to game log: " + gameLog);
        WriteToLog(gameLog);
        //Debug.Log("Check: " + log[log.Count - 1]); 
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
        //events.Add(new Event("fight","A","B",null));
        //events.Add(new Event("escape", "A", "B", null));
        //events.Add(new Event("kill", "A", "B", null));
        //events.Add(new Event("chase", "A", "B", null));
        //events.Add(new Event("catch", "A", "B", null));
        //events.Add(new Event("talk", "A", "B", null));

        //events.Add(new Event("drop", "A", "B", "AA"));
        //events.Add(new Event("pickup", "A", "B", "AA"));
        //events.Add(new Event("give", "A", "B", "AA"));
        //events.Add(new Event("steal_sucess", "A", "B", "AA"));
        //events.Add(new Event("steal_fail", "A", "B", "AA"));

        

    }
    private void SetUpPatterns()
    {
        //vengence pattern
        List<Event> pat1 = new List<Event>();
        pat1.Add(new Event("fight", "A", "B", null));
        pat1.Add(new Event("escape", "B", "A", null));
        pat1.Add(new Event("fight", "B", "A", null));
        Pattern pattern1 = new(PatternName.VENGENCE, pat1, false);

        //reclaim - kill and steal back pattern
        List<Event> pat2 = new List<Event>();
        pat2.Add(new Event("steal_success", "A", "B", "AA"));
        pat2.Add(new Event("fight", "B", "A", null));
        pat2.Add(new Event("kill", "B", "A", null));
        pat2.Add(new Event("loot", "B", "A", "AA"));
        Pattern pattern2 = new(PatternName.RECLAIM, pat2, false);


        //revenge - killing character that killed friend
        List<Event> pat3 = new List<Event>();
        pat3.Add(new Event("kill", "A", "B", null));
        pat3.Add(new Event("fight", "C", "A", null));
        pat3.Add(new Event("kill", "C", "A", null));
        Pattern pattern3 = new(PatternName.REVENGE, pat3, true);
        //conditions: character is aggressive. C and A friends

        // stealing back item from character that killed and looted from friend
        //List<Event> pat4 = new List<Event>();
        //pat4.Add(new Event("kill", "A", "B", null));
        //pat4.Add(new Event("loot", "A", "B", "AA"));
        //pat4.Add(new Event("steal_success", "C", "A", "AA"));
        //Pattern pattern4 = new(PatternName.revenge, pat4, true);
        //conditions: character not agressive. C and A friends

        //annoyance - character failed a steal and kept getting caught, annoys other character
        List<Event> pat5 = new List<Event>();
        pat5.Add(new Event("steal_fail", "A", "B", null));
        pat5.Add(new Event("steal_fail", "A", "B", null));  //maybe you should try stealing again later
        pat5.Add(new Event("fight", "B", "A", null));  //he keeps stealing from you, he won't stop unless you stop him.
        Pattern pattern5 = new(PatternName.ANNOYANCE, pat5, false);

        patterns.Add(pattern1);
        patterns.Add(pattern2);
        patterns.Add(pattern3);
        //patterns.Add(pattern4);
        patterns.Add(pattern5);


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

    public PatternName name;
    List<Event> events;
    public bool conditionFriendCA;

    public Pattern(PatternName name, List<Event> events, bool conditionFriendCA)
    {
        this.name = name;
        this.events = events;
        this.conditionFriendCA = conditionFriendCA;
    }

    public List<Event> GetEvents()
    {
        return events;
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
    npcScript charA;
    npcScript charB;
    npcScript charC; //can end up having patterns with 3 characters (but an individual events would only have max 2 characters)
    string obj;
    float started;
    float duration;

    public PartialPatternBlock(Pattern pattern, npcScript charA, npcScript charB)
    {
        this.pattern = pattern;
        this.charA = charA;
        this.charB = charB;
        started = Time.deltaTime;
        nextEvent = 1;
    }
    public Event getNextEvent()
    {
       return pattern.getEvent(nextEvent);      
    }

    //3 events. next one is 2 its okay, if its 3 then passed
    //returns TRUE if it is completed
    public bool incrementNextEvent()
    {
        nextEvent++;
        return !(nextEvent < pattern.noEvents());
    }

    public void SetCharC(npcScript charC)
    {
        this.charC = charC;
    }

    public string GetStringVersion()
    {
        string text = "Partial Pattern Started";

        return text;
    }

}


class PartialBlock
{
    Pattern patternTemplate;
    Pattern patternFollow;
    npcScript charA;
    npcScript charB;
    npcScript charC;
    List<Event> patternFollowEventsList;
    int currentEvent;
    int patternLength;

    public PartialBlock(Pattern pattern, npcScript _charA, npcScript _charB)
    {
        patternTemplate = pattern;
        charA = _charA;
        charB = _charB;
        patternFollowEventsList = new List<Event>();
        currentEvent = 0;
 
        //sets up event list with correct characters in the pattern (as strings of their ID
        foreach(Event _event in patternTemplate.GetEvents())
        {
            if(_event.char1.Equals("A"))
            {
                if (_event.char2.Equals("B"))
                {
                    patternFollowEventsList.Add(new Event(_event.action, charA.IDController.ToString(), charB.IDController.ToString(), null));
                } else
                {
                    patternFollowEventsList.Add(new Event(_event.action, charA.IDController.ToString(), "", null));
                }
               
            } else if (_event.char1.Equals("B"))
            {
                if (_event.char2.Equals("A"))
                {
                    patternFollowEventsList.Add(new Event(_event.action, charB.IDController.ToString(), charA.IDController.ToString(), null));
                }
                else
                {
                    patternFollowEventsList.Add(new Event(_event.action, charB.IDController.ToString(), "", null));
                }
            } else
            {
                if (_event.char2.Equals("A"))
                {
                    patternFollowEventsList.Add(new Event(_event.action, "", charA.IDController.ToString(), null));
                }
                else
                {
                    patternFollowEventsList.Add(new Event(_event.action, "", charB.IDController.ToString(), null));
                }
            }
            
        }
        patternFollow = new Pattern(patternTemplate.name, patternFollowEventsList, patternTemplate.conditionFriendCA);

        patternLength = patternFollowEventsList.Count;

    }

    public void SetCharC(npcScript _charC)
    {
        charC = _charC;
        for (int i = 0; i < patternFollowEventsList.Count; i++)
        {
            if (patternFollowEventsList[i].char1.Equals(""))
            {
                patternFollowEventsList[i].char1 = charC.IDController.ToString();
            }
            if (patternFollowEventsList[i].char2.Equals(""))
            {
                patternFollowEventsList[i].char2 = charC.IDController.ToString();
            }
        }
        patternFollow = new Pattern(patternTemplate.name, patternFollowEventsList, patternTemplate.conditionFriendCA);
    }

    public Event GetCurrentEvent()
    {
        return (patternFollowEventsList[currentEvent]);
    }

    public void incrementEvent()
    {
        currentEvent++;
    }

    public bool patternComplete()
    {
        //length 3. if i am on current 2, there is one more to complete
        if (currentEvent >= patternLength)
        {
            return true;
        } else
        {
            return false;
        }
    }

}


//pattern names
enum PatternName
{
    REVENGE, VENGENCE, RECLAIM, ANNOYANCE
}

