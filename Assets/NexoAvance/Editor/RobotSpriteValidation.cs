using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NexoAvance {
public static class RobotSpriteValidation {
    const string Root="C:/Users/jose/Documents/Codex/2026-09-22/ho/";
    const string Asset="Assets/NexoAvance/Art/RobotFusileroNaranja.png";
    static double after;
    [InitializeOnLoadMethod] static void Boot(){after=EditorApplication.timeSinceStartup+8;EditorApplication.update+=Tick;}
    static void Tick(){
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.timeSinceStartup<after)return;
        if(!File.Exists(Root+"work/robot-validar-pendiente.txt"))return;
        EditorApplication.update-=Tick;
        try {
            AssetDatabase.ImportAsset(Asset,ImportAssetOptions.ForceSynchronousImport|ImportAssetOptions.ForceUpdate);
            var importer=(TextureImporter)AssetImporter.GetAtPath(Asset);
            if(!importer)throw new Exception("Unity no pudo importar la textura");
            importer.textureType=TextureImporterType.Sprite;
            importer.spriteImportMode=SpriteImportMode.Single;
            importer.filterMode=FilterMode.Point;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled=false;
            importer.alphaIsTransparency=true;
            importer.SaveAndReimport();
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Asset);
            if(!sprite)throw new Exception("El recurso importado no contiene un Sprite");
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite,out string guid,out long id);
            if(guid!="99432d4778f248ca8f5cc872daa12851" || id!=21300000)throw new Exception("La referencia importada no coincide con las escenas: "+guid+" / "+id);
            var report=new StringBuilder("OK Sprite importado por Unity: "+sprite.name+"\nRect: "+sprite.rect+"\nBounds: "+sprite.bounds.size+"\n");
            foreach(var sceneName in new[]{"CallesBajoControl","FabricaAutomatizada","NucleoNexo"}){
                var scene=EditorSceneManager.OpenPreviewScene("Assets/NexoAvance/Scenes/"+sceneName+".unity");
                try {
                    int count=0;
                    foreach(var root in scene.GetRootGameObjects())foreach(var enemy in root.GetComponentsInChildren<StreetEnemy>(true)){
                        if(enemy.kind!=StreetEnemyKind.Fusilero)continue;
                        if(!enemy.visual || enemy.visual.sprite!=sprite || !enemy.visual.enabled || enemy.visual.color.a<=0)throw new Exception(sceneName+": fusilero sin sprite visible");
                        count++;
                    }
                    if(count==0)throw new Exception(sceneName+": no se encontraron fusileros");
                    report.AppendLine("OK "+sceneName+": "+count+" fusileros con sprite válido y visible");
                } finally {EditorSceneManager.ClosePreviewScene(scene);}
            }
            report.AppendLine("RESULTADO: CORRECTO");
            File.WriteAllText(Root+"outputs/RobotNaranja/validacion-unity.txt",report.ToString());
            File.Delete(Root+"work/robot-validar-pendiente.txt");
        }catch(Exception e){File.WriteAllText(Root+"outputs/RobotNaranja/validacion-unity.txt","ERROR: "+e);Debug.LogException(e);}
    }
}
}
