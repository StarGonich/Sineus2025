using UnityEngine;

public class BaseInteractable : Interactable
{
    [Header("Base Settings")]
    public string itemName = "Объект";
    public Sprite itemSprite;

    protected EnergyManager energyManager;

    void Start()
    {
        energyManager = EnergyManager.Instance;
        interactionName = itemName;

        // Автоматическое создание подсказки если не назначена
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

        // Настройка RectTransform
        RectTransform rt = promptObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(300, 40);
        rt.anchoredPosition = new Vector2(0, 100);

        // Настройка Text
        UnityEngine.UI.Text text = promptObj.AddComponent<UnityEngine.UI.Text>();
        text.text = $"Нажмите E чтобы взаимодействовать с {itemName}";
        text.fontSize = 20;
        text.color = Color.yellow;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        interactionPrompt = promptObj;
        interactionPrompt.SetActive(false);
    }

    public override void StartMiniGame()
    {
        // Базовая реализация - можно переопределить в дочерних классах
        Debug.Log($"Начато взаимодействие с {itemName}");
    }

    public override void EndMiniGame()
    {
        Debug.Log($"Завершено взаимодействие с {itemName}");
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