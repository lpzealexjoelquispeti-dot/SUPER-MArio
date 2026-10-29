using UnityEngine;
public class BubbleRise : MonoBehaviour
{
    private void Start() { Destroy(gameObject, 5); }
    private void Update() { transform.position += new Vector3(Mathf.Sin(Time.time * 3) * 0.2f, 2, 0) * Time.deltaTime; }
}
