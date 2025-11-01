// LaptopInteractable.cs
using UnityEngine;

public class NotebookInteractable : InteractiveObject
{
    void Start()
    {
        interactionName = "Ноутбук";
        animationTrigger = "UseLaptop"; // Название триггера анимации работы с ноутбуком
        attentionGainRate = 12f;
    }

    protected override void EndInteraction()
    {
        base.EndInteraction();
        Debug.Log("Работа за ноутбуком завершена. Внимание восстановлено!");
    }
}