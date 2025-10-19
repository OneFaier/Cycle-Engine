using UnityEngine;
using UnityEngine.SceneManagement;

public class ReloadScene : MonoBehaviour
{
    void Update()
    {
        // Vérifie si la touche R est pressée
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Recharge la scène active
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
