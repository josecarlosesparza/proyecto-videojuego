using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
namespace NexoAvance {
public static class WreckSceneBuilder {
    const string Root="Assets/NexoAvance";
    static Texture2D tex;
    static void R(int x,int y,int w,int h,string hex) { ColorUtility.TryParseHtmlString("#"+hex,out var c); for(int yy=y;yy<y+h;yy++)for(int xx=x;xx<x+w;xx++)tex.SetPixel(xx,tex.height-1-yy,c); }
    static Sprite Save(string name) {
        tex.Apply(); string p=Root+"/Art/"+name+".png";File.WriteAllBytes(p,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(p,ImportAssetOptions.ForceSynchronousImport);
        var i=(TextureImporter)AssetImporter.GetAtPath(p);i.textureType=TextureImporterType.Sprite;i.spritePixelsPerUnit=24;i.filterMode=FilterMode.Point;i.textureCompression=TextureImporterCompression.Uncompressed;i.mipmapEnabled=false;i.alphaIsTransparency=true;i.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(p);
    }
    static Sprite Car() {
        tex=new Texture2D(80,40,TextureFormat.RGBA32,false);tex.SetPixels(new Color[3200]);
        // Perfil de sedán accidentado: techo hundido, capó doblado, ventanas rotas.
        R(4,33,72,4,"14202A");R(12,27,14,12,"101922");R(14,30,10,8,"283541");R(17,32,4,4,"68717A");
        R(57,27,14,12,"101922");R(59,30,10,8,"283541");R(62,32,4,4,"68717A");
        R(5,20,70,11,"781F29");R(8,18,61,10,"C33235");R(10,19,53,3,"E65349");R(7,28,64,4,"571F28");
        R(22,9,30,11,"9E2832");R(26,7,22,4,"D44240");R(20,14,37,7,"B33237");
        R(26,11,12,8,"203744");R(41,11,10,8,"243B47");R(25,16,13,3,"385565");
        R(30,11,2,4,"91A6AF");R(32,15,3,2,"91A6AF");R(45,12,2,2,"8FA8AF");R(47,14,2,3,"8FA8AF");
        R(35,7,9,3,"432B32");R(39,10,2,10,"712430");R(39,21,2,8,"6E2731");R(43,22,5,2,"2B2931");
        R(55,19,13,4,"29272D");R(61,16,10,3,"D14840");R(67,13,7,3,"9E3033");R(72,19,4,8,"3B3036");
        R(3,25,6,4,"7D8184");R(71,28,7,3,"8C8986");R(7,21,4,3,"EF7856");R(71,23,4,2,"F5C592");
        R(26,24,6,2,"67232B");R(29,26,6,2,"67232B");R(51,26,8,2,"50242D");
        return Save("AutoRojoDestrozado");
    }
    static Sprite Flame(int frame) {
        tex=new Texture2D(16,20,TextureFormat.RGBA32,false);tex.SetPixels(new Color[320]);
        int sway=frame%3-1;
        R(3,12,10,7,"B94627");R(5,7,7,11,"F07832");R(7+sway,3,3,10,"F3943E");R(4-sway,9,3,9,"E9672C");R(7,12,4,6,"FFD581");R(8,9,2,5,"FFBB57");
        return Save("FuegoAuto"+frame);
    }
    const string Pending=@"C:\Users\jose\Documents\Codex\2026-09-22\ho\work\auto-pendiente.txt";
    [InitializeOnLoadMethod] static void OnLoaded(){if(!Application.isBatchMode && File.Exists(Pending))EditorApplication.update+=RunPending;if(!Application.isBatchMode && File.Exists(Ready)){readyTime=EditorApplication.timeSinceStartup+12;EditorApplication.update+=RunReady;}}
    static void RunPending(){if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)return;EditorApplication.update-=RunPending;if(!File.Exists(Pending))return;if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;File.Delete(Pending);Build();}
    const string Ready=@"C:\Users\jose\Documents\Codex\2026-09-22\ho\work\auto-exportar.txt";
    static double readyTime;
    static void RunReady(){if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.timeSinceStartup<readyTime)return;EditorApplication.update-=RunReady;File.Delete(Ready);Export(@"C:\Users\jose\Documents\Codex\2026-09-22\ho\outputs\AutoRojo");}
    public static void Build() {
        var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-nexoOutput");string output=index>=0?args[index+1]:@"C:\Users\jose\Documents\Codex\2026-09-22\ho\outputs\AutoRojo";Directory.CreateDirectory(output);
        var scene=EditorSceneManager.OpenScene(Root+"/Scenes/PrimerAvance.unity");
        foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None)) {
            string n=t.name;
            if(n.StartsWith("Plataforma ") || n.StartsWith("Caja de suministros") || n=="Marca de caja" || n.StartsWith("Límite ") || n=="Auto rojo destrozado" || (n=="Banda de seguridad" && t.position.y> -1.05f)) UnityEngine.Object.DestroyImmediate(t.gameObject);
        }
        var car=new GameObject("Auto rojo destrozado");car.transform.position=new Vector3(6.7f,-.19f,0);
        var sr=car.AddComponent<SpriteRenderer>();sr.sprite=Car();sr.sortingOrder=6;sr.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Art/PixelMaterial.mat");
        var lower=car.AddComponent<BoxCollider2D>();lower.size=new Vector2(2.9f,.65f);lower.offset=new Vector2(0,-.36f);
        var roof=car.AddComponent<BoxCollider2D>();roof.size=new Vector2(1.35f,.45f);roof.offset=new Vector2(-.05f,.17f);
        var fire=new GameObject("Fuego decorativo sin daño");fire.transform.SetParent(car.transform,false);fire.transform.localPosition=new Vector3(1,.32f,0);fire.transform.localScale=Vector3.one*.8f;
        var flame=fire.AddComponent<SpriteRenderer>();flame.sharedMaterial=sr.sharedMaterial;flame.sortingOrder=7;
        var animation=fire.AddComponent<SmallCarFire>();animation.flame=flame;animation.frames=new[]{Flame(0),Flame(1),Flame(2)};flame.sprite=animation.frames[0];
        var enemy=UnityEngine.Object.FindAnyObjectByType<RobotShooter>();enemy.transform.position=new Vector3(11.6f,-.15f,0);
        var session=UnityEngine.Object.FindAnyObjectByType<CombatSession>();session.objective.gameObject.SetActive(false);
        var hud=(RectTransform)session.healthLabel.transform.parent;hud.anchorMin=hud.anchorMax=hud.pivot=Vector2.zero;hud.anchoredPosition=new Vector2(24,18);
        var help=session.defeatPanel.transform.Find("Ayuda");if(help)help.GetComponent<UnityEngine.UI.Text>().text="Salta el auto y esquiva las balas del centinela.";
        EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        if(!Application.isBatchMode){File.WriteAllText(Ready,"ready");readyTime=EditorApplication.timeSinceStartup+12;EditorApplication.update+=RunReady;return;}
        Export(output);
    }
    static void Export(string output){
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Root+"/Scenes/MenuPrincipal.unity",Root+"/Scenes/PrimerAvance.unity"},locationPathName=Path.Combine(output,"NEXO-Jugable/NEXO.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Error de compilación");
        File.WriteAllText(Path.Combine(output,"compilacion.txt"),"Compilación correcta");
    }
}
}
