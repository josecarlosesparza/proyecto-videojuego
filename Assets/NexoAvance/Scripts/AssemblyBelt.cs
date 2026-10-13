using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace NexoAvance {
public class AssemblyBelt:MonoBehaviour {
    public Transform[] marks;public float width=6;float phase;
    void Update(){phase=Mathf.Repeat(phase+Time.deltaTime*1.4f,width);for(int i=0;i<marks.Length;i++){var p=marks[i].localPosition;p.x=Mathf.Repeat(i*width/marks.Length+phase,width)-width/2;marks[i].localPosition=p;}}
}

}