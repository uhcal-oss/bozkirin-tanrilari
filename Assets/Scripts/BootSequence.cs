using UnityEngine;
using UnityEngine.UI;

public class BootSequence : MonoBehaviour
{
    public CanvasGroup blackScreen;
    public float fadeSpeed = 0.5f;
    public AudioSource audioSource;
    public AudioClip turnOnSound; // Optional: The "TV static" or "Hum" sound

    void Awake()
    {
        if (blackScreen == null) blackScreen = GetComponent<CanvasGroup>();
        if (audioSource == null)
        {
            var menu = FindAnyObjectByType<MainMenuController>();
            if (menu != null) audioSource = menu.GetComponent<AudioSource>();
        }
    }

    void Start()
    {
        // Ensure screen is black at start
        blackScreen.alpha = 1; 
        // Play the "TV Turn On" sound
        if (turnOnSound!= null && audioSource!= null)
        {
            audioSource.PlayOneShot(turnOnSound);
        }
    }

    void Update()
    {
        // Slowly fade the black screen to invisible
        if (blackScreen.alpha > 0)
        {
            blackScreen.alpha -= Time.deltaTime * fadeSpeed;
        }
        else
        {
            // Once invisible, turn off this object so buttons work
            gameObject.SetActive(false); 
        }
    }
}