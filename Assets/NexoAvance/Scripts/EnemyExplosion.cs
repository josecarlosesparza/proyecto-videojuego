using UnityEngine;
namespace NexoAvance {
// Efecto visual sin colliders ni daño. Se detiene con la pausa del juego.
public sealed class EnemyExplosion : MonoBehaviour {
    const float Duration = .7f;
    struct Fragment {
        public SpriteRenderer renderer;
        public Vector2 direction;
        public float distance, size;
        public bool smoke;
    }
    readonly Fragment[] fragments = new Fragment[22];
    float age;
    public static EnemyExplosion Spawn(Vector3 position, Sprite pixel, Material material) {
        var root = new GameObject("Explosion del enemigo");
        root.transform.position = position;
        var effect = root.AddComponent<EnemyExplosion>();
        for(int i=0;i<effect.fragments.Length;i++) {
            bool smoke = i>=14;
            float angle = (i * 137.5f) * Mathf.Deg2Rad;
            var piece = new GameObject(smoke ? "Humo" : "Chispa");
            piece.transform.SetParent(root.transform,false);
            var sr = piece.AddComponent<SpriteRenderer>();
            sr.sprite = pixel; sr.sharedMaterial = material; sr.sortingOrder = smoke ? 29 : 30;
            effect.fragments[i] = new Fragment {
                renderer = sr, direction = new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)),
                distance = smoke ? .45f : .6f+(i%4)*.15f,
                size = smoke ? .28f : (i<5 ? .4f : .12f), smoke = smoke
            };
        }
        effect.Draw();
        return effect;
    }
    void Update() {
        age += Time.deltaTime;
        if(age>=Duration) { Destroy(gameObject); return; }
        Draw();
    }
    void Draw() {
        float t = Mathf.Clamp01(age/Duration);
        foreach(var f in fragments) {
            float travel = 1-Mathf.Pow(1-t,3);
            Vector2 position = f.direction*f.distance*travel;
            if(f.smoke) position += Vector2.up*t*.5f;
            else position += Vector2.down*t*t*.35f;
            f.renderer.transform.localPosition = position;
            float size = f.size*(f.smoke ? .6f+t : Mathf.Lerp(1,.15f,t));
            f.renderer.transform.localScale = new Vector3(size,size,1);
            Color c;
            if(f.smoke) c = new Color(.27f,.31f,.34f,(1-t)*.65f);
            else {
                c = t<.25f ? Color.Lerp(new Color(1,.96f,.65f),new Color(1,.65f,.15f),t*4)
                    : Color.Lerp(new Color(1,.65f,.15f),new Color(.8f,.19f,.07f),(t-.25f)/.75f);
                c.a = 1-t;
            }
            f.renderer.color = c;
        }
    }
}
}
