using UnityEngine;

public class WindowInteractable : InteractiveObject
{
    void Start()
    {
        interactionName = "����";
        animationTrigger = "LookWindow"; // �������� �������� �������� ������� � ����
        attentionGainRate = 1f;
    }

    protected override void EndInteractionImmediately()
    {
        base.EndInteractionImmediately();
        Debug.Log("��������� � ���� - �������� �������������!");
    }
}