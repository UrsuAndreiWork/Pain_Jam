using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class DialogChoice
{
    public string choiceText; // The text for the choice
    public string[] resultingDialogLines; // The dialog lines that follow this choice
}

public class Dialog : MonoBehaviour
{
    public string[] dialogLines;
    public DialogChoice[] dialogChoices; // Array to store dialog choices
    public GameObject dialogPanel; // The single UI panel that holds the dialog text, background, and choice buttons
    public TextMeshProUGUI dialogText; // The TMP Text component where the dialog will be displayed
    public Button[] choiceButtons; // Buttons for each choice
    public float typingSpeed = 0.05f;

    private int currentLineIndex = 0;
    private bool isDialogActive = false;
    private bool isPlayerInTrigger = false;
    private bool isTyping = false;

    private void Start()
    {
        dialogPanel.SetActive(false); // Ensure the dialog panel is hidden initially
        Debug.Log("DialogTrigger script initialized. Dialog panel is hidden.");

        // Ensure all choice buttons are initially disabled and hidden
        foreach (Button button in choiceButtons)
        {
            button.gameObject.SetActive(false); // Hide the button
        }
    }

    private void Update()
    {
        if (isPlayerInTrigger && !isDialogActive)
        {
            Debug.Log("Player entered trigger area, starting dialog.");
            StartDialog();
        }

        if (isDialogActive && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            Debug.Log("Space or mouse button pressed. Handling dialog input.");
            if (isTyping)
            {
                Debug.Log("Currently typing, completing the current line.");
                CompleteLine();
            }
            else if (dialogChoices.Length > 0 && currentLineIndex >= dialogLines.Length)
            {
                Debug.Log("Displaying dialog choices.");
                DisplayChoices();
            }
            else
            {
                Debug.Log("Displaying next line or ending dialog.");
                DisplayNextLine();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            Debug.Log("Player entered the trigger zone.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
            Debug.Log("Player exited the trigger zone.");
        }
    }

    private void StartDialog()
    {
        isDialogActive = true;
        currentLineIndex = 0;
        dialogPanel.SetActive(true); // Show the dialog panel
        FindObjectOfType<Movement>().enabled = false; // Disable player movement
        Debug.Log("Dialog started. Player movement disabled.");
        DisplayNextLine();
    }

    private void DisplayNextLine()
    {
        Debug.Log($"Current line index: {currentLineIndex}, Total lines: {dialogLines.Length}");

        if (currentLineIndex < dialogLines.Length)
        {
            Debug.Log($"Displaying line {currentLineIndex + 1}/{dialogLines.Length}: {dialogLines[currentLineIndex]}");
            StartCoroutine(TypeLine(dialogLines[currentLineIndex]));
            currentLineIndex++;
        }
        else if (dialogChoices.Length > 0)
        {
            Debug.Log("All dialog lines displayed. Presenting choices.");
            DisplayChoices();
        }
        else
        {
            Debug.Log("All dialog lines displayed. Ending dialog.");
            EndDialog();
        }
    }

    private IEnumerator TypeLine(string line)
    {
        dialogText.text = "";
        isTyping = true;
        Debug.Log($"Typing line: {line}");

        foreach (char letter in line.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        Debug.Log("Line typing complete.");
    }

    private void CompleteLine()
    {
        StopAllCoroutines();
        dialogText.text = dialogLines[currentLineIndex - 1];
        isTyping = false;
        Debug.Log("Line typing skipped. Displaying full line.");
    }

    private void DisplayChoices()
    {
        Debug.Log("Dialog choices available.");

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < dialogChoices.Length)
            {
                choiceButtons[i].gameObject.SetActive(true); // Make the button visible
                choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = dialogChoices[i].choiceText;
                int choiceIndex = i; // Local copy of i for the closure
                choiceButtons[i].onClick.RemoveAllListeners(); // Clear any existing listeners
                choiceButtons[i].onClick.AddListener(() =>
                {
                    Debug.Log($"Button {choiceIndex + 1} pressed.");
                    OnChoiceSelected(choiceIndex);
                });
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false); // Hide unused buttons
            }
        }
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        Debug.Log($"Choice {choiceIndex + 1} selected.");
        dialogLines = dialogChoices[choiceIndex].resultingDialogLines;
        currentLineIndex = 0;

        // Hide the buttons after selection
        foreach (Button button in choiceButtons)
        {
            button.gameObject.SetActive(false);
        }
        EndDialog();
    }
    private void EndDialog()
    {
        dialogPanel.SetActive(false); // Hide the dialog panel
        FindObjectOfType<Movement>().enabled = true; // Re-enable player movement
        isDialogActive = false;
        isPlayerInTrigger = false;
        dialogText.text = "";

        // Destroy the GameObject that this script is attached to
        Destroy(gameObject);

        Debug.Log("Dialog ended. Player movement enabled and dialog panel deactivated. GameObject destroyed.");
    }
}

