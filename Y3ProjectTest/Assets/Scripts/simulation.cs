using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;

public class simulation : MonoBehaviour
{

    public static List<string> log;
    public static int lastViewedLog = -1;
    public static string currentLog;

    private List<Pattern> patterns;
    public static List<string> deadNPCs;
    private int partialsAdded = 0;
    public List<PartialBlock> completedPatterns;
    public Dictionary<string, string> npcNames;

    public Dictionary<string, string> suggestionTextDictionary;
    private string[] randomText;

    public GameObject influenceUI;
    public TextMeshProUGUI suggestionText;
    public float influenceTimer;
    public bool influenceOn;
    public float influenceAppearTimer;
    private int nicePatternCount = 0;
    private int meanPatternCount = 0;

   

    private static List<PartialBlock> PartialPatternPool;

    //public List<npcScript> characters;
    //they have: HP, AC, ATK, traits, relationships, objectsHolding, location

    //things NPC can do:
    ////talk to each other
    ////fight
    ////escape
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

    public static void ClearPartialPoolText()
    {
        string path = "Assets/TextFiles/PartialPool.txt";
        StreamWriter writer = new StreamWriter(path, false);
        writer.WriteLine("PARTIAL POOL");
        writer.Close();
    }

    public static void WriteCompletedPattern(string text)
    {
        string path = "Assets/TextFiles/CompletedPatterns.txt";
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(text);
        writer.Close();
    }

    public static void ClearCompletedText()
    {
        string path = "Assets/TextFiles/CompletedPatterns.txt";
        StreamWriter writer = new StreamWriter(path, false);
        writer.WriteLine("COMPLETED PATTERNS");
        writer.Close();
    }

    // Start is called before the first frame update
    void Start()
    {
        log = new List<string>();
        PartialPatternPool = new List<PartialBlock>();
        patterns = new List<Pattern>();
        deadNPCs = new List<string>();
        completedPatterns = new List<PartialBlock>();
        

        influenceTimer = 0f;
        influenceUI.SetActive(false);

        SetUpPatterns();
        SetUpInfluenceDictionary();
        SetUpNames();

        ClearLogText();
        ClearPartialPoolText();
        ClearCompletedText();

    }

