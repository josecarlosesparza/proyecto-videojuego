using System;
using UnityEngine;
namespace NexoAvance {
public enum CombatTeam { Player, Robot }
public class CombatHealth : MonoBehaviour {
    public CombatTeam team;
    [Min(1)] public int maximum = 100;
    public SpriteRenderer visual;
    public int Current { get; private set; }
    public bool Alive => Current > 0;
    public event Action Changed;
    public event Action Died;
    float flashUntil;
    void Awake() { Current = maximum; }
    void Update() { if(visual) visual.color = Time.time < flashUntil ? new Color(1,.35f,.3f) : Color.white; }
    public bool Hit(int damage, CombatTeam source) {
        if(!Alive || damage <= 0 || source == team || Time.timeScale == 0) return false;
        Current = Mathf.Max(0, Current - damage);
        flashUntil = Time.time + .12f;
        Changed?.Invoke();
        if(!Alive) Died?.Invoke();
        return true;
    }
    public void ResetHealth() { Current = maximum; flashUntil = 0; if(visual) visual.color = Color.white; Changed?.Invoke(); }
}
}
