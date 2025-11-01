using UnityEngine;

public class CameraCollisionHandler : MonoBehaviour
{
    [Header("Collision Settings")]
    public float minDistance = 0.5f;
    public float maxDistance = 10f; // ��������� ���������
    public float smoothness = 5f; // ��������� ���������
    public LayerMask collisionMask = ~0;

    private Vector3 dollyDir;
    private float distance;
    private float currentDistance;
    private Vector3 desiredCameraPos;

    void Awake()
    {
        dollyDir = transform.localPosition.normalized;
        distance = transform.localPosition.magnitude;
        currentDistance = distance;
    }

    void LateUpdate()
    {
        HandleCameraCollision();
    }

    void HandleCameraCollision()
    {
        // �������� ������� ������
        desiredCameraPos = transform.parent.TransformPoint(dollyDir * maxDistance);

        RaycastHit hit;
        float targetDistance = maxDistance;

        // ��������� ������������ � ����� ������� ��������
        if (Physics.SphereCast(transform.parent.position, 0.2f,
            (desiredCameraPos - transform.parent.position).normalized,
            out hit, maxDistance, collisionMask))
        {
            targetDistance = Mathf.Clamp(hit.distance * 0.9f, minDistance, maxDistance);
        }

        // ������� ��������� ���������
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, smoothness * Time.deltaTime);

        // ��������� ����� �������
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            dollyDir * currentDistance,
            smoothness * Time.deltaTime
        );
    }
}