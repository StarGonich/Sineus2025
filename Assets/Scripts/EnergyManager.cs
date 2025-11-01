using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnergyManager : MonoBehaviour
{
    [Header("Energy Settings")]
    public float maxEnergy = 300f; // �������� ������������ �������
    public float energyDrainPerNote = 3f;
    public float energyDrainOnFail = 8f;
    private float currentEnergy;

    [Header("Attention Settings")]
    public float maxAttention = 100f;
    public float attentionDrainPerNote = 10f;
    public float attentionDrainOnFail = 20f;
    private float currentAttention;

    [Header("UI References")]
    public Slider energyBar;
    public Slider attentionBar;
    public Text energyText;
    public Text attentionText;
    public GameObject tiredWarning;
    public GameObject energyDepletedWarning;

    [Header("Colors")]
    public Color energyColor = Color.blue;
    public Color attentionColor = Color.yellow;
    public Color lowEnergyColor = Color.red;

    private Canvas energyCanvas; // ������ �� ��� Canvas

    private bool isGainingAttention = false;
    private float currentAttentionGainRate = 0f;

    public static EnergyManager Instance;

    void Awake()
    {
        Debug.Log($"Awake ������ ��� EnergyManager. Instance is null: {Instance == null}");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("EnergyManager ������ � ����� ����������� ����� �������");

            // �������������� �����
            InitializeResources();
            CreateUI();
            UpdateUI();

            // ������������� �� ������� �������� �����
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Debug.Log("���������� �������� EnergyManager");
            DestroyImmediate(gameObject);
            return;
        }
    }

    void OnDestroy()
    {
        // ������������ �� ������� ��� �����������
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void HideUI()
    {
        if (energyCanvas != null)
        {
            energyCanvas.enabled = false;
            Debug.Log("UI EnergyManager �����");
        }
    }

    public void ShowUI()
    {
        if (energyCanvas != null)
        {
            energyCanvas.enabled = true;
            UpdateUI(); // ��������� �������� ��� ������
            Debug.Log("UI EnergyManager �������");
        }
        else
        {
            Debug.LogWarning("Canvas EnergyManager �� ������ ��� ������� �������� UI");
        }
    }

    // ����� ������� OnSceneLoaded ��� ��������������� ����������
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"��������� �����: {scene.name}");

        // ���������� UI � ������� �����, �������� � ����-����
        if (scene.name == "MainScene")
        {
            ShowUI();
        }
        else if (scene.name == "MelodyMiniGameScene" || scene.name == "PianoMiniGameScene")
        {
            HideUI();
        }

        UpdateUI();
    }

    void InitializeResources()
    {
        currentEnergy = maxEnergy;
        currentAttention = maxAttention;
    }

    void CreateUI()
    {
        // ������� ����������� Canvas ��� EnergyManager
        CreateEnergyCanvas();

        // ���� UI �������� �� �������, ������� ��
        if (energyBar == null || attentionBar == null)
        {
            CreateEnergyUI();
        }
    }

    void CreateEnergyCanvas()
    {
        // ���������, ���� �� ��� Canvas
        energyCanvas = GetComponentInChildren<Canvas>();
        if (energyCanvas == null)
        {
            GameObject canvasObj = new GameObject("EnergyManagerCanvas");
            canvasObj.transform.SetParent(transform); // ������ �������� �������� EnergyManager
            energyCanvas = canvasObj.AddComponent<Canvas>();
            energyCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            energyCanvas.sortingOrder = 1000; // ������� ������� ����� ��� ������ ������ UI

            CanvasScaler canvasScaler = canvasObj.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("������ EnergyManagerCanvas");
        }
    }

    void CreateEnergyUI()
    {
        if (energyCanvas == null)
        {
            Debug.LogError("Canvas �� ������!");
            return;
        }

        // ������� ������ ��� ����
        GameObject panel = new GameObject("EnergyPanel");
        panel.transform.SetParent(energyCanvas.transform);
        RectTransform panelRT = panel.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 1f);
        panelRT.anchorMax = new Vector2(0.5f, 1f);
        panelRT.pivot = new Vector2(0.5f, 1f);
        panelRT.sizeDelta = new Vector2(250, 80);
        panelRT.anchoredPosition = new Vector2(0, -20);

        // ����� �������
        GameObject energySlider = CreateSlider("EnergyBar", panel.transform, new Vector2(0, 0), energyColor);
        energyBar = energySlider.GetComponent<Slider>();

        // ����� ��������
        GameObject attentionSlider = CreateSlider("AttentionBar", panel.transform, new Vector2(0, -30), attentionColor);
        attentionBar = attentionSlider.GetComponent<Slider>();

        // ������
        CreateTextLabels(panel.transform);

        // ��������������
        CreateWarnings(energyCanvas.transform);

        Debug.Log("UI EnergyManager ������ �� ����������� Canvas");
    }

    GameObject CreateSlider(string name, Transform parent, Vector2 position, Color color)
    {
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(parent);

        RectTransform rt = sliderObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(200, 20);
        rt.anchoredPosition = position;

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 1;

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = Color.gray;
        RectTransform bgRT = bg.GetComponent<RectTransform>();
        SetFullRectTransform(bgRT);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform);
        RectTransform fillAreaRT = fillArea.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = new Vector2(0, 0.25f);
        fillAreaRT.anchorMax = new Vector2(1, 0.75f);
        fillAreaRT.sizeDelta = Vector2.zero;

        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = color;
        RectTransform fillRT = fill.GetComponent<RectTransform>();
        SetFullRectTransform(fillRT);
        slider.fillRect = fillRT;

        return sliderObj;
    }

    void CreateTextLabels(Transform parent)
    {
        // ����� �������
        GameObject energyTextObj = new GameObject("EnergyText");
        energyTextObj.transform.SetParent(parent);
        RectTransform energyRT = energyTextObj.AddComponent<RectTransform>();
        energyRT.anchorMin = new Vector2(0.5f, 1f);
        energyRT.anchorMax = new Vector2(0.5f, 1f);
        energyRT.pivot = new Vector2(1f, 1f);
        energyRT.sizeDelta = new Vector2(100, 20);
        energyRT.anchoredPosition = new Vector2(-110, 0);
        energyText = energyTextObj.AddComponent<Text>();
        energyText.text = $"�������: {Mathf.RoundToInt(currentEnergy)}";
        energyText.fontSize = 12;
        energyText.color = Color.white;
        energyText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        energyText.alignment = TextAnchor.MiddleLeft;

        // ����� ��������
        GameObject attentionTextObj = new GameObject("AttentionText");
        attentionTextObj.transform.SetParent(parent);
        RectTransform attentionRT = attentionTextObj.AddComponent<RectTransform>();
        attentionRT.anchorMin = new Vector2(0.5f, 1f);
        attentionRT.anchorMax = new Vector2(0.5f, 1f);
        attentionRT.pivot = new Vector2(1f, 1f);
        attentionRT.sizeDelta = new Vector2(100, 20);
        attentionRT.anchoredPosition = new Vector2(-110, -30);
        attentionText = attentionTextObj.AddComponent<Text>();
        attentionText.text = $"��������: {Mathf.RoundToInt(currentAttention)}";
        attentionText.fontSize = 12;
        attentionText.color = Color.white;
        attentionText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        attentionText.alignment = TextAnchor.MiddleLeft;
    }

    void CreateWarnings(Transform parent)
    {
        // �������������� �� ���������
        tiredWarning = CreateWarningText("TiredWarning", "������� �����! ����� ���������", new Vector2(0, -100), parent);
        tiredWarning.SetActive(false);

        // �������������� �� ��������� �������
        energyDepletedWarning = CreateWarningText("EnergyDepleted", "������� �� ������! ���� ����� ����������", new Vector2(0, -150), parent);
        energyDepletedWarning.SetActive(false);
    }

    GameObject CreateWarningText(string name, string text, Vector2 position, Transform parent)
    {
        GameObject warning = new GameObject(name);
        warning.transform.SetParent(parent);
        RectTransform rt = warning.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(400, 40);
        rt.anchoredPosition = position;

        Text warningText = warning.AddComponent<Text>();
        warningText.text = text;
        warningText.fontSize = 18;
        warningText.color = Color.red;
        warningText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        warningText.alignment = TextAnchor.MiddleCenter;

        return warning;
    }

    void SetFullRectTransform(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    // === PUBLIC METHODS ===

    public bool CanPlayPiano()
    {
        return currentAttention >= attentionDrainPerNote && currentEnergy >= energyDrainPerNote;
    }

    public void DrainAttentionForSequence(int sequenceLength, bool failed = false)
    {
        float drainAmount = failed ? attentionDrainOnFail : sequenceLength * attentionDrainPerNote;
        currentAttention = Mathf.Max(0, currentAttention - drainAmount);
        UpdateUI();

        if (currentAttention <= 0)
        {
            ShowTiredWarning();
        }
    }

    public void StartGainingAttention(float gainRate)
    {
        isGainingAttention = true;
        currentAttentionGainRate = gainRate;
        Debug.Log($"������ �������������� ��������: {gainRate}/���");
    }

    public void StopGainingAttention()
    {
        isGainingAttention = false;
        currentAttentionGainRate = 0f;
        Debug.Log("�������������� �������� �����������");
    }

    public void DrainEnergyForSequence(int sequenceLength, bool failed = false)
    {
        float drainAmount = failed ? energyDrainOnFail : sequenceLength * energyDrainPerNote;
        currentEnergy = Mathf.Max(0, currentEnergy - drainAmount);
        UpdateUI();

        if (currentEnergy <= 0)
        {
            ShowEnergyDepletedWarning();
        }
    }

    public void RestoreAttention(float amount)
    {
        currentAttention = Mathf.Min(maxAttention, currentAttention + amount);
        UpdateUI();
        HideTiredWarning();
    }

    public void RestoreAttentionFull()
    {
        currentAttention = maxAttention;
        UpdateUI();
        HideTiredWarning();
    }

    public void RestoreEnergy(float amount)
    {
        currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);
        UpdateUI();
        HideEnergyDepletedWarning();
    }

    void ShowTiredWarning()
    {
        if (tiredWarning != null)
            tiredWarning.SetActive(true);
    }

    void HideTiredWarning()
    {
        if (tiredWarning != null)
            tiredWarning.SetActive(false);
    }

    void ShowEnergyDepletedWarning()
    {
        if (energyDepletedWarning != null)
            energyDepletedWarning.SetActive(true);
    }

    void HideEnergyDepletedWarning()
    {
        if (energyDepletedWarning != null)
            energyDepletedWarning.SetActive(false);
    }

    public float GetCurrentEnergy()
    {
        return currentEnergy;
    }

    public float GetCurrentAttention()
    {
        return currentAttention;
    }

    void UpdateUI()
    {
        if (energyBar != null)
        {
            energyBar.value = currentEnergy / maxEnergy;
            var fillImage = energyBar.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = currentEnergy < 30f ? lowEnergyColor : energyColor;
            }
        }

        if (attentionBar != null)
        {
            attentionBar.value = currentAttention / maxAttention;
        }

        if (energyText != null)
        {
            energyText.text = $"�������: {Mathf.RoundToInt(currentEnergy)}";
        }

        if (attentionText != null)
        {
            attentionText.text = $"��������: {Mathf.RoundToInt(currentAttention)}";
        }
    }

    void Update()
    {
        // �������������� ��������
        if (isGainingAttention && currentAttention < maxAttention)
        {
            currentAttention = Mathf.Min(maxAttention, currentAttention + currentAttentionGainRate * Time.deltaTime);
            UpdateUI();
        }
    }

    public bool HasEnergy()
    {
        return currentEnergy > 0;
    }

    public bool HasAttention()
    {
        return currentAttention > 0;
    }

    public float GetEnergyPercent()
    {
        return currentEnergy / maxEnergy;
    }

    public float GetAttentionPercent()
    {
        return currentAttention / maxAttention;
    }

    public void ResetAllResources()
    {
        currentEnergy = maxEnergy;
        currentAttention = maxAttention;
        UpdateUI();
        HideTiredWarning();
        HideEnergyDepletedWarning();
    }

    public void ResetEnergy()
    {
        currentEnergy = maxEnergy;
        UpdateUI();
    }

    public void ResetAttention()
    {
        currentAttention = maxAttention;
        UpdateUI();
    }

}