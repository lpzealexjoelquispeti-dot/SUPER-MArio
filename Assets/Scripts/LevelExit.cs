using System.Collections;
using UnityEngine;

public class LevelExit : MonoBehaviour
{
    public int nextStage = 3;
    private bool entered;
    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (!entered && player != null && !player.dead) StartCoroutine(Leave(player));
    }
    private IEnumerator Leave(Player player)
    {
        entered = true;
        player.GetComponent<PlayerMovement>().enabled = false;
        GamePresentation.Sound(880f, 0.4f);
        Vector3 start = player.transform.position;
        for (float t = 0; t < 1; t += Time.deltaTime) {
            player.transform.position = Vector3.Lerp(start, transform.position, t);
            player.transform.localScale = Vector3.one * (1 - t * 0.8f);
            yield return null;
        }
        if (nextStage > 4) UnityEngine.SceneManagement.SceneManager.LoadScene("Victory");
        else GameManager.Instance.LoadLevel(1, nextStage);
    }
}
