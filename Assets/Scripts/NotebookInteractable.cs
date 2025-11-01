using UnityEngine;

public class NotebookInteractable : InteractiveObject
{
    void Start()
    {
        interactionName = "Ноутбук";
        animationTrigger = "UseLaptop"; // Название триггера анимации работы с ноутбуком
        attentionGainRate = 1f;
    }

    protected override void EndInteractionImmediately()
    {
        base.EndInteractionImmediately();
        Debug.Log("Поработал за ноутбуком - внимание восстановлено!");
    }
}