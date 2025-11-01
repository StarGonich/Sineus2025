using UnityEngine;
using System.Collections;

public abstract class InteractiveObject : Interactable
{
    [Header("Interactive Object Settings")]
    public Transform animationSpot;
    public string animationTrigger;
    public float attentionGainRate = 10f;

    protected PenguinController penguin;
    protected Animator penguinAnimator;
    protected Vector3 originalPosition;
    protected Quaternion originalRotation;

    public override void Interact()
    {
        if (EnergyManager.Instance != null && !EnergyManager.Instance.HasEnergy())
        {
            Debug.Log("������� �����������! ���� ��������.");
            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.ShowEndDayPanel();
            }
            return;
        }

        if (EnergyManager.Instance.GetCurrentAttention() >= EnergyManager.Instance.maxAttention)
        {
            Debug.Log("�������� ��������� ������������� - �������������� ����������");
            return;
        }

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

        // ��������� ������������ �������
        originalPosition = penguin.transform.position;
        originalRotation = penguin.transform.rotation;

        // ������������� ��������
        if (animationSpot != null)
        {
            penguin.transform.position = animationSpot.position;
            penguin.transform.rotation = animationSpot.rotation;
        }

        // ��������� ����������
        penguin.FixMovement(true);
        penguin.FixCamera(true);

        // ����������� ������ (������ ��� ������ �� ����� ������)
        SetupFixedCamera();

        // ��������� ��������
        penguinAnimator = penguin.GetComponent<Animator>();
        if (penguinAnimator != null)
        {
            PlayLoopingAnimation();
        }

        StartCoroutine(InteractionRoutine());
    }

    protected virtual void SetupFixedCamera()
    {
        if (penguin.playerCamera == null || animationSpot == null) return;

        float cameraDistance = 4f;

        // ������ ����� �� ��������� �� ������������� ������
        Vector3 cameraOffset = -penguin.transform.forward * cameraDistance;

        // ���������� ������������� ������ �� PenguinController
        Vector3 cameraWorldPos = penguin.transform.position +
                               new Vector3(0, penguin.fixedCameraHeight, 0) +
                               cameraOffset;

        penguin.playerCamera.transform.position = cameraWorldPos;

        // ������� �� ��������
        Vector3 lookAtPoint = penguin.transform.position +
                            new Vector3(0, penguin.fixedCameraHeight * 0.8f, 0);

        penguin.playerCamera.transform.LookAt(lookAtPoint);
    }

    protected virtual IEnumerator InteractionRoutine()
    {
        Debug.Log("������ ��������������");

        EnergyManager.Instance.StartGainingAttention(attentionGainRate);

        while (isInteracting)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("����� �� Escape");
                break;
            }

            if (EnergyManager.Instance.GetCurrentAttention() >= EnergyManager.Instance.maxAttention)
            {
                Debug.Log("�������� ��������� �������������");
                break;
            }

            yield return null;
        }

        EndInteractionImmediately();
    }

    protected virtual void PlayLoopingAnimation()
    {
        if (penguinAnimator != null && !string.IsNullOrEmpty(animationTrigger))
        {
            penguinAnimator.SetBool(animationTrigger, true);
            penguinAnimator.Update(0f);
        }
    }

    protected virtual void StopAnimationImmediately()
    {
        if (penguinAnimator != null && !string.IsNullOrEmpty(animationTrigger))
        {
            penguinAnimator.SetBool(animationTrigger, false);
            penguinAnimator.Update(0f);
        }
    }

    protected virtual void EndInteractionImmediately()
    {
        Debug.Log("���������� ��������������");

        StopAnimationImmediately();
        EnergyManager.Instance.StopGainingAttention();

        penguin.transform.position = originalPosition;
        penguin.transform.rotation = originalRotation;

        penguin.ResetCameraAfterInteraction();

        penguin.FixMovement(false);
        penguin.FixCamera(false);

        StopInteracting();
    }

    public override void StartMiniGame()
    {
        // �� ������������
    }

    public override void EndMiniGame()
    {
        EndInteractionImmediately();
    }
}