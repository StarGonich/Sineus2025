using UnityEngine;

public class RoomBackgroundMusic : MonoBehaviour
{
    [Header("Music Settings")]
    public AudioSource backgroundMusic;

    public static RoomBackgroundMusic Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Музыка продолжается между сценами
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Запускаем музыку если она еще не играет
        if (backgroundMusic != null && !backgroundMusic.isPlaying)
        {
            backgroundMusic.Play();
        }
    }

    // Метод для остановки музыки (если понадобится)
    public void StopMusic()
    {
        if (backgroundMusic != null && backgroundMusic.isPlaying)
        {
            backgroundMusic.Stop();
        }
    }

    // Метод для возобновления музыки
    public void PlayMusic()
    {
        if (backgroundMusic != null && !backgroundMusic.isPlaying)
        {
            backgroundMusic.Play();
        }
    }
}