using UnityEngine;

public enum PianoKeyNote { A, S, D, F }

public class Note2D : MonoBehaviour
{
    [Header("Note Settings")]
    public KeyNote targetNote;
    public float speed = 3f;
    public float hitLineY = -3f;

    private bool canBeHit = true;
    private bool wasHit = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetColorByNote();
    }

    void Update()
    {
        if (!wasHit)
        {
            // ƒвигаем ноту вниз
            transform.Translate(Vector3.down * speed * Time.deltaTime);

            // ѕровер€ем, прошла ли нота линию попадани€
            if (transform.position.y <= hitLineY && canBeHit)
            {
                MissNote();
            }
        }
    }

    void SetColorByNote()
    {
        if (spriteRenderer == null) return;

        switch (targetNote)
        {
            case KeyNote.A: spriteRenderer.color = Color.red; break;
            case KeyNote.S: spriteRenderer.color = Color.green; break;
            case KeyNote.D: spriteRenderer.color = Color.blue; break;
            case KeyNote.F: spriteRenderer.color = Color.yellow; break;
        }
    }

    public bool CanBeHit()
    {
        return canBeHit && Mathf.Abs(transform.position.y - hitLineY) < 0.5f;
    }

    public void Hit()
    {
        canBeHit = false;
        wasHit = true;

        // Ёффект попадани€
        spriteRenderer.color = Color.white;
        transform.localScale *= 1.3f;

        Destroy(gameObject, 0.2f);
    }

    void MissNote()
    {
        canBeHit = false;
        wasHit = false;

        // Ёффект промаха
        spriteRenderer.color = Color.gray;

        Destroy(gameObject, 1f);
    }

    void OnDrawGizmos()
    {
        // –исуем линию попадани€
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(-5, hitLineY, 0), new Vector3(5, hitLineY, 0));
    }
}