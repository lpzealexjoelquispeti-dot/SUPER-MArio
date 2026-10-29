using UnityEngine;
public class CastleHazard : MonoBehaviour
{
    public bool rotating;
    private Vector3 origin;
    private void Start() { origin = transform.position; }
    private void Update()
    {
        if (rotating) transform.position = origin + new Vector3(Mathf.Cos(Time.time * 1.7f), Mathf.Sin(Time.time * 1.7f), 0) * 2;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null && !player.dead) { if (rotating) player.Hit(); else player.Death(); }
    }
}
