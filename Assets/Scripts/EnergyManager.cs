using UnityEngine;
using UnityEngine.UI;

public class EnergyManager : MonoBehaviour
{
    [Header("Energy Settings")]
    public float maxEnergy = 100f;
    public float energyDrainRate = 10f;
    public float energyGainRate = 15f;

    [Header("UI")]
    public Slider energyBar;
    public GameObject lowEnergyWarning;

    private float currentEnergy;
    private bool isDraining = false;
    private bool isGaining = false;

    public static EnergyManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentEnergy = maxEnergy;
        UpdateUI();
    }

    void Update()
    {
        if (isDraining)
        {
            currentEnergy -= energyDrainRate * Time.deltaTime;
            currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
            UpdateUI();

            if (currentEnergy <= 0)
            {
                OnEnergyDepleted();
            }
        }
        else if (isGaining)
        {
            currentEnergy += energyGainRate * Time.deltaTime;
            currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
            UpdateUI();

            if (currentEnergy >= maxEnergy)
            {
                StopGainingEnergy();
            }
        }
    }

    public void StartDrainingEnergy()
    {
        isDraining = true;
        isGaining = false;
    }

    public void StopDrainingEnergy()
    {
        isDraining = false;
    }

    public void StartGainingEnergy()
    {
        isGaining = true;
        isDraining = false;
        lowEnergyWarning.SetActive(false);
    }

    void StopGainingEnergy()
    {
        isGaining = false;
    }

    void OnEnergyDepleted()
    {
        isDraining = false;
        lowEnergyWarning.SetActive(true);

        // Автоматически встаём из-за пианино
        PianoMiniGame pianoGame = FindObjectOfType<PianoMiniGame>();
        if (pianoGame != null && pianoGame.IsPlaying())
        {
            pianoGame.StopGame();
        }

        // Также останавливаем взаимодействие
        PenguinController penguin = FindObjectOfType<PenguinController>();
        if (penguin != null)
        {
            penguin.StopInteraction();
        }
    }

    void UpdateUI()
    {
        if (energyBar != null)
        {
            energyBar.value = currentEnergy / maxEnergy;
        }
    }

    public bool HasEnergy()
    {
        return currentEnergy > 20f;
    }

    public float GetEnergyPercent()
    {
        return currentEnergy / maxEnergy;
    }

    public void ResetEnergy()
    {
        currentEnergy = maxEnergy;
        UpdateUI();
    }
}