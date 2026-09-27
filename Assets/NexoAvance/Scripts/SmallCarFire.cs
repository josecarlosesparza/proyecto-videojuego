using UnityEngine;
namespace NexoAvance {
// Solo animación visual: sin colisiones, proyectiles ni daño.
public class SmallCarFire : MonoBehaviour {
    public SpriteRenderer flame;
    public Sprite[] frames;
    void Update() { if (frames.Length > 0) flame.sprite = frames[(int)(Time.time * 7) % frames.Length]; }
}
}
