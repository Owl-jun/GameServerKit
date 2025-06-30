using UnityEngine;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main == null) return;

        Vector3 dir = transform.position - Camera.main.transform.position;
        dir.y = 0; // �� ���� ȸ�� ����, Y�� ���� (�ؽ�Ʈ ��Ʋ�� ����)
        transform.rotation = Quaternion.LookRotation(dir);
    }
}
