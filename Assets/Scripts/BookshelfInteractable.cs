// BookshelfInteractable.cs
using UnityEngine;

public class BookshelfInteractable : InteractiveObject
{
    void Start()
    {
        interactionName = "������� �����";
        animationTrigger = "ReadBooks"; // ������ ��� Bool ��������
        attentionGainRate = 2f;
    }

    protected override void EndInteractionImmediately()
    {
        base.EndInteractionImmediately();
        Debug.Log("������ ���� ���������. �������� �������������!");
    }
}