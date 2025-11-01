using UnityEngine;
using UnityEngine.SceneManagement;

public class MelodyPianoInteractable : Interactable
{
    [Header("Melody Game Settings")]
    public string melodySceneName = "MelodyMiniGameScene";

    public override void Interact()
    {
        if (!EnergyManager.Instance.HasEnergy())
        {
            Debug.Log("������� ����� ��� ���� �� �������!");
            return;
        }

        base.Interact();

        // ��������� � ����� ����-����
        SceneManager.LoadScene(melodySceneName);
    }

    public override void StartMiniGame()
    {
        // ������������� ����������� � MelodyMiniGame
    }

    public override void EndMiniGame()
    {
        StopInteracting();
    }
}