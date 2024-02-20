using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mainPlayer : MonoBehaviour

    
{

    private bool menu;
    private GameObject menuImage;
    private npcScript lastNPC;


    // Start is called before the first frame update
    void Start()
    {
        CursorOff();
        menu = false;
        menuImage = GameObject.Find("Canvas").transform.Find("KeyMap").gameObject;
        menuImage.SetActive(false);
        lastNPC = null;
    }

    // Update is called once per frame
    void Update()
    {

        if (menu)
        {
            menuImage.SetActive(true);
            if (Input.GetKey(KeyCode.F))
            {
                Debug.Log("Pressed F");
                simulation.AddToLog("fight", "0", lastNPC.IDController.ToString());
            }
            else if (Input.GetKey(KeyCode.G))
            {
                simulation.AddToLog("give", "0", lastNPC.IDController.ToString());
            }
            else if (Input.GetKey(KeyCode.R))
            {
                simulation.AddToLog("steal", "0", lastNPC.IDController.ToString());
            }
            else if (Input.GetKey(KeyCode.T))
            {
                simulation.AddToLog("talk", "0", lastNPC.IDController.ToString());
            }
        } else
        {
            menuImage.SetActive(false);
        }
 
    }


    private void OnCollisionEnter(Collision collision)
    {

        switch (collision.gameObject.tag)
        {
            case "NPC":
                Debug.Log("Player crashed into NPC");
                menu = true;
                //bring up menu for talk or fight? can have it as f to fight and t to talk. g to give and r to steal
                lastNPC = collision.gameObject.GetComponent<npcScript>();

                break;
        }
    }

    private void OnCollisionExit(Collision collision)
    {

        switch (collision.gameObject.tag)
        {
            case "NPC":
                Debug.Log("Player left into NPC");
                menu = false;
                lastNPC = null;
                break;
        }
    }

    public void CursorOn()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CursorOff()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

}
