using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MelodyMiniGame : MonoBehaviour
{
    [Header("Game Settings")]
    public float noteDisplayTime = 1f;
    public float betweenNotesTime = 0.3f;
    public float inputTimeLimit = 5f;

    [Header("UI References")]
    public Text sequenceText;
    public Text inputText;
    public Text scoreText;
    public Text messageText;
    public Text levelText;
    public GameObject gamePanel;
    public Button exitButton;

    [Header("Key Buttons")]
    public Button keyButtonA;
    public Button keyButtonS;
    public Button keyButtonD;
    public Button keyButtonF;

    [Header("Note Display")]
    public GameObject noteDisplayPanel;
    public Text[] noteDisplays; // 4 текста для отображения нот над клавишами

    [Header("Audio")]
    public AudioClip[] noteSounds;
    public AudioClip successSound;
    public AudioClip failSound;

    private List<int> currentMelody = new List<int>();
    private List<int> playerInput = new List<int>();
    private int currentScore = 0;
    private int currentLevel = 1;
    private bool isPlayingSequence = false;
    private bool isWaitingForInput = false;

    public static MelodyMiniGame Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Автоматически находим камеру и настраиваем
        SetupCamera();

        // Автоматически находим UI элементы если не назначены
        AutoAssignReferences();

        SetupKeyButtons();
        StartNewGame();
    }

    void SetupCamera()
    {
        // Убираем предупреждение "No cameras rendering"
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObj = new GameObject("MainCamera");
            cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
            Debug.Log("Создана камера автоматически");
        }
    }

    void AutoAssignReferences()
    {
        // Автоматически находим UI элементы по именам
        if (sequenceText == null) sequenceText = GameObject.Find("SequenceText")?.GetComponent<Text>();
        if (inputText == null) inputText = GameObject.Find("InputText")?.GetComponent<Text>();
        if (scoreText == null) scoreText = GameObject.Find("ScoreText")?.GetComponent<Text>();
        if (messageText == null) messageText = GameObject.Find("MessageText")?.GetComponent<Text>();
        if (levelText == null) levelText = GameObject.Find("LevelText")?.GetComponent<Text>();
        if (gamePanel == null) gamePanel = GameObject.Find("GamePanel");
        if (exitButton == null) exitButton = GameObject.Find("ExitButton")?.GetComponent<Button>();

        if (keyButtonA == null) keyButtonA = GameObject.Find("KeyButtonA")?.GetComponent<Button>();
        if (keyButtonS == null) keyButtonS = GameObject.Find("KeyButtonS")?.GetComponent<Button>();
        if (keyButtonD == null) keyButtonD = GameObject.Find("KeyButtonD")?.GetComponent<Button>();
        if (keyButtonF == null) keyButtonF = GameObject.Find("KeyButtonF")?.GetComponent<Button>();

        if (noteDisplayPanel == null) noteDisplayPanel = GameObject.Find("NoteDisplayPanel");

        // Находим note displays
        if (noteDisplays == null || noteDisplays.Length == 0)
        {
            noteDisplays = new Text[4];
            for (int i = 0; i < 4; i++)
            {
                noteDisplays[i] = GameObject.Find($"NoteDisplay{i}")?.GetComponent<Text>();
            }
        }
    }

    void SetupKeyButtons()
    {
        if (keyButtonA != null)
        {
            keyButtonA.onClick.AddListener(() => OnKeyPressed(0));
            keyButtonA.image.color = Color.white;
        }

        if (keyButtonS != null)
        {
            keyButtonS.onClick.AddListener(() => OnKeyPressed(1));
            keyButtonS.image.color = Color.white;
        }

        if (keyButtonD != null)
        {
            keyButtonD.onClick.AddListener(() => OnKeyPressed(2));
            keyButtonD.image.color = Color.white;
        }

        if (keyButtonF != null)
        {
            keyButtonF.onClick.AddListener(() => OnKeyPressed(3));
            keyButtonF.image.color = Color.white;
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ReturnToMainScene);
        }

        // Скрываем ноты в начале
        HideNoteDisplays();
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
        isPlayingSequence = true;
        isWaitingForInput = false;

        if (messageText != null) messageText.text = "Слушайте мелодию...";
        if (sequenceText != null) sequenceText.text = $"Последовательность из {currentMelody.Count} нот";
        if (inputText != null) inputText.text = "Ваш ввод: ";

        HideNoteDisplays();

        // Проигрываем каждую ноту с отображением над клавишами
        for (int i = 0; i < currentMelody.Count; i++)
        {
            int noteIndex = currentMelody[i];
            ShowNoteDisplay(noteIndex);
            PlayNoteSound(noteIndex);

            yield return new WaitForSeconds(noteDisplayTime);
            HideNoteDisplays();
            yield return new WaitForSeconds(betweenNotesTime);
        }

        isPlayingSequence = false;
        isWaitingForInput = true;
        if (messageText != null) messageText.text = "Ваша очередь! Повторите мелодию";
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
            button.image.color = Color.green;
            yield return new WaitForSeconds(0.3f);
            button.image.color = Color.white;
        }
    }

    void ShowNoteDisplay(int noteIndex)
    {
        if (noteDisplays != null && noteIndex >= 0 && noteIndex < noteDisplays.Length)
        {
            if (noteDisplays[noteIndex] != null)
            {
                noteDisplays[noteIndex].text = GetKeyName(noteIndex);
                noteDisplays[noteIndex].color = GetNoteColor(noteIndex);
                noteDisplays[noteIndex].gameObject.SetActive(true);
            }
        }
    }

    void HideNoteDisplays()
    {
        if (noteDisplays != null)
        {
            foreach (Text display in noteDisplays)
            {
                if (display != null) display.gameObject.SetActive(false);
            }
        }
    }

    Color GetNoteColor(int noteIndex)
    {
        switch (noteIndex)
        {
            case 0: return Color.red;
            case 1: return Color.green;
            case 2: return Color.blue;
            case 3: return Color.yellow;
            default: return Color.white;
        }
    }

    void CheckInput()
    {
        for (int i = 0; i < playerInput.Count; i++)
        {
            if (playerInput[i] != currentMelody[i])
            {
                StartCoroutine(HandleWrongInput());
                return;
            }
        }

        if (playerInput.Count == currentMelody.Count)
        {
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

        // Анимация успеха
        yield return StartCoroutine(SuccessAnimation());
        yield return new WaitForSeconds(1f);

        GenerateNewMelody();
        StartCoroutine(PlayMelodySequence());
        UpdateUI();
    }

    IEnumerator HandleWrongInput()
    {
        isWaitingForInput = false;

        if (messageText != null) messageText.text = "Неправильно! Попробуйте еще раз";
        PlaySound(failSound);

        // Анимация ошибки
        for (int i = 0; i < 2; i++)
        {
            HighlightAllKeys(Color.red);
            yield return new WaitForSeconds(0.3f);
            UnhighlightAllKeys();
            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(1f);

        playerInput.Clear();
        StartCoroutine(PlayMelodySequence());
        UpdateUI();
    }

    IEnumerator SuccessAnimation()
    {
        for (int i = 0; i < 3; i++)
        {
            HighlightAllKeys(Color.green);
            yield return new WaitForSeconds(0.2f);
            UnhighlightAllKeys();
            yield return new WaitForSeconds(0.2f);
        }
    }

    void HighlightAllKeys(Color color)
    {
        for (int i = 0; i < 4; i++)
        {
            Button button = GetKeyButton(i);
            if (button != null && button.image != null)
                button.image.color = color;
        }
    }

    void UnhighlightAllKeys()
    {
        for (int i = 0; i < 4; i++)
        {
            Button button = GetKeyButton(i);
            if (button != null && button.image != null)
                button.image.color = Color.white;
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

    void PlayNoteSound(int noteIndex)
    {
        if (noteSounds != null && noteIndex >= 0 && noteIndex < noteSounds.Length && noteSounds[noteIndex] != null)
        {
            AudioSource.PlayClipAtPoint(noteSounds[noteIndex], Camera.main.transform.position, 0.5f);
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, 0.7f);
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
        // Проверяем что сцена есть в Build Settings
        if (Application.CanStreamedLevelBeLoaded("MainScene"))
        {
            if (EnergyManager.Instance != null)
            {
                EnergyManager.Instance.StopDrainingEnergy();
            }
            SceneManager.LoadScene("MainScene");
        }
        else
        {
            Debug.LogError("MainScene не может быть загружена! Проверьте Build Settings.");
            // Создаем сообщение об ошибке
            CreateErrorMessage("MainScene не найдена в Build Settings!\nДобавьте сцену через File->Build Settings");
        }
    }

    void CreateErrorMessage(string message)
    {
        GameObject errorObj = new GameObject("ErrorMessage");
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        errorObj.transform.SetParent(canvas.transform);
        RectTransform rt = errorObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(500, 100);

        UnityEngine.UI.Text text = errorObj.AddComponent<UnityEngine.UI.Text>();
        text.text = message;
        text.color = Color.red;
        text.fontSize = 18;
        text.alignment = TextAnchor.MiddleCenter;
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
    }
}