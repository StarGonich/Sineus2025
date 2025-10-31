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
        // ��������� ��� ������� �������
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

        // �������� ����-���� � �������
        if (PianoMiniGame2D.Instance != null)
        {
            PianoMiniGame2D.Instance.OnKeyPressed(keyNote);
        }

        // ������������� ���� (���� ����)
        PlayKeySound();
    }

    public void ReleaseKey()
    {
        spriteRenderer.color = originalColor;
    }

    void PlayKeySound()
    {
        // ����� ����� �������� ��������������� ����� �������
        // AudioSource.PlayClipAtPoint(keySound, transform.position);
    }

    // ����� ��� �������� ������� (��������, �� UI)
    public void OnMouseDown()
    {
        PressKey();
    }

    public void OnMouseUp()
    {
        ReleaseKey();
    }
}