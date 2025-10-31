using UnityEngine;

public class PenguinController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 50f;
    public float rotationSpeed = 10f;
    public float cameraRotationSpeed = 2f;
    public float stoppingPower = 10f; // Сила остановки

    [Header("References")]
    public Transform cameraPivot;
    public Camera playerCamera;
    public Transform pianoSpot;

    private Rigidbody rb;
    private Vector3 movement;
    private float mouseX;
    private float mouseY;
    private float xRotation = 0f;
    private float yRotation = 0f;

    private bool isMoving = false;
    private bool isInteracting = false;
    private Interactable currentInteractable;

    [Header("Camera Settings")]
    public float cameraDistance = 20f;
    public float minCameraAngle = -30f;
    public float maxCameraAngle = 60f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Настраиваем Rigidbody для предотвращения скольжения
        rb.drag = 5f; // Сопротивление движению
        rb.angularDrag = 5f; // Сопротивление вращению

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
        pivotObj.transform.localPosition = new Vector3(0, 1.5f, 0);
        cameraPivot = pivotObj.transform;

        // Создаем камеру
        GameObject cameraObj = new GameObject("PlayerCamera");
        cameraObj.transform.SetParent(cameraPivot);
        cameraObj.transform.localPosition = new Vector3(0, 0, -cameraDistance);
        playerCamera = cameraObj.AddComponent<Camera>();
        playerCamera.tag = "MainCamera";

        // Добавляем обработчик столкновений камеры
        cameraObj.AddComponent<CameraCollisionHandler>();

        Debug.Log("Автоматически создана система камеры");
    }

    void Update()
    {
        if (!isInteracting)
        {
            HandleCameraInput();
            HandleMovementInput();
        }

        HandleInteraction();

        // Обработка разблокировки курсора
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleCursorLock();
        }
    }

    void FixedUpdate()
    {
        if (!isInteracting)
        {
            if (isMoving)
            {
                MoveCharacter();
            }
            else
            {
                // Принудительная остановка когда не движемся
                StopMovement();
            }
        }
    }

    void HandleCameraInput()
    {
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

        // Игнорируем наклон камеры по Y для движения
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Рассчитываем движение относительно камеры
        movement = (cameraForward * vertical + cameraRight * horizontal).normalized;

        isMoving = movement.magnitude > 0.1f;
    }

    void MoveCharacter()
    {
        // Движение Rigidbody с помощью AddForce для лучшего контроля
        Vector3 targetVelocity = movement * moveSpeed;
        Vector3 velocityChange = targetVelocity - rb.velocity;
        velocityChange.y = 0; // Не меняем вертикальную скорость

        // Ограничиваем изменение скорости для плавности
        velocityChange = Vector3.ClampMagnitude(velocityChange, moveSpeed);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    void StopMovement()
    {
        // Быстрая остановка при отпускании клавиш
        if (rb.velocity.magnitude > 0.1f)
        {
            Vector3 stoppingForce = -rb.velocity * stoppingPower;
            stoppingForce.y = 0; // Не влияем на вертикальную скорость
            rb.AddForce(stoppingForce, ForceMode.Acceleration);
        }
        else
        {
            // Полная остановка если скорость очень мала
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
    }

    void StartInteraction(Interactable interactable)
    {
        isInteracting = true;
        movement = Vector3.zero;
        isMoving = false;

        // Останавливаем движение при начале взаимодействия
        rb.velocity = new Vector3(0, rb.velocity.y, 0);
        rb.angularVelocity = Vector3.zero;

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
            // Останавливаем движение перед телепортацией
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Поворачиваемся к пианино
            Vector3 direction = (pianoSpot.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
                // Синхронизируем вращение камеры
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