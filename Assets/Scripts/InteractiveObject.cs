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
            Debug.Log("Энергия закончилась! День завершен.");
            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.ShowEndDayPanel();
            }
            return;
        }

        if (EnergyManager.Instance.GetCurrentAttention() >= EnergyManager.Instance.maxAttention)
        {
            Debug.Log("Внимание полностью восстановлено - взаимодействие невозможно");
            return;
        }

        if (!EnergyManager.Instance.HasEnergy())
        {
            Debug.Log("Слишком устал для этого!");
            return;
        }

        base.Interact();
        StartInteraction();
    }

    protected virtual void StartInteraction()
    {
        penguin = PenguinController.Instance;
        if (penguin == null) return;

        // СОХРАНЯЕМ ОРИГИНАЛЬНУЮ ПОЗИЦИЮ
        originalPosition = penguin.transform.position;
        originalRotation = penguin.transform.rotation;

        // ТЕЛЕПОРТИРУЕМ ПИНГВИНА
        if (animationSpot != null)
        {
            penguin.transform.position = animationSpot.position;
            penguin.transform.rotation = animationSpot.rotation;
        }

        // ФИКСИРУЕМ УПРАВЛЕНИЕ
        penguin.FixMovement(true);
        penguin.FixCamera(true);

        // НАСТРАИВАЕМ КАМЕРУ (теперь она всегда на одной высоте)
        SetupFixedCamera();

        // ЗАПУСКАЕМ АНИМАЦИЮ
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

        // КАМЕРА СТОИТ ЗА ПИНГВИНОМ НА ФИКСИРОВАННОЙ ВЫСОТЕ
        Vector3 cameraOffset = -penguin.transform.forward * cameraDistance;

        // Используем фиксированную высоту из PenguinController
        Vector3 cameraWorldPos = penguin.transform.position +
                               new Vector3(0, penguin.fixedCameraHeight, 0) +
                               cameraOffset;

        penguin.playerCamera.transform.position = cameraWorldPos;

        // СМОТРИМ НА ПИНГВИНА
        Vector3 lookAtPoint = penguin.transform.position +
                            new Vector3(0, penguin.fixedCameraHeight * 0.8f, 0);

        penguin.playerCamera.transform.LookAt(lookAtPoint);
    }

    protected virtual IEnumerator InteractionRoutine()
    {
        Debug.Log("Начало взаимодействия");

        EnergyManager.Instance.StartGainingAttention(attentionGainRate);

        while (isInteracting)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Выход по Escape");
                break;
            }

            if (EnergyManager.Instance.GetCurrentAttention() >= EnergyManager.Instance.maxAttention)
            {
                Debug.Log("Внимание полностью восстановлено");
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
        Debug.Log("Завершение взаимодействия");

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
        // Не используется
    }

    public override void EndMiniGame()
    {
        EndInteractionImmediately();
    }
}