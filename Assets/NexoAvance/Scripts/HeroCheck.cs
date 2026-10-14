using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Rendering;
namespace NexoAvance {
public class HeroCheck:MonoBehaviour {
    const string Out="C:/Users/jose/Documents/Codex/2026-09-22/ho/outputs/Protagonistas/";
    bool failed;float began;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-heroCheck")<0)return;var go=new GameObject("Comprobación protagonistas");DontDestroyOnLoad(go);go.AddComponent<HeroCheck>();}
    void Check(bool ok,string label){failed|=!ok;File.AppendAllText(Out+"validacion.txt",(ok?"OK ":"ERROR ")+label+"\n");}
    void Update(){if(Time.realtimeSinceStartup-began>240){File.AppendAllText(Out+"validacion.txt","ERROR tiempo agotado");Application.Quit(2);}}
    void Click(Button button){ExecuteEvents.Execute(button.gameObject,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);}
    void Quiet(){foreach(var e in FindObjectsByType<StreetEnemy>(FindObjectsInactive.Include,FindObjectsSortMode.None))e.AutoFire=false;foreach(var b in FindObjectsByType<MachineBoss>(FindObjectsInactive.Include,FindObjectsSortMode.None))b.AutoFire=false;foreach(var b in FindObjectsByType<OctopusBoss>(FindObjectsInactive.Include,FindObjectsSortMode.None))b.AutoFire=false;}
    ResistancePlayer Prepare(){Quiet();var p=FindAnyObjectByType<ResistancePlayer>();p.TestMode=true;p.SetPaused(false);return p;}
    void Place(ResistancePlayer p,Vector2 pos){p.transform.position=pos;p.GetComponent<Rigidbody2D>().position=pos;p.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;Physics2D.SyncTransforms();}
    IEnumerator Shot(ResistancePlayer p,int direction,string tag){
        p.visual.flipX=direction<0;var look=p.GetComponent<HeroAppearance>();var combat=p.GetComponent<PlayerCombat>();
        var target=new GameObject("Diana temporal");target.transform.position=look.Muzzle+Vector2.right*direction*1.5f;target.AddComponent<BoxCollider2D>().size=new Vector2(.25f,.6f);var hp=target.AddComponent<CombatHealth>();hp.team=CombatTeam.Robot;Physics2D.SyncTransforms();
        yield return new WaitForSeconds(.6f);int before=hp.Current;bool shot=combat.TryShoot();Check(shot&&Vector2.Distance(combat.LastShotOrigin,look.Muzzle)<.03f,tag+" salida de proyectil "+direction);
        yield return new WaitForSeconds(.2f);Check(hp.Current<before,tag+" disparo causa daño "+direction);Destroy(target);CombatBullet.ClearAll();
    }
    void Capture(string name){Canvas.ForceUpdateCanvases();var rt=new RenderTexture(1280,720,24);rt.Create();RenderPipeline.SubmitRenderRequest(Camera.main,new RenderPipeline.StandardRequest{destination=rt});var prev=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes(Out+name,tex.EncodeToPNG());RenderTexture.active=prev;rt.Release();Destroy(rt);Destroy(tex);}
    IEnumerator Start(){
        began=Time.realtimeSinceStartup;Application.runInBackground=true;File.WriteAllText(Out+"validacion.txt","");yield return null;
        // Direct editor-style mission entry, before any selection.
        yield return SceneManager.LoadSceneAsync("NucleoNexo");yield return null;var direct=Prepare();Check(direct.GetComponent<HeroAppearance>().AppliedIndex==0,"Sin selección previa usa Asalto");
        yield return SceneManager.LoadSceneAsync("MenuPrincipal");yield return null;
        for(int hero=0;hero<4;hero++){
            var menu=FindAnyObjectByType<MainMenuController>();Click(menu.playButton);yield return null;Check(menu.CurrentPage=="Personajes","Nueva partida abre selección "+hero);
            var chooser=menu.characterSelection;Click(chooser.cards[hero]);Check(chooser.Current==hero,"Ratón selecciona personaje "+hero);
            if(hero==0){yield return new WaitForEndOfFrame();Capture("seleccion.png");}
            Click(chooser.confirm);while(SceneManager.GetActiveScene().name!="CallesBajoControl")yield return null;yield return null;
            for(int mission=1;mission<=3;mission++){
                var p=Prepare();var look=p.GetComponent<HeroAppearance>();var health=p.GetComponent<CombatHealth>();var session=FindAnyObjectByType<CombatSession>();string tag="Personaje "+hero+" misión "+mission;
                Check(FindObjectsByType<ResistancePlayer>(FindObjectsSortMode.None).Length==1,tag+" jugador único");
                Check(look.AppliedIndex==hero&&p.visual.sprite==look.roster.heroes[hero].pose,tag+" diseño elegido");
                Check(p.speed==6&&p.jumpSpeed==11&&health.maximum==100&&p.GetComponent<BoxCollider2D>().size==new Vector2(.68f,1.7f),tag+" capacidades y collider comunes");
                yield return new WaitForSeconds(.5f);float x=p.transform.position.x;p.TestAxis=1;yield return new WaitForSeconds(.25f);p.TestAxis=0;Check(p.transform.position.x>x+.3f,tag+" movimiento");
                yield return new WaitForSeconds(.15f);float y=p.transform.position.y;p.TestJump=true;yield return new WaitForSeconds(.15f);Check(p.transform.position.y>y+.3f,tag+" salto");yield return new WaitForSeconds(.7f);
                Check(p.visual.transform.localScale==Vector3.one&&p.visual.sprite==look.roster.heroes[hero].pose,tag+" pose original sin deformación ni animación antigua");
                // Open space for isolated muzzle and projectile tests.
                Place(p,new Vector2(2,-.15f));yield return new WaitForSeconds(.2f);
                yield return Shot(p,1,tag);yield return Shot(p,-1,tag);
                p.visual.flipX=false;yield return new WaitForEndOfFrame();Capture("personaje-"+hero+"-mision-"+mission+".png");
                int before=health.Current;CombatBullet.Fire((Vector2)p.transform.position+new Vector2(1.2f,0),-1,CombatTeam.Robot,10,10,p.GetComponent<PlayerCombat>().pixel,p.GetComponent<PlayerCombat>().material);yield return new WaitForSeconds(.3f);Check(health.Current<before,tag+" recibe daño de proyectil");
                yield return new WaitForSeconds(.6f);health.Hit(1000,CombatTeam.Robot);Check(session.Dead&&!p.enabled,tag+" derrota");session.Restart();yield return null;Check(health.Alive&&p.enabled&&look.AppliedIndex==hero,tag+" reaparición conserva selección");
                if(mission==1){FindAnyObjectByType<StreetMission>().Replay();yield return null;}else{FindAnyObjectByType<CampaignMission>().Replay();yield return null;while(!FindAnyObjectByType<CampaignMission>())yield return null;yield return new WaitForSecondsRealtime(.2f);}
                p=Prepare();look=p.GetComponent<HeroAppearance>();Check(look.AppliedIndex==hero&&p.visual.sprite==look.roster.heroes[hero].pose,tag+" reinicio conserva selección");
                if(mission==1){var m=FindAnyObjectByType<StreetMission>();Place(p,new Vector2(174,-.15f));yield return new WaitForSeconds(.2f);m.boss.health.Hit(10000,CombatTeam.Player);yield return new WaitForSecondsRealtime(1.2f);Check(m.Complete,tag+" victoria habilita transición");FindAnyObjectByType<CampaignExit>().Continue();while(SceneManager.GetActiveScene().name!="FabricaAutomatizada")yield return null;yield return null;}
                else if(mission==2){var m=FindAnyObjectByType<CampaignMission>();foreach(var o in m.objectives)o.health.Hit(10000,CombatTeam.Player);Place(p,new Vector2(m.exitX+.1f,-.15f));yield return new WaitForSeconds(.2f);m.boss.health.Hit(10000,CombatTeam.Player);yield return new WaitForSecondsRealtime(1.2f);Check(m.Complete,tag+" objetivos y victoria habilitan transición");m.Continue();while(SceneManager.GetActiveScene().name!="NucleoNexo")yield return null;yield return null;}
                else {FindAnyObjectByType<CombatSession>().BackToMenu();while(SceneManager.GetActiveScene().name!="MenuPrincipal")yield return null;yield return null;}
            }
        }
        File.AppendAllText(Out+"validacion.txt",failed?"RESULTADO: ERROR":"RESULTADO: CORRECTO");Application.Quit(failed?1:0);
    }
}
}
