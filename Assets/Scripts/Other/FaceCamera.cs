using UnityEngine;

/// <summary>
/// 让物体始终面向主摄像机（用户正脸方向）。
/// 挂在 TextObject 根节点上即可。
/// </summary>
public class FaceCamera : MonoBehaviour
{
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        // 只绕 Y 轴旋转，保持水平不倾斜
        Vector3 dir = transform.position - cam.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);
    }
}
