using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
namespace NexoAvance {
public static class DaylightLevelBuilder {
    const string Root="Assets/NexoAvance";
    const string Work="C:/Users/jose/Documents/Codex/2026-09-22/ho/";
    const string ScenePath=Root+"/Scenes/CallesBajoControl_Dia.unity";
    static Sprite pixel;static Material material;static double after;
    [InitializeOnLoadMethod]static void Boot(){after=EditorApplication.timeSinceStartup+8;EditorApplication.update+=Tick;}
    static void Tick(){
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.timeSinceStartup<after)return;
        if(File.Exists(Work+"work/dia-pendiente.txt")){
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            File.Delete(Work+"work/dia-pendiente.txt");
            try{Create();File.WriteAllText(Work+"work/dia-exportar.txt","build");after=EditorApplication.timeSinceStartup+15;}catch(Exception e){File.WriteAllText(Work+"outputs/EscenarioDia/error.txt",e.ToString());throw;}
        }else if(File.Exists(Work+"work/dia-exportar.txt")){
            File.Delete(Work+"work/dia-exportar.txt");EditorApplication.update-=Tick;
            try{Export();}catch(Exception e){File.WriteAllText(Work+"outputs/EscenarioDia/error.txt",e.ToString());throw;}
        }
    }
    static Color C(string hex){ColorUtility.TryParseHtmlString("#"+hex,out var c);return c;}
    static Transform Group(Transform p,string n){var t=new GameObject(n).transform;t.SetParent(p,false);return t;}
    static SpriteRenderer R(Transform p,string n,float x,float y,float w,float h,string color,int order=-20,float angle=0){
        var s=new GameObject(n).AddComponent<SpriteRenderer>();s.transform.SetParent(p,false);s.transform.localPosition=new Vector3(x,y,0);s.transform.localScale=new Vector3(w,h,1);s.transform.localRotation=Quaternion.Euler(0,0,angle);s.sprite=pixel;s.sharedMaterial=material;s.color=C(color);s.sortingOrder=order;return s;
    }
    static Transform Layer(Transform p,string n,float fraction,Camera camera){var t=Group(p,n);var para=t.gameObject.AddComponent<DaylightParallax>();para.followFraction=fraction;para.targetCamera=camera;return t;}
    static void Window(Transform p,float x,float y,string shutter,int o){R(p,"Marco de piedra",x,y,.92f,1.04f,"D8CFAE",o);R(p,"Hueco de ventana",x,y,.65f,.83f,"52636A",o+1);R(p,"Reflejo",x-.12f,y+.1f,.12f,.45f,"9AAEAA",o+2);R(p,"Contraventana",x-.55f,y,.22f,.95f,shutter,o+1);R(p,"Contraventana",x+.55f,y,.22f,.95f,shutter,o+1);R(p,"Alfeizar",x,y-.53f,1.2f,.12f,"E4D8B6",o+2);}
    static void Ivy(Transform p,float x,float y,int seed,int o){for(int j=0;j<7;j++){float dy=j*.28f,dx=Mathf.Sin(j*1.8f+seed)*.18f;R(p,"Hiedra",x+dx,y+dy,.45f,.35f,j%2==0?"6C8552":"92A565",o);}}
    static Transform House(Transform parent,int variant){
        var g=Group(parent,"Casa modular "+variant);float w=4.3f+variant*.45f,h=3.4f+(variant%3)*1.05f;
        string[] walls={"C5B899","B7BAAA","C8AA91","B7B6A0"};string wall=walls[variant%4];
        R(g,"Sombra lateral",.15f,h/2-1,w,h,"918C7E",-27);R(g,"Fachada",-.12f,h/2-1,w-.3f,h,wall,-26);
        R(g,"Zocalo",0,-.72f,w,.55f,"A39983",-25);
        for(int floor=0;floor<(variant%3==0?1:2);floor++)for(int k=0;k<2;k++)Window(g,-w*.26f+k*w*.52f,.7f+floor*1.55f,variant%2==0?"697F73":"819080",-24);
        R(g,"Puerta",0,-.16f,.85f,1.68f,"667369",-23);R(g,"Jamba",-.51f,-.13f,.14f,1.75f,"E1D4B5",-22);
        for(int row=0;row<4;row++){float roofW=w+1-row*.42f;R(g,"Tejado escalonado",0,h-1+.12f+row*.2f,roofW,.24f,row%2==0?"AA7355":"BC8962",-23);for(float x=-roofW/2+.15f;x<roofW/2;x+=.44f)R(g,"Junta teja",x,h-1+.18f+row*.2f,.05f,.12f,"D09C6D",-22);}
        R(g,"Chimenea",w*.26f,h-.05f,.55f,1.05f,"B2A080",-25);R(g,"Remate chimenea",w*.26f,h+.45f,.7f,.14f,"D4C3A2",-23);
        if(variant%2==0)Ivy(g,-w*.45f,-.65f,variant,-21);
        for(int i=0;i<4;i++)R(g,"Ladrillo descubierto",w*.35f+(i%2)*.16f,-.45f+i*.18f,.38f,.12f,"A78F76",-23);
        return g;
    }
    static Transform Ruin(Transform p,int variant){
        var g=Group(p,"Ruina modular "+variant);float w=5.1f+variant*.6f,h=4+variant*.65f;
        R(g,"Interior en sombra",0,(h-1)/2-.5f,w,h,"8F9890",-29);
        for(int floor=0;floor<3;floor++){float y=-.6f+floor*1.55f;R(g,"Piso expuesto",0,y,w+.35f,.22f,"BDB496",-24);for(int k=0;k<3;k++)R(g,"Vigueta",-w/2+.5f+k*1.7f,y-.16f,.15f,.25f,"7C8074",-23);}
        R(g,"Muro izquierdo",-w/2+.3f,h/2-1,.65f,h,"BDB59A",-23);R(g,"Muro roto derecho",w/2-.3f,.25f,.65f,2.5f,"C6BCA1",-23);
        R(g,"Pilar interior",.8f,1.35f,.22f,4.7f,"7B7466",-24);
        R(g,"Viga caída",-.7f,h-.9f,3.7f,.18f,"786957",-22,-13);R(g,"Viga de cubierta",.85f,h-.5f,2.7f,.15f,"928062",-22,19);
        for(int i=0;i<6;i++){R(g,"Borde roto",-w/2+.3f+i*.31f,h-1-i*.27f,.48f,.4f,"BDB59A",-23);R(g,"Escombro decorativo",-2+i*.75f,-.72f,.7f,.4f,i%2==0?"B4AE94":"918D7A",-21,i*17);}
        for(int i=0;i<4;i++)R(g,"Escalera expuesta",-1.45f+i*.4f,.15f+i*.33f,.55f,.14f,"C8BCA0",-22);
        Ivy(g,-w/2+.1f,-.3f,variant,-20);return g;
    }
    static Transform Tree(Transform p,int v){var g=Group(p,"Árbol modular "+v);R(g,"Tronco",0,.75f,.36f,3.5f,"818575",-24);R(g,"Rama",.5f,1.65f,1.5f,.18f,"818575",-24,38);R(g,"Rama",-.45f,1.95f,1.3f,.18f,"818575",-24,-35);
        for(int j=0;j<8;j++){float x=Mathf.Sin(j*2.4f+v)*1.25f,y=2.5f+(j%3)*.47f;R(g,"Copa",x,y,1.7f-j%2*.3f,1.05f,j%3==0?"93AB72":j%3==1?"79945F":"A5B87D",-23+j%2);}return g;}
    static Transform Workshop(Transform p){var g=Group(p,"Taller modular abandonado");R(g,"Nave",0,1.6f,9,5.2f,"A0ADA8",-27);R(g,"Cubierta",0,4.3f,9.7f,.35f,"80918F",-23);R(g,"Cornisa iluminada",0,4.52f,9.6f,.12f,"C3C9B4",-22);
        for(int i=0;i<8;i++)R(g,"Lámina vertical",-4+i*1.1f,1.65f,.07f,5,"899C99",-26);
        R(g,"Entrada del taller",.5f,.25f,3.2f,2.5f,"687A7B",-24);for(int i=0;i<5;i++)R(g,"Persiana oxidada",.5f,1.5f-i*.14f,3.25f,.08f,"ABAA90",-23);
        for(int i=0;i<3;i++)Window(g,-3+i*2.8f,3,"809590",-23);
        R(g,"Tubo vertical",-3.7f,1.55f,.28f,4.8f,"879C9A",-21);R(g,"Tubo horizontal",-2,3.6f,3.6f,.28f,"8DA19D",-21);
        for(int i=0;i<3;i++)R(g,"Unión de tubería",-3.7f,.5f+i*1.3f,.4f,.14f,"C1C6AE",-20);
        R(g,"Óxido",3.5f,.8f,.3f,2.2f,"B09579",-22);Ivy(g,4,-.6f,0,-20);return g;}
    static GameObject Prefab(Transform piece,string name){string path=Root+"/Prefabs/EscenarioDia/"+name+".prefab";var prefab=PrefabUtility.SaveAsPrefabAsset(piece.gameObject,path);UnityEngine.Object.DestroyImmediate(piece.gameObject);return prefab;}
    static void Place(GameObject prefab,Transform parent,float x,float scale=1){var obj=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);obj.transform.localPosition=new Vector3(x*.88f+.96f,0,0);obj.transform.localScale=Vector3.one*scale;}
    static void Platform(Transform p,float x,float height,float width){var g=Group(p,"PLATAFORMA · colisión · "+x);g.localPosition=new Vector3(x,0,0);var block=R(g,"Piedra transitable",0,-1+height/2,width,height,"817F64",5);block.gameObject.AddComponent<BoxCollider2D>();R(g,"Borde transitable claro",0,-1+height-.04f,width,.08f,"E4CFA0",6);for(float a=-width/2+.2f;a<width/2;a+=.5f)R(g,"Junta de piedra",a,-1+height*.4f,.04f,height*.65f,"626E5C",6);}
    [MenuItem("NEXO/Crear copia diurna de Calles Bajo Control")]
    public static void Create(){
        Directory.CreateDirectory(Work+"outputs/EscenarioDia");Directory.CreateDirectory(Root+"/Prefabs/EscenarioDia");AssetDatabase.Refresh();
        pixel=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/Pixel.png");material=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Art/PixelMaterial.mat");
        if(!pixel||!material)throw new Exception("Faltan Pixel.png o PixelMaterial");
        var scene=EditorSceneManager.OpenScene(Root+"/Scenes/CallesBajoControl.unity");EditorSceneManager.SaveScene(scene,ScenePath);
        var world=GameObject.Find("Avenida · Calles Bajo Control").transform;
        string[] remove={"Edificio","Ventana","Fachada","Cornisa","Persiana","Lámina","Poste","Farola","Pavimento","Borde de acera","Zona del jefe"};
        foreach(Transform t in world.Cast<Transform>().ToArray())if(remove.Contains(t.name))UnityEngine.Object.DestroyImmediate(t.gameObject);
        var ground=world.Find("Avenida");ground.GetComponent<SpriteRenderer>().color=C("716C54");
        var camera=Camera.main;camera.backgroundColor=C("8ECBE8");camera.transform.position=new Vector3(8,2,-10);
        var daylight=Group(null,"ESCENARIO DIURNO · 4 zonas");
        var clouds=Layer(daylight,"01 · Cielo y nubes",.92f,camera);
        for(int i=0;i<12;i++){float x=-14+i*7.5f,y=5.4f+(i%3)*.5f;R(clouds,"Nube",x,y,4.2f,.4f,"E9F3EC",-100);R(clouds,"Nube alta",x-.5f,y+.35f,2.8f,.45f,"F8F5E7",-99);R(clouds,"Nube pequeña",x+.8f,y+.24f,1.7f,.45f,"F8F5E7",-99);}
        var hills=Layer(daylight,"02 · Colinas lejanas",.76f,camera);
        R(hills,"Base del paisaje lejano",70,-.15f,240,2.5f,"97BDC2",-91);
        for(int i=0;i<18;i++){float x=-20+i*7;for(int s=0;s<5;s++)R(hills,"Montaña escalonada",x,1+s*.55f,10-s*1.55f,.65f,i%2==0?"83ACB6":"97BDC2",-90);}
        var horizon=Layer(daylight,"03 · Pueblo distante",.5f,camera);
        R(horizon,"Terreno del pueblo lejano",75,-.8f,240,2.2f,"B8C4AA",-71);
        for(int i=0;i<27;i++){float x=-15+i*6.2f,h=1.6f+i%4*.45f;R(horizon,"Silueta distante",x,h/2,2.5f+(i%3)*.5f,h,"A8B9AA",-70);R(horizon,"Techo distante",x,h+.1f,3.1f,.22f,"9DAB9D",-69);}
        var buildings=Layer(daylight,"04 · Arquitectura y vegetación",.12f,camera);
        var temporary=Group(null,"Plantillas temporales");var houses=new GameObject[4];var ruins=new GameObject[3];var trees=new GameObject[2];
        for(int i=0;i<4;i++)houses[i]=Prefab(House(temporary,i),"Casa"+i);
        for(int i=0;i<3;i++)ruins[i]=Prefab(Ruin(temporary,i),"Ruina"+i);
        for(int i=0;i<2;i++)trees[i]=Prefab(Tree(temporary,i),"Arbol"+i);
        var workshop=Prefab(Workshop(temporary),"Taller");UnityEngine.Object.DestroyImmediate(temporary.gameObject);
        var street=Group(buildings,"ZONA 1 · Calle · 0 a 46");float[] positions={-2,6,14,23,33,42};for(int i=0;i<positions.Length;i++)Place(houses[(i*3)%4],street,positions[i],i==3?.92f:1);
        Place(trees[0],street,18,.7f);Place(trees[1],street,38,.8f);
        var ruinsZone=Group(buildings,"ZONA 2 · Destrucción · 46 a 94");Place(houses[1],ruinsZone,49,.9f);for(int i=0;i<5;i++)Place(ruins[i%3],ruinsZone,57+i*8.2f,i%2==0?1:.9f);
        var garden=Group(buildings,"ZONA 3 · Espacio abierto · 94 a 140");float[] tx={97,106,119,131,139};for(int i=0;i<tx.Length;i++)Place(trees[i%2],garden,tx[i],.85f+i%3*.13f);
        for(int i=0;i<9;i++){float x=94+i*5;for(int k=0;k<2+i%3;k++)R(garden,"Muro derrumbado decorativo",x*.88f+.96f+k*.48f,-.75f+(k%2)*.22f,.55f,.5f,"B3B199",-22);}
        var industrial=Group(buildings,"ZONA 4 · Taller industrial · 140 a 202");Place(workshop,industrial,150);Place(ruins[1],industrial,164,.9f);Place(workshop,industrial,194,.88f);
        for(int i=0;i<3;i++){float x=(172+i*7)*.88f+.96f;R(industrial,"Columna metálica",x,1.7f,.22f,5.4f,"91A29A",-25);R(industrial,"Travesaño",x+3,4.3f,6.2f,.22f,"A5B0A1",-25);R(industrial,"Refuerzo",x+1,3.3f,2.7f,.12f,"839991",-25,45);}
        var terrain=Group(daylight,"05 · Suelo y plataformas · sin parallax");
        R(terrain,"Subsuelo continuo",99,-4.5f,230,6,"555A46",2);
        for(int row=0;row<3;row++)for(int i=0;i<95;i++)R(terrain,"Roca del subsuelo",-5+i*2.3f+(row%2)*.8f,-2.3f-row*.65f,1.1f+(i%3)*.23f,.35f+(i%2)*.1f,i%3==0?"74775A":"64694E",3);
        R(terrain,"Franja de camino",99,-1.12f,206,.24f,"CAB58A",4);R(terrain,"Borde iluminado",99,-1.01f,206,.06f,"EEE0B3",5);
        for(int i=0;i<136;i++){float x=-3+i*1.53f;R(terrain,"Piedra del terraplén",x,-1.6f,.9f+i%3*.13f,.32f,"8E8766",3);R(terrain,"Grieta",x+.3f,-1.31f,.12f,.15f,"655F4C",4);}
        for(int i=0;i<65;i++){float x=-1+i*3.15f;R(terrain,"Hierba decorativa",x,-.84f,.12f,.3f,"778552",-5);R(terrain,"Hierba decorativa",x+.16f,-.87f,.12f,.2f,"A0A666",-5);}
        for(int i=0;i<16;i++){float x=95+i*2.8f;R(terrain,"Arbusto decorativo",x,-.72f,.8f,.48f,i%2==0?"8FA167":"A5AF77",-8);R(terrain,"Arbusto decorativo",x+.3f,-.5f,.65f,.3f,"A5AF77",-8);}
        Platform(terrain,23,.45f,2.2f);Platform(terrain,68,.55f,2);Platform(terrain,99,.4f,2);Platform(terrain,124,.45f,2.2f);Platform(terrain,157,.6f,2.4f);
        // This copy retains every original enemy, collider, checkpoint and supply truck.
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        var settings=EditorBuildSettings.scenes.ToList();if(!settings.Any(s=>s.path==ScenePath))settings.Add(new EditorBuildSettingsScene(ScenePath,true));EditorBuildSettings.scenes=settings.ToArray();
        File.WriteAllText(Work+"outputs/EscenarioDia/integracion.txt","Escena creada: "+ScenePath+"\n10 prefabs reutilizables; 4 zonas; 4 capas parallax; 5 plataformas bajas.\n");
    }
    public static void Export(){
        var scenes=new[]{ScenePath,Root+"/Scenes/MenuPrincipal.unity",Root+"/Scenes/CallesBajoControl.unity",Root+"/Scenes/FabricaAutomatizada.unity",Root+"/Scenes/NucleoNexo.unity"};
        var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=Work+"outputs/EscenarioDia/NEXO-Dia/NEXO.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
        if(result.summary.result!=BuildResult.Succeeded)throw new Exception("No se pudo compilar la copia diurna");File.WriteAllText(Work+"outputs/EscenarioDia/compilacion.txt","CORRECTO");
    }
}
}
