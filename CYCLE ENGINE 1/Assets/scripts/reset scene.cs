using UnityEngine;
using UnityEngine.SceneManagement; // Nécessaire pour gérer les scènes

public class ResetSceneOnKey : MonoBehaviour
{
    public KeyCode resetKey = KeyCode.R; // Touche pour reset

    void Update()
    {
        if (Input.GetKeyDown(resetKey))
        {
            // Recharge la scène active
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}