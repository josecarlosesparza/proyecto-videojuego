using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace NexoAvance {
public static class ReferenceMissionBuilder {
    const string Root="Assets/NexoAvance",Art=Root+"/Art/Mision1Referencia";
    const string Work="C:/Users/jose/Documents/Codex/2026-09-22/ho/";
    public const string Output=Work+"outputs/Mision1Referencia/";
    static double after;
    [InitializeOnLoadMethod]static void Boot(){after=EditorApplication.timeSinceStartup+8;EditorApplication.update+=Tick;}
    static void Tick(){
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.timeSinceStartup<after)return;
        if(File.Exists(Work+"work/referencia-pendiente.txt")){
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            File.Delete(Work+"work/referencia-pendiente.txt");
            try{Install();File.WriteAllText(Work+"work/referencia-exportar.txt","build");after=EditorApplication.timeSinceStartup+15;}catch(Exception e){File.WriteAllText(Output+"error.txt",e.ToString());throw;}
        }else if(File.Exists(Work+"work/referencia-exportar.txt")){
            File.Delete(Work+"work/referencia-exportar.txt");EditorApplication.update-=Tick;
            try{Export();}catch(Exception e){File.WriteAllText(Output+"error.txt",e.ToString());throw;}
        }
    }
    static Sprite ImportPanel(string name){string path=Art+"/"+name+".png";AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;imp.spritePixelsPerUnit=60;imp.filterMode=FilterMode.Point;imp.mipmapEnabled=false;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.maxTextureSize=4096;imp.npotScale=TextureImporterNPOTScale.None;
        var settings=new TextureImporterSettings();imp.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteAlignment=(int)SpriteAlignment.Center;imp.SetTextureSettings(settings);imp.SaveAndReimport();
        var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(!sprite)throw new Exception("Sprite no importado: "+path);return sprite;
    }
    static Sprite[] Obstacles(){string path=Art+"/Obstaculos.png";AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Multiple;imp.isReadable=true;imp.maxTextureSize=4096;imp.npotScale=TextureImporterNPOTScale.None;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.filterMode=FilterMode.Point;imp.mipmapEnabled=false;imp.alphaIsTransparency=true;imp.SaveAndReimport();
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);var pixels=texture.GetPixels32();var rects=new SpriteMetaData[3];
        for(int cell=0;cell<3;cell++){int minX=texture.width,maxX=0,minY=texture.height,maxY=0;for(int y=0;y<texture.height;y++)for(int x=cell*texture.width/3;x<(cell+1)*texture.width/3;x++)if(pixels[y*texture.width+x].a>128){minX=Math.Min(x,minX);maxX=Math.Max(x,maxX);minY=Math.Min(y,minY);maxY=Math.Max(y,maxY);}rects[cell]=new SpriteMetaData{name="Obstaculo"+cell,rect=new Rect(minX,minY,maxX-minX+1,maxY-minY+1),alignment=(int)SpriteAlignment.BottomCenter,pivot=new Vector2(.5f,0)};}
        imp.spritesheet=rects;imp.isReadable=false;imp.SaveAndReimport();return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
    }
    static float[] Seam(string previous,string next){
        var a=new Texture2D(2,2);var b=new Texture2D(2,2);a.LoadImage(File.ReadAllBytes(Art+"/"+previous+".png"));b.LoadImage(File.ReadAllBytes(Art+"/"+next+".png"));
        const int width=135;int height=a.height;var ap=a.GetPixels32();var bp=b.GetPixels32();var cost=new float[height,width];var parent=new int[height,width];
        for(int y=0;y<height;y++)for(int x=4;x<width-4;x++){
            var ca=ap[y*a.width+a.width-width+x];var cb=bp[y*b.width+x];float dr=ca.r-cb.r,dg=ca.g-cb.g,db=ca.b-cb.b;
            float local=dr*dr+dg*dg+db*db+.02f*(x-width*.5f)*(x-width*.5f);
            float best=0;int from=x;
            if(y>0){best=float.MaxValue;for(int k=Math.Max(4,x-2);k<=Math.Min(width-5,x+2);k++){float candidate=cost[y-1,k]+Math.Abs(k-x)*5;if(candidate<best){best=candidate;from=k;}}}
            cost[y,x]=local+best;parent[y,x]=from;
        }
        int end=4;for(int x=5;x<width-4;x++)if(cost[height-1,x]<cost[height-1,end])end=x;
        var curve=new float[height];for(int y=height-1;y>=0;y--){curve[y]=(end+.5f)/b.width;end=parent[y,end];}
        UnityEngine.Object.DestroyImmediate(a);UnityEngine.Object.DestroyImmediate(b);return curve;
    }
    [MenuItem("NEXO/Integrar escenario de referencia en misión 1")]
    public static void Install(){
        Directory.CreateDirectory(Output);AssetDatabase.Refresh();
        string[] names={"00-Original","01-Pueblo","02-Ruinas","03-Arboleda","04-Afueras","05-Taller"};var sprites=names.Select(ImportPanel).ToArray();var obstacles=Obstacles();
        var shader=Shader.Find("NEXO/Reference Backdrop Blend");if(!shader)throw new Exception("Shader de unión no encontrado");
        var scene=EditorSceneManager.OpenScene(Root+"/Scenes/CallesBajoControl.unity");
        var existing=GameObject.Find("MISIÓN 1 · paisaje de referencia");if(existing)UnityEngine.Object.DestroyImmediate(existing);
        var world=GameObject.Find("Avenida · Calles Bajo Control").transform;
        string[] oldDecor={"Edificio","Ventana","Fachada","Cornisa","Persiana","Lámina","Poste","Farola","Pavimento","Borde de acera","Zona del jefe"};
        foreach(Transform t in world.Cast<Transform>().ToArray())if(oldDecor.Contains(t.name))UnityEngine.Object.DestroyImmediate(t.gameObject);
        var oldDay=GameObject.Find("ESCENARIO DIURNO · 4 zonas");if(oldDay)UnityEngine.Object.DestroyImmediate(oldDay);
        world.Find("Avenida").GetComponent<SpriteRenderer>().enabled=false;
        foreach(string border in new[]{"Limite de avenida","Final de avenida"})if(world.Find(border))world.Find(border).GetComponent<SpriteRenderer>().enabled=false;
        var root=new GameObject("MISIÓN 1 · paisaje de referencia");var backdrop=root.AddComponent<ReferenceBackdrop>();backdrop.worldCamera=Camera.main;backdrop.panels=new SpriteRenderer[6];float left=-5;
        for(int i=0;i<sprites.Length;i++){
            var sprite=sprites[i];float width=sprite.bounds.size.x,height=sprite.bounds.size.y;
            var sr=new GameObject(names[i]).AddComponent<SpriteRenderer>();sr.transform.SetParent(root.transform,false);
            // Walkway/retaining-wall boundary occurs at 80 percent of each image's height.
            sr.transform.position=new Vector3(left+width/2,-1+height*.30f,0);sr.sprite=sprite;sr.sortingOrder=-100+i;
            string path=Art+"/Panel"+i+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(!mat){mat=new Material(shader);AssetDatabase.CreateAsset(mat,path);}mat.shader=shader;mat.SetFloat("_LeftFade",i==0?0:2.25f/width);sr.sharedMaterial=mat;EditorUtility.SetDirty(mat);backdrop.panels[i]=sr;left+=width-2.25f;
            if(i>0){var seam=sr.gameObject.AddComponent<BackdropSeam>();seam.columns=Seam(names[i-1],names[i]);seam.enabled=false;seam.enabled=true;}
        }
        backdrop.left=-5;backdrop.right=left+2.25f;
        // Preserve physical obstacles; replace their old flat decoration with detailed isolated sprites.
        foreach(Transform t in world){int kind=Array.IndexOf(new[]{"Barrera de hormigón","Sacos de contención","Chatarra apilada"},t.name);if(kind<0)continue;
            var collider=t.GetComponentInChildren<BoxCollider2D>();if(!collider)continue;var bounds=collider.bounds;
            var oldVisual=t.Find("Aspecto de referencia");if(oldVisual)UnityEngine.Object.DestroyImmediate(oldVisual.gameObject);
            foreach(var renderer in t.GetComponentsInChildren<SpriteRenderer>())renderer.enabled=false;
            var visual=new GameObject("Aspecto de referencia").AddComponent<SpriteRenderer>();visual.transform.SetParent(t,false);visual.transform.position=new Vector3(bounds.center.x,bounds.min.y,0);visual.sprite=obstacles[kind];visual.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Art/PixelMaterial.mat");visual.sortingOrder=6;visual.transform.localScale=Vector3.one*(bounds.size.y/obstacles[kind].bounds.size.y);
        }
        Camera.main.backgroundColor=new Color(.43f,.7f,.88f);Camera.main.transform.position=new Vector3(8,2,-10);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        File.WriteAllText(Output+"integracion.txt","Escena activa: CallesBajoControl\n6 panoramas distintos (original intacto + 5 extensiones), escala uniforme 60 px/unidad.\nCobertura X: "+backdrop.left+" a "+backdrop.right+".\nSin parallax de imágenes aplanadas. Obstáculos y combate conservados.\n");
    }
    static void Export(){var scenes=new[]{Root+"/Scenes/MenuPrincipal.unity",Root+"/Scenes/CallesBajoControl.unity",Root+"/Scenes/FabricaAutomatizada.unity",Root+"/Scenes/NucleoNexo.unity"};var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=Output+"NEXO/NEXO.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Falló compilación");File.WriteAllText(Output+"compilacion.txt","CORRECTO");}
}
}
