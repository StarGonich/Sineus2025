using UnityEngine;

public class RoomObject : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null)
        {
            // Простой и надежный способ - поворачиваемся к камере
            Vector3 direction = Camera.main.transform.position - transform.position;
            direction.y = 0; // Игнорируем вертикальную составляющую

            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}