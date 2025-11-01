using UnityEngine;

public class BedInteractable : BaseInteractable
{
    [Header("Bed Settings")]
    public float energyGainRate = 30f;
    public float sleepDuration = 5f;

    void Start()
    {
        itemName = "Кровать";
    }

    public override void Interact()
    {
        base.Interact();
        StartSleeping();
    }

    void StartSleeping()
    {
        // Сон восстанавливает больше энергии но дольше
        Invoke("WakeUp", sleepDuration);

        Debug.Log("Легли спать...");
    }

    void WakeUp()
    {
        EnergyManager.Instance.ResetEnergy(); // Полное восстановление
        EndMiniGame();

        Debug.Log("Проснулись с полной энергией!");
    }

    public override void EndMiniGame()
    {
        CancelInvoke("WakeUp");
        base.EndMiniGame();
    }
}