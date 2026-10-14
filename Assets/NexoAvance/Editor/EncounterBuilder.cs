using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace NexoAvance {
public static class EncounterBuilder {
    const string Root="Assets/NexoAvance",Out="C:/Users/jose/Documents/Codex/2026-09-22/ho/outputs/Encuentros/",Marker="C:/Users/jose/Documents/Codex/2026-09-22/ho/work/encounters/pending";
    static double after;
    [InitializeOnLoadMethod]static void Boot(){after=EditorApplication.timeSinceStartup+10;EditorApplication.update+=Tick;}
    static void Tick(){if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.timeSinceStartup<after)return;
        try{if(File.Exists(Marker)){if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;File.Delete(Marker);Install();File.WriteAllText(Marker+".export","build");after=EditorApplication.timeSinceStartup+15;}
        else if(File.Exists(Marker+".export")){File.Delete(Marker+".export");EditorApplication.update-=Tick;Export();}}
        catch(IOException){after=EditorApplication.timeSinceStartup+3;}catch(Exception e){File.WriteAllText(Out+"error.txt",e.ToString());EditorApplication.update-=Tick;Debug.LogException(e);}}
    [MenuItem("NEXO/Integrar encuentros por avance")]public static void Install(){
        Directory.CreateDirectory(Out+"Antes");Directory.CreateDirectory(Root+"/Prefabs/Refuerzos");AssetDatabase.Refresh();
        string[] missions={"CallesBajoControl","FabricaAutomatizada","NucleoNexo"};
        for(int m=0;m<3;m++){
            string path=Root+"/Scenes/"+missions[m]+".unity";if(!File.Exists(Out+"Antes/"+missions[m]+".unity"))File.Copy(path,Out+"Antes/"+missions[m]+".unity");
            var scene=EditorSceneManager.OpenScene(path);var session=UnityEngine.Object.FindAnyObjectByType<CombatSession>();var existing=UnityEngine.Object.FindAnyObjectByType<EncounterDirector>();if(existing)UnityEngine.Object.DestroyImmediate(existing.gameObject);
            var original=UnityEngine.Object.FindObjectsByType<StreetEnemy>(FindObjectsInactive.Include);var root=new GameObject("ENCUENTROS · refuerzos por avance");var d=root.AddComponent<EncounterDirector>();d.session=session;d.routeRight=m==0?199:132;d.maxActive=new[]{9,11,13}[m];
            var kinds=original.Select(e=>e.kind).Distinct().OrderBy(k=>(int)k).ToArray();d.templates=new StreetEnemy[kinds.Length];
            for(int k=0;k<kinds.Length;k++){
                var copy=UnityEngine.Object.Instantiate(original.First(e=>e.kind==kinds[k]).gameObject);copy.SetActive(false);copy.name=kinds[k].ToString();copy.transform.position=Vector3.zero;var enemy=copy.GetComponent<StreetEnemy>();enemy.target=null;enemy.origin=Vector3.zero;enemy.damage=enemy.kind==StreetEnemyKind.Fusilero?10:8;enemy.attackInterval=enemy.kind==StreetEnemyKind.Dron?2.8f:2.3f;enemy.warningTime=(enemy.kind==StreetEnemyKind.Blindado||enemy.kind==StreetEnemyKind.Torreta)?.9f:.65f;
                var box=copy.GetComponent<BoxCollider2D>();box.size=new Vector2(1/copy.transform.localScale.x,1.7f/copy.transform.localScale.y);box.offset=Vector2.zero;
                var body=copy.GetComponent<Rigidbody2D>();if(!body)body=copy.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;body.gravityScale=0;body.freezeRotation=true;body.interpolation=RigidbodyInterpolation2D.Interpolate;
                if(!copy.GetComponent<EnemyPursuit>())copy.AddComponent<EnemyPursuit>();
                string prefab=Root+"/Prefabs/Refuerzos/M"+(m+1)+"-"+kinds[k]+".prefab";PrefabUtility.SaveAsPrefabAsset(copy,prefab);UnityEngine.Object.DestroyImmediate(copy);d.templates[k]=AssetDatabase.LoadAssetAtPath<GameObject>(prefab).GetComponent<StreetEnemy>();
            }
            var street=session.GetComponent<StreetMission>();var campaign=session.GetComponent<CampaignMission>();
            if(street){street.encounters=d;foreach(var sector in street.sectors)foreach(var group in sector.waves)group.SetActive(false);}else{campaign.encounters=d;foreach(var group in campaign.groups)group.SetActive(false);}
            float[][] triggers={new[]{4f,26,51,76,103,130,151},new[]{4f,20,39,54,72,86,98},new[]{4f,18,33,43,58,70,84,98}};
            int[][] counts={new[]{3,4,5,6,4,7,6},new[]{4,6,4,7,6,8,5},new[]{4,6,5,7,6,8,7,8}};
            d.encounters=new AdvanceEncounter[triggers[m].Length];
            for(int z=0;z<d.encounters.Length;z++)d.encounters[z]=new AdvanceEncounter{label=(z%3==1?"Refuerzo intenso ":z%3==2?"Reagrupación ":"Encuentro ")+(z+1),triggerX=triggers[m][z],count=counts[m][z],interval=m==0?.85f:m==1?.7f:.6f,initialDelay=z%3==2?2.5f:.6f,allowRear=z%3==1||z==d.encounters.Length-1,composition=z==0?new[]{StreetEnemyKind.Fusilero,StreetEnemyKind.Fusilero,StreetEnemyKind.Dron}:kinds};
            // NEXO doors constrain entry; these encounters can also send accessible rear reinforcements.
            if(m==2){d.encounters[2].allowRear=true;d.encounters[5].allowRear=true;}
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(Root+"/Scenes/MenuPrincipal.unity");File.WriteAllText(Out+"integracion.txt","Tres misiones integradas: 35 / 40 / 51 enemigos; máximos activos 9 / 11 / 13.");
    }
    [MenuItem("NEXO/Compilar encuentros")]public static void Export(){var scenes=new[]{"MenuPrincipal","CallesBajoControl","FabricaAutomatizada","NucleoNexo"}.Select(n=>Root+"/Scenes/"+n+".unity").ToArray();var r=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=Out+"NEXO/NEXO.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});File.WriteAllText(Out+"compilacion.txt",r.summary.result.ToString());if(r.summary.result!=BuildResult.Succeeded)throw new Exception("Compilación fallida");}
}
}
