using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class GameProgressManager : MonoBehaviour
{
    [Header("Day System")]
    public int currentDay = 1;
    public int totalDays = 3;
    public int targetTotalScore = 5000;
    public int currentDayScore = 0;
    public int totalScore = 0;
    public int maxSequenceLength = 0;

    [Header("UI References")]
    public Text dayText;
    public Text dayScoreText;
    public Text totalScoreText;
    public Text maxSequenceText;
    public GameObject endDayPanel;
    public Button endDayButton;
    public Text endDayMessage;
    public Button confirmEndDayButton;
    public Button cancelEndDayButton;

    [Header("Ending Screen")]
    public GameObject endingScreen;
    public Image endingBackground;
    public Text endingTitle;
    public Text endingDescription;
    public Button exitGameButton;
    public Sprite goodEndingSprite;
    public Sprite badEndingSprite;

    [Header("Energy Reference")]
    public EnergyManager energyManager;

    [Header("Respawn Settings")]
    public Transform playerRespawnPoint;
    public string playerTag = "Player";

    [Header("Energy Panel References")]
    public Slider energyProgressBar;
    public Slider attentionProgressBar;
    public Text energyText;
    public Text attentionText;

    // СИНГЛТОН ДЛЯ СОХРАНЕНИЯ ДАННЫХ
    private static GameProgressManager _instance;

    // СТАТИЧЕСКИЕ ПЕРЕМЕННЫЕ ДЛЯ СОХРАНЕНИЯ ДАННЫХ МЕЖДУ СЦЕНАМИ
    private static int _savedCurrentDay = 1;
    private static int _savedCurrentDayScore = 0;
    private static int _savedTotalScore = 0;
    private static int _savedMaxSequenceLength = 0;

    // ДОБАВЛЯЕМ ФЛАГ ДЛЯ ОТСЛЕЖИВАНИЯ ИНИЦИАЛИЗАЦИИ
    private bool isUIInitialized = false;

    // ДОБАВЛЯЕМ ДЛЯ РЕЗЕРВНЫХ ПАНЕЛЕЙ
    private GameObject createdEndDayPanel;
    private GameObject createdEndingScreen;

    public static GameProgressManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameProgressManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("GameProgressManager");
                    _instance = obj.AddComponent<GameProgressManager>();
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            // ВОССТАНАВЛИВАЕМ СОХРАНЕННЫЕ ДАННЫЕ
            LoadSavedData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadSavedData()
    {
        currentDay = _savedCurrentDay;
        currentDayScore = _savedCurrentDayScore;
        totalScore = _savedTotalScore;
        maxSequenceLength = _savedMaxSequenceLength;

        Debug.Log($"Данные загружены: День {currentDay}, Очки: {currentDayScore}, Всего: {totalScore}");
    }

    public void SaveData()
    {
        _savedCurrentDay = currentDay;
        _savedCurrentDayScore = currentDayScore;
        _savedTotalScore = totalScore;
        _savedMaxSequenceLength = maxSequenceLength;

        Debug.Log($"Данные сохранены: День {currentDay}, Очки: {currentDayScore}, Всего: {totalScore}");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Сцена загружена: {scene.name}");

        // Сбрасываем флаг инициализации
        isUIInitialized = false;

        // Находим точку респавна
        FindRespawnPoint();

        // Ждем немного перед поиском UI элементов
        StartCoroutine(InitializeUIAfterDelay());
    }

    void FindRespawnPoint()
    {
        if (playerRespawnPoint == null)
        {
            GameObject respawnObj = GameObject.FindGameObjectWithTag("Respawn");
            if (respawnObj != null)
            {
                playerRespawnPoint = respawnObj.transform;
                Debug.Log("Точка респавна найдена: " + respawnObj.name);
            }
            else
            {
                // Создаем точку респавна по умолчанию
                GameObject defaultRespawn = new GameObject("DefaultRespawnPoint");
                defaultRespawn.tag = "Respawn";
                defaultRespawn.transform.position = Vector3.zero;
                playerRespawnPoint = defaultRespawn.transform;
                Debug.Log("Создана точка респавна по умолчанию");
            }
        }
    }

    IEnumerator InitializeUIAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);

        // ПОВТОРНАЯ ИНИЦИАЛИЗАЦИЯ UI
        FindUIReferences();
        SetupButtonListeners();
        UpdateUI();

        // Скрываем панели при загрузке сцены
        if (endDayPanel != null)
            endDayPanel.SetActive(false);

        if (endingScreen != null)
            endingScreen.SetActive(false);

        // Скрываем созданные резервные панели
        if (createdEndDayPanel != null)
            createdEndDayPanel.SetActive(false);
        if (createdEndingScreen != null)
            createdEndingScreen.SetActive(false);

        isUIInitialized = true;

        Debug.Log("UI инициализирован после загрузки сцены");
    }

    void Start()
    {
        FindUIReferences();
        SetupButtonListeners();
        UpdateUI();

        if (endDayPanel != null)
            endDayPanel.SetActive(false);

        if (endingScreen != null)
            endingScreen.SetActive(false);

        isUIInitialized = true;
    }

    void FindUIReferences()
    {
        Debug.Log("Поиск UI элементов...");

        // Ищем основные UI элементы
        if (endDayPanel == null)
        {
            endDayPanel = GameObject.Find("EndDayPanel");
            if (endDayPanel != null) Debug.Log("EndDayPanel найден");
        }

        if (endDayButton == null)
        {
            GameObject buttonObj = GameObject.Find("EndDayButton");
            if (buttonObj != null)
            {
                endDayButton = buttonObj.GetComponent<Button>();
                Debug.Log("EndDayButton найден");
            }
        }

        if (confirmEndDayButton == null)
        {
            GameObject confirmObj = GameObject.Find("ConfirmButton");
            if (confirmObj != null)
            {
                confirmEndDayButton = confirmObj.GetComponent<Button>();
                Debug.Log("ConfirmButton найден");
            }
        }

        if (cancelEndDayButton == null)
        {
            GameObject cancelObj = GameObject.Find("CancelButton");
            if (cancelObj != null)
            {
                cancelEndDayButton = cancelObj.GetComponent<Button>();
                Debug.Log("CancelButton найден");
            }
        }

        if (endDayMessage == null)
        {
            GameObject messageObj = GameObject.Find("EndDayMessage");
            if (messageObj != null)
            {
                endDayMessage = messageObj.GetComponent<Text>();
                Debug.Log("EndDayMessage найден");
            }
        }

        // Ищем элементы экрана концовки - БОЛЕЕ ТЩАТЕЛЬНЫЙ ПОИСК
        if (endingScreen == null)
        {
            endingScreen = GameObject.Find("EndingScreen");
            if (endingScreen == null)
            {
                // Ищем среди всех канвасов
                Canvas[] canvases = FindObjectsOfType<Canvas>(true);
                foreach (Canvas canvas in canvases)
                {
                    Transform screen = canvas.transform.Find("EndingScreen");
                    if (screen != null)
                    {
                        endingScreen = screen.gameObject;
                        break;
                    }
                }
            }
            if (endingScreen != null)
            {
                Debug.Log("EndingScreen найден");

                // Находим дочерние элементы
                if (endingBackground == null)
                    endingBackground = endingScreen.GetComponentInChildren<Image>();

                if (endingTitle == null)
                    endingTitle = endingScreen.GetComponentInChildren<Text>();

                if (endingDescription == null)
                {
                    Text[] texts = endingScreen.GetComponentsInChildren<Text>();
                    foreach (Text text in texts)
                    {
                        if (text != endingTitle)
                        {
                            endingDescription = text;
                            break;
                        }
                    }
                }

                if (exitGameButton == null)
                    exitGameButton = endingScreen.GetComponentInChildren<Button>();
            }
        }

        // Ищем текстовые элементы прогресса
        if (dayText == null)
        {
            GameObject dayTextObj = GameObject.Find("DayText");
            if (dayTextObj != null) dayText = dayTextObj.GetComponent<Text>();
        }

        if (dayScoreText == null)
        {
            GameObject dayScoreObj = GameObject.Find("DayScoreText");
            if (dayScoreObj != null) dayScoreText = dayScoreObj.GetComponent<Text>();
        }

        if (totalScoreText == null)
        {
            GameObject totalScoreObj = GameObject.Find("TotalScoreText");
            if (totalScoreObj != null) totalScoreText = totalScoreObj.GetComponent<Text>();
        }

        if (maxSequenceText == null)
        {
            GameObject maxSeqObj = GameObject.Find("MaxSequenceText");
            if (maxSeqObj != null) maxSequenceText = maxSeqObj.GetComponent<Text>();
        }

        // Находим EnergyManager
        if (energyManager == null)
            energyManager = FindObjectOfType<EnergyManager>();

        Debug.Log($"Найдены элементы: EndDayPanel={endDayPanel != null}, EndingScreen={endingScreen != null}");
    }
    void SetupButtonListeners()
    {
        // Очищаем старые обработчики и устанавливаем новые
        if (endDayButton != null)
        {
            endDayButton.onClick.RemoveAllListeners();
            endDayButton.onClick.AddListener(ShowEndDayPanel);
            Debug.Log("EndDayButton listener установлен");
        }

        if (confirmEndDayButton != null)
        {
            confirmEndDayButton.onClick.RemoveAllListeners();
            confirmEndDayButton.onClick.AddListener(CompleteDay);
            Debug.Log("ConfirmButton listener установлен");
        }

        if (cancelEndDayButton != null)
        {
            cancelEndDayButton.onClick.RemoveAllListeners();
            cancelEndDayButton.onClick.AddListener(CancelEndDay);
            Debug.Log("CancelButton listener установлен");
        }

        if (exitGameButton != null)
        {
            exitGameButton.onClick.RemoveAllListeners();
            exitGameButton.onClick.AddListener(ExitGame);
            Debug.Log("ExitGameButton listener установлен");
        }
    }

    public void AddScore(int score)
    {
        currentDayScore += score;
        totalScore += score;
        SaveData();
        UpdateUI();
        Debug.Log($"Добавлены очки: {score}. Сегодня: {currentDayScore}, Всего: {totalScore}");
    }

    public void UpdateMaxSequence(int sequenceLength)
    {
        if (sequenceLength > maxSequenceLength)
        {
            maxSequenceLength = sequenceLength;
            SaveData();
            UpdateUI();
            Debug.Log($"Новая максимальная последовательность: {maxSequenceLength}");
        }
    }

    void UpdateUI()
    {
        if (dayText != null) dayText.text = $"День: {currentDay}/{totalDays}";
        if (dayScoreText != null) dayScoreText.text = $"Очки сегодня: {currentDayScore}";
        if (totalScoreText != null) totalScoreText.text = $"Всего очков: {totalScore}/{targetTotalScore}";
        if (maxSequenceText != null) maxSequenceText.text = $"Макс. последовательность: {maxSequenceLength}";
    }

    public void ShowEndDayPanel()
    {
        Debug.Log("ShowEndDayPanel вызван");

        if (!isUIInitialized || endDayPanel == null)
        {
            Debug.Log("UI не инициализирован, переискиваем элементы...");
            FindUIReferences();
            SetupButtonListeners();
        }

        // Если панель все еще не найдена, создаем резервную
        if (endDayPanel == null && createdEndDayPanel == null)
        {
            Debug.Log("EndDayPanel не найден, создаем резервную панель...");
            CreateFallbackEndDayPanel();
        }

        GameObject panelToShow = endDayPanel != null ? endDayPanel : createdEndDayPanel;

        if (panelToShow != null)
        {
            if (endDayMessage != null)
            {
                string message = $"День {currentDay} завершен!\n\n";
                message += $"Очков сегодня: {currentDayScore}\n";
                message += $"Всего очков: {totalScore}/{targetTotalScore}\n";
                message += $"Максимальная последовательность: {maxSequenceLength}\n\n";

                if (currentDay < totalDays)
                {
                    message += $"Завершить день {currentDay} и перейти к следующему?";
                }
                else
                {
                    message += $"Это последний день! Завершить игру?";
                }

                endDayMessage.text = message;
            }

            panelToShow.SetActive(true);
            Debug.Log("Панель завершения дня активирована");

            UnlockCursor();

            if (PenguinController.Instance != null)
            {
                PenguinController.Instance.FixMovement(true);
                PenguinController.Instance.FixCamera(true);
            }
        }
        else
        {
            Debug.LogError("EndDayPanel не найден после поиска и создания!");
        }
    }

    public void CompleteDay()
    {
        Debug.Log("CompleteDay вызван");

        // Скрываем обе панели (оригинальную и резервную)
        if (endDayPanel != null)
            endDayPanel.SetActive(false);
        if (createdEndDayPanel != null)
            createdEndDayPanel.SetActive(false);

        LockCursor();
        if (PenguinController.Instance != null)
        {
            PenguinController.Instance.FixMovement(false);
            PenguinController.Instance.FixCamera(false);
        }

        int completedDay = currentDay;
        currentDay++;

        if (currentDay > totalDays)
        {
            Debug.Log("ПОСЛЕДНИЙ ДЕНЬ ЗАВЕРШЕН! Показываем концовку...");
            ShowEndingScreen();
        }
        else
        {
            currentDayScore = 0;
            maxSequenceLength = 0;

            SaveData();
            UpdateUI();
            Debug.Log($"День {completedDay} завершен. Переход к дню {currentDay}. Общий счет сохранен: {totalScore}");

            if (energyManager != null)
            {
                energyManager.ResetAllResources();
                Debug.Log("Энергия восстановлена для нового дня");
            }

            RespawnPlayer();
        }
    }

    void RespawnPlayer()
    {
        Debug.Log("Респавн игрока...");

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null && playerRespawnPoint != null)
        {
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            player.transform.position = playerRespawnPoint.position;
            player.transform.rotation = playerRespawnPoint.rotation;

            if (controller != null)
            {
                controller.enabled = true;
            }

            Debug.Log($"Игрок перемещен к точке респавна: {playerRespawnPoint.position}");
        }
        else
        {
            Debug.LogWarning("Игрок или точка респавна не найдены!");
        }
    }

    void ShowEndingScreen()
    {
        Debug.Log("Показ экрана концовки...");

        bool goodEnding = totalScore >= targetTotalScore;

        GameObject screenToShow = endingScreen != null ? endingScreen : createdEndingScreen;

        if (screenToShow != null)
        {
            SetupEndingScreenContent(goodEnding);
            screenToShow.SetActive(true);
            UnlockCursor();

            if (PenguinController.Instance != null)
            {
                PenguinController.Instance.FixMovement(true);
                PenguinController.Instance.FixCamera(true);
            }

            Debug.Log($"Экран концовки показан: {(goodEnding ? "Хорошая" : "Плохая")} концовка");
        }
        else
        {
            Debug.LogError("EndingScreen не найден! Создаем резервный...");
            CreateFallbackEndingScreen(goodEnding);
        }
    }
    void CreateFallbackEndDayPanel()
    {
        Debug.Log("Создаем резервную панель завершения дня...");

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            CreateMainCanvas();
            canvas = FindObjectOfType<Canvas>();
        }

        // Создаем панель
        GameObject panel = new GameObject("EndDayPanel_Fallback");
        createdEndDayPanel = panel;
        panel.transform.SetParent(canvas.transform);

        RectTransform panelRt = panel.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.anchoredPosition = Vector2.zero;
        panelRt.sizeDelta = new Vector2(839.088f, 476.417f);

        // Фон панели
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(1, 1, 1, 0.392f);
        panelImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");

        // Заголовок
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform);
        RectTransform titleRt = titleObj.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0, -44f);
        titleRt.sizeDelta = new Vector2(260.366f, 78.529f);

        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "Завершение дня";
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 30;
        titleText.color = Color.black;
        titleText.alignment = TextAnchor.MiddleCenter;

        // Сообщение
        GameObject messageObj = new GameObject("EndDayMessage");
        messageObj.transform.SetParent(panel.transform);
        RectTransform messageRt = messageObj.AddComponent<RectTransform>();
        messageRt.anchorMin = new Vector2(0.5f, 0.5f);
        messageRt.anchorMax = new Vector2(0.5f, 0.5f);
        messageRt.anchoredPosition = new Vector2(0.000035f, 15.9f);
        messageRt.sizeDelta = new Vector2(310.475f, 88.084f);

        endDayMessage = messageObj.AddComponent<Text>();
        endDayMessage.text = $"День {currentDay} завершен!\n\nОчков сегодня: {currentDayScore}\nВсего очков: {totalScore}/{targetTotalScore}";
        endDayMessage.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        endDayMessage.fontSize = 24;
        endDayMessage.color = Color.black;
        endDayMessage.alignment = TextAnchor.MiddleCenter;

        // Кнопка подтверждения
        GameObject confirmObj = new GameObject("ConfirmButton");
        confirmObj.transform.SetParent(panel.transform);
        RectTransform confirmRt = confirmObj.AddComponent<RectTransform>();
        confirmRt.anchorMin = new Vector2(0, 0);
        confirmRt.anchorMax = new Vector2(0, 0);
        confirmRt.anchoredPosition = new Vector2(115.806f, 40.182f);
        confirmRt.sizeDelta = new Vector2(231.611f, 80.364f);

        Image confirmImage = confirmObj.AddComponent<Image>();
        confirmImage.color = Color.white;
        confirmImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");

        confirmEndDayButton = confirmObj.AddComponent<Button>();

        // Текст кнопки подтверждения
        GameObject confirmTextObj = new GameObject("Text");
        confirmTextObj.transform.SetParent(confirmObj.transform);
        RectTransform confirmTextRt = confirmTextObj.AddComponent<RectTransform>();
        confirmTextRt.anchorMin = Vector2.zero;
        confirmTextRt.anchorMax = Vector2.one;
        confirmTextRt.sizeDelta = Vector2.zero;

        Text confirmText = confirmTextObj.AddComponent<Text>();
        confirmText.text = "Подтвердить";
        confirmText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        confirmText.fontSize = 20;
        confirmText.color = Color.black;
        confirmText.alignment = TextAnchor.MiddleCenter;

        // Кнопка отмены
        GameObject cancelObj = new GameObject("CancelButton");
        cancelObj.transform.SetParent(panel.transform);
        RectTransform cancelRt = cancelObj.AddComponent<RectTransform>();
        cancelRt.anchorMin = new Vector2(1, 0);
        cancelRt.anchorMax = new Vector2(1, 0);
        cancelRt.anchoredPosition = new Vector2(-111.223f, 40.182f);
        cancelRt.sizeDelta = new Vector2(222.446f, 80.364f);

        Image cancelImage = cancelObj.AddComponent<Image>();
        cancelImage.color = Color.white;
        cancelImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");

        cancelEndDayButton = cancelObj.AddComponent<Button>();

        // Текст кнопки отмены
        GameObject cancelTextObj = new GameObject("Text");
        cancelTextObj.transform.SetParent(cancelObj.transform);
        RectTransform cancelTextRt = cancelTextObj.AddComponent<RectTransform>();
        cancelTextRt.anchorMin = Vector2.zero;
        cancelTextRt.anchorMax = Vector2.one;
        cancelTextRt.sizeDelta = Vector2.zero;

        Text cancelText = cancelTextObj.AddComponent<Text>();
        cancelText.text = "Отмена";
        cancelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        cancelText.fontSize = 20;
        cancelText.color = Color.black;
        cancelText.alignment = TextAnchor.MiddleCenter;

        // Настраиваем слушатели
        SetupButtonListeners();

        Debug.Log("Резервная панель завершения дня создана");
    }

    void CreateMainCanvas()
    {
        GameObject canvasObj = new GameObject("MainCanvas_Fallback");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800, 600);

        canvasObj.AddComponent<GraphicRaycaster>();

        Debug.Log("Создан резервный канвас");
    }

    void SetupEndingScreenContent(bool goodEnding)
    {
        // Настраиваем фон (спрайт)
        if (endingBackground != null)
        {
            if (goodEnding && goodEndingSprite != null)
            {
                endingBackground.sprite = goodEndingSprite;
                endingBackground.color = Color.white;
            }
            else if (!goodEnding && badEndingSprite != null)
            {
                endingBackground.sprite = badEndingSprite;
                endingBackground.color = Color.white;
            }
            else
            {
                endingBackground.color = goodEnding ? new Color(0.1f, 0.5f, 0.1f, 0.95f) : new Color(0.5f, 0.1f, 0.1f, 0.95f);
            }
        }

        // Настраиваем текст
        if (endingTitle != null)
        {
            endingTitle.text = goodEnding ? "🎉 Поздравляем! 🎉" : "😔 Попробуйте еще раз";
            endingTitle.color = goodEnding ? Color.yellow : Color.white;
        }

        if (endingDescription != null)
        {
            string description = goodEnding ?
                "Вы успешно сдали экзамен по игре на пианино!\nВаше усердие и практика принесли отличные результаты.\n\n" :
                "Вам нужно больше практики...\nНе сдавайтесь, продолжайте тренироваться!\n\n";

            description += $"Итоговый счет: {totalScore}/{targetTotalScore}\n";
            description += $"Максимальная последовательность: {maxSequenceLength}\n";
            description += $"Прогресс: {currentDay - 1} из {totalDays} дней\n\n";

            if (goodEnding)
            {
                description += "Вы доказали, что упорство и практика ведут к успеху!";
            }
            else
            {
                description += "Каждая неудача - это шаг к будущему успеху!";
            }

            endingDescription.text = description;
        }
    }

    void CreateFallbackEndingScreen(bool goodEnding)
    {
        Debug.Log("Создаем резервный экран концовки...");

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            CreateMainCanvas();
            canvas = FindObjectOfType<Canvas>();
        }

        // Создаем основную панель концовки (EndingPanel)
        GameObject endingPanel = new GameObject("EndingPanel_Fallback");
        createdEndingScreen = endingPanel;
        endingPanel.transform.SetParent(canvas.transform);
        RectTransform panelRt = endingPanel.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        // Фон панели как в префабе
        Image panelImage = endingPanel.AddComponent<Image>();
        panelImage.color = new Color(1, 1, 1, 0.3764706f);
        panelImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");

        // Создаем EndingScreen как дочерний объект
        GameObject endingScreenObj = new GameObject("EndingScreen");
        endingScreenObj.transform.SetParent(endingPanel.transform);
        RectTransform screenRt = endingScreenObj.AddComponent<RectTransform>();
        screenRt.anchorMin = Vector2.zero;
        screenRt.anchorMax = Vector2.one;
        screenRt.offsetMin = Vector2.zero;
        screenRt.offsetMax = Vector2.zero;

        // Фон EndingScreen
        Image screenImage = endingScreenObj.AddComponent<Image>();
        if (goodEnding && goodEndingSprite != null)
        {
            screenImage.sprite = goodEndingSprite;
            screenImage.color = Color.white;
            screenImage.type = Image.Type.Simple;
            screenImage.preserveAspect = true;
        }
        else if (!goodEnding && badEndingSprite != null)
        {
            screenImage.sprite = badEndingSprite;
            screenImage.color = Color.white;
            screenImage.type = Image.Type.Simple;
            screenImage.preserveAspect = true;
        }
        else
        {
            screenImage.color = goodEnding ? new Color(0.1f, 0.5f, 0.1f, 0.95f) : new Color(0.5f, 0.1f, 0.1f, 0.95f);
        }

        endingBackground = screenImage;

        // Заголовок
        GameObject titleObj = new GameObject("EndingTitle");
        titleObj.transform.SetParent(endingScreenObj.transform);
        RectTransform titleRt = titleObj.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 0.5f);
        titleRt.anchorMax = new Vector2(0.5f, 0.5f);
        titleRt.anchoredPosition = new Vector2(0, 0);
        titleRt.sizeDelta = new Vector2(503.7708f, 232.2234f);

        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = goodEnding ? "🎉 Поздравляем! 🎉" : "😔 Попробуйте еще раз";
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 40;
        titleText.color = goodEnding ? Color.yellow : Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        endingTitle = titleText;

        // Описание
        GameObject descObj = new GameObject("EndingDescription");
        descObj.transform.SetParent(endingScreenObj.transform);
        RectTransform descRt = descObj.AddComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0.5f, 0.5f);
        descRt.anchorMax = new Vector2(0.5f, 0.5f);
        descRt.anchoredPosition = new Vector2(0, -171.5f);
        descRt.sizeDelta = new Vector2(854.0159f, 342.9971f);

        Text descText = descObj.AddComponent<Text>();
        string description = goodEnding ?
            "Вы успешно сдали экзамен по игре на пианино!\nВаше усердие и практика принесли отличные результаты.\n\n" :
            "Вам нужно больше практики...\nНе сдавайтесь, продолжайте тренироваться!\n\n";

        description += $"Итоговый счет: {totalScore}/{targetTotalScore}\n";
        description += $"Максимальная последовательность: {maxSequenceLength}\n";
        description += $"Прогресс: {currentDay - 1} из {totalDays} дней\n\n";

        if (goodEnding)
        {
            description += "Вы доказали, что упорство и практика ведут к успеху!";
        }
        else
        {
            description += "Каждая неудача - это шаг к будущему успеху!";
        }

        descText.text = description;
        descText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        descText.fontSize = 24;
        descText.color = Color.white;
        descText.alignment = TextAnchor.MiddleCenter;
        endingDescription = descText;

        // Кнопка выхода
        GameObject exitObj = new GameObject("ExitGameButton");
        exitObj.transform.SetParent(endingScreenObj.transform);
        RectTransform exitRt = exitObj.AddComponent<RectTransform>();
        exitRt.anchorMin = new Vector2(0.5f, 0.5f);
        exitRt.anchorMax = new Vector2(0.5f, 0.5f);
        exitRt.anchoredPosition = new Vector2(0, -533f);
        exitRt.sizeDelta = new Vector2(351.5979f, 85.4526f);

        Image exitImage = exitObj.AddComponent<Image>();
        exitImage.color = Color.white;
        exitImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        exitGameButton = exitObj.AddComponent<Button>();

        // Текст кнопки
        GameObject exitTextObj = new GameObject("ButtonText");
        exitTextObj.transform.SetParent(exitObj.transform);
        RectTransform exitTextRt = exitTextObj.AddComponent<RectTransform>();
        exitTextRt.anchorMin = Vector2.zero;
        exitTextRt.anchorMax = Vector2.one;
        exitTextRt.offsetMin = Vector2.zero;
        exitTextRt.offsetMax = Vector2.zero;

        Text exitText = exitTextObj.AddComponent<Text>();
        exitText.text = "Выйти из игры";
        exitText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        exitText.fontSize = 20;
        exitText.color = Color.black;
        exitText.alignment = TextAnchor.MiddleCenter;

        // Сохраняем ссылки
        endingScreen = endingScreenObj;

        // Настраиваем слушатели
        if (exitGameButton != null)
        {
            exitGameButton.onClick.RemoveAllListeners();
            exitGameButton.onClick.AddListener(ExitGame);
        }

        Debug.Log("Резервный экран концовки создан с полной структурой EndingPanel");

        // Показываем экран
        endingPanel.SetActive(true);
        UnlockCursor();

        if (PenguinController.Instance != null)
        {
            PenguinController.Instance.FixMovement(true);
            PenguinController.Instance.FixCamera(true);
        }
    }

    void ExitGame()
    {
        Debug.Log("Выход из игры...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void CancelEndDay()
    {
        Debug.Log("CancelEndDay вызван");

        // Скрываем обе панели
        if (endDayPanel != null)
            endDayPanel.SetActive(false);
        if (createdEndDayPanel != null)
            createdEndDayPanel.SetActive(false);

        LockCursor();
        if (PenguinController.Instance != null)
        {
            PenguinController.Instance.FixMovement(false);
            PenguinController.Instance.FixCamera(false);
        }
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public bool IsUIOpen()
    {
        return (endDayPanel != null && endDayPanel.activeInHierarchy) ||
               (createdEndDayPanel != null && createdEndDayPanel.activeInHierarchy) ||
               (endingScreen != null && endingScreen.activeInHierarchy) ||
               (createdEndingScreen != null && createdEndingScreen.activeInHierarchy);
    }

    public void RefreshAfterMinigame()
    {
        Debug.Log("Принудительное обновление после мини-игры");
        isUIInitialized = false;
        FindUIReferences();
        SetupButtonListeners();
        UpdateUI();
        isUIInitialized = true;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    GameObject CreatePanel(string name, Vector2 sizeDelta)
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            CreateMainCanvas();
            canvas = FindObjectOfType<Canvas>();
        }

        GameObject panel = new GameObject(name);
        panel.transform.SetParent(canvas.transform);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.sizeDelta = sizeDelta;

        Image image = panel.AddComponent<Image>();
        image.color = new Color(1, 1, 1, 0.392f);

        return panel;
    }

    GameObject CreateEnergyPanel()
    {
        GameObject panel = CreatePanel("EnergyPanel_Fallback", new Vector2(404.998f, 178.1622f));
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-328.93f, -224.44f);

        // Добавляем фон как в префабе
        Image bg = panel.GetComponent<Image>();
        bg.color = new Color(1, 1, 1, 0.392f);
        bg.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");

        // Создаем прогресс-бары, связанные с EnergyManager
        CreateEnergyProgressBars(panel);

        return panel;
    }

    void CreateEnergyProgressBars(GameObject parent)
    {
        // Energy Progress Bar
        GameObject energyBarObj = new GameObject("EnergyBar");
        energyBarObj.transform.SetParent(parent.transform);

        RectTransform energyRt = energyBarObj.AddComponent<RectTransform>();
        energyRt.anchorMin = new Vector2(0.5f, 0.5f);
        energyRt.anchorMax = new Vector2(0.5f, 0.5f);
        energyRt.anchoredPosition = new Vector2(0.002f, 23.766f);
        energyRt.sizeDelta = new Vector2(297.616f, 58.532f);

        Slider energySlider = energyBarObj.AddComponent<Slider>();
        energyProgressBar = energySlider;

        // Настраиваем слайдер для работы с EnergyManager
        energySlider.minValue = 0;
        energySlider.maxValue = EnergyManager.Instance != null ? EnergyManager.Instance.maxEnergy : 300f;
        energySlider.value = EnergyManager.Instance != null ? EnergyManager.Instance.GetCurrentEnergy() : 300f;
        energySlider.interactable = false;

        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(energyBarObj.transform);
        RectTransform bgRt = bgObj.AddComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, 0.25f);
        bgRt.anchorMax = new Vector2(1, 0.75f);
        bgRt.sizeDelta = Vector2.zero;

        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.324f, 0.991f, 0.023f, 1f);
        bgImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");

        // Fill Area
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(energyBarObj.transform);
        RectTransform fillAreaRt = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0, 0.25f);
        fillAreaRt.anchorMax = new Vector2(1, 0.75f);
        fillAreaRt.anchoredPosition = new Vector2(-5, 0);
        fillAreaRt.sizeDelta = new Vector2(-20, 0);

        // Fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform);
        RectTransform fillRt = fillObj.AddComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0, 0);
        fillRt.anchorMax = new Vector2(0, 0);
        fillRt.sizeDelta = new Vector2(10, 0);

        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = new Color(0.141f, 1, 0, 1f);
        fillImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");

        // Настраиваем слайдер
        energySlider.fillRect = fillRt;

        // Attention Progress Bar
        CreateAttentionProgressBar(parent);

        // Текстовые элементы
        CreateEnergyTextElements(parent);

        // Регистрируем прогресс-бары в EnergyManager
        if (EnergyManager.Instance != null)
        {
            EnergyManager.Instance.energyProgressBar = energySlider;
            // Attention bar будет зарегистрирован в CreateAttentionProgressBar
        }
    }

    void CreateAttentionProgressBar(GameObject parent)
    {
        // Attention Progress Bar
        GameObject attentionBarObj = new GameObject("AttentionBar");
        attentionBarObj.transform.SetParent(parent.transform);

        RectTransform attentionRt = attentionBarObj.AddComponent<RectTransform>();
        attentionRt.anchorMin = new Vector2(0.5f, 0.5f);
        attentionRt.anchorMax = new Vector2(0.5f, 0.5f);
        attentionRt.anchoredPosition = new Vector2(0, -45.5f);
        attentionRt.sizeDelta = new Vector2(297.62f, 53.0276f);

        Slider attentionSlider = attentionBarObj.AddComponent<Slider>();
        attentionProgressBar = attentionSlider;

        // Настраиваем слайдер для работы с EnergyManager
        attentionSlider.minValue = 0;
        attentionSlider.maxValue = EnergyManager.Instance != null ? EnergyManager.Instance.maxAttention : 100f;
        attentionSlider.value = EnergyManager.Instance != null ? EnergyManager.Instance.GetCurrentAttention() : 100f;
        attentionSlider.interactable = false;

        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(attentionBarObj.transform);
        RectTransform bgRt = bgObj.AddComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, 0.25f);
        bgRt.anchorMax = new Vector2(1, 0.75f);
        bgRt.sizeDelta = Vector2.zero;

        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.929f, 0, 1, 1f);
        bgImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");

        // Fill Area
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(attentionBarObj.transform);
        RectTransform fillAreaRt = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0, 0.25f);
        fillAreaRt.anchorMax = new Vector2(1, 0.75f);
        fillAreaRt.anchoredPosition = new Vector2(-5, 0);
        fillAreaRt.sizeDelta = new Vector2(-20, 0);

        // Fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform);
        RectTransform fillRt = fillObj.AddComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0, 0);
        fillRt.anchorMax = new Vector2(0, 0);
        fillRt.sizeDelta = new Vector2(10, 0);

        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = new Color(0.716f, 0, 1, 1f);
        fillImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");

        // Настраиваем слайдер
        attentionSlider.fillRect = fillRt;

        // Регистрируем в EnergyManager
        if (EnergyManager.Instance != null)
        {
            EnergyManager.Instance.attentionProgressBar = attentionSlider;
        }
    }

    void CreateEnergyTextElements(GameObject parent)
    {
        // Energy Text
        GameObject energyTextObj = new GameObject("EnergyText");
        energyTextObj.transform.SetParent(parent.transform);

        RectTransform textRt = energyTextObj.AddComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0.5f, 0.5f);
        textRt.anchorMax = new Vector2(0.5f, 0.5f);
        textRt.anchoredPosition = new Vector2(0, 52.8f);
        textRt.sizeDelta = new Vector2(198.05f, 42.408f);

        Text energyTextComponent = energyTextObj.AddComponent<Text>();
        energyTextComponent.text = EnergyManager.Instance != null ?
            $"Энергия: {Mathf.RoundToInt(EnergyManager.Instance.GetCurrentEnergy())}/{Mathf.RoundToInt(EnergyManager.Instance.maxEnergy)}" :
            "Энергия: 300/300";
        energyTextComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        energyTextComponent.fontSize = 24;
        energyTextComponent.color = Color.black;
        energyTextComponent.alignment = TextAnchor.MiddleCenter;
        energyText = energyTextComponent;

        // Attention Text
        GameObject attentionTextObj = new GameObject("AttentionText");
        attentionTextObj.transform.SetParent(parent.transform);

        RectTransform attentionTextRt = attentionTextObj.AddComponent<RectTransform>();
        attentionTextRt.anchorMin = new Vector2(0.5f, 0.5f);
        attentionTextRt.anchorMax = new Vector2(0.5f, 0.5f);
        attentionTextRt.anchoredPosition = new Vector2(0.002f, -13.2f);
        attentionTextRt.sizeDelta = new Vector2(185.292f, 35.42f);

        Text attentionTextComponent = attentionTextObj.AddComponent<Text>();
        attentionTextComponent.text = EnergyManager.Instance != null ?
            $"Внимание: {Mathf.RoundToInt(EnergyManager.Instance.GetCurrentAttention())}/{Mathf.RoundToInt(EnergyManager.Instance.maxAttention)}" :
            "Внимание: 100/100";
        attentionTextComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        attentionTextComponent.fontSize = 24;
        attentionTextComponent.color = Color.black;
        attentionTextComponent.alignment = TextAnchor.MiddleCenter;
        attentionText = attentionTextComponent;

        // Регистрируем текстовые элементы в EnergyManager
        if (EnergyManager.Instance != null)
        {
            EnergyManager.Instance.energyText = energyTextComponent;
            EnergyManager.Instance.attentionText = attentionTextComponent;
        }
    }

}