using UnityEngine;

public class BedInteractable : InteractiveObject
{
    void Start()
    {
        interactionName = "Кровать";
        animationTrigger = "Sleep"; // Название триггера анимации сна
        attentionGainRate = 5f;
    }

    protected override void EndInteractionImmediately()
    {
        base.EndInteractionImmediately();
        Debug.Log("Полежал на кровати - внимание восстановлено!");
    }
}