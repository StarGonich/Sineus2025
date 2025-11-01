using UnityEngine;

public class BaseInteractable : Interactable
{
    [Header("Base Settings")]
    public string itemName = "������";
    public Sprite itemSprite;

    protected EnergyManager energyManager;

    void Start()
    {
        energyManager = EnergyManager.Instance;
        interactionName = itemName;

        // �������������� �������� ��������� ���� �� ���������
        if (interactionPrompt == null)
        {
            CreateInteractionPrompt();
        }
    }

    void CreateInteractionPrompt()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        GameObject promptObj = new GameObject($"{itemName}Prompt");
        promptObj.transform.SetParent(canvas.transform);

        // ��������� RectTransform
        RectTransform rt = promptObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(300, 40);
        rt.anchoredPosition = new Vector2(0, 100);

        // ��������� Text
        UnityEngine.UI.Text text = promptObj.AddComponent<UnityEngine.UI.Text>();
        text.text = $"������� E ����� ����������������� � {itemName}";
        text.fontSize = 20;
        text.color = Color.yellow;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        interactionPrompt = promptObj;
        interactionPrompt.SetActive(false);
    }

    public override void StartMiniGame()
    {
        // ������� ���������� - ����� �������������� � �������� �������
        Debug.Log($"������ �������������� � {itemName}");
    }

    public override void EndMiniGame()
    {
        Debug.Log($"��������� �������������� � {itemName}");
        StopInteracting();
    }

    void Update()
    {
        if (isInteracting && Input.GetKeyDown(KeyCode.Escape))
        {
            EndMiniGame();
        }
    }

    void OnDestroy()
    {
        if (interactionPrompt != null)
        {
            Destroy(interactionPrompt);
        }
    }
}