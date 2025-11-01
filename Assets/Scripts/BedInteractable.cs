using UnityEngine;

public class BedInteractable : BaseInteractable
{
    [Header("Bed Settings")]
    public float energyGainRate = 30f;
    public float sleepDuration = 5f;

    void Start()
    {
        itemName = "�������";
    }

    public override void Interact()
    {
        base.Interact();
        StartSleeping();
    }

    void StartSleeping()
    {
        // ��� ��������������� ������ ������� �� ������
        Invoke("WakeUp", sleepDuration);

        Debug.Log("����� �����...");
    }

    void WakeUp()
    {
        EnergyManager.Instance.ResetEnergy(); // ������ ��������������
        EndMiniGame();

        Debug.Log("���������� � ������ ��������!");
    }

    public override void EndMiniGame()
    {
        CancelInvoke("WakeUp");
        base.EndMiniGame();
    }
}