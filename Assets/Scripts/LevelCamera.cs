using UnityEngine;

public class LevelCamera : MonoBehaviour
{
    public Transform target;
    public float levelWidth = 192;
    private Camera view;
    private void Awake() { view = GetComponent<Camera>(); }
    private void LateUpdate()
    {
        if (target == null) return;
        float halfWidth = view.orthographicSize * view.aspect;
        float right = Mathf.Max(halfWidth, levelWidth - halfWidth);
        float x = Mathf.Clamp(Mathf.Max(transform.position.x, target.position.x + 3), halfWidth, right);
        transform.position = new Vector3(x, 6, -10);
    }
}
