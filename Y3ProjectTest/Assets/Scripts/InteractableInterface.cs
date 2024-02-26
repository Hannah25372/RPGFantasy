using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface InteractableInterface
{
    void Interact();
    Transform GetTransform();

    //void Interact(Transform interactorTransform);
    string GetInteractText();
}
