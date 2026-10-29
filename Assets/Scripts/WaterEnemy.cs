using UnityEngine;

public class WaterEnemy : MonoBehaviour
{
    public bool squid;
    public float range = 4;
    private Vector3 origin;
    private Transform target;
    private SpriteRenderer visual;
    private void Start()
    {
        origin = transform.position;
        target = GameObject.FindGameObjectWithTag("Player").transform;
        visual = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        float t = Time.time;
        if (squid && target != null && Mathf.Abs(target.position.x - transform.position.x) < 12) {
            Vector3 goal = new Vector3(target.position.x, Mathf.Clamp(target.position.y + 1, 2, 10), 0);
            transform.position = Vector3.MoveTowards(transform.position, goal, (Mathf.Sin(t * 3) > 0 ? 2.1f : 0.4f) * Time.deltaTime);
        } else transform.position = origin + new Vector3(Mathf.Sin(t * 0.7f + origin.x) * range, Mathf.Sin(t * 1.8f + origin.x) * 0.6f, 0);
        visual.flipX = Mathf.Cos(t * 0.7f + origin.x) > 0;
        transform.localScale = new Vector3(1, 1 + Mathf.Sin(t * 8) * 0.08f, 1);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player == null) return;
        if (player.starpower) { Destroy(gameObject); GamePresentation.Sound(220, 0.12f); }
        else player.Hit();
    }
}
