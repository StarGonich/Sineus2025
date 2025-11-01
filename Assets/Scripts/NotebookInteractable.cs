using UnityEngine;

public class NotebookInteractable : InteractiveObject
{
    void Start()
    {
        interactionName = "�������";
        animationTrigger = "UseLaptop"; // �������� �������� �������� ������ � ���������
        attentionGainRate = 1f;
    }

    protected override void EndInteractionImmediately()
    {
        base.EndInteractionImmediately();
        Debug.Log("��������� �� ��������� - �������� �������������!");
    }
}