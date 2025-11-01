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

    protected override void EndInteraction()
    {
        base.EndInteraction();
        Debug.Log("Чтение книг завершено. Внимание восстановлено!");
    }
}