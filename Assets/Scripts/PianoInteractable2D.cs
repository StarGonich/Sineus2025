using UnityEngine;
using UnityEngine.SceneManagement;

public class PianoInteractable2D : Interactable
{
    [Header("2D Piano Settings")]
    public string pianoSceneName = "PianoMiniGameScene";

    private PenguinController penguin;
    private EnergyManager energyManager;

    void Start()
    {
        penguin = FindObjectOfType<PenguinController>();
        energyManager = EnergyManager.Instance;
    }

    public override void Interact()
    {
        if (penguin == null || energyManager == null)
        {
            Debug.LogError("�� ������� ����������� ����������!");
            return;
        }

        if (!energyManager.HasEnergy())
        {
            Debug.Log("������� ����� ��� ���� �� �������!");
            return;
        }

        base.Interact();

        // ��������� � ����� ����-����
        SceneManager.LoadScene(pianoSceneName);

        // ��������� ����-���� ����� �������� �����
        Invoke("StartMiniGame", 0.1f);
    }

    public override void StartMiniGame()
    {
        PianoMiniGame2D miniGame = FindObjectOfType<PianoMiniGame2D>();
        if (miniGame != null)
        {
            miniGame.StartGame();
        }
    }

    public override void EndMiniGame()
    {
        // ������������ � �������� �����
        SceneManager.LoadScene("MainScene");
        StopInteracting();
    }
}