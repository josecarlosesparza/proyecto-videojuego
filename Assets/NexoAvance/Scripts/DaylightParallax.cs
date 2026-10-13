using UnityEngine;
namespace NexoAvance {
// Runs after AvanceView so a layer always uses the final camera position of this frame.
[DefaultExecutionOrder(100)]
public class DaylightParallax : MonoBehaviour {
    public Camera targetCamera;
    [Range(0,1)] public float followFraction;
    public float referenceCameraX=8;
    Vector3 origin;
    void Awake(){origin=transform.position;}
    void LateUpdate(){if(targetCamera)transform.position=origin+Vector3.right*(targetCamera.transform.position.x-referenceCameraX)*followFraction;}
}
}
