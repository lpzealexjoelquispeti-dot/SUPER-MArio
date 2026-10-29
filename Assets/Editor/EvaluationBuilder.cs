using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Rebuild is explicit: it never overwrites a student's scene on project import.
public static class EvaluationBuilder
{
    private static readonly string[] Scenes = { "1-1", "1-2", "1-3", "1-4", "GameOver", "Victory" };

    [MenuItem("Mario/Generate evaluation scenes")]
    public static void Generate()
    {
        CreateSprites();
        Ending("GameOver");
        Ending("Victory");
        Underground();
        Underwater();
        Castle();
        Scene first = EditorSceneManager.OpenScene("Assets/Scenes/1-1.unity");
        foreach (FlagPole flag in UnityEngine.Object.FindObjectsOfType<FlagPole>()) { flag.nextWorld = 1; flag.nextStage = 2; EditorUtility.SetDirty(flag); }
        EditorSceneManager.SaveScene(first);
        EditorBuildSettings.scenes = Scenes.Select(name => new EditorBuildSettingsScene("Assets/Scenes/" + name + ".unity", true)).ToArray();
        AssetDatabase.SaveAssets();
        Validate();
    }

    private static void Ending(string name)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Camera camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
        camera.tag = "MainCamera";
        camera.backgroundColor = new Color(0.015f, 0.02f, 0.06f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        new GameObject("Game Manager", typeof(GameManager));
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "Assets/Scenes/" + name + ".unity");
    }

    private static GameObject Base(string name, Color background, float width, bool water = false)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("Game Manager", typeof(GameManager));
        Camera camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
        camera.tag = "MainCamera";
        camera.orthographic = true;
        camera.orthographicSize = 7.5f;
        camera.backgroundColor = background;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.transform.position = new Vector3(13.33f, 6, -10);
        GameObject mario = Prefab("Mario", 3, 3);
        mario.GetComponent<PlayerMovement>().swimming = water;
        if (water) {
            mario.GetComponent<PlayerMovement>().moveSpeed = 4.5f;
            mario.AddComponent<WaterBubbles>().bubble = Sprite("Bubble");
        }
        LevelCamera follow = camera.gameObject.AddComponent<LevelCamera>();
        follow.target = mario.transform;
        follow.levelWidth = width;
        GameObject death = new GameObject("Death barrier", typeof(BoxCollider2D), typeof(DeathBarrier));
        death.transform.position = new Vector3(width / 2, -4, 0);
        death.GetComponent<BoxCollider2D>().size = new Vector2(width + 40, 2);
        death.GetComponent<BoxCollider2D>().isTrigger = true;
        return mario;
    }

    private static GameObject Prefab(string name, float x, float y)
    {
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/" + name + ".prefab"));
        go.transform.position = new Vector3(x, y, 0);
        return go;
    }

    private static Sprite Sprite(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Evaluation/" + name + ".png") ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/" + name + ".png");
    }

    private static GameObject Visual(string name, float x, float y, string sprite, float width = 1, float height = 1, bool solid = false)
    {
        GameObject go = new GameObject(name, typeof(SpriteRenderer));
        go.transform.position = new Vector3(x, y, 0);
        SpriteRenderer render = go.GetComponent<SpriteRenderer>();
        render.sprite = Sprite(sprite);
        render.sortingOrder = 2;
        render.drawMode = SpriteDrawMode.Tiled;
        render.size = new Vector2(width, height);
        if (solid) go.AddComponent<BoxCollider2D>().size = render.size;
        return go;
    }

    private static void Map(string filename, bool water, int width)
    {
        string[] rows = File.ReadAllLines("Assets/LevelData/" + filename + ".txt");
        Transform root = new GameObject("Reference terrain - 16px grid").transform;
        for (int row = 0; row < rows.Length; row++) {
            for (int x = 0; x < Mathf.Min(width, rows[row].Length); x++) {
                if (rows[row][x] != '#') continue;
                // Blue enemies in the reference are not terrain tiles.
                if (!water && row == 10 && new[] { 61, 63, 98, 101, 112 }.Contains(x)) continue;
                GameObject tile = water ? Visual("Reef", x, 12 - row, "Reef", 1, 1, true) : Prefab(row >= 11 ? "UndergroundBlock" : "UndergroundBrick", x, 12 - row);
                tile.transform.SetParent(root);
            }
        }
    }

    private static void Coins(int x, int y, int count)
    {
        for (int i = 0; i < count; i++) Prefab("Coin", x + i, y);
    }

    private static void Exit(float x, float y, int next)
    {
        GameObject gate = new GameObject("Exit to stage " + next, typeof(BoxCollider2D), typeof(LevelExit));
        gate.transform.position = new Vector3(x, y, 0);
        gate.GetComponent<BoxCollider2D>().isTrigger = true;
        gate.GetComponent<BoxCollider2D>().size = new Vector2(1.2f, 2);
        gate.GetComponent<LevelExit>().nextStage = next;
    }

    private static void Underground()
    {
        Base("1-2", Color.black, 176);
        Map("Underground", false, 176);
        for (int x = 10; x <= 14; x++) Prefab("MysteryBlock", x, 5);
        Prefab("MysteryBlock", 29, 6);
        foreach (int x in new[] { 16, 17, 29, 31, 62, 64, 99, 101, 103, 113, 135 }) Prefab("Goomba", x, 2);
        foreach (int x in new[] { 44, 46, 59, 146 }) Prefab("Koopa", x, 2);
        Coins(41, 9, 4); Coins(58, 6, 4); Coins(84, 9, 6);
        foreach (int x in new[] { 104, 110, 116 }) Prefab("Pipe", x, x == 110 ? 5 : 4);
        for (int i = 0; i < 4; i++) {
            GameObject lift = Visual("Elevator", i < 2 ? 141 : 156, 5, "Lift", 3, 0.4f, true);
            MovingLift moving = lift.AddComponent<MovingLift>();
            moving.phase = i * Mathf.PI;
        }
        Visual("Exit pipe", 169, 6.5f, "PipeBottom", 2, 11, true);
        Visual("Pipe opening", 167.5f, 5, "PipeTop", 2, 2);
        Exit(167, 5, 3);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "Assets/Scenes/1-2.unity");
    }

    private static void Underwater()
    {
        Base("1-3", new Color(0.08f, 0.18f, 0.78f), 192, true);
        Map("Underwater", true, 192);
        Visual("Water surface", 96, 12.5f, "Wave", 192, 0.4f);
        Visual("Surface ceiling", 96, 13.5f, "Bubble", 192, 1, true).GetComponent<SpriteRenderer>().enabled = false;
        foreach (int x in new[] { 11, 33, 50, 83, 89, 102, 120, 147, 149 }) Visual("Coral", x, 3, "Coral", 1, x % 3 + 2);
        Coins(14, 2, 2); Coins(27, 9, 3); Coins(36, 2, 3); Coins(67, 4, 3); Coins(101, 3, 3); Coins(113, 8, 3); Coins(134, 2, 3); Coins(159, 5, 3);
        foreach (int x in new[] { 22, 46, 55, 83, 94, 105 }) Enemy(x, 3 + x % 7, true);
        foreach (int x in new[] { 76, 79, 81, 95, 98, 117, 128, 132, 137, 145, 150, 164, 167, 175, 183, 186 }) Enemy(x, 3 + x % 8, false);
        Visual("Exit pipe", 190, 7, "PipeBottom", 2, 3, true);
        Visual("Pipe opening", 188.5f, 7, "PipeTop", 2, 2);
        Exit(188, 7, 4);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "Assets/Scenes/1-3.unity");
    }

    private static void Enemy(int x, int y, bool squid)
    {
        GameObject enemy = Visual(squid ? "Blooper" : "Cheep Cheep", x, y, squid ? "Squid" : "Fish");
        enemy.layer = LayerMask.NameToLayer("Enemy");
        enemy.AddComponent<BoxCollider2D>().isTrigger = true;
        enemy.GetComponent<BoxCollider2D>().size = new Vector2(0.7f, 0.7f);
        enemy.AddComponent<WaterEnemy>().squid = squid;
    }

    private static void Castle()
    {
        Base("1-4", new Color(0.045f, 0.025f, 0.07f), 110);
        for (int x = 0; x < 110; x++) {
            Visual("Vault", x, 13, "UndergroundBrick", 1, 1, true);
            bool gap = (x > 18 && x < 24) || (x > 39 && x < 45) || (x > 61 && x < 67);
            if (!gap) Visual("Castle floor", x, 0.5f, "HardBlock", 1, 2, true);
            else {
                GameObject lava = Visual("Lava", x, 0, "Lava", 1, 1);
                lava.AddComponent<BoxCollider2D>().isTrigger = true;
                lava.AddComponent<CastleHazard>();
            }
        }
        for (int x = 8; x < 80; x += 14) {
            Visual("Pillar", x, 2, "HardBlock", 2, 2, true);
            Coins(x, 4, 3);
            GameObject fire = Visual("Firebar", x + 4, 4, "Flame");
            fire.AddComponent<CastleHazard>().rotating = true;
            fire.AddComponent<CircleCollider2D>().isTrigger = true;
        }
        GameObject boss = Visual("Bowser - original placeholder art", 93, 3, "Boss", 3, 3);
        boss.GetComponent<SpriteRenderer>().drawMode = SpriteDrawMode.Simple;
        boss.transform.localScale = Vector3.one * 3;
        boss.AddComponent<BoxCollider2D>().isTrigger = true;
        boss.GetComponent<BoxCollider2D>().size = new Vector2(0.8f, 0.8f);
        boss.AddComponent<CastleBoss>().fireSprite = Sprite("Flame");
        GameObject axe = Visual("Bridge axe - reach to win", 103, 3, "Axe", 1, 1);
        axe.AddComponent<BoxCollider2D>().isTrigger = true;
        axe.AddComponent<CastleAxe>().boss = boss;
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "Assets/Scenes/1-4.unity");
    }

    private static void CreateSprites()
    {
        Directory.CreateDirectory("Assets/Sprites/Evaluation");
        Pixel("Fish", new[] { "........", "..RRR...", ".RWWKR..", "RRWWRR.Y", ".RRRRRYY", "..YYR..Y", "...Y....", "........" });
        Pixel("Squid", new[] { "...WW...", "..WWWW..", ".WWWWWW.", ".WKWWKW.", "..WWWW..", ".WWWWWW.", ".W.W.W..", "..W.W.W." });
        Pixel("Coral", new[] { "...R....", ".R.R.R..", ".RRRR...", "...R.R..", "..RRRR..", ".R.R....", ".RRR....", "...R...." });
        Pixel("Bubble", new[] { "..WWWW..", ".W....W.", "W......W", "W......W", "W......W", "W......W", ".W....W.", "..WWWW.." });
        Pixel("Reef", new[] { "GGGGKGGG", "GYYGKGGY", "GYGGKGYG", "GGGKKGGG", "KKGGGGKK", "GKGYYGKG", "GKGYGGKG", "GGGGGGGG" });
        Pixel("Wave", new[] { ".W...W..", "W.W.W.W.", "...W...W", "........", "........", "........", "........", "........" });
        Pixel("Lift", new[] { "YYYYYYYY", "YRRRRRRY", "YKKYKKYY", "YYYYYYYY", "........", "........", "........", "........" });
        Pixel("Lava", new[] { "YRYRYRYR", "RYYRRYYR", "RRRRRRRR", "RYRRYRRY", "RRRRRRRR", "RRRRRRRR", "RRRRRRRR", "RRRRRRRR" });
        Pixel("Flame", new[] { "...Y....", "..YYR...", "..YRR...", ".YYRRY..", ".YRRRY..", ".YYRRY..", "..YYY...", "........" });
        Pixel("Axe", new[] { "...WWWW.", "...WKKWW", "...WWWW.", "...Y....", "...Y....", "...Y....", "...Y....", "...Y...." });
        Pixel("Boss", new[] { "..W......W......", "..YY....YY......", "..YYYYYYYY......", ".YYWKYYWKYY.....", ".YYYYYYYYYYY....", "..YRRRRRYYYY....", "...WWWWYYYYGGG..", "..YYYYYYYYGGWGG.", ".YY.YYYYYGGGGWG.", "YY..YYYYYGGWGGG.", "....YYYYYGGGGWG.", "....YYYYYYGGGG..", "...YYY..YYYY....", "..YYYY..YYYYY...", ".WWWWW..WWWWW...", "................" });
        AssetDatabase.Refresh();
    }

    private static void Pixel(string name, string[] rows)
    {
        string path = "Assets/Sprites/Evaluation/" + name + ".png";
        Texture2D texture = new Texture2D(rows[0].Length, rows.Length, TextureFormat.RGBA32, false);
        for (int y = 0; y < rows.Length; y++) for (int x = 0; x < rows[y].Length; x++) {
            char c = rows[y][x];
            Color color = c == 'R' ? new Color(0.95f, 0.2f, 0.22f) : c == 'W' ? Color.white : c == 'K' ? new Color(0.03f, 0.08f, 0.12f) : c == 'Y' ? new Color(1, 0.7f, 0.2f) : c == 'G' ? new Color(0.1f, 0.65f, 0.25f) : Color.clear;
            texture.SetPixel(x, rows.Length - y - 1, color);
        }
        texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = rows.Length;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spriteImportMode = SpriteImportMode.Single;
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
    }

    [MenuItem("Mario/Validate evaluation scenes")]
    public static void Validate()
    {
        foreach (string name in Scenes) {
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity");
            foreach (GameObject root in scene.GetRootGameObjects()) foreach (Transform child in root.GetComponentsInChildren<Transform>(true)) {
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) != 0) throw new Exception("Missing script: " + name + "/" + child.name);
            }
            if (Camera.main == null) throw new Exception("No camera in " + name);
            if (name.StartsWith("1-") && UnityEngine.Object.FindObjectOfType<Player>() == null) throw new Exception("No player in " + name);
        }
        Debug.Log("EVALUATION: all six scenes validated; no missing scripts.");
        EditorSceneManager.OpenScene("Assets/Scenes/1-1.unity");
    }

    public static void Build()
    {
        Validate();
        var result = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, "Builds/SuperMario/SuperMario.exe", BuildTarget.StandaloneWindows64, BuildOptions.Development);
        if (result.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new Exception("Build failed: " + result.summary.result);
        Debug.Log("EVALUATION: Windows build succeeded.");
    }
}
