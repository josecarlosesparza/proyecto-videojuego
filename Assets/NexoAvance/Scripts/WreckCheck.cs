using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
namespace NexoAvance {
public class WreckCheck : MonoBehaviour {
    string output; bool failed;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-wreckCheck")<0)return;var g=new GameObject("Validacion auto");DontDestroyOnLoad(g);g.AddComponent<WreckCheck>();}
    IEnumerator Start(){
        var args=Environment.GetCommandLineArgs();output=args[Array.IndexOf(args,"-nexoOutput")+1];File.WriteAllText(Path.Combine(output,"validacion.txt"),"");Application.runInBackground=true;
        yield return SceneManager.LoadSceneAsync("PrimerAvance");yield return null;
        var s=FindAnyObjectByType<CombatSession>();var p=s.player;var b=p.GetComponent<Rigidbody2D>();p.TestMode=true;p.SetPaused(false);s.enemy.AutoFire=false;
        b.position=new Vector2(4.3f,-.15f);p.transform.position=b.position;Physics2D.SyncTransforms();
        yield return new WaitForSeconds(.4f);
        Check(GameObject.Find("Plataforma 01")==null && GameObject.Find("Caja de suministros")==null,"Escenario sin plataformas ni caja");
        var car=GameObject.Find("Auto rojo destrozado");Check(car && car.GetComponents<Collider2D>().Length==2,"Auto con colision solida");
        Check(car.GetComponentInChildren<SmallCarFire>().GetComponentsInChildren<Collider2D>().Length==0,"Fuego decorativo sin colision");
        p.TestAxis=1;yield return new WaitForSeconds(.5f);p.TestAxis=0;
        Check(b.position.x<5.1f,"El auto bloquea el avance a pie");
        p.TestAxis=1;p.TestJump=true;yield return new WaitForSeconds(.76f);p.TestAxis=0;yield return new WaitForSeconds(.35f);
        Check(b.position.x>8.5f,"El salto actual permite cruzar el auto");
        Check(s.health.Current==100,"Cruzar el auto y el fuego no resta salud");
        Check(s.enemy.transform.position.x>car.transform.position.x+4,"Enemigo situado despues del auto");
        Check(s.enemy.HasLineOfSight(),"Hay linea de tiro tras saltar el auto");
        s.enemy.Fire();yield return new WaitForSeconds(.5f);Check(s.health.Current==85,"El enemigo puede herir al jugador en la zona de combate");
        s.health.ResetHealth();CombatBullet.ClearAll();b.position=new Vector2(4.3f,-.15f);p.transform.position=b.position;
        yield return new WaitForSeconds(.4f);Capture();
        File.AppendAllText(Path.Combine(output,"validacion.txt"),failed?"RESULTADO: ERROR":"RESULTADO: CORRECTO");Application.Quit(failed?1:0);
    }
    void Check(bool ok,string label){failed|=!ok;File.AppendAllText(Path.Combine(output,"validacion.txt"),(ok?"OK ":"ERROR ")+label+"\n");}
    void Capture(){Canvas.ForceUpdateCanvases();var r=new RenderTexture(1280,720,24);r.Create();RenderPipeline.SubmitRenderRequest(Camera.main,new RenderPipeline.StandardRequest{destination=r});var old=RenderTexture.active;RenderTexture.active=r;var t=new Texture2D(1280,720,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();File.WriteAllBytes(Path.Combine(output,"auto-rojo.png"),t.EncodeToPNG());RenderTexture.active=old;r.Release();Destroy(r);Destroy(t);}
}
}
