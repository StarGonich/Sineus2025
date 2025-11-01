using UnityEngine;
using System.Collections;

public abstract class InteractiveObject : Interactable
{
    [Header("Interactive Object Settings")]
    public Transform animationSpot;
    public string animationTrigger;
    public float attentionGainRate = 2f;

    protected PenguinController penguin;
    protected Animator penguinAnimator;
    protected Vector3 originalPosition;
    protected Quaternion originalRotation;

    public override void Interact()
    {
        if (!EnergyManager.Instance.HasEnergy())
        {
            Debug.Log("������� ����� ��� �����!");
            return;
        }

        base.Interact();
        StartInteraction();
    }

    protected virtual void StartInteraction()
    {
        penguin = PenguinController.Instance;
        if (penguin == null) return;

        // ����� ��������� ��������� � ������
        penguin.FixMovement(true);
        penguin.FixCamera(true);

        // ��������� ������������ �������
        originalPosition = penguin.transform.position;
        originalRotation = penguin.transform.rotation;

        // ������������� �������� � ����� ��������
        if (animationSpot != null)
        {
            penguin.transform.position = animationSpot.position;
            penguin.transform.rotation = animationSpot.rotation;
        }

        // ����������� ������
        SetupFixedCamera();

        // �������� �������� � ��������� �������� �����
        penguinAnimator = penguin.GetComponent<Animator>();
        if (penguinAnimator != null)
        {
            PlayLoopingAnimation();
        }
        else
        {
            Debug.LogError("Animator �� ������ �� ��������!");
        }

        // ��������� ������� ��������������
        StartCoroutine(InteractionRoutine());
    }

    protected virtual void SetupFixedCamera()
    {
        if (penguin.playerCamera == null) return;

        // ������� ������������� ������ ������� �� ��������
        Vector3 cameraOffset = -penguin.transform.forward * 3f + Vector3.up * 1.5f;
        penguin.playerCamera.transform.position = penguin.transform.position + cameraOffset;
        penguin.playerCamera.transform.LookAt(penguin.transform.position + Vector3.up * 1f);
    }

    protected virtual IEnumerator InteractionRoutine()
    {
        Debug.Log("������ ��������������");

        // �������� ��������������� ��������
        EnergyManager.Instance.StartGainingAttention(attentionGainRate);

        // �������� ���� ��������������
        while (isInteracting)
        {
            // ��������� ����� �� ESCAPE
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("����� �� Escape");
                break;
            }

            // ��������� ������ �������������� ��������
            if (EnergyManager.Instance.GetCurrentAttention() >= EnergyManager.Instance.maxAttention)
            {
                Debug.Log("�������� ��������� �������������");
                break;
            }

            yield return null;
        }

        // ���������� ����������
        EndInteraction();
    }

    protected virtual void PlayLoopingAnimation()
    {
        if (penguinAnimator != null && !string.IsNullOrEmpty(animationTrigger))
        {
            penguinAnimator.SetBool(animationTrigger, true);
            Debug.Log($"�������� ��������: {animationTrigger}");
        }
    }

    protected virtual void StopAnimationImmediately()
    {
        if (penguinAnimator != null && !string.IsNullOrEmpty(animationTrigger))
        {
            penguinAnimator.SetBool(animationTrigger, false);
            Debug.Log($"�������� �����������: {animationTrigger}");
        }
    }

    protected virtual void EndInteraction()
    {
        Debug.Log("���������� ���������� ��������������");

        // 1. ������� ������������� ��������
        StopAnimationImmediately();

        // 2. ������������� �������������� ��������
        EnergyManager.Instance.StopGainingAttention();

        // 3. ���������� ��������
        penguin.transform.position = originalPosition;
        penguin.transform.rotation = originalRotation;

        // 4. ������������ ���������� (������ ������������� ��������)
        penguin.FixMovement(false);
        penguin.FixCamera(false);

        // 5. ��������� ��������������
        StopInteracting();

        Debug.Log("�������������� ��������� ���������");
    }

    public override void StartMiniGame()
    {
        // �� ������������
    }

    public override void EndMiniGame()
    {
        EndInteraction();
    }
}