using UnityEngine;

public class WindowInteractable : InteractiveObject
{
    void Start()
    {
        interactionName = "Окно";
        animationTrigger = "LookWindow"; // Название триггера анимации взгляда в окно
        attentionGainRate = 1f;
    }

    protected override void EndInteractionImmediately()
    {
        base.EndInteractionImmediately();
        Debug.Log("Посмотрел в окно - внимание восстановлено!");
    }
}