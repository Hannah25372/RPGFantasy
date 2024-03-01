using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class mainPlayer : MonoBehaviour

    
{

    private bool menu;
    private GameObject menuImage;
    private npcScript lastNPC;

    public TextMeshProUGUI HPText;
    public TextMeshProUGUI ACText;
    public TextMeshProUGUI ATKText;

    public int HP = 100;
    public int AC = 60;
    public int ATK = 80;

    public State state;



    // Start is called before the first frame update
    void Start()
    {
        CursorOff();
        menu = false;
        menuImage = GameObject.Find("Canvas").transform.Find("KeyMap").gameObject;
        menuImage.SetActive(false);
        lastNPC = null;

        HPText.text = HP.ToString();
        ACText.text = AC.ToString();
        ATKText.text = ATK.ToString();

        state = State.IDLE;
    }

    // Update is called once per frame
    void Update()
    {

       

        if (state == State.FIGHTING)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                simulation.playerEscape = true;
            }
        } else
        {
            simulation.playerEscape = false;
        }
       
 
    }


    public void ReduceHP(int HP)
    {
        this.HP -= HP;
        HPText.text = HP.ToString();
    }
    public void SetHP(int HP)
    {
        this.HP = HP;
        HPText.text = HP.ToString();

    }

    public void SetAC(int AC)
    {
        this.AC = AC;
        ACText.text = AC.ToString();

    }

    public void SetATK(int ATK)
    {
        this.ATK = ATK;
        ATKText.text = ATK.ToString();

    }


    private void OnCollisionEnter(Collision collision)
    {

        switch (collision.gameObject.tag)
        {
            case "NPC":
                Debug.Log("Player crashed into NPC");
                //menu = true;
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
                Debug.Log("Player crashed into NPC");
                //menu = false;
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
