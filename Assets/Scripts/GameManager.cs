using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int world { get; private set; }
    public int stage { get; private set; }
    public int lives { get; private set; }
    public int coins { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            DestroyImmediate(gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) {
            Instance = null;
        }
    }

    private void Start()
    {
        Application.targetFrameRate = 60;

        lives = 3;
        coins = 0;
        string[] level = SceneManager.GetActiveScene().name.Split('-');
        world = 1;
        stage = 1;
        if (level.Length == 2 && int.TryParse(level[0], out int w) && int.TryParse(level[1], out int s)) {
            world = w;
            stage = s;
        }
        gameObject.AddComponent<GamePresentation>();
    }

    public void NewGame()
    {
        CancelInvoke();
        Time.timeScale = 1f;
        lives = 3;
        coins = 0;

        LoadLevel(1, 1);
    }

    public void GameOver()
    {
        CancelInvoke();
        SceneManager.LoadScene("GameOver");
    }

    public void LoadLevel(int world, int stage)
    {
        CancelInvoke();
        this.world = world;
        this.stage = stage;

        SceneManager.LoadScene($"{world}-{stage}");
    }

    public void NextLevel()
    {
        if (stage < 4) LoadLevel(world, stage + 1);
        else SceneManager.LoadScene("Victory");
    }

    public void ResetLevel(float delay)
    {
        CancelInvoke(nameof(ResetLevel));
        Invoke(nameof(ResetLevel), delay);
    }

    public void ResetLevel()
    {
        lives--;

        if (lives > 0) {
            LoadLevel(world, stage);
        } else {
            GameOver();
        }
    }

    public void AddCoin()
    {
        GamePresentation.Sound(1100f, 0.09f);
        coins++;

        if (coins == 100)
        {
            coins = 0;
            AddLife();
        }
    }

    public void AddLife()
    {
        lives++;
    }

}
