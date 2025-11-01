// BookshelfInteractable.cs
using UnityEngine;

public class BookshelfInteractable : InteractiveObject
{
    void Start()
    {
        interactionName = "Книжная полка";
        animationTrigger = "ReadBooks"; // Теперь это Bool параметр
        attentionGainRate = 2f;
    }

    protected override void EndInteractionImmediately()
    {
        base.EndInteractionImmediately();
        Debug.Log("Чтение книг завершено. Внимание восстановлено!");
    }
}