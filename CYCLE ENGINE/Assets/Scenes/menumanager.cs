using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Canvas")]
    public GameObject mainMenu;
    public GameObject playMenu;
    public GameObject settingsMenu;

    [Header("Buttons")]
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;

    public Button soloButton;
    public Button multiButton;
    public Button backFromPlayButton;
    public Button backFromSettingsButton;

    void Start()
    {
        mainMenu.SetActive(true);
        playMenu.SetActive(false);
        settingsMenu.SetActive(false);

        playButton.onClick.AddListener(OpenPlayMenu);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(QuitGame);

        soloButton.onClick.AddListener(PlaySolo);
        multiButton.onClick.AddListener(PlayMultiplayer);
        backFromPlayButton.onClick.AddListener(BackToMainMenu);
        backFromSettingsButton.onClick.AddListener(BackToMainMenu);
    }

    void OpenPlayMenu()
    {
        mainMenu.SetActive(false);
        playMenu.SetActive(true);
    }

    void OpenSettings()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    void BackToMainMenu()
    {
        playMenu.SetActive(false);
        settingsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    void PlaySolo()
    {
        SceneManager.LoadScene("SoloScene");
    }

    void PlayMultiplayer()
    {
        SceneManager.LoadScene("MultiplayerScene");
    }

    void QuitGame()
    {
        Application.Quit();
    }
}