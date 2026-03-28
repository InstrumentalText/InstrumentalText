using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class SpatialAnchorPlacer : MonoBehaviour
{
    [Tooltip("需要添加 Spatial Anchor 的物体列表")]
    public List<GameObject> objectsToAnchor = new List<GameObject>();

    ARAnchorManager m_AnchorManager;

    async void Start()
    {
        m_AnchorManager = FindAnyObjectByType<ARAnchorManager>();
        if (m_AnchorManager == null)
        {
            Debug.LogError("[SpatialAnchorPlacer] 场景中没有找到 ARAnchorManager!");
            return;
        }

        // 等待 AR Session 完全初始化
        while (ARSession.state < ARSessionState.SessionTracking)
        {
            await Awaitable.NextFrameAsync();
        }

        foreach (var obj in objectsToAnchor)
        {
            if (obj == null) continue;

            var pose = new Pose(obj.transform.position, obj.transform.rotation);
            var result = await m_AnchorManager.TryAddAnchorAsync(pose);

            if (result.status.IsSuccess())
            {
                var anchor = result.value;
                Debug.Log($"[SpatialAnchorPlacer] {obj.name} 创建前 worldPos={obj.transform.position} anchorPos={anchor.transform.position}");

                obj.transform.SetParent(anchor.transform, true);

                // 将物体本地坐标归零，确保物体精确锁定在 anchor 位置
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;

                Debug.Log($"[SpatialAnchorPlacer] {obj.name} 创建后 worldPos={obj.transform.position} localPos={obj.transform.localPosition}");
            }
            else
            {
                Debug.LogWarning($"[SpatialAnchorPlacer] 为 {obj.name} 创建 Anchor 失败: {result.status}");
            }
        }
    }
}
