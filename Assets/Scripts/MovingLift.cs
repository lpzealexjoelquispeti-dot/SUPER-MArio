using UnityEngine;

public class MovingLift : MonoBehaviour
{
    public float amplitude = 3;
    public float speed = 0.7f;
    public float phase;
    private Vector3 origin;
    private void Awake() { origin = transform.position; }
    private void FixedUpdate()
    {
        Vector3 next = origin + Vector3.up * Mathf.Sin(Time.fixedTime * speed + phase) * amplitude;
        Vector3 delta = next - transform.position;
        // Carry a grounded rider without parenting the dynamic Rigidbody2D.
        foreach (RaycastHit2D hit in Physics2D.BoxCastAll(transform.position + Vector3.up * 0.3f, new Vector2(2.8f, 0.3f), 0, Vector2.up, 0.3f, LayerMask.GetMask("Player"))) {
            PlayerMovement rider = hit.collider.GetComponent<PlayerMovement>();
            if (rider != null && !rider.jumping) hit.rigidbody.position += (Vector2)delta;
        }
        transform.position = next;
    }
}
