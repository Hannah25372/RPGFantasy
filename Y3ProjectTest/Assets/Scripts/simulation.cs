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
    public static List<string> deadNPCs;
    private int partialsAdded = 0;
    public List<string> completedPatterns;

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
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(text);
        writer.Close();
        //StreamReader reader = new StreamReader(path);
        //Debug.Log(reader.ReadToEnd());
        //reader.Close();
    }

    public static void ClearLogText()
    {
        string path = "Assets/TextFiles/Log.txt";
        StreamWriter writer = new StreamWriter(path, false);
        writer.WriteLine("LOG FILE");
        writer.Close();
    }

    public static void WritePartialPool(string text)
    {
        string path = "Assets/TextFiles/PartialPool.txt";
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(text);
        writer.Close();
    }

    public static void WriteCompletedPattern(string text)
    {
        string path = "Assets/TextFiles/CompletedPattern.txt";
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(text);
        writer.Close();
    }

    public static void ClearPartialPoolText()
    {
        string path = "Assets/TextFiles/PartialPool.txt";
        StreamWriter writer = new StreamWriter(path, false);
        writer.WriteLine("PARTIAL POOL");
        writer.Close();
    }

  


    // Start is called before the first frame update
    void Start()
    {
        log = new List<string>();
        PartialPatternPool = new List<PartialBlock>();
        patterns = new List<Pattern>();
        deadNPCs = new List<string>();
        completedPatterns = new List<string>();

        SetUpPatterns();

        ClearLogText();
        ClearPartialPoolText();

    }

    // Update is called once per frame
    void Update()
    {
        StorySift();
        Influence();


        //maybe call influence function after x amount of time of no patterns being advanced, or when you are in proximity to the character in question
    }


    //both NPCs call this when they crash into each other. each one takes a turn being npc1 and npc2
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
            else if (traits1.Contains(Traits.Selfish) && !traits2.Contains(Traits.Selfish))
            {
                state1 = State.STEALING;
                state2 = State.STOLENFROM;
            }
            else if (traits2.Contains(Traits.Selfish) && !traits1.Contains(Traits.Selfish))
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



    void StorySift()
    {
        if (lastViewedLog < log.Count - 1) //length = 3. last viewed is 2
        {
            bool addedToPartialPattern = false;
            bool createdNewPattern = false;
            bool completedPattern = false;
            lastViewedLog++;
            currentLog = log[lastViewedLog];
            string[] currentLogBreakdown = currentLog.Split(".");  //"fight" "1" "2"


            //ADVANCE PATTERN IN POOL
            //does this log complete /advance any partial patterns?
            //loop through partial patterns, is it a point which would be the next action in any? If yes, increment them
            for (int i = 0; i> PartialPatternPool.Count; i++)
            {
                //if the next log matches something in a partial pattern
                Event eventLooking = PartialPatternPool[i].GetCurrentEvent();
                if ((eventLooking.action == currentLogBreakdown[0]) && (eventLooking.char1 == currentLogBreakdown[1]) && (eventLooking.char2 == currentLogBreakdown[2]))
                {
                    //there is a match. an log occured which matches an event we need
                    addedToPartialPattern = true;
                    PartialPatternPool[i].incrementEvent();
                    if (PartialPatternPool[i].patternComplete())
                    {
                        completedPattern = true;
                        completedPatterns.Add(PartialPatternPool[i].WriteBlock());
                        WriteCompletedPattern(PartialPatternPool[i].WriteBlock());
                    }

                }
                //now have a check for if can intro the new charC too -- if one of the char in pattern is "" then we can add the new charC
                else if ((eventLooking.action == currentLogBreakdown[0]) )
                {
                    if ((eventLooking.char1 == currentLogBreakdown[1]) && (eventLooking.char2 == ""))
                    {
                        npcScript tempCharC = GetNPCByID(currentLogBreakdown[2]);
                        if (tempCharC == null)
                        {
                            Debug.Log("Character doesn't exist anymore cannot add to pool");
                        }
                        else
                        {
                            PartialPatternPool[i].SetCharC(tempCharC.IDController.ToString());
                            addedToPartialPattern = true;
                            PartialPatternPool[i].incrementEvent();
                            if (PartialPatternPool[i].patternComplete())
                            {
                                completedPattern = true;
                                completedPatterns.Add(PartialPatternPool[i].WriteBlock());
                                WriteCompletedPattern(PartialPatternPool[i].WriteBlock());
                            }
                        }

                    }
                    if ((eventLooking.char1 == "") && (eventLooking.char2 == currentLogBreakdown[2]))
                    {
                        npcScript tempCharC = GetNPCByID(currentLogBreakdown[1]);
                        if (tempCharC == null)
                        {
                            Debug.Log("Character doesn't exist anymore cannot add to pool");
                        }
                        else
                        {
                            PartialPatternPool[i].SetCharC(tempCharC.IDController.ToString());
                            addedToPartialPattern = true;
                            PartialPatternPool[i].incrementEvent();
                            if (PartialPatternPool[i].patternComplete())
                            {
                                completedPattern = true;
                                completedPatterns.Add(PartialPatternPool[i].WriteBlock());
                                WriteCompletedPattern(PartialPatternPool[i].WriteBlock());
                            }
                        }
                    }

                }

            }
           

            //CREATE NEW PATTERN FOR POOL
            //does this log start a pattern? Opens a PartialPattern and adds to the PartialPatternPool
            if (!addedToPartialPattern)
            {
                foreach (Pattern pat in patterns)
                {
                    if (pat.getEvent(0).action == currentLogBreakdown[0]) //theres a pattern for this - creates a pattern for any potential one atm - could add heuristics
                    {
                        //need to also check if ID = 0 -> that is the player
                        if (currentLogBreakdown[1] == "0")
                        {
                            npcScript tempCharB = GetNPCByID(currentLogBreakdown[2]);
                            if (tempCharB == null)
                            {
                                Debug.Log("Creating new pattern: Character doesn't exist anymore cannot add to pool");
                            }
                            else
                            {
                                //creating a new partial pattern to add to pool
                                PartialBlock block = new PartialBlock(pat, "0", tempCharB.IDController.ToString(), partialsAdded);
                                PartialPatternPool.Add(block);
                                createdNewPattern = true;
                                partialsAdded++;
                                Debug.Log("Added to partial pool: " + pat.name.ToString());

                            }
                        }
                        else if (currentLogBreakdown[2] == "0")
                        {
                            npcScript tempCharA = GetNPCByID(currentLogBreakdown[1]);
                            if (tempCharA == null)
                            {
                                Debug.Log("Creating new pattern: Character doesn't exist anymore cannot add to pool");
                            }
                            else
                            {
                                //creating a new partial pattern to add to pool
                                PartialBlock block = new PartialBlock(pat, tempCharA.IDController.ToString(), "0", partialsAdded);
                                PartialPatternPool.Add(block);
                                createdNewPattern = true;
                                partialsAdded++;
                                Debug.Log("Added to partial pool: " + pat.name.ToString());

                            }
                        }
                        else 
                        { 
                            npcScript tempCharA = GetNPCByID(currentLogBreakdown[1]);
                            npcScript tempCharB = GetNPCByID(currentLogBreakdown[2]);
                            if (tempCharA == null || tempCharB == null)
                            { 
                                Debug.Log("Creating new pattern: Character doesn't exist anymore cannot add to pool");
                            }
                            else
                            {
                                //creating a new partial pattern to add to pool
                                PartialBlock block = new PartialBlock(pat, tempCharA.IDController.ToString(), tempCharB.IDController.ToString(), partialsAdded);
                                PartialPatternPool.Add(block);
                                createdNewPattern = true;
                                partialsAdded++;
                                Debug.Log("Added to partial pool: " + pat.name.ToString());

                            }
                        }


                    }
                }


            }


            //DELETE FROM POOL
            //anything to remove from partial pool? pattern taking too long or a character dies or pattern complete
            List<PartialBlock> blocksToDelete = new List<PartialBlock>();
            foreach (PartialBlock block in PartialPatternPool)
            {             
                foreach(string npc in deadNPCs)
                {
                    if (block.containNPC(npc))
                    {
                        blocksToDelete.Add(block);
                    }
                }
                if (block.patternComplete())
                {
                    blocksToDelete.Add(block);
                }
            }
            foreach(PartialBlock itemDelete in blocksToDelete)
            {
                PartialPatternPool.Remove(itemDelete);
            }
            

            //INFLUENCER
            //what events will be needed to advance any partial patterns? give suggestions



            //REWRITE PARTIAL POOL
            ClearPartialPoolText();
            foreach (PartialBlock block in PartialPatternPool)
            {
                WritePartialPool(block.WriteBlock());
            }

        }
    }

    
    void Influence()
    {
        //makes a list of strings that can be used to influence the player
        //still need to decide when to use them and how and how often etc.

        //vengence
        // fight.A.B    
        // escape.B.A
        // fight.B.A      PlayerA: it's that guy that ran last time, weakling.   PlayerB: he tried to kill you before, you're stronger now go defeat him.



        //reclaim - kill and steal back pattern
        //steal_success.A.B
        //fight.B.A                 //PlayerA:         PlayerB: he stole from you! get him!
        //kill.B.A                                     PlayerB: go for the kill
        //loot.B.A                                     PlayerB: take back what is yours


        //revenge - killing character that killed friend
        // kill.A.B
        // fight.C.A       //PlayerC: he killed your friend         //PlayerA; he looks pissed, they must have been friends
        // kill.C.A        //PlayerC: give no mercy                       
        //conditions: character is aggressive. C and A friends
        //see whether it happens to any characters that are the players friend

        //annoyance
        //steal_fail.A.B
        //steal_fail.A.B    PlayerA: we can come back and try again later when he's not looking  PlayerB: who's that guy over there?
        //fight.B.A         PlayerA: he looks like he wants to start a fight with you now        PlayerB: he keeps trying to steal from you, show him you won't take it.

        //IDEA
        //leave patterns active for a minute, if they decide not to complete them, scrap them. record how many patterns of each type get completed. and suggest the actions which start those more frequent patterns

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
class PartialBlock
{
    Pattern patternTemplate;
    Pattern patternFollow;
    string charA;
    string charB;
    string charC;
    List<Event> patternFollowEventsList;
    int currentEvent;
    int patternLength;
    int ID;

    public PartialBlock(Pattern pattern, string _charA, string _charB, int _ID)
    {
        patternTemplate = pattern;
        charA = _charA;
        charB = _charB;
        charC = null;
        patternFollowEventsList = new List<Event>();
        currentEvent = 1;
        ID = _ID;
 
        //sets up event list with correct characters in the pattern (as strings of their ID
        foreach(Event _event in patternTemplate.GetEvents())
        {
            if(_event.char1.Equals("A"))
            {
                if (_event.char2.Equals("B"))
                {
                    patternFollowEventsList.Add(new Event(_event.action, charA, charB, null));
                } else
                {
                    patternFollowEventsList.Add(new Event(_event.action, charA, "", null));
                }
               
            } else if (_event.char1.Equals("B"))
            {
                if (_event.char2.Equals("A"))
                {
                    patternFollowEventsList.Add(new Event(_event.action, charB, charA, null));
                }
                else
                {
                    patternFollowEventsList.Add(new Event(_event.action, charB, "", null));
                }
            } else
            {
                if (_event.char2.Equals("A"))
                {
                    patternFollowEventsList.Add(new Event(_event.action, "", charA, null));
                }
                else
                {
                    patternFollowEventsList.Add(new Event(_event.action, "", charB, null));
                }
            }
            
        }
        patternFollow = new Pattern(patternTemplate.name, patternFollowEventsList, patternTemplate.conditionFriendCA);

        patternLength = patternFollowEventsList.Count;

    }

    public int getID()
    {
        return ID;
    }


    public string WriteBlock()
    {
        string text = patternFollow.name.ToString();
        int i = 0;
        foreach(Event e in patternFollowEventsList)
        { 
            text += "\r\n " + e.action + " " + e.char1 + " " + e.char2;
            if(i == currentEvent)
            {
                text += " <-- looking for next";
            }
            i++;
        }
        text += "\r\n";
        return text;
    }

    public void SetCharC(string _charC)
    {
        charC = _charC;
        for (int i = 0; i < patternFollowEventsList.Count; i++)
        {
            if (patternFollowEventsList[i].char1.Equals(""))
            {
                patternFollowEventsList[i].char1 = charC;
            }
            if (patternFollowEventsList[i].char2.Equals(""))
            {
                patternFollowEventsList[i].char2 = charC;
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
        if (currentEvent > patternLength)
        {
            return true;
        } else
        {
            return false;
        }
    }

    public bool containNPC(string id)
    {
        if ((charA == id) || (charB == id))
        {
            return true;
        }
        else if (!(charC == null))
        {
            if (charC == id)
            {
                return true;
            }
        }
            return false;         
    }

}


//pattern names
enum PatternName
{
    REVENGE, VENGENCE, RECLAIM, ANNOYANCE
}

