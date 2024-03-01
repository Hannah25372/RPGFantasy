using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{

    public mainPlayer player;

    //Checks if there is something to interact with
    public void Update()
    {
       
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            InteractableInterface interactable = GetInteractableObject();
            if (interactable != null)
            {
                player.state = State.FIGHTING;
                npcScript npc = interactable.GetTransform().gameObject.GetComponent<npcScript>();
                simulation.AddToLog("fight", "0", npc.IDController.ToString());
                interactable.Interact("fight");

                //int[] stats = new int[] { player.HP, player.AC, player.ATK, npc.HPController, npc.ACControllor, npc.ATKController };
                //FightResult results = simulation.PlayerStartFight(stats);
            }
        }

        else if (Input.GetKeyDown(KeyCode.R))
        {
            InteractableInterface interactable = GetInteractableObject();
            if (interactable != null)
            {
                interactable.Interact("give");
                simulation.AddToLog("give", "0", interactable.GetTransform().gameObject.GetComponent<npcScript>().IDController.ToString());
            }
        }
        else if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            InteractableInterface interactable = GetInteractableObject();
            if (interactable != null)
            {
                if (interactable.GetTransform().gameObject.GetComponent<npcScript>().dead)
                {
                    simulation.AddToLog("loot", "0", interactable.GetTransform().gameObject.GetComponent<npcScript>().IDController.ToString());
                }
                else
                {
                    interactable.Interact("steal");
                }
                
                //simulation.AddToLog("steal", "0", interactable.GetTransform().gameObject.GetComponent<npcScript>().IDController.ToString());
            }
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            InteractableInterface interactable = GetInteractableObject();
            if (interactable != null)
            {
                interactable.Interact("talk");
                simulation.AddToLog("talk", "0", interactable.GetTransform().gameObject.GetComponent<npcScript>().IDController.ToString());
            }
        } 




    }



   

    //Called to find the closest interacable object
    public InteractableInterface GetInteractableObject()
    {
        List<InteractableInterface> interactableList = new List<InteractableInterface>();
        float interactRange = 2f;
        Collider[] array = Physics.OverlapSphere(transform.position, interactRange);
        foreach (Collider collider in array)
        {
            if (collider.TryGetComponent(out InteractableInterface interactable))
            {
                interactableList.Add(interactable);
            }
        }

        InteractableInterface closestInteractable = null;
        foreach (InteractableInterface interactable in interactableList)
        {
            if (closestInteractable == null)
            {
                closestInteractable = interactable;
            }
            else
            {
                if (Vector3.Distance(transform.position, interactable.GetTransform().position) < Vector3.Distance(transform.position, closestInteractable.GetTransform().position))
                {
                    // Closer
                    closestInteractable = interactable;
                }
            }
        }
        return closestInteractable;
    }



}

