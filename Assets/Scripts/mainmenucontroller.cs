using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject loadingScreenPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Buttons")]
    [SerializeField] private Button optionsButton;

    [Header("Loading Screen Elements")]
    [SerializeField] private Slider progressBar;

    [Header("Scene Management")]
    [Tooltip("Add all 8 level scene names here.")]
    [SerializeField] private string[] playableScenes = new string[8];

    private const string HAS_PLAYED_KEY = "HasPlayedGame";

    private void Start()
    {
        // Explicitly force Main Menu visible on start, and hide everything else
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (loadingScreenPanel != null) loadingScreenPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);

        // Lock/unlock options based on whether player has played
        UpdateOptionsButtonState();
    }

    private void Update()
    {
        // Allow the player to press Escape to close Options or Credits and go back to Main Menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if ((optionsPanel != null && optionsPanel.activeSelf) || (creditsPanel != null && creditsPanel.activeSelf))
            {
                OnBackToMainMenuPressed();
            }
        }
    }

    // --- BUTTON CALLBACKS ---

    public void OnPlayButtonPressed()
    {
        if (playableScenes == null || playableScenes.Length == 0)
        {
            Debug.LogError("No scenes assigned in Playable Scenes array!");
            return;
        }

        // Save progress flag so Options menu unlocks permanently
        PlayerPrefs.SetInt(HAS_PLAYED_KEY, 1);
        PlayerPrefs.Save();

        // Pick a random scene index from the 8 available
        int randomIndex = Random.Range(0, playableScenes.Length);
        string selectedSceneName = playableScenes[randomIndex];

        StartCoroutine(LoadSceneAsync(selectedSceneName));
    }

    public void OnOptionsButtonPressed()
    {
        // Double check block just in case
        if (PlayerPrefs.GetInt(HAS_PLAYED_KEY, 0) == 0) return;

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void OnCreditsButtonPressed()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    public void OnBackToMainMenuPressed()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);

        UpdateOptionsButtonState();
    }

    public void OnQuitButtonPressed()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    // --- HELPERS ---

    private void UpdateOptionsButtonState()
    {
        if (optionsButton == null) return;

        bool hasPlayed = PlayerPrefs.GetInt(HAS_PLAYED_KEY, 0) == 1;
        
        // Setting interactable to false automatically grays out the button and blocks clicks
        optionsButton.interactable = hasPlayed;
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);

        if (loadingScreenPanel != null) loadingScreenPanel.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            if (progressBar != null)
            {
                progressBar.value = progress;
            }

            if (operation.progress >= 0.9f)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    [ContextMenu("Reset Has-Played Data (Testing)")]
    public void ResetPlayedStatus()
    {
        PlayerPrefs.DeleteKey(HAS_PLAYED_KEY);
        UpdateOptionsButtonState();
        Debug.Log("HasPlayedGame key reset. Options are now locked again.");
    }
}