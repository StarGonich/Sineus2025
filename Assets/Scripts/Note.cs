using UnityEngine;

public class Note : MonoBehaviour
{
    [Header("Note Settings")]
    public KeyCode targetKey;
    public float hitZoneRadius = 0.5f;

    private Vector3 targetPosition;
    private float speed;
    private bool canBeHit = true;
    private bool wasHit = false;

    public void SetupNote(Vector3 targetPos, float noteSpeed, KeyCode key)
    {
        targetPosition = targetPos;
        speed = noteSpeed;
        targetKey = key;
    }

    void Update()
    {
        if (!wasHit)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                targetPosition, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f && canBeHit)
            {
                MissNote();
            }
        }
    }

    public bool IsInHitZone()
    {
        return canBeHit && Vector3.Distance(transform.position, targetPosition) < hitZoneRadius;
    }

    public void PlayHit()
    {
        canBeHit = false;
        wasHit = true;
        GetComponent<Renderer>().material.color = Color.green;
        Destroy(gameObject, 0.5f);
    }

    void MissNote()
    {
        canBeHit = false;
        wasHit = false;
        GetComponent<Renderer>().material.color = Color.red;
        Destroy(gameObject, 1f);
    }

    public KeyCode GetTargetKey()
    {
        return targetKey;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetPosition, hitZoneRadius);
    }
}