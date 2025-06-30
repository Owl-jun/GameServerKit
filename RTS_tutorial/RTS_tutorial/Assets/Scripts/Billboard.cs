using UnityEngine;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main == null) return;

        Vector3 dir = transform.position - Camera.main.transform.position;
        dir.y = 0; // ← 수직 회전 막고, Y축 고정 (텍스트 비틀림 방지)
        transform.rotation = Quaternion.LookRotation(dir);
    }
}
