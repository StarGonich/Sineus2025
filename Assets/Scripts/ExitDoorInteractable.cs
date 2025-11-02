using UnityEngine;
using UnityEngine.UI;

public class ExitDoorInteractable : Interactable
{
    [Header("Exit Door Settings")]
    public GameObject exitConfirmationPanel;
    public Button yesButton;
    public Button noButton;

    private PenguinController penguin;

    void Start()
    {
        interactionName = "Выходная дверь";

        // Настраиваем кнопки
        if (yesButton != null)
            yesButton.onClick.AddListener(ConfirmExit);

        if (noButton != null)
            noButton.onClick.AddListener(CancelExit);

        // Скрываем панель при старте
        if (exitConfirmationPanel != null)
            exitConfirmationPanel.SetActive(false);
    }

    public override void Interact()
    {
        base.Interact();
        ShowExitConfirmation();
    }

    private void ShowExitConfirmation()
    {
        penguin = PenguinController.Instance;
        if (penguin != null)
        {
            penguin.FixMovement(true);
            penguin.FixCamera(true);
        }

        // Показываем панель подтверждения
        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(true);
            GameProgressManager.Instance.UnlockCursor();
        }

        Debug.Log("Показано меню выхода из игры");
    }

    private void ConfirmExit()
    {
        Debug.Log("Выход из игры подтвержден");

        // Скрываем панель
        if (exitConfirmationPanel != null)
            exitConfirmationPanel.SetActive(false);

        // Выходим из игры
        ExitGame();
    }

    private void CancelExit()
    {
        Debug.Log("Выход из игры отменен");

        // Скрываем панель
        if (exitConfirmationPanel != null)
            exitConfirmationPanel.SetActive(false);

        // Восстанавливаем управление
        if (penguin != null)
        {
            penguin.FixMovement(false);
            penguin.FixCamera(false);
            GameProgressManager.Instance.LockCursor();
        }

        StopInteracting();
    }

    private void ExitGame()
    {
        Debug.Log("Выход из игры...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public override void StartMiniGame()
    {
        // Не используется для двери
    }

    public override void EndMiniGame()
    {
        // Не используется для двери
    }

    public override void StopInteracting()
    {
        base.StopInteracting();

        // Дополнительная очистка
        if (exitConfirmationPanel != null)
            exitConfirmationPanel.SetActive(false);
    }
}