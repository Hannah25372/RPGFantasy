using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class mainPlayer : MonoBehaviour
{

    public TextMeshProUGUI HPText;
    public TextMeshProUGUI ACText;
    public TextMeshProUGUI ATKText;

    public Vector3 move;

    public GameObject influencerNPC;
    private Animator npcAnimator;

    public int HP = 100;
    public int AC = 60;
    public int ATK = 80;

    //public State state;

    // Start is called before the first frame update
    void Start()
    {
        CursorOff();        

        HPText.text = HP.ToString();
        ACText.text = AC.ToString();
        ATKText.text = ATK.ToString();

        npcAnimator = influencerNPC.GetComponent<Animator>();

        //state = State.IDLE;
    }

    // Update is called once per frame
    void Update()
    {

        move = gameObject.GetComponent<CharacterController>().velocity;
        if (move.x == 0 && move.y == 0 && move.z == 0)
        {
            npcAnimator.SetBool("isWalking", false);
        } else
        {
            npcAnimator.SetBool("isWalking", true);
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
