using UnityEngine;
using UnityEngine.SceneManagement;

public class MelodyPianoInteractable : Interactable
{
    [Header("Melody Game Settings")]
    public string melodySceneName = "MelodyMiniGameScene";

    public override void Interact()
    {
        if (!EnergyManager.Instance.HasEnergy())
        {
            Debug.Log("Слишком устал для игры на пианино!");
            return;
        }

        base.Interact();

        // Переходим в сцену мини-игры
        SceneManager.LoadScene(melodySceneName);
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