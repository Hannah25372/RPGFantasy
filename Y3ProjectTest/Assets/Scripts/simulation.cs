using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class simulation : MonoBehaviour
{

    

    public List<npcScript> characters;
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
        characters = new List<npcScript>();
        int i = 0;
        foreach (var npc in characters)
        {
            npc.setState(npcScript.State.MOVING);
            npc.setID(i);
            i++;
        }



    }

    // Update is called once per frame
    void Update()
    {
        


    }

    
}
