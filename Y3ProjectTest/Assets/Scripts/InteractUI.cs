using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractUI : MonoBehaviour
{

    [SerializeField] private GameObject interactButton;
    [SerializeField] private GameObject interactButton2;
    [SerializeField] private PlayerInteract interactor;
    public TextMeshProUGUI interactText;
    public TextMeshProUGUI interactText2;

    public mainPlayer player;

    private void Update()
    {
        if (interactor.GetInteractableObject() != null)
        {
            
            Show(interactor.GetInteractableObject());
            
        } else
        {
            Hide();
        }
    }

    
    private void Show(InteractableInterface interactable)
    {

        if (interactable.GetTransform().gameObject.GetComponent<npcScript>().dead)
        {
            interactButton2.SetActive(true);
            interactButton.SetActive(false);
            interactText2.text = interactable.GetInteractText();
        } else
        {
            interactButton.SetActive(true);
            interactButton2.SetActive(false);
            interactText.text = interactable.GetInteractText();
        }
        
    }

    
    private void Hide()
    {
        interactButton.SetActive(false);
        interactButton2.SetActive(false);
    }
}
