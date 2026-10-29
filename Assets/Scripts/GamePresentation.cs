using UnityEngine;
using UnityEngine.SceneManagement;

// Persistent HUD and synthesized effects. Background music belongs to each scene.
public class GamePresentation : MonoBehaviour
{
    private static AudioSource effects;
    private string scene;

    private void Awake()
    {
        effects = gameObject.AddComponent<AudioSource>();
    }

    public static void Sound(float frequency, float duration)
    {
        if (effects != null) Play(effects, frequency, duration, 0.12f);
    }

    private static void Play(AudioSource source, float frequency, float duration, float volume)
    {
        const int rate = 22050;
        float[] samples = new float[(int)(duration * rate)];
        for (int i = 0; i < samples.Length; i++) {
            float envelope = Mathf.Min(i / 100f, 1f) * (1f - (float)i / samples.Length);
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / rate) * envelope * volume;
        }
        AudioClip clip = AudioClip.Create("Original synth tone", samples.Length, 1, rate, false);
        clip.SetData(samples, 0);
        source.PlayOneShot(clip);
        Destroy(clip, duration + 0.2f);
    }

    private void Update()
    {
        string current = SceneManager.GetActiveScene().name;
        scene = current;
        bool ending = scene == "GameOver" || scene == "Victory";
        if (ending && Input.GetKeyDown(KeyCode.Return)) GameManager.Instance.NewGame();
    }

    private void OnGUI()
    {
        if (GameManager.Instance == null) return;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 960f, Screen.height / 540f, 1));
        GUIStyle text = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        text.normal.textColor = Color.white;
        bool ending = scene == "GameOver" || scene == "Victory";
        if (ending) {
            text.normal.textColor = new Color(1, 1, 1, Mathf.Clamp01(Time.timeSinceLevelLoad / 1.2f));
            text.fontSize = 48;
            GUI.Label(new Rect(80, 145, 800, 90), scene == "GameOver" ? "GAME OVER" : "¡CASTILLO LIBERADO!", text);
            text.fontSize = 22;
            GUI.Label(new Rect(80, 250, 800, 50), "Monedas: " + GameManager.Instance.coins, text);
            if (GUI.Button(new Rect(330, 330, 300, 60), "VOLVER A JUGAR  [ENTER]")) GameManager.Instance.NewGame();
        } else {
            GUI.Box(new Rect(0, 0, 960, 52), GUIContent.none);
            GUI.Label(new Rect(20, 0, 250, 50), "MARIO  × " + GameManager.Instance.lives, text);
            GUI.Label(new Rect(350, 0, 250, 50), "MONEDAS  " + GameManager.Instance.coins.ToString("00"), text);
            GUI.Label(new Rect(690, 0, 250, 50), "MUNDO  " + scene, text);
            text.fontSize = 16;
            if (Time.timeSinceLevelLoad < 6f) GUI.Label(new Rect(60, 485, 840, 40), scene == "1-3" ? "A / D o flechas: moverse   •   ESPACIO repetidamente: nadar" : "A / D o flechas: moverse   •   ESPACIO: saltar   •   S: tuberías", text);
        }
    }
}
