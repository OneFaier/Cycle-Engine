using UnityEngine;
using UnityEngine.SceneManagement; // Obligatoire pour gérer les scènes

public class BackToMenu : MonoBehaviour
{
    [Header("Nom de la scène du menu")]
    public string mainMenuSceneName = "MainMenu"; // Mets ici le nom exact de ta scène principale

    // Appelle cette fonction depuis le bouton UI
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}