using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class CastleAxe : MonoBehaviour
{
    public GameObject boss;
    private bool activated;
    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (activated || player == null || player.dead) return;
        activated = true;
        player.GetComponent<PlayerMovement>().enabled = false;
        StartCoroutine(Win());
    }
    private IEnumerator Win()
    {
        GamePresentation.Sound(1000, 0.5f);
        if (boss != null) {
            boss.GetComponent<CastleBoss>().enabled = false;
            boss.GetComponent<Collider2D>().enabled = false;
            for (float t = 0; t < 1.5f; t += Time.deltaTime) { boss.transform.position += Vector3.down * Time.deltaTime * 5; yield return null; }
        }
        SceneManager.LoadScene("Victory");
    }
}
