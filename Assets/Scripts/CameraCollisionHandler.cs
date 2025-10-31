using UnityEngine;

public class CameraCollisionHandler : MonoBehaviour
{
    [Header("Collision Settings")]
    public float minDistance = 1f;
    public float maxDistance = 20f;
    public float smoothness = 10f;
    public LayerMask collisionMask = ~0;

    private Vector3 dollyDir;
    private float distance;

    void Awake()
    {
        dollyDir = transform.localPosition.normalized;
        distance = transform.localPosition.magnitude;
    }

    void Update()
    {
        HandleCameraCollision();
    }

    void HandleCameraCollision()
    {
        Vector3 desiredCameraPos = transform.parent.TransformPoint(dollyDir * maxDistance);
        RaycastHit hit;

        if (Physics.Linecast(transform.parent.position, desiredCameraPos, out hit, collisionMask))
        {
            distance = Mathf.Clamp(hit.distance * 0.8f, minDistance, maxDistance);
        }
        else
        {
            distance = maxDistance;
        }

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            dollyDir * distance,
            smoothness * Time.deltaTime
        );
    }
}