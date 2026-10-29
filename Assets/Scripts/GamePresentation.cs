using UnityEngine;
using UnityEngine.SceneManagement;

// Persistent HUD and original synthesized sounds; no external audio download needed.
public class GamePresentation : MonoBehaviour
{
    private static AudioSource effects;
    private AudioSource music;
    private string scene;
    private float nextNote;
    private int note;
    private readonly float[] melody = { 261.6f, 329.6f, 392f, 523.2f, 440f, 392f, 329.6f, 293.7f };

    private void Awake()
    {
        effects = gameObject.AddComponent<AudioSource>();
        music = gameObject.AddComponent<AudioSource>();
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
        if (scene != current) { scene = current; note = 0; nextNote = Time.time + 0.2f; }
        bool ending = scene == "GameOver" || scene == "Victory";
        if (ending && Input.GetKeyDown(KeyCode.Return)) GameManager.Instance.NewGame();
        if (Time.time >= nextNote && (!ending || note < 8)) {
            float pitch = scene == "1-2" ? 0.5f : scene == "1-3" ? 1.5f : scene == "1-4" ? 0.7f : 1f;
            if (scene == "GameOver") pitch = 0.6f;
            Play(music, melody[(scene == "GameOver" ? 7 - note % 8 : note % 8)] * pitch, 0.2f, 0.035f);
            nextNote = Time.time + (scene == "1-3" ? 0.35f : 0.24f);
            note++;
        }
    }

    private void OnGUI()
    {
        if (GameManager.Instance == null) return;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 960f, Screen.height / 540f, 1));
        GUIStyle text = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        text.normal.textColor = Color.white;
        bool ending = scene == "GameOver" || scene == "Victory";
        if (ending) {
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
