using UnityEngine;

public class UnderwaterAudioTrigger : MonoBehaviour
{
    public AudioSource underwaterAudio; // Ton son global underwater

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Assure-toi que ton Player a le tag "Player"
        {
            underwaterAudio.Pause(); // stoppe le son quand le joueur entre
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            underwaterAudio.UnPause(); // relance le son quand le joueur sort
        }
    }
}
