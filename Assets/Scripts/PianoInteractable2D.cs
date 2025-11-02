using UnityEngine;
using UnityEngine.SceneManagement;

public class PianoInteractable2D : Interactable
{
    [Header("2D Piano Settings")]
    public string pianoSceneName = "PianoMiniGameScene";

    private PenguinController penguin;
    private EnergyManager energyManager;

    void Start()
    {
        penguin = FindObjectOfType<PenguinController>();
        energyManager = EnergyManager.Instance;
    }

    public override void Interact()
    {
        if (penguin == null || energyManager == null)
        {
            Debug.LogError("Не найдены необходимые компоненты!");
            return;
        }

        // ДЛЯ ПИАНИНО ПРОВЕРЯЕМ, ЧТО ЕСТЬ ЭНЕРГИЯ И ВНИМАНИЕ (обратная логика)
        if (!energyManager.HasEnergy())
        {
            Debug.Log("Слишком устал для игры на пианино!");
            return;
        }

        if (!energyManager.HasAttention())
        {
            Debug.Log("Слишком устал для концентрации на пианино!");
            return;
        }

        // ДОПОЛНИТЕЛЬНАЯ ПРОВЕРКА: достаточно ли ресурсов для хотя бы одной ноты
        if (!energyManager.CanPlayPiano())
        {
            Debug.Log("Недостаточно энергии или внимания для игры на пианино!");
            return;
        }

        base.Interact();

        // Переходим в сцену мини-игры
        SceneManager.LoadScene(pianoSceneName);

        // Запускаем мини-игру после загрузки сцены
        Invoke("StartMiniGame", 0.1f);
    }

    public override void StartMiniGame()
    {
        PianoMiniGame2D miniGame = FindObjectOfType<PianoMiniGame2D>();
        if (miniGame != null)
        {
            miniGame.StartGame();
        }
    }

    public override void EndMiniGame()
    {
        // Возвращаемся в основную сцену
        SceneManager.LoadScene("MainScene");
        StopInteracting();
    }
}