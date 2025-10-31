using UnityEngine;

public enum KeyNote { A, S, D, F }

public class PianoKey : MonoBehaviour
{
    [Header("Key Settings")]
    public KeyNote keyNote;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private KeyCode inputKey;
    private Color pressedColor = Color.yellow;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        SetInputKey();
    }

    void SetInputKey()
    {
        switch (keyNote)
        {
            case KeyNote.A: inputKey = KeyCode.A; break;
            case KeyNote.S: inputKey = KeyCode.S; break;
            case KeyNote.D: inputKey = KeyCode.D; break;
            case KeyNote.F: inputKey = KeyCode.F; break;
        }
    }

    void Update()
    {
        // ѕодсветка при нажатии клавиши
        if (Input.GetKeyDown(inputKey))
        {
            PressKey();
        }

        if (Input.GetKeyUp(inputKey))
        {
            ReleaseKey();
        }
    }

    public void PressKey()
    {
        spriteRenderer.color = pressedColor;

        // —ообщаем мини-игре о нажатии
        if (PianoMiniGame2D.Instance != null)
        {
            PianoMiniGame2D.Instance.OnKeyPressed(keyNote);
        }

        // ¬оспроизводим звук (если есть)
        PlayKeySound();
    }

    public void ReleaseKey()
    {
        spriteRenderer.color = originalColor;
    }

    void PlayKeySound()
    {
        // «десь можно добавить воспроизведение звука клавиши
        // AudioSource.PlayClipAtPoint(keySound, transform.position);
    }

    // ћетод дл€ внешнего нажати€ (например, из UI)
    public void OnMouseDown()
    {
        PressKey();
    }

    public void OnMouseUp()
    {
        ReleaseKey();
    }
}