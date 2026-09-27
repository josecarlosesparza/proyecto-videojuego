using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
namespace NexoAvance {
public static class CombatBuilder {
    const string Root="Assets/NexoAvance";
    static Font font;
    static Sprite pixel;
    static Material material;
    [MenuItem("NEXO/Instalar combate en PrimerAvance")]
    public static void Install() {
        if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(Root+"/Scenes/PrimerAvance.unity");
        font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        pixel=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/Pixel.png");
        material=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Art/PixelMaterial.mat");
        var old=GameObject.Find("Combate"); if(old) UnityEngine.Object.DestroyImmediate(old);
        var root=new GameObject("Combate");
        var player=UnityEngine.Object.FindAnyObjectByType<ResistancePlayer>();
        var health=player.GetComponent<CombatHealth>() ?? player.gameObject.AddComponent<CombatHealth>();
        health.maximum=100; health.team=CombatTeam.Player; health.visual=player.visual;
        var shooter=player.GetComponent<PlayerCombat>() ?? player.gameObject.AddComponent<PlayerCombat>();
        shooter.movement=player; shooter.health=health; shooter.pixel=pixel; shooter.material=material;
        var enemy=new GameObject("Robot centinela"); enemy.transform.SetParent(root.transform);
        enemy.transform.position=new Vector3(10.8f,-.15f,0);
        enemy.AddComponent<BoxCollider2D>().size=new Vector2(.82f,1.68f);
        var visual=enemy.AddComponent<SpriteRenderer>(); visual.sprite=MakeRobot(); visual.sharedMaterial=material; visual.sortingOrder=10; visual.flipX=true;
        var robotHealth=enemy.AddComponent<CombatHealth>(); robotHealth.maximum=60; robotHealth.team=CombatTeam.Robot; robotHealth.visual=visual;
        var robot=enemy.AddComponent<RobotShooter>(); robot.health=robotHealth; robot.target=health;
        robot.visual=visual; robot.pixel=pixel; robot.material=material;
        var warning=new GameObject("Aviso de disparo").AddComponent<SpriteRenderer>();
        warning.transform.SetParent(enemy.transform); warning.transform.localPosition=new Vector3(0,1.1f,0); warning.transform.localScale=new Vector3(.16f,.25f,1);
        warning.sprite=pixel; warning.sharedMaterial=material; warning.color=new Color(1,.75f,.2f); warning.sortingOrder=20; warning.enabled=false;
        robot.warning=warning;
        var session=root.AddComponent<CombatSession>(); session.player=player;session.health=health;session.enemy=robot;shooter.session=session;
        var canvas=new GameObject("Interfaz de combate",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)).GetComponent<Canvas>();
        canvas.transform.SetParent(root.transform);
        canvas.renderMode=RenderMode.ScreenSpaceCamera; canvas.worldCamera=Camera.main;canvas.planeDistance=1;canvas.sortingOrder=120;
        var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
        session.objective=Text(canvas.transform,"Objetivo",new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-119),new Vector2(850,32),"OBJETIVO: NEUTRALIZA AL ROBOT",19,TextAnchor.MiddleCenter);
        session.objective.color=new Color(.8f,1,.93f);
        session.objective.gameObject.SetActive(false);
        var healthBox=Rect(canvas.transform,"Salud del jugador",Vector2.zero,Vector2.zero,new Vector2(24,18),new Vector2(300,88));
        Image(healthBox,new Color(.025f,.065f,.1f,.97f));
        session.healthLabel=Text(healthBox,"Valor de salud",Vector2.up,Vector2.up,new Vector2(18,-9),new Vector2(264,27),"SALUD  100 / 100",20,TextAnchor.MiddleLeft);
        var bar=Rect(healthBox,"Fondo de barra",Vector2.zero,Vector2.zero,new Vector2(18,18),new Vector2(264,20));Image(bar,new Color(.2f,.28f,.33f));
        var fill=new GameObject("Barra de salud",typeof(RectTransform)).GetComponent<RectTransform>();fill.SetParent(bar,false);fill.anchorMin=Vector2.zero;fill.anchorMax=Vector2.one;fill.offsetMin=fill.offsetMax=Vector2.zero;
        session.healthFill=Image(fill,new Color(.35f,.86f,.68f));session.healthFill.type=UnityEngine.UI.Image.Type.Filled;session.healthFill.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;session.healthFill.fillOrigin=0;
        var defeat=new GameObject("Derrota",typeof(RectTransform)).GetComponent<RectTransform>();defeat.SetParent(canvas.transform,false);defeat.anchorMin=Vector2.zero;defeat.anchorMax=Vector2.one;defeat.offsetMin=defeat.offsetMax=Vector2.zero;
        Image(defeat,new Color(.01f,.025f,.045f,.93f));session.defeatPanel=defeat.gameObject;
        Text(defeat,"Titulo",new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(0,110),new Vector2(660,60),"HAS SIDO DERROTADO",38,TextAnchor.MiddleCenter);
        Text(defeat,"Ayuda",new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(0,53),new Vector2(700,46),"Esquiva las balas y utiliza la caja como cobertura.",21,TextAnchor.MiddleCenter);
        session.retryButton=Button(defeat,"Reintentar",new Vector2(0,-30),"REINTENTAR  [ R ]");
        session.menuButton=Button(defeat,"Volver al menú",new Vector2(0,-102),"VOLVER AL MENÚ");
        var nav=new Navigation{mode=Navigation.Mode.Explicit,selectOnDown=session.menuButton,selectOnUp=session.menuButton};session.retryButton.navigation=nav;
        nav.selectOnDown=nav.selectOnUp=session.retryButton;session.menuButton.navigation=nav;
        defeat.gameObject.SetActive(false);
        if(!UnityEngine.Object.FindAnyObjectByType<EventSystem>()) {
            var events=new GameObject("EventSystem",typeof(EventSystem));events.transform.SetParent(root.transform);
#if ENABLE_INPUT_SYSTEM
            events.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
#else
            events.AddComponent<StandaloneInputModule>();
#endif
        }
        root.AddComponent<CombatSmokeTest>();
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        // Actualizar solo los textos: conserva las personalizaciones del menú.
        EditorSceneManager.OpenScene(Root+"/Scenes/MenuPrincipal.unity");
        foreach(var text in UnityEngine.Object.FindObjectsByType<Text>(FindObjectsInactive.Include,FindObjectsSortMode.None)) {
            if(text.name=="Lista de controles") text.text="A / D  o  ← →        Moverte\nESPACIO                 Saltar\nJ                               Disparar (mantener)\nESC                          Pausar / continuar\nR                              Reiniciar el encuentro\n\nMENÚ: flechas, Enter y ratón.";
            if(text.name=="Descripción de jugar") text.text="Enfrenta al centinela y esquiva sus disparos.\nUtiliza las plataformas y la cobertura.";
            if(text.name=="Versión") text.text="PROTOTIPO  /  03";
            if(text.name=="Ficha") text.text="MOVIMIENTO · SALTO · COMBATE";
        }
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        MainMenuBuilder.ConfigureStart();
        AssetDatabase.SaveAssets();
    }
    public static void Build() {
        var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-nexoOutput");if(i<0)throw new Exception("Falta -nexoOutput");
        string output=args[i+1];Directory.CreateDirectory(output);
        Install();
        AssetDatabase.ExportPackage(Root,Path.Combine(output,"NEXO-Combate.unitypackage"),ExportPackageOptions.Recurse);
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Root+"/Scenes/MenuPrincipal.unity",Root+"/Scenes/PrimerAvance.unity"},locationPathName=Path.Combine(output,"NEXO-Jugable/NEXO.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Error al compilar");
        File.WriteAllText(Path.Combine(output,"compilacion.txt"),"Combate compilado correctamente en Unity "+Application.unityVersion);
    }
    static RectTransform Rect(Transform p,string name,Vector2 anchor,Vector2 pivot,Vector2 position,Vector2 size) {
        var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=r.anchorMax=anchor;r.pivot=pivot;r.anchoredPosition=position;r.sizeDelta=size;return r;
    }
    static Image Image(RectTransform r,Color color) {var img=r.gameObject.AddComponent<Image>();img.sprite=pixel;img.color=color;img.raycastTarget=false;return img;}
    static Text Text(Transform p,string name,Vector2 anchor,Vector2 pivot,Vector2 position,Vector2 size,string content,int fontSize,TextAnchor alignment) {
        var t=Rect(p,name,anchor,pivot,position,size).gameObject.AddComponent<Text>();t.font=font;t.text=content;t.fontSize=fontSize;t.color=Color.white;t.alignment=alignment;t.raycastTarget=false;return t;
    }
    static Button Button(Transform p,string name,Vector2 pos,string caption) {
        var rect=Rect(p,name,new Vector2(.5f,.5f),new Vector2(.5f,.5f),pos,new Vector2(440,56));var img=Image(rect,Color.white);img.raycastTarget=true;
        var b=rect.gameObject.AddComponent<Button>();b.targetGraphic=img;var colors=b.colors;colors.normalColor=new Color(.25f,.6f,.54f);colors.highlightedColor=colors.selectedColor=new Color(.48f,.88f,.76f);colors.pressedColor=new Color(.2f,.48f,.4f);b.colors=colors;
        Text(rect,"Texto",new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(420,52),caption,22,TextAnchor.MiddleCenter);return b;
    }
    static Sprite MakeRobot() {
        var t=new Texture2D(32,40,TextureFormat.RGBA32,false);t.SetPixels(new Color[32*40]);
        void R(int x,int y,int w,int h,string hex){ColorUtility.TryParseHtmlString("#"+hex,out var c);for(int yy=y;yy<y+h;yy++)for(int xx=x;xx<x+w;xx++)t.SetPixel(xx,39-yy,c);}
        R(9,4,15,12,"101F30");R(11,5,11,9,"849DA7");R(12,8,11,3,"EF725B");R(15,2,3,3,"B7C9CB");
        R(7,17,19,15,"101F30");R(9,18,15,11,"526E7E");R(11,20,11,6,"93B2B3");R(13,21,7,3,"F2AD63");
        R(8,29,7,9,"253B4A");R(20,29,6,9,"253B4A");R(6,36,10,3,"91A4A8");R(19,36,10,3,"91A4A8");
        R(23,21,7,5,"172F40");R(25,22,6,2,"BACAC8");R(4,20,4,10,"71959B");
        t.Apply();string path=Root+"/Art/RobotCentinela.png";File.WriteAllBytes(path,t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite;importer.spritePixelsPerUnit=24;importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
}
