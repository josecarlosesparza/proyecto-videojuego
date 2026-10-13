using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
namespace NexoAvance {
public static class CampaignBuilder {
    const string Root="Assets/NexoAvance";
    public const string Output=@"C:\Users\jose\Documents\Codex\2026-09-22\ho\outputs\Campana";
    const string Pending=@"C:\Users\jose\Documents\Codex\2026-09-22\ho\work\campana-pendiente.txt";
    const string Ready=@"C:\Users\jose\Documents\Codex\2026-09-22\ho\work\campana-exportar.txt";
    static Sprite pixel;static Material material;static Font font;static double after;static int retries;
    [InitializeOnLoadMethod]static void Boot(){if(Application.isBatchMode)return;after=EditorApplication.timeSinceStartup+30;EditorApplication.update+=Tick;}
    static void Tick(){if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.timeSinceStartup<after)return;if(File.Exists(Pending)){if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;File.Delete(Pending);Install();File.WriteAllText(Ready,"export");after=EditorApplication.timeSinceStartup+30;return;}if(File.Exists(Ready)){File.Delete(Ready);try{Export();EditorApplication.update-=Tick;}catch{if(retries++<2){File.WriteAllText(Ready,"retry");after=EditorApplication.timeSinceStartup+30;}else{EditorApplication.update-=Tick;throw;}}}}
    static Color C(string h){ColorUtility.TryParseHtmlString("#"+h,out var c);return c;}
    static SpriteRenderer Rect(Transform parent,string name,float x,float y,float w,float h,string color,int order=0){var s=new GameObject(name).AddComponent<SpriteRenderer>();s.transform.SetParent(parent,false);s.transform.localPosition=new Vector3(x,y,0);s.transform.localScale=new Vector3(w,h,1);s.sprite=pixel;s.sharedMaterial=material;s.color=C(color);s.sortingOrder=order;return s;}
    static GameObject Solid(Transform p,string n,float x,float y,float w,float h,string c){var s=Rect(p,n,x,y,w,h,c,3);s.gameObject.AddComponent<BoxCollider2D>();return s.gameObject;}
    static Text Label(Transform parent,string name,Vector2 anchor,Vector2 pos,Vector2 size,string caption,int points){var go=new GameObject(name,typeof(RectTransform));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=anchor;r.anchoredPosition=pos;r.sizeDelta=size;var t=go.AddComponent<Text>();t.font=font;t.fontSize=points;t.color=C("DAF6EF");t.alignment=TextAnchor.MiddleCenter;t.text=caption;t.raycastTarget=false;return t;}
    static Button NextButton(StreetMission old){var b=UnityEngine.Object.Instantiate(old.replay,old.replay.transform.parent);b.name="Continuar campaña";b.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-50);b.GetComponentInChildren<Text>().text="CONTINUAR";old.replay.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-115);old.menu.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-180);b.navigation=new Navigation{mode=Navigation.Mode.Explicit,selectOnDown=old.replay,selectOnUp=old.menu};old.replay.navigation=new Navigation{mode=Navigation.Mode.Explicit,selectOnDown=old.menu,selectOnUp=b};old.menu.navigation=new Navigation{mode=Navigation.Mode.Explicit,selectOnDown=b,selectOnUp=old.replay};return b;}
    public static void Install(){
        Directory.CreateDirectory(Output);pixel=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/Pixel.png");material=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Art/PixelMaterial.mat");font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Make(2);Make(3);
        var scene=EditorSceneManager.OpenScene(Root+"/Scenes/CallesBajoControl.unity");var street=UnityEngine.Object.FindAnyObjectByType<StreetMission>();var connection=street.GetComponent<CampaignExit>();if(!connection){connection=street.gameObject.AddComponent<CampaignExit>();connection.mission=street;connection.next=NextButton(street);connection.next.GetComponentInChildren<Text>().text="CONTINUAR A LA FÁBRICA";}EditorSceneManager.SaveScene(scene);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Root+"/Scenes/MenuPrincipal.unity",true),new EditorBuildSettingsScene(Root+"/Scenes/CallesBajoControl.unity",true),new EditorBuildSettingsScene(Root+"/Scenes/FabricaAutomatizada.unity",true),new EditorBuildSettingsScene(Root+"/Scenes/NucleoNexo.unity",true)};AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(Root+"/Scenes/MenuPrincipal.unity");File.WriteAllText(Path.Combine(Output,"integracion.txt"),"Misiones 2 y 3 integradas");
    }
    static void Make(int number){
        var scene=EditorSceneManager.OpenScene(Root+"/Scenes/CallesBajoControl.unity");EditorSceneManager.SaveScene(scene,Root+"/Scenes/"+(number==2?"FabricaAutomatizada":"NucleoNexo")+".unity");
        var old=UnityEngine.Object.FindAnyObjectByType<StreetMission>();var session=old.session;
        var previousExit=old.GetComponent<CampaignExit>();if(previousExit){UnityEngine.Object.DestroyImmediate(previousExit.next.gameObject);UnityEngine.Object.DestroyImmediate(previousExit);}
        var next=NextButton(old);var m=session.gameObject.AddComponent<CampaignMission>();m.number=number;m.session=session;m.bossHud=old.bossHud;m.bossFill=old.bossFill;m.victoryPanel=old.victoryPanel;m.victoryTime=old.victoryTime;m.toast=old.toast;m.replay=old.replay;m.menu=old.menu;m.next=next;m.bossName=old.bossHud.transform.Find("Nombre").GetComponent<Text>();
        m.victoryPanel.transform.Find("Título").GetComponent<Text>().text=number==2?"FÁBRICA AUTOMATIZADA":"NEXO DESCONECTADO";m.victoryPanel.transform.Find("Resultado").GetComponent<Text>().text=number==2?"PRODUCCIÓN DETENIDA":"CAMPAÑA COMPLETADA";next.GetComponentInChildren<Text>().text="CONTINUAR AL NÚCLEO";if(number==3){next.gameObject.SetActive(false);m.replay.navigation=new Navigation{mode=Navigation.Mode.Explicit,selectOnDown=m.menu,selectOnUp=m.menu};m.menu.navigation=new Navigation{mode=Navigation.Mode.Explicit,selectOnDown=m.replay,selectOnUp=m.replay};}
        UnityEngine.Object.DestroyImmediate(GameObject.Find("Avenida · Calles Bajo Control"));UnityEngine.Object.DestroyImmediate(old);
        var world=new GameObject(number==2?"Planta de ensamblaje":"Centro de datos NEXO").transform;
        session.player.transform.position=new Vector3(1,.2f,0);session.enemy=null;session.health.invulnerability=.45f;session.player.SetSpawn(new Vector3(1,.2f,0));
        Camera.main.transform.position=new Vector3(8,2,-10);Camera.main.backgroundColor=C(number==2?"17242D":"0D182B");UnityEngine.Object.FindAnyObjectByType<AvanceView>().cameraEnd=number==2?108:122;
        Solid(world,"Suelo",64,-1.55f,138,1.1f,number==2?"425159":"263F55");Rect(world,"Borde",64,-1.04f,138,.1f,"A8C9C6",4);Solid(world,"Limite izquierdo",-3,3,.3f,9,"253849");Solid(world,"Limite final",135,3,.3f,9,"253849");
        for(int i=-1;i<20;i++){float x=i*7;
            Rect(world,"Panel de fondo",x,2,6.7f,6.5f,number==2?"263B44":"162A44",-30);
            Rect(world,"Viga",x-3,2.2f,.22f,7,number==2?"596665":"355577",-20);
            if(number==2){Rect(world,"Conducto",x,4.6f,6.8f,.55f,"475B60",-19);Rect(world,"Motor",x,1.3f,2.2f,2.4f,"354F58",-18);for(int j=0;j<5;j++)Rect(world,"Rejilla",x, .6f+j*.3f,1.7f,.08f,"182F3D",-17);Rect(world,"Luz industrial",x,4.1f,1.5f,.12f,"F5BF6C",-16);}
            else {for(int rack=0;rack<2;rack++){float rx=x-1.6f+rack*3;Rect(world,"Servidor",rx,1.7f,2.3f,4.7f,"0F2036",-19);for(int j=0;j<7;j++){Rect(world,"Bandeja",rx,-.25f+j*.6f,2.05f,.37f,"264960",-18);Rect(world,"Indicador",rx-.65f,-.25f+j*.6f,.13f,.1f,(j+i)%3==0?"E2A860":"69D8D0",-17);}}Rect(world,"Cable de datos",x,4.8f,6.8f,.08f,"528DB0",-15);}
        }
        var canvas=session.healthLabel.canvas.transform;m.progress=Label(canvas,"Progreso de misión",new Vector2(1,0),new Vector2(-24,110),new Vector2(380,30),"",18);
        m.objectives=new CampaignObjective[2];
        if(number==2){
            for(int i=0;i<4;i++){float x=14+i*23;Belt(world,x);Solid(world,"Plataforma",x+2,0,5,.25f,"68898D");Rect(world,"Borde amarillo",x+2,.15f,5,.08f,"EBC077",5);}
            Solid(world,"Acceso a generador B",71,0,5,.25f,"68898D");Solid(world,"Pasarela elevada",75.75f,1,5.5f,.3f,"668A8E");
            m.objectives[0]=Objective(world,session,34,0,"GENERADOR A",300,null,true);
            m.objectives[1]=Objective(world,session,76,2.1f,"GENERADOR B",350,null,true);
        }else {
            var door1=Door(world,36);var door2=Door(world,72);
            m.objectives[0]=Objective(world,session,28,0,"TERMINAL 01",180,door1,false);m.objectives[1]=Objective(world,session,64,0,"TERMINAL 02",220,door2,false);
            foreach(float x in new[]{15f,49f,86f}){Solid(world,"Protección de servidores",x,-.55f,1.4f,.9f,"547284");Solid(world,"Pasarela de mantenimiento",x+5,.15f,4,.25f,"4F8797");}
        }
        m.groups=new GameObject[3];m.triggers=new[]{8f,39f,79f};
        for(int zone=0;zone<3;zone++){var group=new GameObject("Patrulla "+(zone+1));group.transform.SetParent(world);m.groups[zone]=group;
            float[] positions=zone==0?new[]{11f,22f,30f}:zone==1?new[]{44f,55f,68f}:new[]{83f,88f,94f};
            for(int j=0;j<3;j++)Enemy(group.transform,session,positions[j],j==2?StreetEnemyKind.Torreta:(StreetEnemyKind)j,number);group.SetActive(false);
        }
        Pickup(world,session,5,true);Pickup(world,session,43,true);Pickup(world,session,59,false);Pickup(world,session,84,false);
        m.boss=Boss(world,session,number);m.bossTrigger=number==2?88:97;m.exitX=number==2?113:126;m.exitDoor=Door(world,number==2?111:128);
        Rect(world,"Baliza de salida",number==2?115:130,1,.3f,4,"6BDAC0",7);
        session.defeatPanel.transform.Find("Ayuda").GetComponent<Text>().text="Reintenta desde el último punto de control.";
        EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
    }
    static void Belt(Transform world,float x){var go=new GameObject("Línea de ensamblaje");go.transform.SetParent(world);go.transform.position=new Vector3(x,-.65f,0);Solid(go.transform,"Cinta",0,0,6,.65f,"293F4B");var belt=go.AddComponent<AssemblyBelt>();belt.marks=new Transform[8];for(int i=0;i<8;i++)belt.marks[i]=Rect(go.transform,"Rodillo",i*.7f-2.8f,.3f,.18f,.11f,"E5B86F",5).transform;}
    static GameObject Door(Transform world,float x){var root=new GameObject("Puerta de seguridad");root.transform.SetParent(world);root.transform.position=new Vector3(x,1.7f,0);Solid(root.transform,"Hoja blindada",0,0,.6f,5.4f,"3B6878");for(int i=0;i<5;i++)Rect(root.transform,"Indicador",0,-2+i,.66f,.12f,"72E0E1",6);return root;}
    static CampaignObjective Objective(Transform world,CombatSession session,float x,float y,string title,int hp,GameObject door,bool generator){
        var go=new GameObject(title);go.transform.SetParent(world);go.transform.position=new Vector3(x,y,0);
        var body=Rect(go.transform,"Carcasa",0,0,generator?1.6f:1.1f,1.9f,generator?"A1743E":"447A95",8);
        Rect(go.transform,"Pantalla",0,.2f,generator?1.05f:.8f,.7f,generator?"F6C66D":"72E7DD",9);for(int j=0;j<3;j++)Rect(go.transform,"Rejilla",0,-.45f-j*.12f,.8f,.06f,"233744",10);
        var box=go.AddComponent<BoxCollider2D>();box.size=new Vector2(generator?1.6f:1.1f,1.9f);
        var h=go.AddComponent<CombatHealth>();h.maximum=hp;h.team=CombatTeam.Robot;h.visual=body;
        var o=go.AddComponent<CampaignObjective>();o.health=h;o.door=door;o.pixel=pixel;o.material=material;return o;
    }
    static void Enemy(Transform parent,CombatSession session,float x,StreetEnemyKind kind,int mission){
        var go=new GameObject(kind==StreetEnemyKind.Torreta?"Torreta de seguridad":kind==StreetEnemyKind.Dron?"Dron de vigilancia":"Robot patrullero");go.transform.SetParent(parent);go.transform.position=new Vector3(x,kind==StreetEnemyKind.Dron?.2f:mission==2 && x==11?.55f:mission==2 && x==83?.975f:-.15f,0);
        var visual=go.AddComponent<SpriteRenderer>();visual.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/"+(kind==StreetEnemyKind.Fusilero?"RobotFusileroNaranja":"MisionEnemigo"+(kind==StreetEnemyKind.Torreta?2:(int)kind))+".png");visual.sharedMaterial=material;visual.sortingOrder=10;
        if(kind==StreetEnemyKind.Torreta){visual.transform.localScale=new Vector3(1.15f,.9f,1);}
        var box=go.AddComponent<BoxCollider2D>();box.size=new Vector2(1,1.7f);var h=go.AddComponent<CombatHealth>();h.maximum=kind==StreetEnemyKind.Torreta?160:kind==StreetEnemyKind.Dron?70:90;h.team=CombatTeam.Robot;h.visual=visual;
        var e=go.AddComponent<StreetEnemy>();e.kind=kind;e.health=h;e.target=session.health;e.visual=visual;e.pixel=pixel;e.material=material;e.origin=go.transform.position;e.warning=Rect(go.transform,"Aviso",0,1.15f,.15f,.25f,"FFDB83",15);e.warning.enabled=false;
    }
    static void Pickup(Transform world,CombatSession session,float x,bool grenade){var go=new GameObject(grenade?"Caja EMP":"Botiquín");go.transform.SetParent(world);go.transform.position=new Vector3(x,-.5f,0);Rect(go.transform,"Caja",0,0,.65f,.6f,grenade?"637447":"D7E4D4",8);Rect(go.transform,"Marca",0,0,.45f,.12f,grenade?"EECB7F":"549D70",9);if(!grenade)Rect(go.transform,"Cruz",0,0,.12f,.45f,"549D70",9);var p=go.AddComponent<CampaignPickup>();p.session=session;p.grenades=grenade;}
    static MachineBoss Boss(Transform world,CombatSession session,int number){bool core=number==3;var go=new GameObject(core?"NEXO · Núcleo central":"Supervisor de ensamblaje");go.transform.SetParent(world);go.transform.position=new Vector3(core?115:102,1,0);
        var body=Rect(go.transform,"Núcleo",0,0,core?2.2f:2.8f,3.6f,core?"508EA7":"997B51",15);Rect(go.transform,"Ojo",0,.2f,1.5f,.65f,core?"95F5F0":"FFE1A0",17);Rect(go.transform,"Ranura",0,.2f,.8f,.12f,"1D3143",18);
        var h=go.AddComponent<CombatHealth>();h.maximum=core?1800:700;h.team=CombatTeam.Robot;h.visual=body;var box=go.AddComponent<BoxCollider2D>();box.size=new Vector2(2.4f,3.6f);
        var boss=go.AddComponent<MachineBoss>();boss.core=core;boss.health=h;boss.target=session.health;boss.pixel=pixel;boss.material=material;
        var parts=new List<Transform>();for(int i=0;i<(core?2:2);i++){var part=new GameObject(core?"Anillo de datos":"Brazo industrial").transform;part.SetParent(go.transform,false);parts.Add(part);
            if(core){for(int j=0;j<12;j++){float angle=j*Mathf.PI/6;Rect(part,"Nodo",Mathf.Cos(angle)*(2+i*.6f),Mathf.Sin(angle)*(2+i*.6f),.3f,.3f,i==0?"67D8DE":"507FAA",13);}}
            else{float sign=i==0?-1:1;Rect(part,"Brazo",sign*1.9f,.2f,1.2f,.5f,"677C83",13);Rect(part,"Pinza",sign*2.5f,-.2f,.4f,1.2f,"ADC1BF",14);}
        }boss.parts=parts.ToArray();boss.warning=Rect(go.transform,"Aviso y escudo",0,1.25f,1.6f,.22f,"C6F9FF",20);boss.warning.enabled=false;go.SetActive(false);return boss;
    }
    public static void Export(){Directory.CreateDirectory(Output);var scenes=new[]{Root+"/Scenes/MenuPrincipal.unity",Root+"/Scenes/CallesBajoControl.unity",Root+"/Scenes/FabricaAutomatizada.unity",Root+"/Scenes/NucleoNexo.unity"};var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=Path.Combine(Output,"NEXO-Jugable/NEXO.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Compilación de campaña fallida");File.WriteAllText(Path.Combine(Output,"compilacion.txt"),"Campaña compilada correctamente");}
}
}
