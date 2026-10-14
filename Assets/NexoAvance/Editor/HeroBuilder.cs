using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace NexoAvance {
public static class HeroBuilder {
    const string Root="Assets/NexoAvance", Out="C:/Users/jose/Documents/Codex/2026-09-22/ho/outputs/Protagonistas/";
    const string Marker="C:/Users/jose/Documents/Codex/2026-09-22/ho/work/heroes/pendiente.txt";
    static double after;
    [InitializeOnLoadMethod]static void Boot(){after=EditorApplication.timeSinceStartup+8;EditorApplication.update+=Tick;}
    static void Tick(){
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.timeSinceStartup<after)return;
        try {
            if(File.Exists(Marker)){if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;File.Delete(Marker);Install();File.WriteAllText(Marker+".export","build");after=EditorApplication.timeSinceStartup+15;}
            else if(File.Exists(Marker+".export")){File.Delete(Marker+".export");EditorApplication.update-=Tick;Export();}
        }catch(IOException){after=EditorApplication.timeSinceStartup+3;}catch(Exception e){File.WriteAllText(Out+"error.txt",e.ToString());EditorApplication.update-=Tick;Debug.LogException(e);}
    }
    public static void Build(){Install();Export();}
    [MenuItem("NEXO/Integrar protagonistas")] public static void Install(){
        Directory.CreateDirectory(Out);AssetDatabase.Refresh();Directory.CreateDirectory(Root+"/Resources");AssetDatabase.Refresh();
        string[] names={"Asalto","Exploradora","Artillero","Ingeniera"};int[] x0={15,458,838,1332},y0={60,66,55,86},feetX={224,635,1080,1544},feetY={813,809,813,813},gunX={452,834,1372,1747},gunY={376,416,426,395};
        var roster=AssetDatabase.LoadAssetAtPath<HeroRoster>(Root+"/Resources/HeroRoster.asset");if(!roster){roster=ScriptableObject.CreateInstance<HeroRoster>();AssetDatabase.CreateAsset(roster,Root+"/Resources/HeroRoster.asset");}
        roster.heroes=new HeroDesign[4];
        for(int i=0;i<4;i++){
            string path=Root+"/Art/Protagonistas/"+names[i]+".png";AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var imp=(TextureImporter)AssetImporter.GetAtPath(path);
            imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;imp.spritePixelsPerUnit=370;imp.filterMode=FilterMode.Point;imp.mipmapEnabled=false;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.maxTextureSize=2048;imp.npotScale=TextureImporterNPOTScale.None;imp.alphaIsTransparency=true;
            var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(path);var setting=new TextureImporterSettings();imp.ReadTextureSettings(setting);setting.spriteAlignment=(int)SpriteAlignment.Custom;setting.spritePivot=new Vector2((feetX[i]-x0[i])/(float)tex.width,1-(feetY[i]-y0[i])/(float)tex.height);setting.spriteMeshType=SpriteMeshType.FullRect;imp.SetTextureSettings(setting);imp.SaveAndReimport();
            roster.heroes[i]=new HeroDesign{name=names[i],pose=AssetDatabase.LoadAssetAtPath<Sprite>(path),muzzle=new Vector2((gunX[i]-feetX[i])/370f,(feetY[i]-gunY[i])/370f-.85f)};
        }
        EditorUtility.SetDirty(roster);AssetDatabase.SaveAssets();
        string[] missions={"CallesBajoControl","FabricaAutomatizada","NucleoNexo"};
        foreach(string name in missions){
            string path=Root+"/Scenes/"+name+".unity";string backup=Out+"Antes/"+name+".unity";Directory.CreateDirectory(Out+"Antes");if(!File.Exists(backup))File.Copy(path,backup);
            var scene=EditorSceneManager.OpenScene(path);var players=UnityEngine.Object.FindObjectsByType<ResistancePlayer>(FindObjectsSortMode.None);if(players.Length!=1)throw new Exception("Cantidad de jugadores inválida: "+name);
            var p=players[0];var look=p.GetComponent<HeroAppearance>();if(!look)look=p.gameObject.AddComponent<HeroAppearance>();look.roster=roster;look.Initialize(p);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
        var menuScene=EditorSceneManager.OpenScene(Root+"/Scenes/MenuPrincipal.unity");
        var menuHero=GameObject.Find("Integrante de la resistencia");if(menuHero){var portrait=menuHero.GetComponent<UnityEngine.UI.Image>();portrait.sprite=roster.heroes[0].pose;portrait.preserveAspect=true;EditorSceneManager.MarkSceneDirty(menuScene);EditorSceneManager.SaveScene(menuScene);}
        AssetDatabase.SaveAssets();
        File.WriteAllText(Out+"integracion.txt","Cuatro sprites, catálogo y tres escenas guardados.");
    }
    [MenuItem("NEXO/Compilar protagonistas")] public static void Export(){
        var scenes=new[]{"MenuPrincipal","CallesBajoControl","FabricaAutomatizada","NucleoNexo"}.Select(n=>Root+"/Scenes/"+n+".unity").ToArray();
        var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=Out+"NEXO/NEXO.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
        File.WriteAllText(Out+"compilacion.txt",result.summary.result.ToString());if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Falló compilación de protagonistas");
    }
}
}
