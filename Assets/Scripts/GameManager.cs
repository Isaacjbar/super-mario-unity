using UnityEngine;

public enum GameState { Playing, Won, GameOver }

/// <summary>
/// Entry point: builds the camera and the level at runtime, keeps score/lives and draws the HUD.
/// Works in any (even empty) scene thanks to the RuntimeInitializeOnLoadMethod bootstrap.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState State { get; private set; } = GameState.Playing;
    public int Score { get; private set; }
    public int Coins { get; private set; }
    public int Lives { get; private set; } = 3;

    GameObject world;
    CameraFollow cameraFollow;
    GUIStyle hudStyle, msgStyle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (FindAnyObjectByType<GameManager>() != null) return;
        new GameObject("GameManager").AddComponent<GameManager>();
    }

    void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
        SoundFx.Init(gameObject);
        SetupCamera();
        RebuildLevel();
    }

    void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 7f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color32(92, 148, 252, 255);
        cam.transform.position = new Vector3(0f, 5.5f, -10f);
        cameraFollow = cam.GetComponent<CameraFollow>();
        if (cameraFollow == null) cameraFollow = cam.gameObject.AddComponent<CameraFollow>();
    }

    void RebuildLevel()
    {
        if (world != null) Destroy(world);
        var player = LevelBuilder.Build(out world);
        cameraFollow.Target = player.transform;
        cameraFollow.Snap();
    }

    void Update()
    {
        if (State != GameState.Playing && Input.GetKeyDown(KeyCode.R))
        {
            Score = 0;
            Coins = 0;
            Lives = 3;
            State = GameState.Playing;
            RebuildLevel();
        }
    }

    // ---------- Game events ----------

    public void AddScore(int points) => Score += points;

    public void AddCoin()
    {
        Coins++;
        Score += 200;
        SoundFx.Play(SoundFx.Coin);
        if (Coins >= 100)
        {
            Coins = 0;
            Lives++;
        }
    }

    public void PlayerDied()
    {
        Lives--;
        if (Lives <= 0)
        {
            State = GameState.GameOver;
            return;
        }
        RebuildLevel();
    }

    public void Win()
    {
        if (State != GameState.Playing) return;
        State = GameState.Won;
        Score += 5000;
        SoundFx.Play(SoundFx.Win);
    }

    // ---------- HUD ----------

    void OnGUI()
    {
        if (hudStyle == null || hudStyle.fontSize != Mathf.RoundToInt(Screen.height * 0.045f))
        {
            hudStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(Screen.height * 0.045f),
                fontStyle = FontStyle.Bold,
            };
            msgStyle = new GUIStyle(hudStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(Screen.height * 0.08f),
            };
        }

        string hud = $"SCORE {Score:D6}    COINS x{Coins:D2}    LIVES x{Lives}";
        DrawShadowed(new Rect(20, 10, Screen.width, Screen.height * 0.08f), hud, hudStyle);

        if (State == GameState.Won)
            DrawShadowed(new Rect(0, 0, Screen.width, Screen.height), "LEVEL COMPLETE!\nPress R to play again", msgStyle);
        else if (State == GameState.GameOver)
            DrawShadowed(new Rect(0, 0, Screen.width, Screen.height), "GAME OVER\nPress R to retry", msgStyle);
    }

    static void DrawShadowed(Rect rect, string text, GUIStyle style)
    {
        var old = style.normal.textColor;
        style.normal.textColor = Color.black;
        GUI.Label(new Rect(rect.x + 3, rect.y + 3, rect.width, rect.height), text, style);
        style.normal.textColor = Color.white;
        GUI.Label(rect, text, style);
        style.normal.textColor = old;
    }
}
