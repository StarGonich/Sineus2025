using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MelodyMiniGame : MonoBehaviour
{
    [Header("Game Settings")]
    public float noteDisplayTime = 1.5f;
    public float betweenNotesTime = 0.3f;
    public float inputTimeLimit = 5f;
    public float noteFadeTime = 0.8f;

    [Header("Spawn Area Settings")]
    [Tooltip("Отступ от левого края в пикселях")]
    public float spawnLeftMargin = 50f;
    [Tooltip("Отступ от правого края в пикселях")]
    public float spawnRightMargin = 50f;
    [Tooltip("Отступ от верха в пикселях")]
    public float spawnTopMargin = 150f;
    [Tooltip("Отступ от низа зоны спавна в пикселях")]
    public float spawnBottomMargin = 300f;

    [Header("UI References")]
    public Text sequenceText;
    public Text inputText;
    public Text scoreText;
    public Text messageText;
    public Text levelText;
    public Text energyText;
    public Text attentionText;
    public GameObject gamePanel;
    public Button exitButton;

    [Header("Key Buttons")]
    public Button keyButtonA;
    public Button keyButtonS;
    public Button keyButtonD;
    public Button keyButtonF;

    [Header("Key Sprites")]
    public Sprite keyNormalSprite;
    public Sprite keyPressedSprite;

    [Header("Note Display")]
    public GameObject notePrefab;
    public Transform noteParent;

    [Header("Sprite References")]
    public Image pianoPanel;
    public Image mainBackground;

    [Header("Audio Settings")]
    public AudioClip noteSoundA;
    public AudioClip noteSoundS;
    public AudioClip noteSoundD;
    public AudioClip noteSoundF;
    public AudioClip successSound;
    public AudioClip failSound;
    public float soundVolume = 0.7f;

    [Header("Visual Settings")]
    public Color keyHighlightColor = new Color(1f, 1f, 0.7f, 1f);

    private List<int> currentMelody = new List<int>();
    private List<int> playerInput = new List<int>();
    private int currentScore = 0;
    private int currentLevel = 1;
    private bool isPlayingSequence = false;
    private bool isWaitingForInput = false;
    private AudioSource audioSource;
    private Color[] keyColors = { Color.red, Color.green, Color.blue, Color.yellow };
    private List<GameObject> activeNotes = new List<GameObject>();
    private RectTransform canvasRect;
    private float canvasWidth;
    private float canvasHeight;

    public static MelodyMiniGame Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CreateAudioSource();
        SetupCamera();
        AutoAssignReferences();
        SetupCanvasInfo();
        SetupKeyButtons();

        StartNewGame();
    }

    void SetupCanvasInfo()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
            canvasWidth = canvasRect.rect.width;
            canvasHeight = canvasRect.rect.height;

            Debug.Log($"Canvas размер: {canvasWidth}x{canvasHeight}");
            Debug.Log($"Зона спавна: слева {spawnLeftMargin}, справа {canvasWidth - spawnRightMargin}, сверху {spawnTopMargin}, снизу {spawnBottomMargin}");
        }
        else
        {
            Debug.LogError("Canvas не найден!");
        }
    }

    void CreateAudioSource()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = soundVolume;
    }

    void SetupCamera()
    {
        if (Camera.main == null)
        {
            GameObject cameraObj = new GameObject("MainCamera");
            cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
            // НЕ добавляем AudioListener здесь!
        }
        else
        {
            // Удаляем AudioListener если он уже есть на EnergyManager
            AudioListener[] audioListeners = FindObjectsOfType<AudioListener>();
            if (audioListeners.Length > 1)
            {
                Debug.Log($"Найдено {audioListeners.Length} AudioListener. Удаляем лишние...");
                for (int i = 1; i < audioListeners.Length; i++)
                {
                    if (audioListeners[i].gameObject != EnergyManager.Instance?.gameObject)
                    {
                        Destroy(audioListeners[i]);
                        Debug.Log("Удален лишний AudioListener");
                    }
                }
            }
        }
    }

    void AutoAssignReferences()
    {
        if (sequenceText == null) sequenceText = GameObject.Find("SequenceText")?.GetComponent<Text>();
        if (inputText == null) inputText = GameObject.Find("InputText")?.GetComponent<Text>();
        if (scoreText == null) scoreText = GameObject.Find("ScoreText")?.GetComponent<Text>();
        if (messageText == null) messageText = GameObject.Find("MessageText")?.GetComponent<Text>();
        if (levelText == null) levelText = GameObject.Find("LevelText")?.GetComponent<Text>();
        if (energyText == null) energyText = GameObject.Find("EnergyText")?.GetComponent<Text>();
        if (attentionText == null) attentionText = GameObject.Find("AttentionText")?.GetComponent<Text>();
        if (gamePanel == null) gamePanel = GameObject.Find("GamePanel");
        if (exitButton == null) exitButton = GameObject.Find("ExitButton")?.GetComponent<Button>();

        if (keyButtonA == null) keyButtonA = GameObject.Find("KeyButtonA")?.GetComponent<Button>();
        if (keyButtonS == null) keyButtonS = GameObject.Find("KeyButtonS")?.GetComponent<Button>();
        if (keyButtonD == null) keyButtonD = GameObject.Find("KeyButtonD")?.GetComponent<Button>();
        if (keyButtonF == null) keyButtonF = GameObject.Find("KeyButtonF")?.GetComponent<Button>();

        if (pianoPanel == null) pianoPanel = GameObject.Find("PianoPanel")?.GetComponent<Image>();
        if (mainBackground == null) mainBackground = GameObject.Find("MainBackground")?.GetComponent<Image>();

        if (noteParent == null)
        {
            noteParent = new GameObject("NoteParent").transform;
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
                noteParent.SetParent(canvas.transform);
        }
    }

    void SetupKeyButtons()
    {
        if (keyButtonA != null)
        {
            keyButtonA.onClick.AddListener(() => OnKeyPressed(0));
            SetKeyButtonNormal(keyButtonA);
        }

        if (keyButtonS != null)
        {
            keyButtonS.onClick.AddListener(() => OnKeyPressed(1));
            SetKeyButtonNormal(keyButtonS);
        }

        if (keyButtonD != null)
        {
            keyButtonD.onClick.AddListener(() => OnKeyPressed(2));
            SetKeyButtonNormal(keyButtonD);
        }

        if (keyButtonF != null)
        {
            keyButtonF.onClick.AddListener(() => OnKeyPressed(3));
            SetKeyButtonNormal(keyButtonF);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ReturnToMainScene);
        }

        ClearAllNotes();
    }

    void SetKeyButtonNormal(Button button)
    {
        if (button != null && button.image != null)
        {
            if (keyNormalSprite != null)
            {
                button.image.sprite = keyNormalSprite;
            }
            button.image.color = Color.white;
        }
    }

    void SetKeyButtonPressed(Button button)
    {
        if (button != null && button.image != null)
        {
            if (keyPressedSprite != null)
            {
                button.image.sprite = keyPressedSprite;
            }
            button.image.color = keyHighlightColor;
        }
    }

    void StartNewGame()
    {
        currentScore = 0;
        currentLevel = 1;
        GenerateNewMelody();
        StartCoroutine(PlayMelodySequence());
        UpdateUI();
    }

    void GenerateNewMelody()
    {
        currentMelody.Clear();
        playerInput.Clear();

        int melodyLength = currentLevel + 2;
        for (int i = 0; i < melodyLength; i++)
        {
            currentMelody.Add(Random.Range(0, 4));
        }
    }

    IEnumerator PlayMelodySequence()
    {
        // Проверяем хватает ли энергии и внимания для игры
        if (EnergyManager.Instance != null && !EnergyManager.Instance.CanPlayPiano())
        {
            if (messageText != null) messageText.text = "Слишком устал! Нужно отдохнуть";
            yield return new WaitForSeconds(2f);
            ReturnToMainScene();
            yield break;
        }

        isPlayingSequence = true;
        isWaitingForInput = false;

        if (messageText != null) messageText.text = "Слушайте мелодию...";
        if (sequenceText != null) sequenceText.text = $"Последовательность из {currentMelody.Count} нот";
        if (inputText != null) inputText.text = "Ваш ввод: ";

        ClearAllNotes();

        for (int i = 0; i < currentMelody.Count; i++)
        {
            int noteIndex = currentMelody[i];
            Vector2 randomPosition = GetRandomSpawnPosition();
            CreateNote(noteIndex, randomPosition);
            PlayNoteSound(noteIndex);

            yield return new WaitForSeconds(noteDisplayTime);
            yield return new WaitForSeconds(betweenNotesTime);
        }

        yield return new WaitForSeconds(noteFadeTime);

        isPlayingSequence = false;
        isWaitingForInput = true;
        if (messageText != null) messageText.text = "Ваша очередь! Повторите мелодию";
    }

    Vector2 GetRandomSpawnPosition()
    {
        if (canvasRect == null)
        {
            Debug.LogWarning("Canvas не найден, используем координаты по умолчанию");
            return new Vector2(0, 200);
        }

        // Рассчитываем абсолютные координаты в пикселях от левого нижнего угла
        float minX = spawnLeftMargin;
        float maxX = canvasWidth - spawnRightMargin;
        float minY = spawnBottomMargin;
        float maxY = canvasHeight - spawnTopMargin;

        // Проверяем что зона спавна валидна
        if (minX >= maxX) minX = 50f;
        if (minY >= maxY) minY = 200f;

        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);

        // Конвертируем в координаты RectTransform (от центра)
        float rectX = x - canvasWidth / 2f;
        float rectY = y - canvasHeight / 2f;

        Debug.Log($"Создана нота: экран({x}, {y}), RectTransform({rectX}, {rectY})");

        return new Vector2(rectX, rectY);
    }

    void CreateNote(int noteIndex, Vector2 position)
    {
        if (notePrefab == null)
        {
            CreateTextNote(noteIndex, position);
        }
        else
        {
            CreateSpriteNote(noteIndex, position);
        }
    }

    void CreateTextNote(int noteIndex, Vector2 position)
    {
        GameObject noteObj = new GameObject($"Note_{noteIndex}");
        noteObj.transform.SetParent(noteParent);

        RectTransform rt = noteObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 100);
        rt.anchoredPosition = position;

        Text text = noteObj.AddComponent<Text>();
        text.text = GetKeyName(noteIndex);
        text.fontSize = 48;
        text.color = keyColors[noteIndex];
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        Shadow shadow = noteObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = new Vector2(2, -2);

        activeNotes.Add(noteObj);
        StartCoroutine(FadeOutNote(noteObj));
    }

    void CreateSpriteNote(int noteIndex, Vector2 position)
    {
        GameObject noteObj = Instantiate(notePrefab, noteParent);
        noteObj.name = $"Note_{noteIndex}";

        RectTransform rt = noteObj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(80, 80);
        }

        Image image = noteObj.GetComponent<Image>();
        if (image != null)
        {
            image.color = keyColors[noteIndex];
        }

        Text text = noteObj.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.text = GetKeyName(noteIndex);
            text.color = Color.white;
            text.fontSize = 24;

            Shadow shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.7f);
            shadow.effectDistance = new Vector2(1, -1);
        }

        activeNotes.Add(noteObj);
        StartCoroutine(FadeOutNote(noteObj));
    }

    IEnumerator FadeOutNote(GameObject noteObj)
    {
        CanvasGroup canvasGroup = noteObj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = noteObj.AddComponent<CanvasGroup>();
        }

        yield return new WaitForSeconds(noteDisplayTime - noteFadeTime);

        float timer = 0f;
        while (timer < noteFadeTime)
        {
            timer += Time.deltaTime;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f - (timer / noteFadeTime);
            }
            yield return null;
        }

        if (noteObj != null)
        {
            activeNotes.Remove(noteObj);
            Destroy(noteObj);
        }
    }

    void ClearAllNotes()
    {
        // Создаем временный список чтобы избежать ошибок при изменении коллекции
        List<GameObject> notesToDestroy = new List<GameObject>(activeNotes);
        activeNotes.Clear();

        foreach (GameObject note in notesToDestroy)
        {
            if (note != null)
            {
                Destroy(note);
            }
        }
    }

    public void OnKeyPressed(int keyIndex)
    {
        if (!isWaitingForInput || isPlayingSequence) return;

        playerInput.Add(keyIndex);
        StartCoroutine(FlashKey(keyIndex));
        PlayNoteSound(keyIndex);

        UpdateInputDisplay();
        CheckInput();
    }

    IEnumerator FlashKey(int keyIndex)
    {
        Button button = GetKeyButton(keyIndex);
        if (button != null)
        {
            SetKeyButtonPressed(button);
            yield return new WaitForSeconds(0.3f);
            SetKeyButtonNormal(button);
        }
    }

    void PlayNoteSound(int noteIndex)
    {
        AudioClip clip = GetNoteSound(noteIndex);
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    AudioClip GetNoteSound(int noteIndex)
    {
        switch (noteIndex)
        {
            case 0: return noteSoundA;
            case 1: return noteSoundS;
            case 2: return noteSoundD;
            case 3: return noteSoundF;
            default: return null;
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void CheckInput()
    {
        for (int i = 0; i < playerInput.Count; i++)
        {
            if (playerInput[i] != currentMelody[i])
            {
                // Неправильный ввод - тратим внимание и энергию
                if (EnergyManager.Instance != null)
                {
                    EnergyManager.Instance.DrainAttentionForSequence(currentMelody.Count, true);
                    EnergyManager.Instance.DrainEnergyForSequence(currentMelody.Count, true);
                }
                StartCoroutine(HandleWrongInput());
                return;
            }
        }

        if (playerInput.Count == currentMelody.Count)
        {
            // Правильный ввод - тратим внимание и энергию пропорционально длине
            if (EnergyManager.Instance != null)
            {
                EnergyManager.Instance.DrainAttentionForSequence(currentMelody.Count);
                EnergyManager.Instance.DrainEnergyForSequence(currentMelody.Count);
            }
            StartCoroutine(HandleCorrectSequence());
        }
    }

    IEnumerator HandleCorrectSequence()
    {
        isWaitingForInput = false;

        currentScore += 100 * currentLevel;
        currentLevel++;

        if (messageText != null) messageText.text = $"Правильно! +{100 * currentLevel} очков!";
        PlaySound(successSound);

        yield return StartCoroutine(SuccessAnimation());
        yield return new WaitForSeconds(1f);

        // Проверяем не закончилось ли внимание или энергия
        if (EnergyManager.Instance != null &&
            EnergyManager.Instance.HasAttention() &&
            EnergyManager.Instance.HasEnergy())
        {
            GenerateNewMelody();
            StartCoroutine(PlayMelodySequence());
        }
        else
        {
            if (messageText != null)
            {
                if (!EnergyManager.Instance.HasAttention())
                    messageText.text = "Внимание закончилось! Нужно отдохнуть";
                else
                    messageText.text = "Энергия закончилась! Нужно отдохнуть";
            }
            yield return new WaitForSeconds(2f);
            ReturnToMainScene();
        }

        UpdateUI();
    }

    IEnumerator HandleWrongInput()
    {
        isWaitingForInput = false;

        if (messageText != null) messageText.text = "Неправильно! Попробуйте еще раз";
        PlaySound(failSound);

        for (int i = 0; i < 2; i++)
        {
            HighlightAllKeys(true);
            yield return new WaitForSeconds(0.3f);
            HighlightAllKeys(false);
            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(1f);

        // Проверяем внимание и энергию перед повторением
        if (EnergyManager.Instance != null &&
            EnergyManager.Instance.HasAttention() &&
            EnergyManager.Instance.HasEnergy())
        {
            playerInput.Clear();
            StartCoroutine(PlayMelodySequence());
        }
        else
        {
            if (messageText != null)
            {
                if (!EnergyManager.Instance.HasAttention())
                    messageText.text = "Внимание закончилось! Нужно отдохнуть";
                else
                    messageText.text = "Энергия закончилась! Нужно отдохнуть";
            }
            yield return new WaitForSeconds(2f);
            ReturnToMainScene();
        }

        UpdateUI();
    }

    IEnumerator SuccessAnimation()
    {
        for (int i = 0; i < 3; i++)
        {
            HighlightAllKeys(true);
            yield return new WaitForSeconds(0.2f);
            HighlightAllKeys(false);
            yield return new WaitForSeconds(0.2f);
        }
    }

    void HighlightAllKeys(bool pressed)
    {
        for (int i = 0; i < 4; i++)
        {
            Button button = GetKeyButton(i);
            if (pressed)
                SetKeyButtonPressed(button);
            else
                SetKeyButtonNormal(button);
        }
    }

    Button GetKeyButton(int keyIndex)
    {
        switch (keyIndex)
        {
            case 0: return keyButtonA;
            case 1: return keyButtonS;
            case 2: return keyButtonD;
            case 3: return keyButtonF;
            default: return null;
        }
    }

    void UpdateInputDisplay()
    {
        if (inputText != null)
            inputText.text = "Ваш ввод: " + GetMelodyString(playerInput);
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Счет: " + currentScore;
        if (levelText != null) levelText.text = "Уровень: " + currentLevel;
        if (sequenceText != null) sequenceText.text = "Длина: " + currentMelody.Count + " нот";

        // Обновляем отображение энергии и внимания
        if (EnergyManager.Instance != null)
        {
            if (energyText != null)
                energyText.text = $"Энергия: {Mathf.RoundToInt(EnergyManager.Instance.GetCurrentEnergy())}/{Mathf.RoundToInt(EnergyManager.Instance.maxEnergy)}";
            if (attentionText != null)
                attentionText.text = $"Внимание: {Mathf.RoundToInt(EnergyManager.Instance.GetCurrentAttention())}/{Mathf.RoundToInt(EnergyManager.Instance.maxAttention)}";
        }
    }

    string GetMelodyString(List<int> melody)
    {
        string result = "";
        foreach (int note in melody)
        {
            result += GetKeyName(note) + " ";
        }
        return result.Trim();
    }

    string GetKeyName(int keyIndex)
    {
        switch (keyIndex)
        {
            case 0: return "A";
            case 1: return "S";
            case 2: return "D";
            case 3: return "F";
            default: return "?";
        }
    }

    public void ReturnToMainScene()
    {
        Debug.Log("Возвращаемся в главную сцену...");

        // ОЧИЩАЕМ ВСЕ НОТЫ ПЕРЕД ВЫХОДОМ
        ClearAllNotesImmediately();

        // Показываем UI EnergyManager
        if (EnergyManager.Instance != null)
        {
            EnergyManager.Instance.ShowUI();
        }

        SceneManager.LoadScene("MainScene");
    }

    // Добавляем метод для немедленной очистки всех нот
    void ClearAllNotesImmediately()
    {
        Debug.Log($"Очищаем все ноты перед выходом. Активных нот: {activeNotes.Count}");

        foreach (GameObject note in activeNotes)
        {
            if (note != null)
            {
                DestroyImmediate(note); // Немедленное уничтожение
            }
        }
        activeNotes.Clear();

        // Дополнительная очистка на всякий случай
        Note2D[] allNotes = FindObjectsOfType<Note2D>();
        foreach (Note2D note in allNotes)
        {
            if (note != null && note.gameObject != null)
            {
                DestroyImmediate(note.gameObject);
            }
        }

        Debug.Log("Все ноты очищены");
    }

    void Update()
    {
        if (isWaitingForInput && !isPlayingSequence)
        {
            if (Input.GetKeyDown(KeyCode.A)) OnKeyPressed(0);
            if (Input.GetKeyDown(KeyCode.S)) OnKeyPressed(1);
            if (Input.GetKeyDown(KeyCode.D)) OnKeyPressed(2);
            if (Input.GetKeyDown(KeyCode.F)) OnKeyPressed(3);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMainScene();
        }

        // Постоянно обновляем UI для отображения изменений энергии
        UpdateUI();
    }
}