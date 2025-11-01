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

    protected override void EndInteraction()
    {
        base.EndInteraction();
        Debug.Log("������ ���� ���������. �������� �������������!");
    }
}