using UnityEngine;
using UnityEngine.UI;

public abstract class Interactable : MonoBehaviour
{
    [Header("Interactable Settings")]
    public string interactionName;
    public Transform interactionSpot;
    public GameObject interactionPrompt;

    protected bool isInteracting = false;

    void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    public virtual void Interact()
    {
        isInteracting = true;
        Debug.Log($"Started interacting with {interactionName}");
    }

    // ДОБАВЛЕНО: виртуальный метод для остановки взаимодействия
    public virtual void StopInteracting()
    {
        isInteracting = false;
        Debug.Log($"Stopped interacting with {interactionName}");
    }

    public void ShowPrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(true);
    }

    public void HidePrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    public bool IsInteracting()
    {
        return isInteracting;
    }

    public abstract void StartMiniGame();
    public abstract void EndMiniGame();
}