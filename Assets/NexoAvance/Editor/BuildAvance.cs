using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.Build.Reporting;

namespace NexoAvance {
public static class BuildAvance {
    const string Root = "Assets/NexoAvance";
    static Sprite white;
    static Material spriteMaterial;
    static Transform scenery;
    static Color C(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var c); return c; }

    [MenuItem("NEXO/Crear escena del primer avance")]
    public static void Create() { Generate(Application.isBatchMode, Path.GetFullPath("..")); }
    public static void Generate(bool build, string outputDirectory) {
        if (!build && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Directory.CreateDirectory(Root + "/Art");
        Directory.CreateDirectory(Root + "/Scenes");
        white = SaveSprite("Pixel", Solid(), 1);
        spriteMaterial = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Sprites/Default"));
        string materialPath = Root + "/Art/PixelMaterial.mat";
        var previous = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (previous) { EditorUtility.CopySerialized(spriteMaterial, previous); Object.DestroyImmediate(spriteMaterial); spriteMaterial = previous; }
        else AssetDatabase.CreateAsset(spriteMaterial, materialPath);
        Sprite[] idle = { Soldier("Idle0", 0, false), Soldier("Idle1", 1, false) };
        Sprite[] run = new Sprite[6];
        for (int i = 0; i < run.Length; i++) run[i] = Soldier("Run" + i, i, true);
        Sprite jump = Soldier("Jump", 6, true);
        var previousScene = SceneManager.GetActiveScene();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, build && !Application.isBatchMode ? NewSceneMode.Additive : NewSceneMode.Single);
        SceneManager.SetActiveScene(scene);
        scenery = new GameObject("Escenario · ciudad ocupada").transform;
        var cam = new GameObject("Main Camera", typeof(Camera)).GetComponent<Camera>();
        cam.tag = "MainCamera";
        cam.orthographic = true; cam.orthographicSize = 5.4f;
        cam.transform.position = new Vector3(8, 2, -10);
        cam.backgroundColor = C("152C40"); cam.clearFlags = CameraClearFlags.SolidColor;
        // Edificios y ventanas en capas. Todas las figuras son arte original.
        for (int i = -3; i < 17; i++) {
            float x = i * 3.5f;
            float h = 4 + (i * i % 5) * .6f;
            Rect("Edificio lejano", x, -1 + h/2, 3.1f, h, "1E3B50", -30);
            Rect("Azotea", x-.7f, h-1, 1, .3f, "1E3B50", -30);
            for (int a = 0; a < 3; a++) for (int b = 0; b < 5; b++)
                Rect("Ventana", x-1+a*.8f, b*.62f, .22f, .28f, (a+b+i)%3 == 0 ? "467F85" : "294D61", -29);
        }
        for (int i = -1; i < 8; i++) {
            float x = i*6.5f;
            Rect("Muro industrial",x,-.05f,5.7f,2,"294555",-20);
            Rect("Cornisa",x,1.02f,5.9f,.15f,"496577",-19);
            Rect("Portón",x,-.05f,1.9f,1.8f,"1C3446",-18);
            for(int j=0;j<6;j++) Rect("Lámina",x,-.75f+j*.26f,1.8f,.035f,"395568",-17);
            Rect("Luz naranja",x+2, .8f,.45f,.13f,"E5A857",-16);
            Rect("Poste",x+2.7f,1.3f,.11f,4.6f,"0F263A",-15);
            Rect("Luminaria",x+2.25f,3.53f,1,.16f,"98CBC6",-14);
        }
        Rect("Franja del horizonte",-2,4.6f,90,.04f,"3D6778",-32);
        Platform("Suelo",17.5f,-1.55f,40,1.1f);
        Platform("Plataforma 01",8,.0f,3,.38f);
        Platform("Plataforma 02",13,1.3f,3,.38f);
        Platform("Plataforma 03",18,2.45f,3,.38f);
        Platform("Plataforma 04",24,.7f,4,.38f);
        // Caja sólida para practicar el salto.
        Platform("Caja de suministros",4.6f,-.55f,.9f,.9f);
        Rect("Marca de caja",4.6f,-.55f,.5f,.08f,"F4BE67",4);
        for(int i=-2;i<38;i+=2) Rect("Marca del pavimento",i,-1.31f,.8f,.045f,"74838A",4);
        Rect("Punto de encuentro / poste",32,.2f,.12f,2.4f,"7BB6B8",3);
        Rect("Punto de encuentro / bandera",32.65f,1.05f,1.3f,.6f,"56D4B5",4);
        Rect("Símbolo de encuentro",32.62f,1.05f,.65f,.1f,"173A48",5);
        Platform("Límite izquierdo",-2.8f,4,.4f,12,false);
        Platform("Límite derecho",37.8f,4,.4f,12,false);

        var hero = new GameObject("Jugador · Resistencia");
        hero.layer = 2; hero.transform.position = new Vector3(1,.2f,0);
        var rb = hero.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3; rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var box = hero.AddComponent<BoxCollider2D>(); box.size = new Vector2(.68f,1.7f);
        var friction = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(Root + "/Art/SinFriccion.physicsMaterial2D");
        if (!friction) {
            friction = new PhysicsMaterial2D("Sin fricción") { friction = 0, bounciness = 0 };
            AssetDatabase.CreateAsset(friction, Root + "/Art/SinFriccion.physicsMaterial2D");
        }
        box.sharedMaterial = friction;
        var visual = new GameObject("Sprite animado").AddComponent<SpriteRenderer>();
        visual.transform.SetParent(hero.transform,false);
        visual.sharedMaterial = spriteMaterial;
        visual.sprite = idle[0]; visual.sortingOrder = 10;
        var controller = hero.AddComponent<ResistancePlayer>();
        controller.idle = idle; controller.run = run; controller.jump = jump; controller.visual = visual;
        var view = new GameObject("Interfaz y seguimiento").AddComponent<AvanceView>();
        view.player = controller; view.worldCamera = cam;
        new GameObject("Pruebas automáticas").AddComponent<AvanceSmokeTest>().player = controller;
        EditorSceneManager.SaveScene(scene, Root + "/Scenes/PrimerAvance.unity");
        AssetDatabase.SaveAssets();
        if (build) {
            if (Application.isBatchMode) {
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Root + "/Scenes/PrimerAvance.unity", true) };
            PlayerSettings.companyName = "Proyecto académico";
            PlayerSettings.productName = "NEXO · Primer avance";
            PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.runInBackground = true;
            }
            AssetDatabase.ExportPackage(new[] { Root + "/Scripts", Root + "/Editor/BuildAvance.cs", Root + "/Art", Root + "/Scenes" }, Path.Combine(outputDirectory,"NEXO-PrimerAvance.unitypackage"), ExportPackageOptions.Recurse);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { Root + "/Scenes/PrimerAvance.unity" },
                locationPathName = Path.Combine(outputDirectory,"NEXO-Jugable/NEXO.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            if(report.summary.result != BuildResult.Succeeded) throw new System.Exception("Build failed: " + report.summary.result);
            Debug.Log("NEXO_BUILD_OK");
            File.WriteAllText(Path.Combine(outputDirectory,"NEXO-compilacion.txt"), "Compilación correcta en Unity " + Application.unityVersion + "\n" + report.summary.result);
            if (!Application.isBatchMode) { EditorSceneManager.CloseScene(scene,true); if(previousScene.IsValid()) SceneManager.SetActiveScene(previousScene); }
        }
    }
    static SpriteRenderer Rect(string name,float x,float y,float w,float h,string color,int order) {
        var s = new GameObject(name).AddComponent<SpriteRenderer>();
        s.transform.SetParent(scenery);
        s.transform.position = new Vector3(x,y,0);
        s.transform.localScale = new Vector3(w,h,1);
        s.sprite = white; s.color = C(color); s.sortingOrder = order; s.sharedMaterial = spriteMaterial;
        return s;
    }
    static void Platform(string name,float x,float y,float w,float h,bool visible=true) {
        var s = Rect(name,x,y,w,h,"344C5B",0);
        s.gameObject.AddComponent<BoxCollider2D>();
        if (!visible) { s.enabled = false; return; }
        Rect(name+" / borde",x,y+h/2-.055f,w,.11f,"82A5A6",1);
        Rect(name+" / sombra",x,y-h/2+.06f,w,.12f,"132B3E",1);
        for(float a=x-w/2+.25f; a<x+w/2; a+=.55f)
            Rect("Banda de seguridad",a,y+h/2-.17f,.23f,.12f,"DFA858",2);
    }
    static Texture2D Solid() {
        var t=new Texture2D(1,1); t.SetPixel(0,0,Color.white); t.Apply(); return t;
    }
    static Sprite SaveSprite(string name,Texture2D tex,int ppu) {
        string path=Root+"/Art/"+name+".png";
        File.WriteAllBytes(path,tex.EncodeToPNG()); Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite;
        importer.spritePixelsPerUnit=ppu;
        importer.filterMode=FilterMode.Point;
        importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled=false; importer.alphaIsTransparency=true;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    // Sprite original de 40 x 48 píxeles. Coordenadas desde arriba.
    static Sprite Soldier(string name,int frame,bool running) {
        var t=new Texture2D(40,48,TextureFormat.RGBA32,false);
        t.SetPixels(new Color[40*48]);
        int bob=running ? (frame%3==1 ? 1:0) : frame;
        void R(int x,int y,int w,int h,string col) {
            for(int yy=y; yy<y+h; yy++) for(int xx=x; xx<x+w; xx++)
                if(xx>=0 && xx<40 && yy>=0 && yy<48) t.SetPixel(xx,47-yy,C(col));
        }
        int stride=running ? new[]{-5,-2,2,5,2,-2,3}[frame] : 0;
        // Mochila, piernas y botas.
        R(9,20+bob,7,13,"101E30"); R(10,21+bob,5,10,"536B67");
        R(14-stride,33,7,10,"172638"); R(23+stride,33,6,10,"172638");
        R(15-stride,33,4,8,"647974"); R(24+stride,33,3,8,"839181");
        R(12-stride,41,10,4,"101C2C"); R(22+stride,41,10,4,"101C2C");
        R(13-stride,41,7,1,"61727B"); R(23+stride,41,7,1,"61727B");
        // Torso y chaleco.
        R(14,19+bob,14,17,"122236"); R(15,20+bob,12,13,"899B84");
        R(16,22+bob,10,9,"476762"); R(17,23+bob,3,6,"B5B28A");
        R(22,23+bob,3,6,"879979"); R(15,32+bob,13,3,"283C46");
        R(21,32+bob,3,2,"E1B365");
        // Cabeza, pelo y pañuelo.
        R(16,5+bob,12,15,"101E30"); R(18,8+bob,10,10,"DBA478");
        R(20,9+bob,8,7,"F3C28D"); R(27,12+bob,3,3,"EAB783");
        R(24,10+bob,2,2,"122236"); R(25,16+bob,3,2,"855B48");
        R(16,5+bob,12,5,"364347"); R(18,4+bob,8,2,"364347");
        R(15,8+bob,14,3,"DB6659"); R(17,8+bob,10,1,"FFAA7B");
        R(11,10+bob,6,3,"B24B4D"); R(8,12+bob,5,2,"DB6659");
        // Brazos y rifle de utilería (el disparo queda para otro avance).
        R(25,22+bob,5,7,"172A38"); R(26,23+bob,4,5,"E9B783");
        R(24,27+bob,11,4,"132537"); R(25,27+bob,9,2,"899FA1");
        R(32,25+bob,5,3,"122434"); R(36,26+bob,3,2,"96A6A7");
        R(29,30+bob,3,4,"142536"); R(24,27+bob,4,3,"F0C18B");
        t.Apply();
        return SaveSprite(name,t,24);
    }
}
}


