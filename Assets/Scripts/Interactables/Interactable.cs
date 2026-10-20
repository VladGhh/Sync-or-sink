using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    // Start is called before the first frame update
    public void Interact()
    {
        Debug.Log("Interacting with " + gameObject.name);
        // Add your interact logic here
    }
    public void ShowPrompt()
    {
        Debug.Log("Interacting with " + gameObject.name);
        // Add your interact logic here
    }
}
