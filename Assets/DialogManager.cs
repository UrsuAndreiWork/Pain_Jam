using System.Collections.Generic;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    public static DialogManager instance;

    // List to store the choice indices
    public List<int> choiceIndices = new List<int>();

    private void Awake()
    {
        // Ensure only one instance of DialogManager exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Method to add a choice index to the list
    public void SaveChoice(int choiceIndex)
    {
        choiceIndices.Add(choiceIndex);
        Debug.Log("Choice saved: " + choiceIndex);
    }

    // Optionally, method to get all choices
    public int[] GetAllChoices()
    {
        return choiceIndices.ToArray();
    }
}
