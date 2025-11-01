using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnergyManager : MonoBehaviour
{
    [Header("Energy Settings")]
    public float maxEnergy = 300f;
    public float energyDrainPerNote = 3f;
    public float energyDrainOnFail = 8f;
    private float currentEnergy;

    [Header("Attention Settings")]
    public float maxAttention = 100f;
    public float attentionDrainPerNote = 10f;
    public float attentionDrainOnFail = 20f;
    private float currentAttention;

    [Header("UI References")]
    public GameObject tiredWarning;
    public GameObject energyDepletedWarning;

    [Header("UI References - Progress Bars")]
    public Slider energyProgressBar;
    public Slider attentionProgressBar;
    public Text energyText;
    public Text attentionText;

    [Header("Progress Bar Colors")]
    public Color energyFullColor = Color.green;
    public Color energyLowColor = Color.red;
    public Color attentionFullColor = Color.blue;
    public Color attentionLowColor = Color.yellow;


    private bool isGainingAttention = false;
    private float currentAttentionGainRate = 0f;

    public static EnergyManager Instance;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeResources();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"EnergyManager: Загружена сцена {scene.name}");
        UpdateProgressBars();
    }

    void SetupProgressBars()
    {
        // Настраиваем прогресс-бары при старте
        if (energyProgressBar != null)
        {
            energyProgressBar.minValue = 0;
            energyProgressBar.maxValue = maxEnergy;
            energyProgressBar.value = currentEnergy;
            energyProgressBar.interactable = false;
            SetupProgressBarColors(energyProgressBar, energyFullColor, energyLowColor);
        }

        if (attentionProgressBar != null)
        {
            attentionProgressBar.minValue = 0;
            attentionProgressBar.maxValue = maxAttention;
            attentionProgressBar.value = currentAttention;
            attentionProgressBar.interactable = false;
            SetupProgressBarColors(attentionProgressBar, attentionFullColor, attentionLowColor);
        }
    }

    void SetupProgressBarColors(Slider progressBar, Color fullColor, Color lowColor)
    {
        // Находим Fill изображение и меняем цвет
        Transform fillArea = progressBar.transform.Find("Fill Area");
        if (fillArea != null)
        {
            Transform fill = fillArea.Find("Fill");
            if (fill != null)
            {
                Image fillImage = fill.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = fullColor;
                }
            }
        }
    }

    void UpdateProgressBars()
    {
        // Обновляем энергетический прогресс-бар
        if (energyProgressBar != null)
        {
            energyProgressBar.value = currentEnergy;
            UpdateProgressBarColor(energyProgressBar, currentEnergy / maxEnergy, energyFullColor, energyLowColor);
        }

        // Обновляем прогресс-бар внимания
        if (attentionProgressBar != null)
        {
            attentionProgressBar.value = currentAttention;
            UpdateProgressBarColor(attentionProgressBar, currentAttention / maxAttention, attentionFullColor, attentionLowColor);
        }

        // Обновляем текстовую информацию
        UpdateProgressText();
    }

    void UpdateProgressText()
    {
        // Обновляем текстовое отображение
        if (energyText != null)
        {
            energyText.text = $"{Mathf.RoundToInt(currentEnergy)}/{Mathf.RoundToInt(maxEnergy)}";
        }

        if (attentionText != null)
        {
            attentionText.text = $"{Mathf.RoundToInt(currentAttention)}/{Mathf.RoundToInt(maxAttention)}";
        }
    }

    void UpdateProgressBarColor(Slider progressBar, float percent, Color fullColor, Color lowColor)
    {
        Transform fillArea = progressBar.transform.Find("Fill Area");
        if (fillArea != null)
        {
            Transform fill = fillArea.Find("Fill");
            if (fill != null)
            {
                Image fillImage = fill.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = Color.Lerp(lowColor, fullColor, percent);
                }
            }
        }
    }

    void InitializeResources()
    {
        currentEnergy = maxEnergy;
        currentAttention = maxAttention;

        // Инициализируем UI после установки значений
        StartCoroutine(InitializeUIAfterDelay());
    }

    System.Collections.IEnumerator InitializeUIAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        SetupProgressBars();
        UpdateProgressBars();
    }

    void Start()
    {
        SetupProgressBars();
        UpdateProgressBars();
    }

    // === PUBLIC METHODS ===

    public bool CanPlayPiano()
    {
        return currentAttention >= attentionDrainPerNote && currentEnergy >= energyDrainPerNote;
    }

    public void CheckDayCompletion()
    {
        if (!HasEnergy() && GameProgressManager.Instance != null)
        {
            Debug.Log("Энергия закончилась - предлагаем завершить день");
            GameProgressManager.Instance.ShowEndDayPanel();
        }
    }

    public void DrainAttentionForSequence(int sequenceLength, bool failed = false)
    {
        float drainAmount = failed ? attentionDrainOnFail : sequenceLength * attentionDrainPerNote;
        currentAttention = Mathf.Max(0, currentAttention - drainAmount);

        UpdateProgressBars(); // ОБНОВЛЯЕМ ПРОГРЕСС-БАР

        if (currentAttention <= 0)
        {
            ShowTiredWarning();
        }
    }

    public void StartGainingAttention(float gainRate)
    {
        isGainingAttention = true;
        currentAttentionGainRate = gainRate;
        Debug.Log($"Начато восстановление внимания: {gainRate}/сек");
    }

    public void StopGainingAttention()
    {
        isGainingAttention = false;
        currentAttentionGainRate = 0f;
        Debug.Log("Восстановление внимания остановлено");
    }

    public void DrainEnergyForSequence(int sequenceLength, bool failed = false)
    {
        float drainAmount = failed ? energyDrainOnFail : sequenceLength * energyDrainPerNote;
        currentEnergy = Mathf.Max(0, currentEnergy - drainAmount);

        UpdateProgressBars(); // ОБНОВЛЯЕМ ПРОГРЕСС-БАР

        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.SaveData();
        }

        if (currentEnergy <= 0)
        {
            ShowEnergyDepletedWarning();
        }
    }

    public void RestoreAttention(float amount)
    {
        currentAttention = Mathf.Min(maxAttention, currentAttention + amount);

        UpdateProgressBars(); // ОБНОВЛЯЕМ ПРОГРЕСС-БАР

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

        UpdateProgressBars(); // ОБНОВЛЯЕМ ПРОГРЕСС-БАР

        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.SaveData();
        }

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
        // Обновление UI теперь должно обрабатываться внешними системами
        // Этот метод оставлен для совместимости
    }

    void Update()
    {
        // Восстановление внимания
        if (isGainingAttention && currentAttention < maxAttention)
        {
            currentAttention = Mathf.Min(maxAttention, currentAttention + currentAttentionGainRate * Time.deltaTime);
            UpdateProgressBars(); // ОБНОВЛЯЕМ ПРОГРЕСС-БАР В РЕАЛЬНОМ ВРЕМЕНИ
        }

        // Горячая клавиша для завершения дня
        if (Input.GetKeyDown(KeyCode.Backspace) && GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.ShowEndDayPanel();
        }
    }

    public void OnExitPianoMinigame()
    {
        Debug.Log("EnergyManager: Выход из мини-игры пианино");

        // Просто вызываем обновление, без сложной логики
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.RefreshAfterMinigame();
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

    public bool IsAttentionFull()
    {
        return Mathf.Approximately(currentAttention, maxAttention) || currentAttention >= maxAttention;
    }

    public bool CanInteractWithObjects()
    {
        return !IsAttentionFull() && HasEnergy();
    }

    public void ResetAllResources()
    {
        currentEnergy = maxEnergy;
        currentAttention = maxAttention;

        UpdateProgressBars(); // ОБНОВЛЯЕМ ПРОГРЕСС-БАР

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