using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogTrigger : MonoBehaviour
{
    public string[] dialogLines;
    public GameObject dialogUI; // The UI element that holds the dialog text and background
    public TextMeshProUGUI dialogText; // The TMP Text component where the dialog will be displayed
    public float typingSpeed = 0.05f;

    private int currentLineIndex = 0;
    private bool isDialogActive = false;
    private bool isPlayerInTrigger = false;
    private bool isTyping = false;

    private void Start()
    {
        dialogUI.SetActive(false); // Ensure the dialog UI is hidden initially
        Debug.Log("DialogTrigger script initialized. Dialog UI is hidden.");
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
        dialogUI.SetActive(true);
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

    private void EndDialog()
    {
        dialogUI.SetActive(false);
        FindObjectOfType<Movement>().enabled = true; // Re-enable player movement
        isDialogActive = false;
        isPlayerInTrigger = false;
        gameObject.SetActive(false); // Make the object disappear
        Debug.Log("Dialog ended. Player movement enabled and dialog object deactivated.");
    }
}
