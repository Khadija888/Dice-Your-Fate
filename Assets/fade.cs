using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class FadeToBlackTrigger : MonoBehaviour
{
    [Header("UI Setup")]
    [SerializeField] private GameObject fadeCanvasObject;   // Drag your whole FadeCanvas GameObject here
    [SerializeField] private CanvasGroup fadeCanvasGroup;   // Drag the CanvasGroup component here
    [SerializeField] private TMP_Text fadeMessageText;       // Text element for the screen message
    [SerializeField] private Button menuButton;             // The UI Button to return to menu
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Exact name of your Main Menu scene
    
    [TextArea] 
    [SerializeField] private string customMessage = "The shadow catches up to you..."; 

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 2.0f;       // Time in seconds to reach pitch black
    [SerializeField] private float messageDelay = 0.5f;       // Pause before fading in text

    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fadeStingSound;       // Creepy sound playing during fade

    private bool hasTriggered = false;

    private void Start()
    {
        // Ensure canvas and button start hidden
        if (fadeCanvasObject != null) fadeCanvasObject.SetActive(false);
        if (fadeCanvasGroup != null) fadeCanvasGroup.alpha = 0f;
        if (menuButton != null) menuButton.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(ExecuteFadeSequence());
        }
    }

    private IEnumerator ExecuteFadeSequence()
    {
        // 1. Force the canvas object to be active
        if (fadeCanvasObject != null)
        {
            fadeCanvasObject.SetActive(true);
        }
        else
        {
            yield break;
        }

        // Optional audio sting
        if (audioSource != null && fadeStingSound != null)
        {
            audioSource.PlayOneShot(fadeStingSound);
        }

        if (fadeMessageText != null)
        {
            fadeMessageText.text = customMessage;
            Color c = fadeMessageText.color;
            c.a = 0f;
            fadeMessageText.color = c; 
        }

        fadeCanvasGroup.blocksRaycasts = true;
        float elapsed = 0f;

        // 2. Smoothly Fade to Black
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;

        yield return new WaitForSeconds(messageDelay);

        // 3. Fade in the message text
        if (fadeMessageText != null)
        {
            float elapsedText = 0f;
            float textFadeDuration = 1.0f;
            Color textColor = fadeMessageText.color;

            while (elapsedText < textFadeDuration)
            {
                elapsedText += Time.deltaTime;
                textColor.a = Mathf.Clamp01(elapsedText / textFadeDuration);
                fadeMessageText.color = textColor;
                yield return null;
            }

            textColor.a = 1f;
            fadeMessageText.color = textColor;
        }

        // 4. Reveal the Main Menu button once the cutscene text finishes
        if (menuButton != null)
        {
            menuButton.gameObject.SetActive(true);
        }

        Debug.Log("Screen fade complete. Menu button is now active.");
    }

    // --- BUTTON HOOK: Call this method when the button is clicked ---
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}