    // Update is called once per frame
    void Update()
    {
        //every frame
        StorySift();


        if (influenceOn)
        {

            //if nothing has changed on influence after 3 seconds, it will dissapear
            if (influenceAppearTimer > 3f)
            {
                influenceAppearTimer = 0f;
                influenceUI.SetActive(false);

            }
            else
            {
                influenceAppearTimer += Time.deltaTime;
            }

            //every 6 seconds call the influencer
            if (influenceTimer > 6f)
            {
                influenceTimer = 0f;
                Influence2();
                
            }
            else
            {
                influenceTimer += Time.deltaTime;
            }



        } else
        {

        }

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
            bool patternExists = false;
            bool createdNewPattern = false;
            bool completedPattern = false;
            lastViewedLog++;
            currentLog = log[lastViewedLog];
            string[] currentLogBreakdown = currentLog.Split(".");  //"fight" "1" "2"


            //ADVANCE PATTERN IN POOL
            //does this log complete /advance any partial patterns?
            //loop through partial patterns, is it a point which would be the next action in any? If yes, increment them
            for (int i = 0; i < PartialPatternPool.Count; i++)
            {
                //if the next log matches something in a partial pattern
                Event eventLooking = PartialPatternPool[i].GetCurrentEvent();

                //Debug.Log("Event Looking: " + eventLooking + " current Log: " + currentLogBreakdown);
                if ((eventLooking.action.Equals(currentLogBreakdown[0])) && (eventLooking.char1.Equals(currentLogBreakdown[1])) && (eventLooking.char2.Equals(currentLogBreakdown[2])))
                {
                    //there is a match. an log occured which matches an event we need
                    addedToPartialPattern = true;
                    PartialPatternPool[i].incrementEvent();
                    if (PartialPatternPool[i].patternComplete())
                    {
                        Debug.Log("Pattern Complete: " + PartialPatternPool[i].name);
                        completedPattern = true;
                        PartialPatternPool[i].complete = true;
                        //completedPatterns.Add(PartialPatternPool[i].WriteBlock());
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
                                completedPatterns.Add(PartialPatternPool[i]);
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
                                completedPatterns.Add(PartialPatternPool[i]);
                                WriteCompletedPattern(PartialPatternPool[i].WriteBlock());
                            }
                        }
                    }

                }

            }

            //PARTIAL PATTERN EXISTS FOR THAT PATTERN AND CHARACTERS
            if (!addedToPartialPattern)
            {
                foreach (PartialBlock pat in PartialPatternPool)
                {
                    if (currentLogBreakdown[0] == pat.firstAction && currentLogBreakdown[1] == pat.charA && currentLogBreakdown[2] == pat.charB)
                    {
                        patternExists = true;

                    }
                }
            }



            //CREATE NEW PATTERN FOR POOL
            //does this log start a pattern? Opens a PartialPattern and adds to the PartialPatternPool
            if (!addedToPartialPattern && !patternExists)
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
                                Debug.Log("Creating new pattern: Character " + currentLogBreakdown[2] + " doesn't exist anymore cannot add to pool");
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
                                Debug.Log("Creating new pattern: Character " + currentLogBreakdown[1] + " doesn't exist anymore cannot add to pool");
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


            //ADD COMPLETE PATTERNS TO BEHAVIOUR COUNT
            foreach (PartialBlock block in PartialPatternPool)
            {
                if (block.patternComplete())
                {
                    if (block.name == PatternName.FRIENDS)
                    {
                        nicePatternCount++;
                    } else
                    {
                        meanPatternCount++;
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
                    if (block.DeadNPC(npc))
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

    


    void SetUpInfluenceDictionary()
    {
        suggestionTextDictionary = new Dictionary<string, string>();

        suggestionTextDictionary.Add("VENGENCE11",""); //no suggestion for escape
        suggestionTextDictionary.Add("VENGENCE12", ""); //no suggestion for escape
        suggestionTextDictionary.Add("VENGENCE21", "X tried to fight you before. You're stronger now you should go and defeat him.");
        suggestionTextDictionary.Add("VENGENCE22", "X ran from you, what a weakling.");

        suggestionTextDictionary.Add("RECLAIM11", "X stole from you! Get him! Fight him!");
        suggestionTextDictionary.Add("RECLAIM12", "");
        suggestionTextDictionary.Add("RECLAIM21", "Go for the kill! X won't get away with taking what's yours.");
        suggestionTextDictionary.Add("RECLAIM22", "");
        suggestionTextDictionary.Add("RECLAIM31", "Time to loot X now!");
        suggestionTextDictionary.Add("RECLAIM32", "");

        suggestionTextDictionary.Add("ANNOYANCE11", "We can try stealing from X again when he's not looking.");
        suggestionTextDictionary.Add("ANNOYANCE12", "It's X over there, shall we say hello?");
        suggestionTextDictionary.Add("ANNOYANCE21", "We can try stealing from X again when he's not looking.");
        suggestionTextDictionary.Add("ANNOYANCE22", "X has tried stealing from you twice now! You gonna take this?");

        suggestionTextDictionary.Add("FRIENDS11", "X was really friendly I wanna get to know that guy better.");
        suggestionTextDictionary.Add("FRIENDS12", "X was really friendly I wanna get to know that guy better.");
        suggestionTextDictionary.Add("FRIENDS21", "I wanna be friends with X.");
        suggestionTextDictionary.Add("FRIENDS22", "What's X carrying?");



        randomText = new string[10];
        randomText[0] = "That guy over there keeps staring at us.";
        randomText[1] = "This town feels strange...";
        randomText[2] = "They look friendly, I like them.";
        randomText[3] = "Hmmm we're running low on funds.";
        randomText[4] = "Think we can convince someone to aid us?";
        randomText[5] = "Who was that?";
        randomText[6] = "I wanna fight. If someone just looks at me funny...";
        randomText[7] = "Wait, go back there.";
        randomText[8] = "Let's move on there's nothing here.";
        randomText[9] = "I want to explore more places.";
    }
    
    
    

    List<PartialBlock> GetPlayerPatterns(List<PartialBlock> pool)
    {
        List<PartialBlock> potentialBlocks = new List<PartialBlock>();
        //gets patterns that player is involved in. can only influence these.
        int n = pool.Count;
        for (int i = 0; i < n; i++)
        {
            if (pool[i].GetCurrentEvent().char1.Equals("0") || pool[i].GetCurrentEvent().char2.Equals("0"))
            {
                potentialBlocks.Add(pool[i]);
            }
        }
        return potentialBlocks;
    }
    List<PartialBlock> GetSameTownPatterns(List<PartialBlock> pool)
    {
        List<PartialBlock> potentialBlocks = new List<PartialBlock>();
        int n = pool.Count;
        for (int i = 0; i < n; i++)
        {
            npcScript otherNPC;
            if (pool[i].GetCurrentEvent().char1.Equals("0"))
            {
                otherNPC = GetNPCByID(pool[i].GetCurrentEvent().char2);
            }
            else
            {
                otherNPC = GetNPCByID(pool[i].GetCurrentEvent().char1);
            }
            if (GameObject.FindGameObjectWithTag("Player").GetComponent<mainPlayer>().currentTown == otherNPC.TownController)
            {
                potentialBlocks.Add(pool[i]);
            }
        }
        return potentialBlocks;
    }
    //chooses from a list of potential patterns that work based on the players behaviour before
    PartialBlock ChooseFavouredPattern(List<PartialBlock> pool)
    {
        //go through completed pool and decide which pattern player favours
        int n = pool.Count;
        for (int i = 0; i < n; i++)
        {
            if (nicePatternCount > meanPatternCount)
            {
                if (pool[i].name == PatternName.FRIENDS)
                {
                    return pool[i];
                }
            }
            else
            {
                if (pool[i].name != PatternName.FRIENDS)
                {
                    return pool[i];
                }
            }
        }

       
       return pool[0];
        
    }
    string GetSuggestedText(PartialBlock pattern)
    {
            PatternName patName = pattern.name;
            Event nextEvent = pattern.GetCurrentEvent();
            string name;
            string selection;

            //check which one is the zero and get name of other character
            if (nextEvent.char1.Equals("0"))
            {
                name = npcNames[nextEvent.char2];
                selection = "1";
            }
            else
            {
                name = npcNames[nextEvent.char1];
                selection = "2";
            }

            //get the appropriate text
            string sText = suggestionTextDictionary[patName.ToString() + pattern.GetCurrentEventNo() + selection];

            if (sText != null && !sText.Equals(""))
            {
                //swap the name into it
                sText = swapInName(sText, name);
                //suggestionText.text = sText;
                //influenceUI.SetActive(true);
                //influenceAppearTimer = 0f;

            }
        //else
        //{
        //    //suggestionText.text = "";
        //}
        return sText;
    }
    void Influence2()
    {

        //these will be empty lists if there is none
        List<PartialBlock> player0Blocks = GetPlayerPatterns(PartialPatternPool);
        List<PartialBlock> sameTownBlocks = GetSameTownPatterns(player0Blocks);
        PartialBlock influencePattern;
        string suggestedText = null;
       
        if (player0Blocks.Count == 0)
        {
            //player involved in none
            //random text
            suggestedText = randomText[Random.Range(0,10)];

        } else if (sameTownBlocks.Count == 0)
        {
            // none are in the same town, but there are player 0 patterns
            influencePattern = ChooseFavouredPattern(player0Blocks);

            //could tell them to move towns, maybe to the town of the chosen block??
            suggestedText = "This town is kind of boring";

        } else
        {
            //there are player 0 patterns in same town
            influencePattern = ChooseFavouredPattern(sameTownBlocks);
            suggestedText = GetSuggestedText(influencePattern);
        }

        //add some randomness to the text when there are patterns. 25% chance of it being random text anyway.
        if (Random.Range(0, 4) == 0)
        {
            suggestedText = randomText[Random.Range(0, 10)];
        }

        //put text up
        if (suggestedText != null && !suggestedText.Equals(""))
        {
            suggestionText.text = suggestedText;
            influenceUI.SetActive(true);
            influenceAppearTimer = 0f;
        }


    }



    PartialBlock ChoosePatternToInfluence()
    {
        //make sure it is one that involves the player
        //make sure npc in same town
        //choose a pattern that has been completed often by the player before
        //choose NPCs that player interacts with more often
        //and other heuritstics 
        //return null if there is no option

        //model the players behaviour. who their favourite NPCs are and what their favourote patterns are. encourage those more.
        //also give random statements for the influencer to say as well

        //also want the reason for none. if it's because they aren't in the town, want it to return this town is boring.
        //if there are no live patterns for player, just suggest an action.


        int n = PartialPatternPool.Count;
        for (int i = 0; i < n; i++)
        {
            if (PartialPatternPool[i].GetCurrentEvent().char1.Equals("0") || PartialPatternPool[i].GetCurrentEvent().char2.Equals("0"))
            {
                return (PartialPatternPool[i]);
            }
        }
        return null;
    }
    void Influence()
    {

        PartialBlock influencePattern = ChoosePatternToInfluence();

        if (influencePattern != null)
        {

            PatternName pattern = influencePattern.name;
            Event nextEvent = influencePattern.GetCurrentEvent();
            string name;
            string selection;

            //check which one is the zero and get name of other character
            if (nextEvent.char1.Equals("0"))
            {
                name = npcNames[nextEvent.char2];
                selection = "1";
            } else
            {
                name = npcNames[nextEvent.char1];
                selection = "2";
            }

            //get the appropriate text
            string sText = suggestionTextDictionary[pattern.ToString() + influencePattern.GetCurrentEventNo() + selection];           

            if (sText != null && !sText.Equals(""))
            {
                //swap the name into it
                sText = swapInName(sText, name);
                suggestionText.text = sText;
                influenceUI.SetActive(true);
                influenceAppearTimer = 0f;

            } else
            {
                suggestionText.text = "";
            }
            




        }

        //comments
        {
            //makes a list of strings that can be used to influence the player
            //still need to decide when to use them and how and how often etc.

           


            //revenge - killing character that killed friend
            // kill.A.B
            // fight.C.A       //PlayerC: he killed your friend         //PlayerA; he looks pissed, they must have been friends
            // kill.C.A        //PlayerC: give no mercy                       
            //conditions: character is aggressive. C and A friends
            //see whether it happens to any characters that are the players friend

            
            //IDEA
            //leave patterns active for a minute, if they decide not to complete them, scrap them. record how many patterns of each type get completed. and suggest the actions which start those more frequent patterns

        }
    }



    private string swapInName(string text, string name)
    {
        //"cat X is"
        // 01234567
        int index = text.IndexOf("X");
        string newText = text.Substring(0, index) + name + text.Substring(index + 1);

        return newText;
    }
    
    
    
    public static npcScript GetNPCByID(string IDstring)
    {
        //when this gets called on "" because some patterns don't have the character yet, it breaks the first line as no number can be given.
        //just gonna remove 3 person patterns instead

        int ID = int.Parse(IDstring);
        GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");
        //Debug.Log("NPCs found: " + npcs.Length);
        List<npcScript> npcScripts = new List<npcScript>();
        foreach (GameObject npc in npcs) {
            npcScripts.Add(npc.GetComponent<npcScript>());      
        }
        foreach(npcScript npc in npcScripts)
        {
            //Debug.Log("NPC test: " + npc);
            //Debug.Log("ID searching: " + ID.ToString() + ". NPC: " + npc.GetID().ToString());
            if (npc.GetID() == ID)
            {
                return npc;
                //if (!npc.dead)
                //{
                //    return npc;
                //}
            }
        }
        return null;          
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
        pat1.Add(new Event("fight", "A", "B"));
        pat1.Add(new Event("escape", "B", "A"));
        pat1.Add(new Event("fight", "B", "A"));
        Pattern pattern1 = new(PatternName.VENGENCE, pat1, false);

        //reclaim - kill and steal back pattern
        List<Event> pat2 = new List<Event>();
        pat2.Add(new Event("steal_success", "A", "B"));
        pat2.Add(new Event("fight", "B", "A"));
        pat2.Add(new Event("kill", "B", "A"));
        pat2.Add(new Event("loot", "B", "A"));
        Pattern pattern2 = new(PatternName.RECLAIM, pat2, false);


        //revenge - killing character that killed friend
        //List<Event> pat3 = new List<Event>();
        //pat3.Add(new Event("kill", "A", "B"));
        //pat3.Add(new Event("fight", "C", "A"));
        //pat3.Add(new Event("kill", "C", "A"));
        //Pattern pattern3 = new(PatternName.REVENGE, pat3, true);
        //conditions: character is aggressive. C and A friends

        // stealing back item from character that killed and looted from friend
        //List<Event> pat4 = new List<Event>();
        //pat4.Add(new Event("kill", "A", "B", null));
        //pat4.Add(new Event("loot", "A", "B", "AA"));
        //pat4.Add(new Event("steal_success", "C", "A", "AA"));
        //Pattern pattern4 = new(PatternName.REVENGE2, pat4, true);
        //conditions: character not agressive. C and A friends

        //annoyance - character failed a steal and kept getting caught, annoys other character
        List<Event> pat5 = new List<Event>();
        pat5.Add(new Event("steal_fail", "A", "B"));
        pat5.Add(new Event("steal_fail", "A", "B"));
        //pat5.Add(new Event("steal_fail", "A", "B", null));  //maybe you should try stealing again later
        pat5.Add(new Event("fight", "B", "A"));  //he keeps stealing from you, he won't stop unless you stop him.
        Pattern pattern5 = new(PatternName.ANNOYANCE, pat5, false);

        //friends
        List<Event> pat6 = new List<Event>();
        pat6.Add(new Event("talk", "A", "B"));
        pat6.Add(new Event("talk", "B", "A"));
        pat6.Add(new Event("give", "B", "A"));
        Pattern pattern6 = new(PatternName.FRIENDS, pat6, true);

        patterns.Add(pattern1);
        patterns.Add(pattern2);
        //patterns.Add(pattern3);
        //patterns.Add(pattern4);
        patterns.Add(pattern5);
        patterns.Add(pattern6);


    }

    private void SetUpNames()
    {
        npcNames = new Dictionary<string, string>();

        npcNames.Add("1", "Rowan");
        npcNames.Add("2", "Lukas");
        npcNames.Add("3", "Gabriel");
        npcNames.Add("4", "Victor");
        npcNames.Add("5", "Ian");
        npcNames.Add("6", "Akira");
        npcNames.Add("7", "Theo");
        npcNames.Add("8", "Luca");
        npcNames.Add("9", "Jack");
        npcNames.Add("10", "William");
        npcNames.Add("11", "Samuel");
        npcNames.Add("12", "Ryan");
        npcNames.Add("13", "Aiden");
        npcNames.Add("14", "Daniel");
        npcNames.Add("15", "Tobias");
        npcNames.Add("16", "Eden");
        npcNames.Add("17", "Thomas");
        npcNames.Add("18", "Jakob");
        npcNames.Add("19", "Keith");
        npcNames.Add("20", "Oscar");


    }
}

//stores an individual event, with the parts that make up an event
public class Event
{
    public string action;
    public string char1;
    public string char2;

    public Event(string action, string char1, string char2)
    {
        this.action = action;
        this.char1 = char1;
        this.char2 = char2;
    }
}


//stores a story pattern, including the events and the nullifying events for a pattern
//the default character letter used denotes where conditions have to be met of the same character being used etc.
public class Pattern {

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
public class PartialBlock
{
    Pattern patternTemplate;
    Pattern patternFollow;
    public string charA;
    public string charB;
    string charC;
    List<Event> patternFollowEventsList;
    int currentEvent;
    int patternLength;
    int ID;
    public string firstAction;
    public bool complete;
    public PatternName name;

    public PartialBlock(Pattern pattern, string _charA, string _charB, int _ID)
    {
        complete = false;
        patternTemplate = pattern;
        charA = _charA;
        charB = _charB;
        charC = null;
        patternFollowEventsList = new List<Event>();
        currentEvent = 1;
        ID = _ID;
        firstAction = patternTemplate.getEvent(0).action;
        name = pattern.name;
 
        //sets up event list with correct characters in the pattern (as strings of their ID
        foreach(Event _event in patternTemplate.GetEvents())
        {
            if(_event.char1.Equals("A"))
            {
                if (_event.char2.Equals("B"))
                {
                    patternFollowEventsList.Add(new Event(_event.action, charA, charB));
                } else
                {
                    patternFollowEventsList.Add(new Event(_event.action, charA, ""));
                }
               
            } else if (_event.char1.Equals("B"))
            {
                if (_event.char2.Equals("A"))
                {
                    patternFollowEventsList.Add(new Event(_event.action, charB, charA));
                }
                else
                {
                    patternFollowEventsList.Add(new Event(_event.action, charB, ""));
                }
            } else
            {
                if (_event.char2.Equals("A"))
                {
                    patternFollowEventsList.Add(new Event(_event.action, "", charA));
                }
                else
                {
                    patternFollowEventsList.Add(new Event(_event.action, "", charB));
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

    //the event we want to happen next
    public Event GetCurrentEvent()
    {
        return (patternFollowEventsList[currentEvent]);
    }

    public int GetCurrentEventNo()
    {
        return currentEvent;
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

    public bool DeadNPC(string id)
    {
        //Debug.Log("pattern " + name);
        for (int i = currentEvent; i < patternFollowEventsList.Count; i++)
        {
            //Debug.Log(patternFollowEventsList[i].action + " " + patternFollowEventsList[i].char1 + " " + patternFollowEventsList[i].char2);
            if (patternFollowEventsList[i].char2 == id || patternFollowEventsList[i].char1 == id)
            {
                if (patternFollowEventsList[i].char2 == id && patternFollowEventsList[i].action == "loot")
                {
                    //they can be dead and looted
                    //Debug.Log("can be dead and looted");
                } 
                else if (patternFollowEventsList[i].char2 == id && patternFollowEventsList[i].action == "kill")
                {
                   
                    //Debug.Log("kill check if still the current one");
                } 
                else 
                {
                    //Debug.Log("dead character involved");
                    return true;

                }
            }
        }
        //Debug.Log("no dead character involved");
        return false;
    }

}


//pattern names
public enum PatternName
{
    REVENGE, VENGENCE, RECLAIM, ANNOYANCE, REVENGE2, FRIENDS
}


public class PlayerBehaviourModel {

    int annoyanceCount;
    int friendsCount;
    int reclaimCount;
    int revengeCount;
    int vengenceCount;

    int[] patternCounts = new int[5];

    public PlayerBehaviourModel()
    {
        patternCounts[0] = 0;
        patternCounts[1] = 0;
        patternCounts[2] = 0;
        patternCounts[3] = 0;
        patternCounts[4] = 0;

    }

    public void UpdateCounts(PartialBlock completed)
    {
            switch (completed.name)
            {
                case PatternName.ANNOYANCE:
                    annoyanceCount++;
                    patternCounts[0]++;
                    break;
                case PatternName.FRIENDS:
                    friendsCount++;
                    patternCounts[1]++;
                    break;
                case PatternName.RECLAIM:
                    reclaimCount++;
                    patternCounts[2]++;
                    break;
                case PatternName.REVENGE:
                    revengeCount++;
                    patternCounts[3]++;
                    break;
                case PatternName.VENGENCE:
                    vengenceCount++;
                    patternCounts[4]++;
                    break;
            }
        
    }

   
}