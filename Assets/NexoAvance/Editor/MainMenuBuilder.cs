using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace NexoAvance
{
    public static class MainMenuBuilder
    {
        public const string MenuPath = "Assets/NexoAvance/Scenes/MenuPrincipal.unity";
        const string GamePath = "Assets/NexoAvance/Scenes/PrimerAvance.unity";
        static Font font;
        static Color Navy => ColorHex("0D1F30");
        static Color Mint => ColorHex("77D8C1");
        static Color Ink => ColorHex("E4EFEF");

        [MenuItem("NEXO/Crear o actualizar menú principal")]
        public static void CreateMenu()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var source = EditorSceneManager.OpenScene(GamePath, OpenSceneMode.Single);
            var sourceScenery = source.GetRootGameObjects().First(x => x.name.StartsWith("Escenario"));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            var scenery = UnityEngine.Object.Instantiate(sourceScenery);
            scenery.name = "Ciudad · fondo del menú";
            SceneManager.MoveGameObjectToScene(scenery, scene);
            foreach (var collider in scenery.GetComponentsInChildren<Collider2D>()) UnityEngine.Object.DestroyImmediate(collider);
            EditorSceneManager.CloseScene(source, true);
            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.tag = "MainCamera"; camera.orthographic = true; camera.orthographicSize = 5.4f;
            camera.transform.position = new Vector3(11, 2, -10);
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = ColorHex("152C40");
            var canvas = new GameObject("Menú principal", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1; canvas.sortingOrder = 100;
            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280,720); scaler.matchWidthOrHeight = .5f;
            var stage = new GameObject("Composición 16 a 9", typeof(RectTransform), typeof(AspectRatioFitter)).GetComponent<RectTransform>();
            stage.SetParent(canvas.transform,false);
            var aspect = stage.GetComponent<AspectRatioFitter>(); aspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent; aspect.aspectRatio = 1280f/720;
            stage.anchorMin = stage.anchorMax = new Vector2(.5f,.5f);
            // Todos los elementos usan proporciones sobre una composición de 1280 x 720.
            Box(stage,"Sombra general",0,0,1280,720,new Color(.025f,.065f,.10f,.42f));
            Box(stage,"Panel izquierdo",0,0,620,720,new Color(.025f,.065f,.105f,.97f));
            Box(stage,"Línea vertical",620,0,2,720,new Color(.47f,.85f,.76f,.5f));
            Label(stage,"Identificador",60,43,500,30,"R E S I S T E N C I A   /   S E C T O R   0 1",15,Mint);
            Label(stage,"Título",54,78,530,110,"NEXO",88,Ink,FontStyle.Bold);
            Label(stage,"Subtítulo",60,190,490,32,"LA CIUDAD TODAVÍA ES NUESTRA.",20,Mint,FontStyle.Bold);
            Label(stage,"Introducción",60,236,480,52,"Cruza las calles bajo control de las máquinas.\nEl primer paso de la resistencia comienza contigo.",17,ColorHex("A7BAC6"));
            Label(stage,"Pie",60,667,550,24,"↑ ↓  ELEGIR     ENTER  CONFIRMAR     ESC  VOLVER",13,ColorHex("8EA9B7"));
            Label(stage,"Versión",1000,671,220,22,"PROTOTIPO  /  03",13,Mint,FontStyle.Normal,TextAnchor.MiddleRight);
            Label(stage,"Zona",698,65,470,34,"01 / CALLES BAJO CONTROL",21,Ink,FontStyle.Bold);
            Label(stage,"Estado",700,102,460,30,"ZONA DE ENTRENAMIENTO",14,Mint);
            Box(stage,"Placa del personaje",764,251,350,279,new Color(.035f,.09f,.13f,.7f));
            var hero=Box(stage,"Integrante de la resistencia",838,269,190,228,Color.white);
            hero.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/NexoAvance/Art/Idle0.png");
            hero.preserveAspect=true;
            Box(stage,"Línea de personaje",788,540,304,3,Mint);
            Label(stage,"Ficha",784,558,330,35,"MOVIMIENTO · SALTO · COMBATE",14,Ink,FontStyle.Bold,TextAnchor.MiddleCenter);

            var controller=canvas.gameObject.AddComponent<MainMenuController>();
            controller.uiAudio=canvas.gameObject.AddComponent<AudioSource>();
            controller.uiAudio.playOnAwake=false;
            controller.home=Panel(stage,"Inicio");
            controller.playButton=Button(controller.home.transform,"Jugar",60,318,480,55,"JUGAR  →",true);
            controller.controlsButton=Button(controller.home.transform,"Controles",60,385,234,49,"CONTROLES");
            controller.optionsButton=Button(controller.home.transform,"Opciones",306,385,234,49,"OPCIONES");
            controller.creditsButton=Button(controller.home.transform,"Créditos",60,446,234,49,"CRÉDITOS");
            controller.exitButton=Button(controller.home.transform,"Salir",306,446,234,49,"SALIR");
            Label(controller.home.transform,"Descripción de jugar",62,517,480,60,"Explora el recorrido y practica los saltos.\nDispara con J y esquiva las balas del robot.",16,ColorHex("A7BAC6"));
            GridNavigation(controller);
            controller.controls=Modal(stage,"Controles","CONTROLES","Lo necesario para recorrer la ciudad.");
            Label(controller.controls.transform,"Lista de controles",345,232,620,240,"A / D  o  ← →        Moverte\nESPACIO                 Saltar\nJ                               Disparar (mantener)\nESC                          Pausar / continuar\nR                              Reiniciar el recorrido\n\nMENÚ: flechas, Enter y ratón.",22,Ink);
            controller.controlsBack=Button(controller.controls.transform,"Volver",345,514,590,52,"VOLVER AL MENÚ",true);
            controller.options=Modal(stage,"Opciones","OPCIONES","Ajusta el juego a tu equipo.");
            Label(controller.options.transform,"Volumen",345,237,420,30,"VOLUMEN",20,Ink,FontStyle.Bold);
            controller.volumeValue=Label(controller.options.transform,"Porcentaje",841,237,94,30,"65%",20,Mint,FontStyle.Bold,TextAnchor.MiddleRight);
            controller.volume=Slider(controller.options.transform);
            Label(controller.options.transform,"Ayuda de volumen",345,319,590,36,"Actualmente ajusta los sonidos del menú.",16,ColorHex("A7BAC6"));
            controller.fullscreen=Toggle(controller.options.transform);
            Label(controller.options.transform,"Ayuda de pantalla",345,403,590,30,"El cambio de pantalla se aplica en la versión ejecutable.",15,ColorHex("A7BAC6"));
            controller.defaultsButton=Button(controller.options.transform,"Restablecer",345,455,590,42,"RESTABLECER AJUSTES");
            controller.optionsBack=Button(controller.options.transform,"Volver",345,514,590,52,"GUARDAR Y VOLVER",true);
            Vertical(controller.volume,controller.fullscreen,controller.defaultsButton,controller.optionsBack);
            controller.credits=Modal(stage,"Créditos","CRÉDITOS","Un proyecto académico de acción en 2D.");
            Label(controller.credits.transform,"Autoría",345,235,590,236,"JOSÉ CARLOS ESPARZA LUNA\nDesarrollo y diseño del proyecto\n\n10.º cuatrimestre · Creación de Videojuegos\nUniversidad Tecnológica de Aguascalientes\n\nCreado en Unity con C# y arte original para NEXO.",20,Ink);
            controller.creditsBack=Button(controller.credits.transform,"Volver",345,514,590,52,"VOLVER AL MENÚ",true);
            controller.exitConfirm=Modal(stage,"Confirmar salida","¿SALIR DEL JUEGO?","Puedes volver cuando quieras.");
            Label(controller.exitConfirm.transform,"Mensaje",345,265,590,110,"Se cerrará la aplicación.\nTus ajustes quedarán guardados.",23,Ink);
            controller.cancelExit=Button(controller.exitConfirm.transform,"Cancelar",345,450,282,56,"CANCELAR",true);
            controller.confirmExit=Button(controller.exitConfirm.transform,"Confirmar",653,450,282,56,"SALIR");
            Horizontal(controller.cancelExit,controller.confirmExit);
            controller.loading=Modal(stage,"Cargando","PREPARANDO EL RECORRIDO","La resistencia está lista.");
            controller.loadMessage=Label(controller.loading.transform,"Mensaje de carga",345,300,590,130,"Entrando a Calles bajo control…",24,Ink);
            foreach(var panel in new[]{controller.controls,controller.options,controller.credits,controller.exitConfirm,controller.loading}) panel.SetActive(false);
            var eventSystem=new GameObject("EventSystem",typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            eventSystem.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
#else
            eventSystem.AddComponent<StandaloneInputModule>();
#endif
            new GameObject("Pruebas del menú").AddComponent<MainMenuSmokeTest>();
            EditorSceneManager.SaveScene(scene,MenuPath);
            ConfigureStart();
            AssetDatabase.SaveAssets();
        }

        public static void ConfigureStart()
        {
            var extra=EditorBuildSettings.scenes.Where(s=>s.path!=MenuPath && s.path!=GamePath && !s.path.EndsWith("/SampleScene.unity"));
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(MenuPath,true),new EditorBuildSettingsScene(GamePath,true)}.Concat(extra).ToArray();
            EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(MenuPath);
        }
        [MenuItem("NEXO/Probar escena actual sin pasar por el menú")]
        public static void PlayCurrentScene() { EditorSceneManager.playModeStartScene=null; EditorApplication.isPlaying=true; }
        [MenuItem("NEXO/Iniciar siempre en el menú")]
        public static void StartInMenu() { ConfigureStart(); }

        public static void InstallAndBuild()
        {
            string output=Argument("-nexoOutput");
            if(string.IsNullOrEmpty(output)) throw new Exception("Falta -nexoOutput");
            Directory.CreateDirectory(output);
            CreateMenu();
            PlayerSettings.defaultScreenWidth=1280; PlayerSettings.defaultScreenHeight=720;
            PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            AssetDatabase.ExportPackage("Assets/NexoAvance",Path.Combine(output,"NEXO-MenuPrincipal.unitypackage"),ExportPackageOptions.Recurse);
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[]{MenuPath,GamePath},target=BuildTarget.StandaloneWindows64,
                locationPathName=Path.Combine(output,"NEXO-Jugable/NEXO.exe"),options=BuildOptions.Development
            });
            if(result.summary.result!=BuildResult.Succeeded) throw new Exception("Compilación: "+result.summary.result);
            File.WriteAllText(Path.Combine(output,"compilacion.txt"),"Unity "+Application.unityVersion+"\n"+result.summary.result+"\nInicio: MenuPrincipal\nJuego: PrimerAvance");
        }
        static string Argument(string name) { var a=Environment.GetCommandLineArgs(); int i=Array.IndexOf(a,name); return i>=0 && i+1<a.Length?a[i+1]:null; }
        static Color ColorHex(string value) { ColorUtility.TryParseHtmlString("#"+value,out var c); return c; }
        static RectTransform Place(Transform parent,string name,float x,float y,float w,float h)
        {
            var rt=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); rt.SetParent(parent,false);
            rt.anchorMin=new Vector2(x/1280f,1-(y+h)/720f); rt.anchorMax=new Vector2((x+w)/1280f,1-y/720f);
            rt.offsetMin=rt.offsetMax=Vector2.zero; return rt;
        }
        static Image Box(Transform p,string name,float x,float y,float w,float h,Color color)
        { var img=Place(p,name,x,y,w,h).gameObject.AddComponent<Image>(); img.color=color; img.raycastTarget=false; return img; }
        static Text Label(Transform p,string name,float x,float y,float w,float h,string text,int size,Color color,FontStyle weight=FontStyle.Normal,TextAnchor align=TextAnchor.UpperLeft)
        {
            var t=Place(p,name,x,y,w,h).gameObject.AddComponent<Text>(); t.font=font; t.text=text; t.fontSize=size;
            t.fontStyle=weight; t.color=color; t.alignment=align; t.raycastTarget=false;
            t.horizontalOverflow=HorizontalWrapMode.Wrap; t.verticalOverflow=VerticalWrapMode.Truncate;
            t.resizeTextForBestFit=true; t.resizeTextMinSize=Mathf.Max(11,size-5); t.resizeTextMaxSize=size;
            return t;
        }
        static GameObject Panel(Transform p,string name) { return Place(p,name,0,0,1280,720).gameObject; }
        static GameObject Modal(Transform p,string name,string title,string subtitle)
        {
            var root=Panel(p,name);
            var veil=Box(root.transform,"Oscurecer",0,0,1280,720,new Color(.01f,.025f,.04f,.93f)); veil.raycastTarget=true;
            Box(root.transform,"Ventana",300,130,680,475,Navy);
            Box(root.transform,"Acento",300,130,680,3,Mint);
            Label(root.transform,"Título",345,165,590,45,title,31,Ink,FontStyle.Bold);
            Label(root.transform,"Subtítulo",345,207,590,28,subtitle,16,ColorHex("A7BAC6"));
            return root;
        }
        static Button Button(Transform p,string name,float x,float y,float w,float h,string caption,bool primary=false)
        {
            var img=Box(p,name,x,y,w,h,Color.white); img.raycastTarget=true;
            var button=img.gameObject.AddComponent<Button>(); button.targetGraphic=img;
            var colors=button.colors; colors.normalColor=primary?Mint:ColorHex("20394B");
            colors.highlightedColor=primary?ColorHex("B2FFE9"):ColorHex("456577"); colors.selectedColor=colors.highlightedColor;
            colors.pressedColor=ColorHex("51A991"); colors.disabledColor=ColorHex("38505C"); colors.fadeDuration=.08f; button.colors=colors;
            var label=Label(img.transform,"Texto",35,0,1210,720,caption,20,primary?Navy:Ink,FontStyle.Bold,TextAnchor.MiddleCenter);
            return button;
        }
        static Slider Slider(Transform p)
        {
            var rt=Place(p,"Volumen",345,280,590,28); var trackHit=rt.gameObject.AddComponent<Image>(); trackHit.color=Color.clear; var slider=rt.gameObject.AddComponent<Slider>();
            Box(rt,"Pista",0,230,1280,260,ColorHex("30495B"));
            var fill=Box(rt,"Nivel",0,230,1280,260,Mint); slider.fillRect=fill.rectTransform;
            var handle=Box(rt,"Control",0,0,35,720,Ink); handle.raycastTarget=true;
            handle.rectTransform.sizeDelta=new Vector2(18,0); slider.handleRect=handle.rectTransform; slider.targetGraphic=handle; slider.minValue=0; slider.maxValue=1; slider.value=.65f; var colors=slider.colors; colors.selectedColor=ColorHex("FFC87B"); colors.highlightedColor=colors.selectedColor; slider.colors=colors;
            return slider;
        }
        static Toggle Toggle(Transform p)
        {
            var rt=Place(p,"Pantalla completa",345,366,590,32); var toggle=rt.gameObject.AddComponent<Toggle>();
            var bg=Box(rt,"Casilla",0,0,65,720,ColorHex("30495B")); bg.raycastTarget=true;
            var check=Box(rt,"Activo",12,130,41,460,Mint); toggle.targetGraphic=bg; toggle.graphic=check; toggle.toggleTransition=UnityEngine.UI.Toggle.ToggleTransition.None;
            Label(rt,"Texto",95,0,1185,720,"PANTALLA COMPLETA",19,Ink,FontStyle.Bold,TextAnchor.MiddleLeft);
            var hit=rt.gameObject.AddComponent<Image>(); hit.color=Color.clear; hit.raycastTarget=true;
            return toggle;
        }
        static void Vertical(params Selectable[] items)
        {
            for(int i=0;i<items.Length;i++){var n=new Navigation{mode=Navigation.Mode.Explicit};n.selectOnUp=items[(i+items.Length-1)%items.Length];n.selectOnDown=items[(i+1)%items.Length];items[i].navigation=n;}
        }
        static void Horizontal(Selectable a,Selectable b)
        {
            var n=new Navigation{mode=Navigation.Mode.Explicit,selectOnLeft=b,selectOnRight=b,selectOnUp=b,selectOnDown=b}; a.navigation=n;
            n.selectOnLeft=n.selectOnRight=n.selectOnUp=n.selectOnDown=a;b.navigation=n;
        }
        static void GridNavigation(MainMenuController m)
        {
            void Nav(Selectable s,Selectable up,Selectable down,Selectable left,Selectable right){s.navigation=new Navigation{mode=Navigation.Mode.Explicit,selectOnUp=up,selectOnDown=down,selectOnLeft=left,selectOnRight=right};}
            Nav(m.playButton,m.exitButton,m.controlsButton,m.playButton,m.playButton);
            Nav(m.controlsButton,m.playButton,m.creditsButton,m.optionsButton,m.optionsButton);
            Nav(m.optionsButton,m.playButton,m.exitButton,m.controlsButton,m.controlsButton);
            Nav(m.creditsButton,m.controlsButton,m.playButton,m.exitButton,m.exitButton);
            Nav(m.exitButton,m.optionsButton,m.playButton,m.creditsButton,m.creditsButton);
        }
    }
}

