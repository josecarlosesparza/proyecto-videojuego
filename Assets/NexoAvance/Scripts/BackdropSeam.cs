using UnityEngine;
namespace NexoAvance {
// The curve selects between two overlapping originals. It never synthesizes scenery.
[ExecuteAlways]
public class BackdropSeam:MonoBehaviour {
    public float[] columns;
    void OnEnable(){Apply();}
    void OnValidate(){Apply();}
    void Apply(){if(columns==null||columns.Length!=724)return;var renderer=GetComponent<SpriteRenderer>();if(!renderer)return;var block=new MaterialPropertyBlock();renderer.GetPropertyBlock(block);block.SetFloatArray("_SeamPath",columns);block.SetFloat("_HasSeam",1);renderer.SetPropertyBlock(block);}
}
}
