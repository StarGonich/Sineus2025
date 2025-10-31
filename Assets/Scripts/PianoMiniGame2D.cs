using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class PianoMiniGame2D : MonoBehaviour
{
    [Header("Game Settings")]
    public GameObject notePrefab;
    public Transform[] spawnPoints;
    public float noteSpeed = 3f;
    public float spawnInterval = 1.5f;
    public float gameDuration = 30f;

    [Header("UI References")]
    public Text scoreText;
    public Text timerText;
    public Text accuracyText;
    public GameObject gamePanel;

    [Header("Audio")]
    public AudioClip[] keySounds;

    private int score = 0;
    private int notesHit = 0;
    private int notesMissed = 0;
    private float gameTimer = 0f;
    private bool isPlaying = false;
    private Coroutine spawnCoroutine;

    public static PianoMiniGame2D Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (gamePanel != null)
            gamePanel.SetActive(false);
    }

    public void StartGame()
    {
        isPlaying = true;
        score = 0;
        notesHit = 0;
        notesMissed = 0;
        gameTimer = gameDuration;

        if (gamePanel != null)
            gamePanel.SetActive(true);

        UpdateUI();
        spawnCoroutine = StartCoroutine(SpawnNotes());

        // Скрываем курсор
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void StopGame()
    {
        isPlaying = false;

        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        if (gamePanel != null)
            gamePanel.SetActive(false);

        // Убираем все активные ноты
        Note2D[] allNotes = FindObjectsOfType<Note2D>();
        foreach (var note in allNotes)
        {
            if (note != null)
                Destroy(note.gameObject);
        }

        // Возвращаем курсор
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    IEnumerator SpawnNotes()
    {
        while (isPlaying && gameTimer > 0)
        {
            // Случайная клавиша
            int randomIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[randomIndex];
            KeyNote randomNote = (KeyNote)randomIndex;

            if (notePrefab != null && spawnPoint != null)
            {
                GameObject noteObj = Instantiate(notePrefab, spawnPoint.position, Quaternion.identity);
                Note2D note = noteObj.GetComponent<Note2D>();
                if (note != null)
                {
                    note.targetNote = randomNote;
                    note.speed = noteSpeed;
                }
            }

            yield return new WaitForSeconds(spawnInterval);
        }

        // Завершаем игру когда время вышло
        if (isPlaying)
        {
            StopGame();
        }
    }

    void Update()
    {
        if (!isPlaying) return;

        // Таймер игры
        gameTimer -= Time.deltaTime;
        if (gameTimer <= 0)
        {
            gameTimer = 0;
            StopGame();
        }

        UpdateUI();

        // Выход по Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopGame();
            ReturnToMainScene();
        }
    }

    public void OnKeyPressed(KeyNote note)
    {
        // Воспроизводим звук
        PlayKeySound(note);

        // Проверяем попадание по нотам
        CheckNoteHit(note);
    }

    void CheckNoteHit(KeyNote pressedNote)
    {
        Note2D[] notes = FindObjectsOfType<Note2D>();
        bool hit = false;

        foreach (Note2D note in notes)
        {
            if (note != null && note.CanBeHit() && note.targetNote == pressedNote)
            {
                note.Hit();
                notesHit++;
                score += 100;
                hit = true;
                break;
            }
        }

        if (!hit)
        {
            // Штраф за нажатие не в такт
            notesMissed++;
            score = Mathf.Max(0, score - 30);
        }
    }

    void PlayKeySound(KeyNote note)
    {
        if (keySounds != null && keySounds.Length > (int)note)
        {
            AudioSource.PlayClipAtPoint(keySounds[(int)note], Camera.main.transform.position, 0.5f);
        }
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";

        if (timerText != null)
            timerText.text = $"Time: {Mathf.Ceil(gameTimer)}s";

        if (accuracyText != null)
        {
            float totalNotes = notesHit + notesMissed;
            float accuracy = totalNotes > 0 ? (float)notesHit / totalNotes * 100f : 100f;
            accuracyText.text = $"Accuracy: {accuracy:F1}%";
        }
    }

    void ReturnToMainScene()
    {
        // Возвращаемся в основную сцену
        SceneManager.LoadScene("MainScene");
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }
}