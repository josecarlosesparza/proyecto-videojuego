using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
namespace NexoAvance {
public static class RobotBuilder {
 const string Root="Assets/NexoAvance",Out="C:/Users/jose/Documents/Codex/2026-09-22/ho/outputs/Robots/",Marker="C:/Users/jose/Documents/Codex/2026-09-22/ho/work/robots/pending";
 static double after;
 [InitializeOnLoadMethod]static void Boot(){after=EditorApplication.timeSinceStartup+8;EditorApplication.update+=Tick;}
 static void Tick(){if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.timeSinceStartup<after)return;try{if(File.Exists(Marker)){if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;File.Delete(Marker);Install();File.WriteAllText(Marker+".export","build");after=EditorApplication.timeSinceStartup+12;}else if(File.Exists(Marker+".export")){File.Delete(Marker+".export");EditorApplication.update-=Tick;Export();}}catch(IOException){after=EditorApplication.timeSinceStartup+3;}catch(Exception e){File.WriteAllText(Out+"error.txt",e.ToString());EditorApplication.update-=Tick;Debug.LogException(e);}}
 [MenuItem("NEXO/Integrar escuadrón y calamar")]
 public static void Install(){
  Directory.CreateDirectory(Out+"Antes");Directory.CreateDirectory(Root+"/Profiles/Robots");Directory.CreateDirectory(Root+"/Prefabs/RobotSquad");AssetDatabase.Refresh();
  string[] names={"Soldado","Cazador","Artillero","Bombardero","Incinerador","Electrico"};float[] heights={1.7f,1.85f,2.3f,1.65f,1.95f,1.45f};
  foreach(string name in names.Skip(1).Concat(new[]{"NaranjaReferencia","Calamar"})){string path=Root+"/Art/RobotSquad/"+name+".png";var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);var ti=(TextureImporter)AssetImporter.GetAtPath(path);ti.textureType=TextureImporterType.Sprite;ti.spriteImportMode=SpriteImportMode.Single;ti.spritePixelsPerUnit=texture.height/(name=="Calamar"?7.6f:name=="NaranjaReferencia"?1.7f:heights[Array.IndexOf(names,name)]);ti.spritePivot=new Vector2(.5f,0);var settings=new TextureImporterSettings();ti.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=new Vector2(.5f,0);settings.spriteMeshType=SpriteMeshType.FullRect;ti.SetTextureSettings(settings);ti.filterMode=FilterMode.Point;ti.textureCompression=TextureImporterCompression.Uncompressed;ti.mipmapEnabled=false;ti.alphaIsTransparency=true;ti.SaveAndReimport();}
  var orange=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/Refuerzos/M1-Fusilero.prefab");var profiles=new RobotProfile[6];var prefabs=new StreetEnemy[6];
  StreetEnemyKind[] kinds={StreetEnemyKind.Fusilero,StreetEnemyKind.Cazador,StreetEnemyKind.Blindado,StreetEnemyKind.Bombardero,StreetEnemyKind.Incinerador,StreetEnemyKind.Electrico};
  for(int i=0;i<6;i++){
   string path=Root+"/Profiles/Robots/"+names[i]+".asset";var r=AssetDatabase.LoadAssetAtPath<RobotProfile>(path);if(!r){r=ScriptableObject.CreateInstance<RobotProfile>();AssetDatabase.CreateAsset(r,path);}profiles[i]=r;r.displayName=names[i];r.kind=kinds[i];r.attack=(RobotAttack)i;r.pose=i==0?orange.GetComponent<StreetEnemy>().visual.sprite:AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/RobotSquad/"+names[i]+".png");
   r.health=new[]{100,80,190,120,120,85}[i];r.damage=new[]{8,5,6,20,5,12}[i];r.speed=new[]{2.8f,4.1f,1.8f,2.1f,3.1f,3.4f}[i];r.stopDistance=new[]{4.5f,4.2f,6,7,1.8f,4.5f}[i];r.range=i==4?3:12;r.warning=new[]{.65f,.65f,1.3f,1,.9f,1}[i];r.cooldown=new[]{2.3f,2.3f,3.2f,3.8f,2.1f,2.8f}[i];r.burstCount=new[]{1,4,8,1,1,1}[i];r.burstInterval=i==2?.17f:.2f;r.projectileSpeed=i==5?6:7;r.heavy=i==2||i==3;r.flying=i==5;
   r.muzzleA=new[]{new Vector2(.78f,-.1f),new Vector2(.8f,.25f),new Vector2(1.2f,.22f),new Vector2(.33f,.65f),new Vector2(1.16f,.1f),new Vector2(.67f,-.36f)}[i];r.muzzleB=i==1?new Vector2(.03f,.18f):r.muzzleA;EditorUtility.SetDirty(r);
   var go=UnityEngine.Object.Instantiate(orange);go.SetActive(false);go.name=names[i];var enemy=go.GetComponent<StreetEnemy>();enemy.kind=kinds[i];enemy.health.maximum=r.health;enemy.damage=r.damage;enemy.target=null;enemy.transform.localScale=Vector3.one;enemy.GetComponent<BoxCollider2D>().size=new Vector2(1,1.7f);
   if(i!=0){enemy.visual.enabled=false;var art=new GameObject("Diseño original · pose estática").AddComponent<SpriteRenderer>();art.transform.SetParent(go.transform,false);art.transform.localPosition=Vector3.down*.85f;art.sprite=r.pose;art.sharedMaterial=enemy.material;art.sortingOrder=10;enemy.visual=art;enemy.health.visual=art;}
   var arsenal=go.AddComponent<RobotArsenal>();arsenal.profile=r;var mover=go.GetComponent<EnemyPursuit>();mover.moveSpeed=r.speed;mover.attackDistance=r.stopDistance;mover.separation=1.05f;
   string prefab=Root+"/Prefabs/RobotSquad/"+names[i]+".prefab";PrefabUtility.SaveAsPrefabAsset(go,prefab);UnityEngine.Object.DestroyImmediate(go);prefabs[i]=AssetDatabase.LoadAssetAtPath<GameObject>(prefab).GetComponent<StreetEnemy>();
  }
  string[] scenes={"CallesBajoControl","FabricaAutomatizada","NucleoNexo"};
  int[][][] mixes={new[]{new[]{0},new[]{0,0,1},new[]{0,1,0},new[]{1,0,1},new[]{0,1,0},new[]{0,1,0,1,0,3,0},new[]{1,0,1,0,3,0}},new[]{new[]{0,1},new[]{0,1,2,0},new[]{0,4,0},new[]{0,5,1,0},new[]{1,0,3,0,2,0},new[]{0,1,4,0,5,0,1,2},new[]{1,0,3,0,5}},new[]{new[]{0,1},new[]{1,5,0,5,1,0},new[]{0,3,1,4,0},new[]{1,0,2,0,1,5,0},new[]{0,4,1,0,3,0},new[]{5,1,0,5,1,0,2,0},new[]{0,1,4,0,3,1,0},new[]{1,0,2,5,0,4,0,3}}};
  for(int m=0;m<3;m++){
   string path=Root+"/Scenes/"+scenes[m]+".unity";if(!File.Exists(Out+"Antes/"+scenes[m]+".unity"))File.Copy(path,Out+"Antes/"+scenes[m]+".unity");var scene=EditorSceneManager.OpenScene(path);var d=UnityEngine.Object.FindAnyObjectByType<EncounterDirector>();d.templates=m==0?new[]{prefabs[0],prefabs[1],prefabs[3]}:prefabs;d.maxHeavyActive=2;d.maxBossSupports=3;for(int z=0;z<d.encounters.Length;z++)d.encounters[z].composition=mixes[m][z].Select(i=>kinds[i]).ToArray();
   if(m==0){var mission=d.session.GetComponent<StreetMission>();var boss=mission.boss;boss.encounters=d;mission.bossTrigger=172;boss.health.maximum=1200;foreach(var sr in boss.GetComponentsInChildren<SpriteRenderer>(true))sr.enabled=false;foreach(var arm in boss.arms)arm.gameObject.SetActive(false);boss.arms=new Transform[0];
    var old=boss.transform.Find("Calamar · diseño definitivo");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);var art=new GameObject("Calamar · diseño definitivo").AddComponent<SpriteRenderer>();art.transform.SetParent(boss.transform,false);art.transform.localPosition=new Vector3(0,-1-boss.transform.position.y,0);art.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/RobotSquad/Calamar.png");art.sharedMaterial=boss.material;art.sortingOrder=10;boss.art=art;boss.health.visual=art;
    var box=boss.GetComponent<BoxCollider2D>();box.size=new Vector2(7,5.6f);box.offset=new Vector2(.7f,1.1f);
    foreach(var prior in boss.GetComponentsInChildren<BossCore>(true))UnityEngine.Object.DestroyImmediate(prior.gameObject);
    var core=new GameObject("Núcleo azul · punto débil fase 3");core.transform.SetParent(boss.transform,false);core.transform.localPosition=new Vector3(.5f,2.8f-boss.transform.position.y,0);var weak=core.AddComponent<BossCore>();weak.boss=boss;boss.core=weak;var circle=core.AddComponent<CircleCollider2D>();circle.radius=.55f;circle.isTrigger=true;circle.enabled=false;
    boss.warning.gameObject.SetActive(true);boss.warning.transform.localScale=Vector3.one*.35f;boss.warning.enabled=false;
   }
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
  }
  AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(Root+"/Scenes/MenuPrincipal.unity");File.WriteAllText(Out+"integracion.txt","Seis perfiles, tres misiones, naranja original conservado, calamar integrado.");
 }
 [MenuItem("NEXO/Compilar escuadrón y calamar")]public static void Export(){var r=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"MenuPrincipal","CallesBajoControl","FabricaAutomatizada","NucleoNexo"}.Select(n=>Root+"/Scenes/"+n+".unity").ToArray(),locationPathName=Out+"NEXO/NEXO.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});File.WriteAllText(Out+"compilacion.txt",r.summary.result.ToString());}
}
}
