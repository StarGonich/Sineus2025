using UnityEngine;
using UnityEngine.SceneManagement;

public class MelodyPianoInteractable : Interactable
{
    [Header("Melody Game Settings")]
    public string melodySceneName = "MelodyMiniGameScene";

    public override void Interact()
    {
        // ИСПРАВЛЕНИЕ: при нулевой энергии показываем окно завершения дня
        if (!EnergyManager.Instance.HasEnergy())
        {
            Debug.Log("Энергия закончилась! День завершен.");
            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.ShowEndDayPanel();
            }
            return;
        }

        // ДЛЯ ПИАНИНО: проверяем что внимание ЕСТЬ (больше 0)
        if (EnergyManager.Instance.GetCurrentAttention() > 0)
        {
            Debug.Log("Запуск мини-игры на пианино");

            // Переходим в сцену мини-игры
            SceneManager.LoadScene(melodySceneName);
        }
        else
        {
            Debug.Log("Слишком устал для концентрации на пианино! Внимание: " + EnergyManager.Instance.GetCurrentAttention());
        }
    }

    public override void StartMiniGame()
    {
        // Автоматически запускается в MelodyMiniGame
    }

    public override void EndMiniGame()
    {
        StopInteracting();
    }
}