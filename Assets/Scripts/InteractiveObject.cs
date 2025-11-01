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
            Debug.Log("Ñëèøêîì óñòàë äëÿ ıòîãî!");
            return;
        }

        base.Interact();
        StartInteraction();
    }

    protected virtual void StartInteraction()
    {
        penguin = PenguinController.Instance;
        if (penguin == null) return;

        // ÑĞÀÇÓ ÔÈÊÑÈĞÓÅÌ ÏÅĞÑÎÍÀÆÀ È ÊÀÌÅĞÓ
        penguin.FixMovement(true);
        penguin.FixCamera(true);

        // ÑÎÕĞÀÍßÅÌ ÎĞÈÃÈÍÀËÜÍÓŞ ÏÎÇÈÖÈŞ
        originalPosition = penguin.transform.position;
        originalRotation = penguin.transform.rotation;

        // ÒÅËÅÏÎĞÒÈĞÓÅÌ ÏÈÍÃÂÈÍÀ Â ÒÎ×ÊÓ ÀÍÈÌÀÖÈÈ
        if (animationSpot != null)
        {
            penguin.transform.position = animationSpot.position;
            penguin.transform.rotation = animationSpot.rotation;
        }

        // ÍÀÑÒĞÀÈÂÀÅÌ ÊÀÌÅĞÓ
        SetupFixedCamera();

        // ÏÎËÓ×ÀÅÌ ÀÍÈÌÀÒÎĞ È ÇÀÏÓÑÊÀÅÌ ÀÍÈÌÀÖÈŞ ÑĞÀÇÓ
        penguinAnimator = penguin.GetComponent<Animator>();
        if (penguinAnimator != null)
        {
            PlayLoopingAnimation();
        }
        else
        {
            Debug.LogError("Animator íå íàéäåí íà ïèíãâèíå!");
        }

        // ÇÀÏÓÑÊÀÅÌ ÏĞÎÖÅÑÑ ÂÇÀÈÌÎÄÅÉÑÒÂÈß
        StartCoroutine(InteractionRoutine());
    }

    protected virtual void SetupFixedCamera()
    {
        if (penguin.playerCamera == null) return;

        // ÏĞÎÑÒÀß ÔÈÊÑÈĞÎÂÀÍÍÀß ÊÀÌÅĞÀ ÑÏÅĞÅÄÈ ÎÒ ÏÈÍÃÂÈÍÀ
        Vector3 cameraOffset = -penguin.transform.forward * 3f + Vector3.up * 1.5f;
        penguin.playerCamera.transform.position = penguin.transform.position + cameraOffset;
        penguin.playerCamera.transform.LookAt(penguin.transform.position + Vector3.up * 1f);
    }

    protected virtual IEnumerator InteractionRoutine()
    {
        Debug.Log("Íà÷àëî âçàèìîäåéñòâèÿ");

        // ÍÀ×ÈÍÀÅÌ ÂÎÑÑÒÀÍÀÂËÈÂÀÒÜ ÂÍÈÌÀÍÈÅ
        EnergyManager.Instance.StartGainingAttention(attentionGainRate);

        // ÎÑÍÎÂÍÎÉ ÖÈÊË ÂÇÀÈÌÎÄÅÉÑÒÂÈß
        while (isInteracting)
        {
            // ÏĞÎÂÅĞßÅÌ ÂÛÕÎÄ ÏÎ ESCAPE
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Âûõîä ïî Escape");
                break;
            }

            // ÏĞÎÂÅĞßÅÌ ÏÎËÍÎÅ ÂÎÑÑÒÀÍÎÂËÅÍÈÅ ÂÍÈÌÀÍÈß
            if (EnergyManager.Instance.GetCurrentAttention() >= EnergyManager.Instance.maxAttention)
            {
                Debug.Log("Âíèìàíèå ïîëíîñòüş âîññòàíîâëåíî");
                break;
            }

            yield return null;
        }

        // ÌÃÍÎÂÅÍÍÎÅ ÇÀÂÅĞØÅÍÈÅ
        EndInteraction();
    }

    protected virtual void PlayLoopingAnimation()
    {
        if (penguinAnimator != null && !string.IsNullOrEmpty(animationTrigger))
        {
            penguinAnimator.SetBool(animationTrigger, true);
            Debug.Log($"Àíèìàöèÿ çàïóùåíà: {animationTrigger}");
        }
    }

    protected virtual void StopAnimationImmediately()
    {
        if (penguinAnimator != null && !string.IsNullOrEmpty(animationTrigger))
        {
            penguinAnimator.SetBool(animationTrigger, false);
            Debug.Log($"Àíèìàöèÿ îñòàíîâëåíà: {animationTrigger}");
        }
    }

    protected virtual void EndInteraction()
    {
        Debug.Log("Ìãíîâåííîå çàâåğøåíèå âçàèìîäåéñòâèÿ");

        // 1. ÑÍÀ×ÀËÀ ÎÑÒÀÍÀÂËÈÂÀÅÌ ÀÍÈÌÀÖÈŞ
        StopAnimationImmediately();

        // 2. ÎÑÒÀÍÀÂËÈÂÀÅÌ ÂÎÑÑÒÀÍÎÂËÅÍÈÅ ÂÍÈÌÀÍÈß
        EnergyManager.Instance.StopGainingAttention();

        // 3. ÂÎÇÂĞÀÙÀÅÌ ÏÈÍÃÂÈÍÀ
        penguin.transform.position = originalPosition;
        penguin.transform.rotation = originalRotation;

        // 4. ĞÀÇÁËÎÊÈĞÓÅÌ ÓÏĞÀÂËÅÍÈÅ (ÊÀÌÅĞÀ ÀÂÒÎÌÀÒÈ×ÅÑÊÈ ÂÅĞÍÅÒÑß)
        penguin.FixMovement(false);
        penguin.FixCamera(false);

        // 5. ÇÀÂÅĞØÀÅÌ ÂÇÀÈÌÎÄÅÉÑÒÂÈÅ
        StopInteracting();

        Debug.Log("Âçàèìîäåéñòâèå ïîëíîñòüş çàâåğøåíî");
    }

    public override void StartMiniGame()
    {
        // Íå èñïîëüçóåòñÿ
    }

    public override void EndMiniGame()
    {
        EndInteraction();
    }
}