using UnityEngine;
namespace NexoAvance {
[DefaultExecutionOrder(110)]
public class ReferenceBackdrop:MonoBehaviour {
    public Camera worldCamera;
    public SpriteRenderer[] panels;
    public float left=-5,right=200.95f;
    // The artwork is flattened. Every panel stays fixed in world space.
    void LateUpdate(){
        if(!worldCamera)return;
        float half=worldCamera.orthographicSize*worldCamera.aspect;
        var pos=worldCamera.transform.position;
        if(right-left>half*2)pos.x=Mathf.Clamp(pos.x,left+half,right-half);
        worldCamera.transform.position=pos;
    }
}
}
