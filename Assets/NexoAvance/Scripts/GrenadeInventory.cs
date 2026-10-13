using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace NexoAvance {
public class GrenadeInventory:MonoBehaviour {
    public int Count {get;private set;}
    public bool Collected {get;private set;}
    public Vector3 SupplyPosition {get;private set;}
    CombatSession session;System.Action<string,float> notice;bool truckSupplies;PlayerCombat weapon;Text counter;GameObject supply;
    float nextThrow;
    public static GrenadeInventory Install(StreetMission mission){return Install(mission.session,mission.ShowNotice,true);}
    public static GrenadeInventory Install(CombatSession session,System.Action<string,float> notice,bool truckSupplies){
        var inventory=session.player.GetComponent<GrenadeInventory>();if(inventory)return inventory;
        inventory=session.player.gameObject.AddComponent<GrenadeInventory>();inventory.session=session;inventory.notice=notice;inventory.truckSupplies=truckSupplies;inventory.weapon=inventory.GetComponent<PlayerCombat>();inventory.Setup();return inventory;
    }
    public void Give(int amount){Count=Mathf.Max(0,Count+amount);Refresh();}    void Setup(){
        // El primer camión está después del fusilero, el dron y el blindado iniciales.
        Transform truck=null;float nearest=float.MaxValue;
        if(truckSupplies)foreach(var t in FindObjectsByType<Transform>(FindObjectsSortMode.None))if(t.name=="Camión abandonado"){float d=Mathf.Abs(t.position.x-44.7f);if(d<nearest){nearest=d;truck=t;}}
        SupplyPosition=new Vector3(41.5f,-.6f,0);
        if(truck){truck.name="Camión militar · suministro de granadas";truck.GetComponent<SpriteRenderer>().color=new Color(.66f,.83f,.47f);SupplyPosition=new Vector3(truck.position.x-3.2f,-.6f,0);Piece(truck,"Insignia militar",new Vector3(-.7f,.12f,0),new Vector3(.6f,.14f,1),new Color(.9f,.88f,.65f));Piece(truck,"Insignia vertical",new Vector3(-.7f,.12f,0),new Vector3(.14f,.5f,1),new Color(.9f,.88f,.65f));}
        supply=new GameObject("Caja de granadas · recoger al acercarse");supply.transform.position=SupplyPosition;
        Piece(supply.transform,"Caja",Vector3.zero,new Vector3(.7f,.65f,1),new Color(.35f,.43f,.22f));Piece(supply.transform,"Franja",Vector3.zero,new Vector3(.75f,.12f,1),new Color(.95f,.77f,.36f));
        var label=new GameObject("Granadas disponibles",typeof(RectTransform)).GetComponent<RectTransform>();label.SetParent(session.healthLabel.canvas.transform,false);label.anchorMin=label.anchorMax=label.pivot=Vector2.zero;label.anchoredPosition=new Vector2(24,112);label.sizeDelta=new Vector2(330,30);
        counter=label.gameObject.AddComponent<Text>();counter.font=session.healthLabel.font;counter.fontSize=19;counter.color=new Color(.9f,.94f,.8f);counter.raycastTarget=false;supply.SetActive(truckSupplies);Refresh();
    }
    void Piece(Transform parent,string name,Vector3 position,Vector3 size,Color color){var sr=new GameObject(name).AddComponent<SpriteRenderer>();sr.transform.SetParent(parent,false);sr.transform.localPosition=position;sr.transform.localScale=size;sr.sprite=weapon.pixel;sr.sharedMaterial=weapon.material;sr.color=color;sr.sortingOrder=9;}
    void Update(){
        if(!weapon.health.Alive || !weapon.movement.enabled || weapon.movement.Paused || Time.timeScale==0)return;
        if(truckSupplies && !Collected && Vector2.Distance(transform.position,SupplyPosition)<1.35f){Collected=true;Count=5;supply.SetActive(false);Refresh();notice?.Invoke("+5 GRANADAS · G PARA LANZAR",3);}
        if(weapon.movement.TestMode)return;
#if ENABLE_INPUT_SYSTEM
        if(Keyboard.current!=null && Keyboard.current.gKey.wasPressedThisFrame)TryThrow();
#elif ENABLE_LEGACY_INPUT_MANAGER
        if(Input.GetKeyDown(KeyCode.G))TryThrow();
#endif
    }
    public bool TryThrow(){
        if(Count<=0 || !weapon.health.Alive || !weapon.movement.enabled || weapon.movement.Paused || Time.timeScale==0 || Time.time<nextThrow)return false;
        float side=weapon.movement.visual.flipX?-1:1;
        ThrownGrenade.Launch(GetComponent<Rigidbody2D>().position+new Vector2(side*.45f,.2f),side,weapon);Count--;nextThrow=Time.time+.65f;Refresh();return true;
    }
    public void ResetRun(){ThrownGrenade.ClearAll();Count=0;Collected=false;nextThrow=0;supply.SetActive(truckSupplies);Refresh();}
    public void ClearThrows(){ThrownGrenade.ClearAll();nextThrow=0;}
    void Refresh(){counter.text="GRANADAS  "+Count+"  [ G ]";}
}
}
