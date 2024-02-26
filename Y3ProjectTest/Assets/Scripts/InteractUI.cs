using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractUI : MonoBehaviour
{

    [SerializeField] private GameObject interactButton;
    [SerializeField] private PlayerInteract interactor;
    public TextMeshProUGUI interactText;

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
        interactButton.SetActive(true);
        interactText.text = interactable.GetInteractText();
    }

    
    private void Hide()
    {
        interactButton.SetActive(false);
    }
}
