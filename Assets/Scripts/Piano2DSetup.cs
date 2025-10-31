using UnityEngine;

public class Piano2DSetup : MonoBehaviour
{
    [Header("Piano Settings")]
    public int keyCount = 4;
    public float keyWidth = 1.6f;
    public float keyHeight = 3f;
    public float spacing = 0.2f;

    [Header("Key Colors")]
    public Color keyColorA = Color.red;
    public Color keyColorS = Color.green;
    public Color keyColorD = Color.blue;
    public Color keyColorF = Color.yellow;

    [Header("Key Positions")]
    public float startY = -3f;

    void Start()
    {
        CreatePianoKeys();
        Debug.Log("������� �������!");
    }

    void CreatePianoKeys()
    {
        // ������� ������ ������� ���� ����
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }

        // ������������ �������
        float totalWidth = (keyCount * keyWidth) + ((keyCount - 1) * spacing);
        float startX = -totalWidth / 2 + keyWidth / 2;

        // ������� �������
        for (int i = 0; i < keyCount; i++)
        {
            CreateKey(i, startX + i * (keyWidth + spacing));
        }
    }

    void CreateKey(int index, float xPos)
    {
        // ������� ������ �������
        GameObject key = new GameObject($"Key_{(PianoKeyNote)index}");
        key.transform.SetParent(transform);
        key.transform.localPosition = new Vector3(xPos, startY, 0);

        // ��������� SpriteRenderer
        SpriteRenderer sr = key.AddComponent<SpriteRenderer>();
        sr.sprite = CreateKeySprite();

        // ��������� ����
        Color keyColor = GetKeyColor(index);
        sr.color = keyColor;

        // ��������� ���������
        BoxCollider2D collider = key.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(keyWidth, keyHeight);

        // ��������� ������ �������
        PianoKey pianoKey = key.AddComponent<PianoKey>();
        pianoKey.keyNote = (KeyNote)(PianoKeyNote)index;

        // ��������� ����� � ��������� �������
        CreateKeyLabel(key, index, keyColor);

        Debug.Log($"������� �������: {(PianoKeyNote)index} �� ������� {xPos}");
    }

    Sprite CreateKeySprite()
    {
        // ������� ������� ����� ������
        return Sprite.Create(
            new Texture2D(1, 1),
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f)
        );
    }

    Color GetKeyColor(int index)
    {
        switch (index)
        {
            case 0: return keyColorA;
            case 1: return keyColorS;
            case 2: return keyColorD;
            case 3: return keyColorF;
            default: return Color.white;
        }
    }

    void CreateKeyLabel(GameObject parent, int keyIndex, Color backgroundColor)
    {
        GameObject label = new GameObject("KeyLabel");
        label.transform.SetParent(parent.transform);
        label.transform.localPosition = new Vector3(0, -keyHeight / 2 - 0.5f, 0);

        TextMesh textMesh = label.AddComponent<TextMesh>();
        textMesh.text = GetKeyName(keyIndex);
        textMesh.fontSize = 20;
        textMesh.characterSize = 0.1f;
        textMesh.anchor = TextAnchor.UpperCenter;
        textMesh.color = Color.black; // ������ ����� �� ������� ����
    }

    string GetKeyName(int index)
    {
        switch (index)
        {
            case 0: return "A";
            case 1: return "S";
            case 2: return "D";
            case 3: return "F";
            default: return "?";
        }
    }
}