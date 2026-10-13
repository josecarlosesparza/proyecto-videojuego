using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
namespace NexoAvance {
public static class StreetMissionBuilder {
    const string Root="Assets/NexoAvance";
    public const string Output=@"C:\Users\jose\Documents\Codex\2026-09-22\ho\outputs\Mision01";
    const string Pending=@"C:\Users\jose\Documents\Codex\2026-09-22\ho\work\mision-pendiente.txt";
    const string Ready=@"C:\Users\jose\Documents\Codex\2026-09-22\ho\work\mision-exportar.txt";
    static double after; static int exportRetries;
    static Sprite pixel;static Material material;static Font font;static Texture2D tex;
    [InitializeOnLoadMethod] static void Load(){if(Application.isBatchMode)return;after=EditorApplication.timeSinceStartup+30;EditorApplication.update+=Tick;}
    static void Tick(){if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.timeSinceStartup<after)return;
        if(File.Exists(Pending)){if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;File.Delete(Pending);Install();File.WriteAllText(Ready,"export");after=EditorApplication.timeSinceStartup+15;return;}
        if(File.Exists(Ready)){File.Delete(Ready);try{Export();EditorApplication.update-=Tick;}catch(Exception){if(exportRetries++<2){File.WriteAllText(Ready,"retry");after=EditorApplication.timeSinceStartup+30;}else{EditorApplication.update-=Tick;throw;}}}
    }
    static Color C(string hex){ColorUtility.TryParseHtmlString("#"+hex,out var c);return c;}
    static SpriteRenderer Rect(Transform parent,string name,float x,float y,float w,float h,string color,int order=0){
        var s=new GameObject(name).AddComponent<SpriteRenderer>();s.transform.SetParent(parent,false);s.transform.localPosition=new Vector3(x,y,0);s.transform.localScale=new Vector3(w,h,1);s.sprite=pixel;s.sharedMaterial=material;s.color=C(color);s.sortingOrder=order;return s;
    }
    static GameObject Solid(Transform parent,string name,float x,float y,float w,float h,string color){var s=Rect(parent,name,x,y,w,h,color,3);s.gameObject.AddComponent<BoxCollider2D>();return s.gameObject;}
    static void R(int x,int y,int w,int h,string hex){for(int yy=y;yy<y+h;yy++)for(int xx=x;xx<x+w;xx++)tex.SetPixel(xx,tex.height-1-yy,C(hex));}
    static void Texture(int w,int h){tex=new Texture2D(w,h,TextureFormat.RGBA32,false);tex.SetPixels(new Color[w*h]);}
    static Sprite Save(string name,int ppu=24){tex.Apply();string p=Root+"/Art/"+name+".png";File.WriteAllBytes(p,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(p,ImportAssetOptions.ForceSynchronousImport);var i=(TextureImporter)AssetImporter.GetAtPath(p);i.textureType=TextureImporterType.Sprite;i.spritePixelsPerUnit=ppu;i.filterMode=FilterMode.Point;i.textureCompression=TextureImporterCompression.Uncompressed;i.mipmapEnabled=false;i.alphaIsTransparency=true;i.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(p);}
    static Sprite EnemyArt(int type){if(type==0)return AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/RobotFusileroNaranja.png");Texture(40,44);
        if(type==1){R(2,13,36,5,"182E40");R(4,10,10,3,"ABC9CE");R(26,10,10,3,"ABC9CE");R(9,17,22,16,"243750");R(12,16,16,14,"9272B5");R(15,19,10,6,"D8C6ED");R(18,20,5,4,"FF966C");R(16,30,8,7,"34465D");R(17,36,6,3,"F3B25F");}
        else {string armor=type==2?"B89255":"4B9091";R(11,3,18,13,"172A38");R(13,5,14,8,armor);R(15,8,12,3,"FF936B");R(7,17,26,17,"182C3C");R(10,18,20,13,armor);R(15,20,10,5,"BCD6D1");R(9,32,8,10,"263C4D");R(24,32,8,10,"263C4D");R(7,39,12,4,"91A7AF");R(23,39,12,4,"91A7AF");R(1,22,12,6,"243341");R(0,24,9,2,"B8D5D2");if(type==2){R(5,15,8,18,"D0A964");R(28,15,8,18,"D0A964");R(16,27,8,3,"503E32");}}
        return Save("MisionEnemigo"+type);
    }
    static Sprite Truck(){Texture(112,48);R(3,8,72,28,"233B4B");R(6,10,66,22,"4B666F");for(int x=12;x<70;x+=10)R(x,12,2,17,"6E8589");R(75,18,29,19,"A57942");R(77,11,18,22,"C89C59");R(79,14,13,10,"274351");R(91,25,15,7,"BF8A4D");R(1,34,107,6,"283442");R(14,32,17,15,"111D2A");R(18,36,9,7,"7A888B");R(83,32,17,15,"111D2A");R(87,36,9,7,"7A888B");R(103,28,6,4,"FBE09A");R(101,36,10,3,"879592");R(44,22,15,4,"263B46");R(48,26,11,4,"263B46");return Save("CamionAbandonado");}
    static Sprite BossArt(){Texture(88,96);R(24,3,40,7,"182B3E");R(15,10,58,9,"182B3E");R(8,19,72,51,"182B3E");R(13,21,62,42,"53778A");R(22,11,44,12,"6F98A5");R(26,8,36,8,"A1BEC4");R(17,24,54,7,"88ABB5");R(20,33,48,23,"172A3E");R(24,36,40,16,"B34242");R(29,39,30,10,"FB795A");R(37,40,14,8,"FFE3A0");R(16,60,56,14,"2D4A5D");R(24,74,40,12,"162C40");for(int x=24;x<66;x+=10)R(x,61,5,9,"A6C9C9");R(5,34,8,16,"263F53");R(75,34,8,16,"263F53");R(33,76,22,9,"74939D");R(39,85,10,8,"FFB369");return Save("Kraken01Nucleo");}
    static RectTransform UI(Transform parent,string name,Vector2 anchor,Vector2 pos,Vector2 size){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=anchor;r.anchoredPosition=pos;r.sizeDelta=size;return r;}
    static Text Label(Transform parent,string name,Vector2 anchor,Vector2 pos,Vector2 size,string text,int points){var t=UI(parent,name,anchor,pos,size).gameObject.AddComponent<Text>();t.font=font;t.fontSize=points;t.text=text;t.color=C("D5F5EC");t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;return t;}
    static Button Button(Transform parent,string text,float y){var r=UI(parent,text,new Vector2(.5f,.5f),new Vector2(0,y),new Vector2(480,54));var image=r.gameObject.AddComponent<Image>();image.color=C("397D79");var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;var colors=b.colors;colors.highlightedColor=colors.selectedColor=C("A8F8D5");b.colors=colors;Label(r,"Texto",new Vector2(.5f,.5f),Vector2.zero,new Vector2(470,54),text,21);return b;}
    [MenuItem("NEXO/Crear misión Calles Bajo Control")]
    public static void Install(){
        Directory.CreateDirectory(Output);pixel=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/Pixel.png");material=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Art/PixelMaterial.mat");font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var scene=EditorSceneManager.OpenScene(Root+"/Scenes/PrimerAvance.unity");EditorSceneManager.SaveScene(scene,Root+"/Scenes/CallesBajoControl.unity");
        var old=GameObject.Find("Escenario · ciudad ocupada");if(old)UnityEngine.Object.DestroyImmediate(old);old=GameObject.Find("Auto rojo destrozado");if(old)UnityEngine.Object.DestroyImmediate(old);
        var session=UnityEngine.Object.FindAnyObjectByType<CombatSession>();if(session.enemy)UnityEngine.Object.DestroyImmediate(session.enemy.gameObject);session.enemy=null;
        foreach(var test in UnityEngine.Object.FindObjectsByType<AvanceSmokeTest>(FindObjectsInactive.Include,FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(test);
        foreach(var test in UnityEngine.Object.FindObjectsByType<CombatSmokeTest>(FindObjectsInactive.Include,FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(test);
        var player=session.player;player.transform.position=new Vector3(1,.2f,0);session.health.invulnerability=.45f;
        UnityEngine.Object.FindAnyObjectByType<AvanceView>().cameraEnd=191;
        var world=new GameObject("Avenida · Calles Bajo Control").transform;
        Solid(world,"Avenida",99,-1.55f,206,1.1f,"344C5B");
        Rect(world,"Borde de acera",99,-1.05f,206,.1f,"93A8AB",4);
        for(int x=-2;x<204;x+=3)Rect(world,"Pavimento",x,-1.34f,.95f,.05f,"A0A399",4);
        for(int i=-1;i<31;i++){
            float x=i*7,h=4.2f+(i*i%4)*.8f;
            Rect(world,"Edificio",x,-1+h/2,6.4f,h,i%3==0?"203E52":"1C354A",-30);
            for(int a=0;a<3;a++)for(int b=0;b<4;b++)Rect(world,"Ventana",x-2+a*1.4f,.6f+b*.7f,.32f,.38f,(a+b+i)%3==0?"5D8589":"2C4F61",-29);
            Rect(world,"Fachada",x,.05f,6.5f,2.1f,"2A4655",-20);Rect(world,"Cornisa",x,1.12f,6.7f,.12f,"5A7883",-19);
            Rect(world,"Persiana",x,-.12f,2,1.6f,"1A3042",-18);for(int j=0;j<5;j++)Rect(world,"Lámina",x,-.7f+j*.3f,1.95f,.035f,"476574",-17);
            Rect(world,"Poste",x+2.8f,1.2f,.1f,4.4f,"172C3C",-16);Rect(world,"Farola",x+2.3f,3.38f,1.1f,.14f,"8FBAB7",-15);
        }
        Solid(world,"Limite de avenida",-3,3,.3f,9,"172C3C");Solid(world,"Final de avenida",202,3,.3f,9,"172C3C");
        var carSprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/AutoRojoDestrozado.png");var truckSprite=Truck();
        Sprite[] arts={EnemyArt(0),EnemyArt(1),EnemyArt(2)};
        var mission=session.gameObject.AddComponent<StreetMission>();mission.session=session;mission.sectors=new StreetSector[4];
        for(int section=0;section<4;section++){
            float offset=section*38;
            var vehicle=new GameObject(section%2==0?"Auto accidentado":"Camión abandonado");vehicle.transform.SetParent(world);vehicle.transform.position=new Vector3(offset+6.7f,section%2==0?-.19f:0,0);
            var sr=vehicle.AddComponent<SpriteRenderer>();sr.sprite=section%2==0?carSprite:truckSprite;sr.sharedMaterial=material;sr.sortingOrder=6;
            var box=vehicle.AddComponent<BoxCollider2D>();box.size=section%2==0?new Vector2(2.9f,.65f):new Vector2(4.4f,1.1f);box.offset=section%2==0?new Vector2(0,-.36f):new Vector2(0,-.4f);
            var upper=vehicle.AddComponent<BoxCollider2D>();upper.size=section%2==0?new Vector2(1.35f,.45f):new Vector2(2.9f,.35f);upper.offset=section%2==0?new Vector2(-.05f,.17f):new Vector2(-.45f,.3f);
            if(section%2==0){var f=new GameObject("Fuego decorativo");f.transform.SetParent(vehicle.transform,false);f.transform.localPosition=new Vector3(1,.32f,0);f.transform.localScale=Vector3.one*.8f;var flame=f.AddComponent<SpriteRenderer>();flame.sharedMaterial=material;flame.sortingOrder=7;var fire=f.AddComponent<SmallCarFire>();fire.flame=flame;fire.frames=new[]{AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/FuegoAuto0.png"),AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/FuegoAuto1.png"),AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/FuegoAuto2.png")};flame.sprite=fire.frames[0];}
            Barricade(world,offset+15,section%3);Barricade(world,offset+30,(section+1)%3);
            var sector=new StreetSector{trigger=offset+10,checkpoint=offset+40,gate=Gate(world,offset+38),waves=new GameObject[2]};mission.sectors[section]=sector;
            for(int wave=0;wave<2;wave++){
                var group=new GameObject("Sector "+(section+1)+" · oleada "+(wave+1));group.transform.SetParent(world);sector.waves[wave]=group;
                for(int kind=0;kind<3;kind++){
                    var e=new GameObject(new[]{"Fusilero","Dron rastreador","Blindado"}[kind]);e.transform.SetParent(group.transform);e.transform.position=new Vector3(offset+19+kind*7+(wave==1?1:0),kind==1?.2f:-.15f,0);
                    var visual=e.AddComponent<SpriteRenderer>();visual.sprite=arts[kind];visual.sharedMaterial=material;visual.sortingOrder=10;if(kind==2)e.transform.localScale=Vector3.one*1.08f;
                    var collider=e.AddComponent<BoxCollider2D>();collider.size=kind==1?new Vector2(1.1f,1.3f):new Vector2(.9f,1.7f);
                    var health=e.AddComponent<CombatHealth>();health.team=CombatTeam.Robot;health.maximum=new[]{100,80,160}[kind];health.visual=visual;
                    var enemy=e.AddComponent<StreetEnemy>();enemy.kind=(StreetEnemyKind)kind;enemy.health=health;enemy.target=session.health;enemy.visual=visual;enemy.pixel=pixel;enemy.material=material;enemy.origin=e.transform.position;
                    enemy.warning=Rect(e.transform,"Aviso de disparo",0,1.15f,.15f,.22f,"FFD17B",15);enemy.warning.enabled=false;
                }group.SetActive(false);
            }
        }
        var bossObject=new GameObject("KRAKEN-01 · Pulpo robótico");bossObject.transform.SetParent(world);bossObject.transform.position=new Vector3(183,1.25f,0);
        var core=bossObject.AddComponent<SpriteRenderer>();core.sprite=BossArt();core.sharedMaterial=material;core.sortingOrder=14;
        var bossHealth=bossObject.AddComponent<CombatHealth>();bossHealth.maximum=1600;bossHealth.team=CombatTeam.Robot;bossHealth.visual=core;
        var bossCollider=bossObject.AddComponent<BoxCollider2D>();bossCollider.size=new Vector2(2.6f,3.9f);
        var boss=bossObject.AddComponent<OctopusBoss>();mission.boss=boss;boss.health=bossHealth;boss.target=session.health;boss.pixel=pixel;boss.material=material;boss.arms=new Transform[8];
        for(int i=0;i<8;i++){float side=i<4?-1:1;int row=i%4;var arm=new GameObject("Tentáculo "+(i+1)).transform;arm.SetParent(bossObject.transform,false);boss.arms[i]=arm;
            for(int j=0;j<6;j++){float x=side*(.85f+j*.42f),y=.55f-row*.52f-j*.13f+Mathf.Sin(j*.8f+row)*.25f;Rect(arm,"Eslabón",x,y,.52f,.35f,row%2==0?"597B8B":"3D6176",12-row);Rect(arm,"Articulación",x,y,.17f,.17f,"9FBFC2",13-row);}Rect(arm,"Pinza",side*3.35f,-.2f-row*.52f,.35f,.4f,"CB9867",13);
        }
        boss.warning=Rect(bossObject.transform,"Carga del núcleo",0,.48f,.5f,.25f,"FFF1AF",20);boss.warning.enabled=false;bossObject.SetActive(false);
        mission.bossEntrance=Gate(world,160);mission.bossEntrance.SetActive(false);
        for(int x=167;x<199;x+=4)Rect(world,"Zona del jefe",x,-1.1f,1.3f,.08f,"C88D5F",5);
        var canvas=session.healthLabel.canvas.transform;session.objective.gameObject.SetActive(false);
        mission.toast=Label(canvas,"Avisos breves",new Vector2(.5f,0),new Vector2(0,115),new Vector2(850,36),"",22);
        var bossHud=UI(canvas,"Salud de KRAKEN",new Vector2(1,0),new Vector2(-24,18),new Vector2(370,76));mission.bossHud=bossHud.gameObject;bossHud.gameObject.AddComponent<Image>().color=C("102434");Label(bossHud,"Nombre",new Vector2(.5f,1),new Vector2(0,-6),new Vector2(350,30),"KRAKEN-01",20);
        var fill=UI(bossHud,"Vida",Vector2.zero,new Vector2(16,16),new Vector2(338,16)).gameObject.AddComponent<Image>();fill.sprite=pixel;fill.color=C("EB785D");fill.type=Image.Type.Filled;fill.fillMethod=Image.FillMethod.Horizontal;mission.bossFill=fill;bossHud.gameObject.SetActive(false);
        var win=UI(canvas,"Misión completada",new Vector2(.5f,.5f),Vector2.zero,new Vector2(1280,720));win.gameObject.AddComponent<Image>().color=new Color(.03f,.07f,.11f,.96f);mission.victoryPanel=win.gameObject;
        Label(win,"Título",new Vector2(.5f,.5f),new Vector2(0,125),new Vector2(850,65),"CALLES BAJO CONTROL",38);
        Label(win,"Resultado",new Vector2(.5f,.5f),new Vector2(0,65),new Vector2(850,45),"MISIÓN COMPLETADA",25);
        mission.victoryTime=Label(win,"Tiempo",new Vector2(.5f,.5f),new Vector2(0,16),new Vector2(850,35),"",20);mission.replay=Button(win,"REPETIR MISIÓN",-65);mission.menu=Button(win,"VOLVER AL MENÚ",-130);
        mission.replay.navigation=new Navigation{mode=Navigation.Mode.Explicit,selectOnDown=mission.menu,selectOnUp=mission.menu};mission.menu.navigation=new Navigation{mode=Navigation.Mode.Explicit,selectOnDown=mission.replay,selectOnUp=mission.replay};win.gameObject.SetActive(false);
        session.defeatPanel.transform.Find("Ayuda").GetComponent<Text>().text="Reintenta desde el último punto de control.";
        EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        var menuScene=EditorSceneManager.OpenScene(Root+"/Scenes/MenuPrincipal.unity");UnityEngine.Object.FindAnyObjectByType<MainMenuController>().gameScene="CallesBajoControl";
        foreach(var t in UnityEngine.Object.FindObjectsByType<Text>(FindObjectsInactive.Include,FindObjectsSortMode.None)){if(t.name=="Descripción de jugar")t.text="MISIÓN 01 · CALLES BAJO CONTROL\nLibera la avenida y derrota a KRAKEN-01.";if(t.name=="Versión")t.text="MISIÓN 01 / 3–5 MIN";if(t.name=="Ficha")t.text="TRES ENEMIGOS · PUNTOS DE CONTROL · JEFE";}
        EditorSceneManager.SaveScene(menuScene);MainMenuBuilder.ConfigureStart();EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Root+"/Scenes/MenuPrincipal.unity",true),new EditorBuildSettingsScene(Root+"/Scenes/CallesBajoControl.unity",true)};AssetDatabase.SaveAssets();File.WriteAllText(Path.Combine(Output,"integracion.txt"),"Misión integrada");
    }
    static GameObject Gate(Transform parent,float x){var go=Solid(parent,"Barrera de control",x,1.4f,.3f,4.8f,"713F4D");Rect(go.transform,"Luz",0,0,.5f,.92f,"DD8765",5);go.SetActive(false);return go;}
    static void Barricade(Transform parent,float x,int kind){
        var root=new GameObject(new[]{"Barrera de hormigón","Sacos de contención","Chatarra apilada"}[kind]).transform;root.SetParent(parent);root.position=new Vector3(x,0,0);
        if(kind==0){Solid(root,"Hormigón",0,-.62f,1.6f,.76f,"71828A");for(int i=0;i<4;i++)Rect(root,"Reflector",-.6f+i*.4f,-.5f,.2f,.16f,"F4BA68",5);}
        else if(kind==1){Solid(root,"Sacos",0,-.51f,1.7f,.98f,"827752");for(int i=0;i<3;i++)for(int j=0;j<2;j++)Rect(root,"Costura",-.54f+i*.55f,-.83f+j*.42f,.48f,.28f,"B0A179",5);}
        else {Solid(root,"Metal",0,-.48f,1.25f,1.04f,"4B6271");Rect(root,"Panel doblado",.05f,-.38f,1.4f,.22f,"9B6955",5).transform.localRotation=Quaternion.Euler(0,0,15);Rect(root,"Remache",-.35f,-.68f,.12f,.12f,"C5B890",6);}
    }
    public static void Build(){Install();Export();}
    static void Export(){Directory.CreateDirectory(Output);var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Root+"/Scenes/MenuPrincipal.unity",Root+"/Scenes/CallesBajoControl.unity"},locationPathName=Path.Combine(Output,"NEXO-Jugable/NEXO.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});if(report.summary.result!=BuildResult.Succeeded)throw new Exception("No se pudo compilar la misión");File.WriteAllText(Path.Combine(Output,"compilacion.txt"),"Misión compilada correctamente");}
}
}
