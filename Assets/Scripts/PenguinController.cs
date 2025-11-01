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
    public float cameraRotationSpeed = 2f;
    public float cameraDistance = 5f;
    public float minCameraAngle = -30f;
    public float maxCameraAngle = 60f;
 
    
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
        animator = GetComponent<Animator>(); // ПОЛУЧАЕМ АНИМАТОР

        // Настройка Rigidbody
        rb.drag = 0f;
        rb.angularDrag = 0f;

        // Автоматическое создание камеры если не назначена
        if (cameraPivot == null)
        {
            CreateCameraRig();
        }

        // Скрываем и фиксируем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void CreateCameraRig()
    {
        // Создаем pivot для камеры
        GameObject pivotObj = new GameObject("CameraPivot");
        pivotObj.transform.SetParent(transform);
        pivotObj.transform.localPosition = new Vector3(0, 0.5f, 0);
        cameraPivot = pivotObj.transform;

        // Создаем камеру
        GameObject cameraObj = new GameObject("PlayerCamera");
        cameraObj.transform.SetParent(cameraPivot);
        cameraObj.transform.localPosition = new Vector3(0, 0, -cameraDistance);
        playerCamera = cameraObj.AddComponent<Camera>();
        playerCamera.tag = "MainCamera";

        // Добавляем обработчик столкновений камеры
        cameraObj.AddComponent<CameraCollisionHandler>();
    }

    void Update()
    {
        if (!isInteracting)
        {
            HandleCameraInput();
            HandleMovementInput();
            HandleRunningInput();
        }

        HandleInteraction();
        UpdateAnimation(); // ОБНОВЛЯЕМ АНИМАЦИЮ
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

    private bool isCameraFixed = false;

    public void FixCamera(bool fixedState)
    {
        isCameraFixed = fixedState;

        if (!fixedState)
        {
            // ПРИ РАЗБЛОКИРОВКЕ КАМЕРЫ СБРАСЫВАЕМ ВРАЩЕНИЕ КАМЕРЫ К ЗНАЧЕНИЯМ ПИНГВИНА
            if (playerCamera != null && cameraPivot != null)
            {
                // Синхронизируем вращение камеры с вращением пингвина
                yRotation = transform.eulerAngles.y;
                xRotation = 0f; // Сбрасываем вертикальный наклон

                cameraPivot.rotation = Quaternion.Euler(xRotation, yRotation, 0);
                playerCamera.transform.localPosition = new Vector3(0, 0, -cameraDistance);

                Debug.Log("Камера восстановлена к настройкам пингвина");
            }
        }
    }

    void HandleCameraInput()
    {
        if (isCameraFixed || isInteracting) return;

        if (cameraPivot == null) return;

        // Вращение камеры мышью
        mouseX = Input.GetAxis("Mouse X") * cameraRotationSpeed;
        mouseY = Input.GetAxis("Mouse Y") * cameraRotationSpeed;

        // Накопление вращения
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minCameraAngle, maxCameraAngle);

        // Применяем вращение к камере
        cameraPivot.rotation = Quaternion.Euler(xRotation, yRotation, 0);

        // Персонаж всегда поворачивается в направлении камеры (только по Y)
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
        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null && !isInteracting)
        {
            StartInteraction(currentInteractable);
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isInteracting)
        {
            StopInteraction();
        }

        // Разблокировка курсора по Tab
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleCursorLock();
        }
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