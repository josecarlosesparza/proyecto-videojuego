using System;
using UnityEngine;
namespace NexoAvance {
public enum CombatTeam { Player, Robot }
public class CombatHealth : MonoBehaviour {
    public CombatTeam team;
    [Min(1)] public int maximum = 100;
    public SpriteRenderer visual;
    public int Current { get; private set; }
    public bool Shielded { get; set; }
    public bool Alive => Current > 0;
    public event Action Changed;
    public event Action Died;
    public float invulnerability;
    float flashUntil, immuneUntil;
    Color normalColor=Color.white;
    void Awake() { Current = maximum; if(visual) normalColor=visual.color; }
    void Update() { if(visual) visual.color = Alive && Time.time < flashUntil ? new Color(1,.35f,.3f) : normalColor; }
    public bool Hit(int damage, CombatTeam source) {
        if(Shielded || !Alive || damage <= 0 || source == team || Time.timeScale == 0 || Time.time < immuneUntil) return false;
        immuneUntil = Time.time + invulnerability;
        Current = Mathf.Max(0, Current - damage);
        flashUntil = Time.time + .12f;
        Changed?.Invoke();
        if(!Alive) Died?.Invoke();
        return true;
    }
    public void Heal(int amount){if(!Alive)return;Current=Mathf.Min(maximum,Current+Mathf.Max(0,amount));Changed?.Invoke();}
    public void ResetHealth() { Current = maximum; flashUntil = immuneUntil = 0; if(visual) visual.color = normalColor; Changed?.Invoke(); }
}
}
