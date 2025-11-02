using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MelodyMiniGame : MonoBehaviour
{
    [Header("Game Settings")]
    public float noteDisplayTime = 2f;
    public float betweenNotesTime = 0.5f;
    public float noteFadeTime = 0.8f;

    [Header("Note Spawn Area")]
    public RectTransform noteDisplayArea; // Область для отображения нот
    public float noteMargin = 20f; // Отступ от краев области

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

    [Header("Individual Key Sprites")]
    public Sprite keyNormalA;
    public Sprite keyPressedA;
    public Sprite keyNormalS;
    public Sprite keyPressedS;
    public Sprite keyNormalD;
    public Sprite keyPressedD;
    public Sprite keyNormalF;
    public Sprite keyPressedF;

    [Header("Note Display")]
    public GameObject notePrefab;

    [Header("Note Sprites")]
    public Sprite noteSpriteA;
    public Sprite noteSpriteS;
    public Sprite noteSpriteD;
    public Sprite noteSpriteF;

    [Header("Audio Settings")]
    public AudioClip noteSoundA;
    public AudioClip noteSoundS;
    public AudioClip noteSoundD;
    public AudioClip noteSoundF;
    public AudioClip successSound;
    public AudioClip failSound;
    public float soundVolume = 0.7f;

    private List<int> currentMelody = new List<int>();
    private List<int> playerInput = new List<int>();
    private int currentScore = 0;
    private int currentLevel = 1;
    private bool isPlayingSequence = false;
    private bool isWaitingForInput = false;
    private AudioSource audioSource;
    private GameObject currentNote; // Текущая отображаемая нота

    private int currentSequenceLength = 0;
    private static int savedLevel = 1;
    private static int savedScore = 0;

    private Dictionary<KeyCode, (int index, Button button)> keyMappings = new Dictionary<KeyCode, (int, Button)>();
    private Dictionary<int, bool> keyPressedState = new Dictionary<int, bool>();

    public static MelodyMiniGame Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CreateAudioSource();
        AutoAssignReferences();
        SetupKeyButtons();
        SetupKeyMappings();

        // ВОССТАНАВЛИВАЕМ СОХРАНЕННЫЙ УРОВЕНЬ И ОЧКИ
        currentLevel = savedLevel;
        currentScore = savedScore;

        StartNewGame();
    }

    void SetupKeyMappings()
    {
        keyMappings[KeyCode.A] = (0, keyButtonA);
        keyMappings[KeyCode.S] = (1, keyButtonS);
        keyMappings[KeyCode.D] = (2, keyButtonD);
        keyMappings[KeyCode.F] = (3, keyButtonF);

        for (int i = 0; i < 4; i++)
        {
            keyPressedState[i] = false;
        }
    }

    void CreateAudioSource()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = soundVolume;
    }

    void AutoAssignReferences()
    {
        // Автоназначение UI элементов с улучшенным поиском
        if (sequenceText == null)
        {
            sequenceText = GameObject.Find("SequenceText")?.GetComponent<Text>();
            if (sequenceText == null) Debug.LogWarning("SequenceText не найден");
        }

        if (inputText == null)
        {
            inputText = GameObject.Find("InputText")?.GetComponent<Text>();
            if (inputText == null) Debug.LogWarning("InputText не найден");
        }

        if (scoreText == null)
        {
            scoreText = GameObject.Find("ScoreText")?.GetComponent<Text>();
            if (scoreText == null) Debug.LogWarning("ScoreText не найден");
        }

        if (messageText == null)
        {
            messageText = GameObject.Find("MessageText")?.GetComponent<Text>();
            if (messageText == null) Debug.LogWarning("MessageText не найден");
        }

        if (levelText == null)
        {
            levelText = GameObject.Find("LevelText")?.GetComponent<Text>();
            if (levelText == null) Debug.LogWarning("LevelText не найден");
        }

        if (energyText == null)
        {
            energyText = GameObject.Find("EnergyText")?.GetComponent<Text>();
            if (energyText == null) Debug.LogWarning("EnergyText не найден");
        }

        if (attentionText == null)
        {
            attentionText = GameObject.Find("AttentionText")?.GetComponent<Text>();
            if (attentionText == null) Debug.LogWarning("AttentionText не найден");
        }

        if (gamePanel == null)
        {
            gamePanel = GameObject.Find("GamePanel");
            if (gamePanel == null) Debug.LogWarning("GamePanel не найден");
        }

        if (exitButton == null)
        {
            exitButton = GameObject.Find("ExitButton")?.GetComponent<Button>();
            if (exitButton == null) Debug.LogWarning("ExitButton не найден");
        }

        // Поиск кнопок клавиш
        if (keyButtonA == null) keyButtonA = GameObject.Find("KeyButtonA")?.GetComponent<Button>();
        if (keyButtonS == null) keyButtonS = GameObject.Find("KeyButtonS")?.GetComponent<Button>();
        if (keyButtonD == null) keyButtonD = GameObject.Find("KeyButtonD")?.GetComponent<Button>();
        if (keyButtonF == null) keyButtonF = GameObject.Find("KeyButtonF")?.GetComponent<Button>();

        // Создаем область для отображения нот если не назначена
        if (noteDisplayArea == null)
        {
            CreateNoteDisplayArea();
        }
    }

    void CreateNoteDisplayArea()
    {
        GameObject areaObj = new GameObject("NoteDisplayArea");
        noteDisplayArea = areaObj.AddComponent<RectTransform>();

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            noteDisplayArea.SetParent(canvas.transform);

            // Настраиваем область занимающую верхние 60% экрана
            noteDisplayArea.anchorMin = new Vector2(0.1f, 0.4f);
            noteDisplayArea.anchorMax = new Vector2(0.9f, 0.9f);
            noteDisplayArea.offsetMin = Vector2.zero;
            noteDisplayArea.offsetMax = Vector2.zero;
            noteDisplayArea.localPosition = Vector3.zero;
            noteDisplayArea.localScale = Vector3.one;

            // Добавляем Image для визуализации (можно отключить)
            Image bg = areaObj.AddComponent<Image>();
            bg.color = new Color(0, 1, 0, 0.1f); // Зеленая прозрачная для отладки
        }
    }

    void SetupKeyButtons()
    {
        if (keyButtonA != null) keyButtonA.onClick.AddListener(() => OnKeyPressed(0));
        if (keyButtonS != null) keyButtonS.onClick.AddListener(() => OnKeyPressed(1));
        if (keyButtonD != null) keyButtonD.onClick.AddListener(() => OnKeyPressed(2));
        if (keyButtonF != null) keyButtonF.onClick.AddListener(() => OnKeyPressed(3));
        if (exitButton != null) exitButton.onClick.AddListener(ReturnToMainScene);

        SetAllKeysNormal();
    }

    void StartNewGame()
    {
        // ПРОВЕРКА ПРИ СТАРТЕ: если внимание 0, сразу выходим
        if (EnergyManager.Instance != null && EnergyManager.Instance.GetCurrentAttention() <= 0)
        {
            Debug.Log("Внимание полностью истощено - выход из пианино");
            if (messageText != null) messageText.text = "Слишком устал для концентрации!";
            StartCoroutine(ExitDueToNoAttention());
            return;
        }

        // currentLevel не сбрасываем - используем сохраненное значение
        currentScore = 0;
        GenerateNewMelody();
        StartCoroutine(PlayMelodySequence());
        UpdateUI();

        Debug.Log($"Начата новая игра с уровнем: {currentLevel}");
    }

    IEnumerator ExitDueToNoAttention()
    {
        yield return new WaitForSeconds(1.5f);
        ReturnToMainScene();
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }
    void GenerateNewMelody()
    {
        currentMelody.Clear();
        playerInput.Clear();

        // НАЧИНАЕМ С ТЕКУЩЕГО УРОВНЯ (а не с 1)
        int melodyLength = currentLevel + 2;
        for (int i = 0; i < melodyLength; i++)
        {
            currentMelody.Add(Random.Range(0, 4));
        }
    }

    public void ResetProgress()
    {
        currentLevel = 1;
        currentScore = 0;
        savedLevel = 1;
        savedScore = 0;
        Debug.Log("Прогресс пианино полностью сброшен");
    }

    public static int GetSavedLevel()
    {
        return savedLevel;
    }

    public static void SetSavedLevel(int level)
    {
        savedLevel = level;
    }

    IEnumerator PlayMelodySequence()
    {
        // ДОПОЛНИТЕЛЬНАЯ ПРОВЕРКА: если внимание 0, выходим
        if (EnergyManager.Instance != null && EnergyManager.Instance.GetCurrentAttention() <= 0)
        {
            if (messageText != null) messageText.text = "Внимание закончилось! Нужно отдохнуть";
            yield return new WaitForSeconds(1.5f);
            ReturnToMainScene();
            yield break;
        }

        if (EnergyManager.Instance != null && !EnergyManager.Instance.CanPlayPiano())
        {
            if (messageText != null) messageText.text = "Слишком устал! Нужно отдохнуть";
            yield return new WaitForSeconds(1.5f);
            ReturnToMainScene();
            yield break;
        }

        isPlayingSequence = true;
        isWaitingForInput = false;

        if (messageText != null) messageText.text = "Слушайте мелодию...";
        if (sequenceText != null) sequenceText.text = $"Последовательность из {currentMelody.Count} нот";
        if (inputText != null) inputText.text = "Ваш ввод: ";

        ClearCurrentNote();

        // ПОСЛЕДОВАТЕЛЬНО ПОКАЗЫВАЕМ НОТЫ В СЛУЧАЙНЫХ МЕСТАХ ВНУТРИ ОБЛАСТИ
        for (int i = 0; i < currentMelody.Count; i++)
        {
            int noteIndex = currentMelody[i];

            // ДОПОЛНИТЕЛЬНАЯ ПРОВЕРКА В ПРОЦЕССЕ: если внимание стало 0, прерываем
            if (EnergyManager.Instance != null && EnergyManager.Instance.GetCurrentAttention() <= 0)
            {
                if (messageText != null) messageText.text = "Внимание закончилось во время игры!";
                yield return new WaitForSeconds(1f);
                ReturnToMainScene();
                yield break;
            }

            // СОЗДАЕМ НОТУ В СЛУЧАЙНОЙ ПОЗИЦИИ ВНУТРИ ОБЛАСТИ
            Vector2 randomPosition = GetRandomPositionInArea();
            CreateNoteAtPosition(noteIndex, randomPosition);
            PlayNoteSound(noteIndex);

            // ЖДЕМ ПОКА НОТА ПОКАЖЕТСЯ И НАЧНЕТ ИСЧЕЗАТЬ
            yield return new WaitForSeconds(noteDisplayTime);

            // КОРОТКАЯ ПАУЗА МЕЖДУ НОТАМИ
            yield return new WaitForSeconds(betweenNotesTime);
        }

        isPlayingSequence = false;
        isWaitingForInput = true;
        if (messageText != null) messageText.text = "Ваша очередь! Повторите мелодию";
    }

    Vector2 GetRandomPositionInArea()
    {
        if (noteDisplayArea == null)
            return Vector2.zero;

        // Получаем размеры области в локальных координатах
        Rect areaRect = noteDisplayArea.rect;

        // Рассчитываем доступную область с учетом отступов
        float availableWidth = areaRect.width - (noteMargin * 2);
        float availableHeight = areaRect.height - (noteMargin * 2);

        // Генерируем случайную позицию ВНУТРИ области
        float randomX = Random.Range(-availableWidth / 2, availableWidth / 2);
        float randomY = Random.Range(-availableHeight / 2, availableHeight / 2);

        Vector2 localPosition = new Vector2(randomX, randomY);

        Debug.Log($"Случайная позиция в области: {localPosition}, границы: {areaRect}");

        return localPosition;
    }

    void CreateNoteAtPosition(int noteIndex, Vector2 position)
    {
        ClearCurrentNote();

        if (notePrefab == null)
        {
            CreateTextNote(noteIndex, position);
        }
        else
        {
            CreateSpriteNote(noteIndex, position);
        }
    }

    void CreateSpriteNote(int noteIndex, Vector2 position)
    {
        if (notePrefab == null) return;

        currentNote = Instantiate(notePrefab, noteDisplayArea);
        currentNote.name = $"Note_{noteIndex}";

        // НАСТРАИВАЕМ ПОЗИЦИЮ - СЛУЧАЙНОЕ МЕСТО В ОБЛАСТИ
        RectTransform rt = currentNote.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = position; // Случайная позиция
            rt.sizeDelta = new Vector2(120, 120);
            rt.localScale = Vector3.one;
        }

        // УСТАНАВЛИВАЕМ СПРАЙТ НОТЫ
        Image image = currentNote.GetComponent<Image>();
        if (image != null)
        {
            Sprite noteSprite = GetNoteSprite(noteIndex);
            if (noteSprite != null)
            {
                image.sprite = noteSprite;
                image.preserveAspect = true;
            }
            image.color = Color.white;
        }

        // ЗАПУСКАЕМ АВТОМАТИЧЕСКОЕ ЗАТУХАНИЕ
        StartCoroutine(AutoFadeNote(currentNote));
    }

    void CreateTextNote(int noteIndex, Vector2 position)
    {
        GameObject noteObj = new GameObject($"Note_{noteIndex}");
        noteObj.transform.SetParent(noteDisplayArea);

        RectTransform rt = noteObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(100, 100);

        Text text = noteObj.AddComponent<Text>();
        text.text = GetKeyName(noteIndex);
        text.fontSize = 40;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.alignment = TextAnchor.MiddleCenter;

        currentNote = noteObj;
        StartCoroutine(AutoFadeNote(currentNote));
    }

    IEnumerator AutoFadeNote(GameObject noteObj)
    {
        CanvasGroup canvasGroup = noteObj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = noteObj.AddComponent<CanvasGroup>();
        }

        // ЖДЕМ ПЕРЕД НАЧАЛОМ ЗАТУХАНИЯ
        yield return new WaitForSeconds(noteDisplayTime - noteFadeTime);

        // ПЛАВНОЕ ЗАТУХАНИЕ
        float timer = 0f;
        while (timer < noteFadeTime && noteObj != null)
        {
            timer += Time.deltaTime;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f - (timer / noteFadeTime);
            }
            yield return null;
        }

        // УДАЛЯЕМ НОТУ ПОСЛЕ ЗАТУХАНИЯ
        if (noteObj != null && noteObj == currentNote)
        {
            Destroy(noteObj);
            currentNote = null;
        }
    }

    void ClearCurrentNote()
    {
        if (currentNote != null)
        {
            Destroy(currentNote);
            currentNote = null;
        }
    }

    // ОСТАЛЬНЫЕ МЕТОДЫ ОСТАЮТСЯ БЕЗ ИЗМЕНЕНИЙ
    public void OnKeyPressed(int keyIndex)
    {
        if (!isWaitingForInput || isPlayingSequence) return;

        // ПРОВЕРКА ПРИ НАЖАТИИ КЛАВИШИ: если внимание 0, выходим
        if (EnergyManager.Instance != null && EnergyManager.Instance.GetCurrentAttention() <= 0)
        {
            if (messageText != null) messageText.text = "Внимание закончилось!";
            StartCoroutine(ExitDueToNoAttention());
            return;
        }

        playerInput.Add(keyIndex);
        StartCoroutine(FlashKey(keyIndex));
        PlayNoteSound(keyIndex);

        UpdateInputDisplay();
        CheckInput();
    }

    IEnumerator FlashKey(int keyIndex)
    {
        SetKeyButtonPressed(keyIndex);
        yield return new WaitForSeconds(0.3f);
        SetKeyButtonNormal(keyIndex);
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

    Sprite GetNoteSprite(int noteIndex)
    {
        switch (noteIndex)
        {
            case 0: return noteSpriteA;
            case 1: return noteSpriteS;
            case 2: return noteSpriteD;
            case 3: return noteSpriteF;
            default: return null;
        }
    }

    void CheckInput()
    {
        if (EnergyManager.Instance == null)
        {
            Debug.LogError("EnergyManager не найден!");
            return;
        }

        // ПРОВЕРКА ПРИ ПРОВЕРКЕ ВВОДА: если внимание 0, выходим
        if (EnergyManager.Instance.GetCurrentAttention() <= 0)
        {
            if (messageText != null) messageText.text = "Внимание закончилось!";
            StartCoroutine(ExitDueToNoAttention());
            return;
        }

        for (int i = 0; i < playerInput.Count; i++)
        {
            if (playerInput[i] != currentMelody[i])
            {
                EnergyManager.Instance.DrainAttentionForSequence(currentMelody.Count, true);
                EnergyManager.Instance.DrainEnergyForSequence(currentMelody.Count, true);
                StartCoroutine(HandleWrongInput());
                return;
            }
        }

        if (playerInput.Count == currentMelody.Count)
        {
            EnergyManager.Instance.DrainAttentionForSequence(currentMelody.Count);
            EnergyManager.Instance.DrainEnergyForSequence(currentMelody.Count);
            StartCoroutine(HandleCorrectSequence());
        }
    }

    IEnumerator HandleCorrectSequence()
    {
        isWaitingForInput = false;

        int scoreEarned = 100 * currentLevel;
        currentScore += scoreEarned;
        currentLevel++;

        // ОБНОВЛЯЕМ ДЛИНУ ПОСЛЕДОВАТЕЛЬНОСТИ
        currentSequenceLength = currentMelody.Count;

        // СОХРАНЯЕМ ПРОГРЕСС
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.AddScore(scoreEarned);
            GameProgressManager.Instance.UpdateMaxSequence(currentSequenceLength);
        }

        if (messageText != null) messageText.text = $"Правильно! +{scoreEarned} очков!";
        PlaySound(successSound);

        yield return StartCoroutine(SuccessAnimation());
        yield return new WaitForSeconds(1f);

        // ПРОВЕРКА ПЕРЕД СЛЕДУЮЩИМ УРОВНЕМ: если внимание 0, выходим
        if (EnergyManager.Instance == null || EnergyManager.Instance.GetCurrentAttention() <= 0)
        {
            if (messageText != null) messageText.text = "Внимание закончилось! Нужно отдохнуть";
            yield return new WaitForSeconds(1.5f);
            ReturnToMainScene();
            yield break;
        }

        if (EnergyManager.Instance.HasEnergy())
        {
            GenerateNewMelody();
            StartCoroutine(PlayMelodySequence());
        }
        else
        {
            if (messageText != null)
            {
                messageText.text = "Энергия закончилась! Нужно отдохнуть";
            }
            yield return new WaitForSeconds(1.5f);
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

        // ПРОВЕРКА ПЕРЕД ПОВТОРОМ: если внимание 0, выходим
        if (EnergyManager.Instance != null && EnergyManager.Instance.GetCurrentAttention() <= 0)
        {
            if (messageText != null) messageText.text = "Внимание закончилось! Нужно отдохнуть";
            yield return new WaitForSeconds(1.5f);
            ReturnToMainScene();
            yield break;
        }

        if (EnergyManager.Instance.HasEnergy())
        {
            playerInput.Clear();
            StartCoroutine(PlayMelodySequence());
        }
        else
        {
            if (messageText != null)
            {
                messageText.text = "Энергия закончилась! Нужно отдохнуть";
            }
            yield return new WaitForSeconds(1.5f);
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
            if (pressed)
                SetKeyButtonPressed(i);
            else
                SetKeyButtonNormal(i);
        }
    }

    void SetKeyButtonNormal(int keyIndex)
    {
        Button button = GetKeyButton(keyIndex);
        if (button != null && button.image != null)
        {
            Sprite normalSprite = GetKeyNormalSprite(keyIndex);
            if (normalSprite != null)
            {
                button.image.sprite = normalSprite;
            }
        }
        keyPressedState[keyIndex] = false;
    }

    void SetKeyButtonPressed(int keyIndex)
    {
        Button button = GetKeyButton(keyIndex);
        if (button != null && button.image != null)
        {
            Sprite pressedSprite = GetKeyPressedSprite(keyIndex);
            if (pressedSprite != null)
            {
                button.image.sprite = pressedSprite;
            }
        }
        keyPressedState[keyIndex] = true;
    }

    void SetAllKeysNormal()
    {
        for (int i = 0; i < 4; i++)
        {
            SetKeyButtonNormal(i);
        }
    }

    Sprite GetKeyNormalSprite(int keyIndex)
    {
        switch (keyIndex)
        {
            case 0: return keyNormalA;
            case 1: return keyNormalS;
            case 2: return keyNormalD;
            case 3: return keyNormalF;
            default: return null;
        }
    }

    Sprite GetKeyPressedSprite(int keyIndex)
    {
        switch (keyIndex)
        {
            case 0: return keyPressedA;
            case 1: return keyPressedS;
            case 2: return keyPressedD;
            case 3: return keyPressedF;
            default: return null;
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

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void ReturnToMainScene()
    {
        string mainSceneName = "MainScene"; // Замени на имя твоей основной сцены

        // Показываем панель завершения дня в основной сцене
        if (GameProgressManager.Instance != null)
        {
            // Загружаем основную сцену и показываем панель
            SceneManager.LoadScene(mainSceneName);

            // Используем корутину чтобы показать панель после загрузки сцены
            StartCoroutine(ShowPanelAfterSceneLoad());
        }
    }

    void Update()
    {
        // ПРОВЕРКА В КАЖДОМ КАДРЕ: если внимание стало 0, выходим
        if (EnergyManager.Instance != null && EnergyManager.Instance.GetCurrentAttention() <= 0)
        {
            if (messageText != null) messageText.text = "Внимание закончилось!";
            StartCoroutine(ExitDueToNoAttention());
            return;
        }

        if (isWaitingForInput && !isPlayingSequence)
        {
            foreach (var mapping in keyMappings)
            {
                if (Input.GetKeyDown(mapping.Key))
                {
                    int keyIndex = mapping.Value.index;
                    if (!keyPressedState[keyIndex])
                    {
                        OnKeyPressed(keyIndex);
                    }
                }

                if (Input.GetKeyUp(mapping.Key))
                {
                    int keyIndex = mapping.Value.index;
                    SetKeyButtonNormal(keyIndex);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMainScene();
        }

        UpdateUI();
        CheckEnergyDuringGame();

        // Горячая клавиша для завершения дня
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            ShowEndDayPanelAndExit();
        }
    }

    void CheckEnergyDuringGame()
    {
        if (EnergyManager.Instance != null && !EnergyManager.Instance.HasEnergy())
        {
            Debug.Log("Энергия закончилась во время игры!");
            ShowEndDayPanelAndExit();
        }
    }

    void ShowEndDayPanelAndExit()
    {
        // Сохраняем прогресс перед выходом
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.SaveData();
        }

        // Возвращаемся в основную сцену
        ReturnToMainScene();
    }

    System.Collections.IEnumerator ShowPanelAfterSceneLoad()
    {
        // Ждем завершения загрузки сцены
        yield return new WaitForSeconds(0.5f);

        // Показываем панель завершения дня
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.ShowEndDayPanel();
        }
    }
}