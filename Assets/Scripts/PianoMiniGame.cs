using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PianoMiniGame : MonoBehaviour
{
    [Header("MiniGame Settings")]
    public GameObject gameUI;
    public Key[] keys;
    public float noteSpeed = 2f;
    public float spawnInterval = 1.5f;

    [Header("Scoring")]
    public int score = 0;
    public int notesHit = 0;
    public int notesMissed = 0;

    [Header("UI References")]
    public Text scoreText;
    public Text notesHitText;
    public Text notesMissedText;
    public GameObject[] keyIndicators;

    private bool isPlaying = false;
    private Coroutine spawnCoroutine;

    [System.Serializable]
    public class Key
    {
        public KeyCode inputKey;
        public GameObject notePrefab;
        public Transform spawnPoint;
        public Transform targetPoint;
    }

    void Start()
    {
        if (gameUI != null)
            gameUI.SetActive(false);
    }

    public void StartGame()
    {
        isPlaying = true;
        score = 0;
        notesHit = 0;
        notesMissed = 0;

        if (gameUI != null)
            gameUI.SetActive(true);

        spawnCoroutine = StartCoroutine(SpawnNotes());
    }

    public void StopGame()
    {
        isPlaying = false;

        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        if (gameUI != null)
            gameUI.SetActive(false);

        Note[] allNotes = FindObjectsOfType<Note>();
        foreach (var note in allNotes)
        {
            if (note != null)
                Destroy(note.gameObject);
        }
    }

    IEnumerator SpawnNotes()
    {
        while (isPlaying)
        {
            Key randomKey = keys[Random.Range(0, keys.Length)];

            GameObject noteObj = Instantiate(randomKey.notePrefab,
                randomKey.spawnPoint.position, Quaternion.identity);

            Note note = noteObj.GetComponent<Note>();
            if (note != null)
            {
                note.SetupNote(randomKey.targetPoint.position, noteSpeed, randomKey.inputKey);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void Update()
    {
        if (!isPlaying) return;

        foreach (Key key in keys)
        {
            if (Input.GetKeyDown(key.inputKey))
            {
                CheckNoteHit(key.inputKey);
            }
        }
    }

    void UpdateKeyIndicators(KeyCode pressedKey = KeyCode.None)
    {
        if (keyIndicators == null || keyIndicators.Length == 0) return;

        for (int i = 0; i < keys.Length && i < keyIndicators.Length; i++)
        {
            Image indicator = keyIndicators[i].GetComponent<Image>();
            if (indicator != null)
            {
                if (keys[i].inputKey == pressedKey)
                {
                    // Подсвечиваем нажатую клавишу
                    indicator.color = Color.yellow;
                }
                else
                {
                    // Возвращаем обычный цвет
                    indicator.color = Color.gray;
                }
            }
        }
    }

    void OnKeyPressed(KeyCode key)
    {
        UpdateKeyIndicators(key);

        // Возвращаем цвет через 0.1 секунды
        Invoke("ResetKeyIndicators", 0.1f);

        // Звук клавиши...
    }

    void ResetKeyIndicators()
    {
        UpdateKeyIndicators(KeyCode.None);
    }

    void CheckNoteHit(KeyCode keyPressed)
    {
        Note[] notes = FindObjectsOfType<Note>();

        foreach (Note note in notes)
        {
            if (note != null && note.IsInHitZone() && note.GetTargetKey() == keyPressed)
            {
                note.PlayHit();
                notesHit++;
                score += 100;
                return;
            }
        }

        notesMissed++;
        score = Mathf.Max(0, score - 50);
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }
}