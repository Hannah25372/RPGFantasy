using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface InteractableInterface
{
    void Interact(string interaction);
    Transform GetTransform();

    //void Interact(Transform interactorTransform);
    string GetInteractText();
}
