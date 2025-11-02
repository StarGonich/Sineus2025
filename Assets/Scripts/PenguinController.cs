using UnityEngine;

public class PenguinController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 15f;
    public float runSpeed = 15f;
    public float rotationSpeed = 15f;
    public float acceleration = 10f;
    public float deceleration = 15f;

    [Header("Camera Settings")]
    public Transform cameraPivot;
    public Camera playerCamera;
    public float cameraRotationSpeed = 3f;
    public float cameraDistance = 10f;
    public float minCameraAngle = -30f;
    public float maxCameraAngle = 60f;
    public float cameraSmoothness = 3f; // ДОБАВИЛИ ПЛАВНОСТЬ
    public float fixedCameraHeight = 1.5f; // ФИКСИРОВАННАЯ ВЫСОТА

    [Header("References")]
    public Transform pianoSpot;

    private Rigidbody rb;
    private Vector3 movement;
    private Vector3 currentVelocity;
    private float mouseX;
    private float mouseY;
    private float xRotation = 0f;
    private float yRotation = 0f;

    private bool isMoving = false;
    private bool isInteracting = false;
    private Interactable currentInteractable;
    private bool isRunning = false;

    private Vector3 targetCameraLocalPos;
    private bool isCameraFixed = false;

    public static PenguinController Instance;

    private Animator animator;
    private float currentSpeed = 0f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // Начальная позиция камеры
        targetCameraLocalPos = new Vector3(0, 0, -cameraDistance);

        if (cameraPivot == null)
        {
            CreateCameraRig();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void CreateCameraRig()
    {
        // Создаем pivot для камеры
        GameObject pivotObj = new GameObject("CameraPivot");
        pivotObj.transform.SetParent(transform);
        pivotObj.transform.localPosition = new Vector3(0, fixedCameraHeight, 0); // ФИКСИРОВАННАЯ ВЫСОТА
        cameraPivot = pivotObj.transform;

        // Создаем камеру
        GameObject cameraObj = new GameObject("PlayerCamera");
        cameraObj.transform.SetParent(cameraPivot);
        cameraObj.transform.localPosition = new Vector3(0, 0, -cameraDistance);
        playerCamera = cameraObj.AddComponent<Camera>();
        playerCamera.tag = "MainCamera";

        // УБИРАЕМ CameraCollisionHandler чтобы не было тряски
        // cameraObj.AddComponent<CameraCollisionHandler>();

        Debug.Log("Камера создана с фиксированной высотой: " + fixedCameraHeight);
    }

    void Update()
    {
        // Проверяем, не открыто ли UI окно
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.IsUIOpen())
        {
            return; // Не обрабатываем ввод, если открыто UI
        }

        if (!isInteracting)
        {
            HandleCameraInput();
            HandleMovementInput();
            HandleRunningInput();
        }

        HandleInteraction();
        UpdateAnimation();
        UpdateCameraPosition();
    }

    // НОВЫЙ МЕТОД: ПЛАВНОЕ ОБНОВЛЕНИЕ ПОЗИЦИИ КАМЕРЫ
    void UpdateCameraPosition()
    {
        if (playerCamera != null && cameraPivot != null && !isCameraFixed)
        {
            // Плавное движение камеры к целевой позиции
            playerCamera.transform.localPosition = Vector3.Lerp(
                playerCamera.transform.localPosition,
                targetCameraLocalPos,
                cameraSmoothness * Time.deltaTime
            );
        }
    }

    void FixedUpdate()
    {
        if (!isInteracting)
        {
            MoveCharacter();
        }
    }

    // НОВЫЙ МЕТОД: ОБНОВЛЕНИЕ АНИМАЦИИ
    void UpdateAnimation()
    {
        if (animator == null) return;

        // РАСЧЕТ СКОРОСТИ ДЛЯ АНИМАЦИИ
        currentSpeed = rb.velocity.magnitude;

        // ПЕРЕДАЕМ ПАРАМЕТРЫ В АНИМАТОР
        animator.SetFloat("Speed", currentSpeed);

        // Отладочная информация
        if (currentSpeed > 0.1f)
        {
            Debug.Log($"Движение: Speed = {currentSpeed}, IsRunning = {isRunning}");
        }
    }

    public void FixMovement(bool fixedState)
    {
        isInteracting = fixedState;
        if (fixedState)
        {
            // МГНОВЕННАЯ ОСТАНОВКА
            movement = Vector3.zero;
            rb.velocity = Vector3.zero;
            isMoving = false;
        }
        // При false управление автоматически восстановится в Update
    }

    public void FixCamera(bool fixedState)
    {
        isCameraFixed = fixedState;

        if (!fixedState)
        {
            // ПРИ РАЗБЛОКИРОВКЕ ВОССТАНАВЛИВАЕМ НОРМАЛЬНЫЕ НАСТРОЙКИ
            if (playerCamera != null && cameraPivot != null)
            {
                // Сбрасываем камеру к нормальной позиции
                targetCameraLocalPos = new Vector3(0, 0, -cameraDistance);

                // Плавно возвращаем управление
                StartCoroutine(SmoothCameraReturn());
            }
        }
    }

    System.Collections.IEnumerator SmoothCameraReturn()
    {
        float timer = 0f;
        float returnTime = 0.5f;

        while (timer < returnTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }
    }

    // В PenguinController.cs добавляем:
    public void ResetCameraAfterInteraction()
    {
        if (cameraPivot != null && playerCamera != null)
        {
            // ВОЗВРАЩАЕМ К СТАНДАРТНЫМ НАСТРОЙКАМ
            cameraPivot.localPosition = new Vector3(0, fixedCameraHeight, 0);
            playerCamera.transform.localPosition = new Vector3(0, 0, -cameraDistance);
            cameraPivot.localRotation = Quaternion.Euler(0, yRotation, 0);

            Debug.Log("Камера восстановлена к фиксированной высоте: " + fixedCameraHeight);
        }
    }

    // Также обновляем существующий метод:
    public void ResetCameraToDefault()
    {
        ResetCameraAfterInteraction(); // Теперь используем один метод
    }

    public bool IsCameraFixed
    {
        get { return isCameraFixed; }
    }

    void HandleCameraInput()
    {
        if (isCameraFixed || isInteracting || cameraPivot == null) return;

        // ТОЛЬКО ГОРИЗОНТАЛЬНОЕ ВРАЩЕНИЕ
        mouseX = Input.GetAxis("Mouse X") * cameraRotationSpeed;
        // УБИРАЕМ mouseY - нет вертикального вращения

        // Накопление вращения (только по Y)
        yRotation += mouseX;

        // Применяем вращение к камере (только по Y)
        cameraPivot.rotation = Quaternion.Euler(0, yRotation, 0);

        // Персонаж всегда поворачивается в направлении камеры
        transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    void HandleMovementInput()
    {
        if (playerCamera == null) return;

        // Ввод с клавиатуры
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Получаем направление относительно камеры
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;

        // Игнорируем наклон камеры по Y
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Рассчитываем движение относительно камеры
        Vector3 targetMovement = (cameraForward * vertical + cameraRight * horizontal).normalized;

        // Плавное изменение скорости
        float targetSpeed = isRunning ? runSpeed : walkSpeed;
        movement = Vector3.SmoothDamp(movement, targetMovement * targetSpeed, ref currentVelocity,
                                    targetMovement.magnitude > 0.1f ? 1f / acceleration : 1f / deceleration);

        isMoving = movement.magnitude > 0.1f;
    }

    void HandleRunningInput()
    {
        // Бег по зажатому Shift
        isRunning = Input.GetKey(KeyCode.LeftShift);
    }

    void MoveCharacter()
    {
        if (isMoving)
        {
            // Движение с физикой но быстрым откликом
            rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);
        }
        else
        {
            // Быстрая остановка
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }

    void HandleInteraction()
    {
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.IsUIOpen())
        {
            return;
        }

        // Горячая клавиша для завершения дня (например, Backspace)
        if (Input.GetKeyDown(KeyCode.Backspace) && !isInteracting)
        {
            GameProgressManager.Instance.ShowEndDayPanel();
            return;
        }

        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null && !isInteracting)
        {

            StartInteraction(currentInteractable);
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isInteracting)
        {
            StopInteraction();
        }

        if (Input.GetKeyDown(KeyCode.Tab) && !GameProgressManager.Instance.IsUIOpen())
        {
            ToggleCursorLock();
        }
    }

    public void OnRespawn()
    {
        // Сбрасываем все состояния движения
        movement = Vector3.zero;
        currentVelocity = Vector3.zero;
        isMoving = false;
        isRunning = false;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Сбрасываем анимацию
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }

        // Восстанавливаем управление
        isInteracting = false;
        isCameraFixed = false;

        Debug.Log("PenguinController: состояние сброшено после респавна");
    }

    void StartInteraction(Interactable interactable)
    {
        isInteracting = true;
        movement = Vector3.zero;
        isMoving = false;
        rb.velocity = Vector3.zero;

        interactable.Interact();
    }

    public void StopInteraction()
    {
        isInteracting = false;
        if (currentInteractable != null)
        {
            currentInteractable.StopInteracting();
        }
    }

    void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Interactable interactable = other.GetComponent<Interactable>();
        if (interactable != null && !isInteracting)
        {
            currentInteractable = interactable;
            interactable.ShowPrompt();
        }
    }

    void OnTriggerExit(Collider other)
    {
        Interactable interactable = other.GetComponent<Interactable>();
        if (interactable == currentInteractable && !isInteracting)
        {
            currentInteractable.HidePrompt();
            currentInteractable = null;
        }
    }

    public void MoveToPiano()
    {
        if (pianoSpot != null)
        {
            Vector3 direction = (pianoSpot.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
                yRotation = transform.eulerAngles.y;
                if (cameraPivot != null)
                {
                    cameraPivot.rotation = Quaternion.Euler(xRotation, yRotation, 0);
                }
            }
            transform.position = pianoSpot.position;
            StopInteraction();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && !isInteracting)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public bool IsInteracting
    {
        get { return isInteracting; }
    }
}