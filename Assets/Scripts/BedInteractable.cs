using UnityEngine;

public class BedInteractable : InteractiveObject
{
    void Start()
    {
        interactionName = "�������";
        animationTrigger = "Sleep"; // �������� �������� �������� ���
        attentionGainRate = 5f;
    }

    protected override void EndInteractionImmediately()
    {
        base.EndInteractionImmediately();
        Debug.Log("������� �� ������� - �������� �������������!");
    }
}