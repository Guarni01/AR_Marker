using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Menu iniziale: al click su Play carica la scena di gioco.
/// </summary>
public class StartMenu : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private string gameSceneName = "Test_00_Alpha";

    private void Awake()
    {
        if (playButton != null)
            playButton.onClick.AddListener(Play);
    }

    private void OnDestroy()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(Play);
    }

    private void Play()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}