using UnityEngine;
public class CastleBoss : MonoBehaviour
{
    public Sprite fireSprite;
    private Vector3 origin;
    private float nextFire;
    private void Start() { origin = transform.position; nextFire = Time.time + 2; }
    private void Update()
    {
        transform.position = origin + new Vector3(Mathf.Sin(Time.time) * 2, Mathf.Max(0, Mathf.Sin(Time.time * 1.5f)) * 2, 0);
        if (Time.time < nextFire || Camera.main.transform.position.x < 76) return;
        nextFire = Time.time + 2.4f;
        GameObject flame = new GameObject("Bowser fire", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(BossFire));
        flame.transform.position = transform.position + Vector3.left;
        flame.GetComponent<SpriteRenderer>().sprite = fireSprite;
        flame.GetComponent<SpriteRenderer>().color = new Color(1, 0.25f, 0.05f);
        flame.transform.localScale = Vector3.one * 0.45f;
        flame.GetComponent<CircleCollider2D>().isTrigger = true;
        GamePresentation.Sound(120, 0.15f);
    }
    private void OnTriggerEnter2D(Collider2D other) { Player player = other.GetComponent<Player>(); if (player != null) player.Hit(); }
}
