using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace NexoAvance {
public class HeroSelectionScreen:MonoBehaviour {
    public Button[] cards=new Button[4];public Button confirm,back;public int Current {get;private set;}
    Image[] borders=new Image[4];Text[] labels=new Text[4];MainMenuController menu;HeroRoster roster;Font font;
    public void Build(MainMenuController owner){
        menu=owner;roster=HeroRoster.Load();font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var outer=gameObject.AddComponent<RectTransform>();outer.anchorMin=Vector2.zero;outer.anchorMax=Vector2.one;outer.offsetMin=outer.offsetMax=Vector2.zero;
        gameObject.AddComponent<Image>().color=new Color(.025f,.045f,.065f,.99f);
        var panel=new GameObject("Contenido",typeof(RectTransform)).GetComponent<RectTransform>();panel.SetParent(transform,false);panel.sizeDelta=new Vector2(1180,650);panel.anchorMin=panel.anchorMax=new Vector2(.5f,.5f);
        TextAt(panel,"ELIGE TU PROTAGONISTA",new Vector2(0,275),new Vector2(1100,50),32);
        TextAt(panel,"Cuatro diseños · Las mismas capacidades de combate",new Vector2(0,230),new Vector2(1100,32),18);
        for(int i=0;i<4;i++){
            int index=i;float x=-435+i*290;
            var button=ButtonAt(panel,"Personaje "+i,new Vector2(x,0),new Vector2(270,390));cards[i]=button;borders[i]=button.GetComponent<Image>();
            var portrait=new GameObject("Diseño original",typeof(RectTransform),typeof(Image)).GetComponent<Image>();portrait.transform.SetParent(button.transform,false);portrait.rectTransform.sizeDelta=new Vector2(248,310);portrait.rectTransform.anchoredPosition=new Vector2(0,25);portrait.sprite=roster.heroes[i].pose;portrait.preserveAspect=true;portrait.raycastTarget=false;
            labels[i]=TextAt(button.transform,roster.heroes[i].name,new Vector2(0,-158),new Vector2(260,38),23);
            button.onClick.AddListener(()=>Choose(index));
        }
        back=ButtonAt(panel,"Volver",new Vector2(-320,-265),new Vector2(220,52));TextAt(back.transform,"Volver",Vector2.zero,new Vector2(210,48),22);back.onClick.AddListener(menu.Home);
        confirm=ButtonAt(panel,"Jugar",new Vector2(320,-265),new Vector2(260,58));TextAt(confirm.transform,"Jugar",Vector2.zero,new Vector2(250,52),25);confirm.onClick.AddListener(()=>{HeroSelection.Choose(Current);CampaignCarry.Ready=false;menu.StartSelectedGame();});
        Choose(0);
    }
    public void Open(){gameObject.SetActive(true);Choose(0);EventSystem.current?.SetSelectedGameObject(cards[0].gameObject);}
    public void Choose(int index){Current=Mathf.Clamp(index,0,3);for(int i=0;i<4;i++){borders[i].color=i==Current?new Color(.19f,.43f,.40f):new Color(.085f,.12f,.16f);labels[i].text=(i==Current?"✓ ":"")+roster.heroes[i].name;}}
    Button ButtonAt(Transform parent,string name,Vector2 pos,Vector2 size){var go=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(parent,false);var rect=go.GetComponent<RectTransform>();rect.anchoredPosition=pos;rect.sizeDelta=size;var image=go.GetComponent<Image>();image.color=new Color(.16f,.30f,.32f);var b=go.GetComponent<Button>();b.targetGraphic=image;return b;}
    Text TextAt(Transform parent,string words,Vector2 pos,Vector2 size,int fontsize){var go=new GameObject(words,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);var t=go.GetComponent<Text>();t.rectTransform.anchoredPosition=pos;t.rectTransform.sizeDelta=size;t.font=font;t.text=words;t.fontSize=fontsize;t.alignment=TextAnchor.MiddleCenter;t.color=Color.white;t.raycastTarget=false;return t;}
}
}
