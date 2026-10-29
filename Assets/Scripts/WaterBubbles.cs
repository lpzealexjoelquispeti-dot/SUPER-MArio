using UnityEngine;

public class WaterBubbles : MonoBehaviour
{
    public Sprite bubble;
    private float next;
    private void Update()
    {
        if (Time.time < next) return;
        next = Time.time + 0.65f;
        GameObject particle = new GameObject("Bubble");
        particle.transform.position = transform.position + Vector3.up * 0.5f;
        particle.transform.localScale = Vector3.one * 0.18f;
        particle.AddComponent<SpriteRenderer>().sprite = bubble;
        particle.AddComponent<BubbleRise>();
    }
}
