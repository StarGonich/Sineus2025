using UnityEngine;

public class RoomObject : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null)
        {
            // ������� � �������� ������ - �������������� � ������
            Vector3 direction = Camera.main.transform.position - transform.position;
            direction.y = 0; // ���������� ������������ ������������

            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}