// LaptopInteractable.cs
using UnityEngine;

public class NotebookInteractable : InteractiveObject
{
    void Start()
    {
        interactionName = "�������";
        animationTrigger = "UseLaptop"; // �������� �������� �������� ������ � ���������
        attentionGainRate = 12f;
    }

    protected override void EndInteraction()
    {
        base.EndInteraction();
        Debug.Log("������ �� ��������� ���������. �������� �������������!");
    }
}