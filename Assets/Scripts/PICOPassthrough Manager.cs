// using UnityEngine;
// using Unity.XR.OpenXR.Features.PICOSupport;

// public class PICOPassthroughManager : MonoBehaviour
// {
//     // 开启透视 
//     private void Awake() 
//     { 
//         PassthroughFeature.EnableSeeThroughManual(true); 
//     } 
    
//     // 应用恢复后，再次开启透视 
//     private void OnApplicationPause(bool pause) 
//     { 
//         if (pause)
//         {
//             PassthroughFeature.EnableSeeThroughManual(false);
//         }
//         else
//         {
//             PassthroughFeature.EnableSeeThroughManual(true);
//         }
//     } 
// }


// // using System.Collections;
// // using System.Collections.Generic;
// // using UnityEngine;
// // using Unity.XR.PXR;

// // public class PICOPassthroughManager : MonoBehaviour
// // {
// //     void Start()
// //     {
// //         PXR_Manager.EnableVideoSeeThrough = true;

// //     }
// //     private void OnApplicationPause(bool pause)
// //     {
// //         PXR_Manager.EnableVideoSeeThrough = !pause;

// //     }
// //     void Update()
// //     {

// //     }
// // }