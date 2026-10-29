using UnityEngine;
public class BossFire : MonoBehaviour
{
    private void Start() { Destroy(gameObject, 8); }
    private void Update() { transform.position += Vector3.left * 5 * Time.deltaTime; }
    private void OnTriggerEnter2D(Collider2D other) { Player player = other.GetComponent<Player>(); if (player != null) { player.Hit(); Destroy(gameObject); } }
}
