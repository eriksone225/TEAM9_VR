using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum SportsVRMode
{
    Lobby,
    Basketball,
    Baseball
}

public class SportsVRGameManager : MonoBehaviour
{
    [Header("Mode Roots - drag scene parent objects here")]
    public GameObject lobbyRoot;
    public GameObject basketballRoot;
    public GameObject baseballRoot;

    [Header("Session Settings")]
    public float basketballSessionSeconds = 60f;
    public float baseballSessionSeconds = 60f;
    public SportsVRMode currentMode = SportsVRMode.Lobby;

    [Header("HUD Text - drag TextMeshProUGUI objects here")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI infoText;

    [Header("SFX")]
    public AudioSource sfxSource;
    public AudioClip scoreClip;
    public AudioClip hitClip;
    public AudioClip clickClip;

    [Header("Auto Setup / Safety")]
    [Tooltip("Repairs common missing scene setup at runtime so the project remains buildable while you are still arranging the scene.")]
    public bool autoSetupScene = true;
    public bool createFallbackUI = true;
    public bool createFallbackColliders = true;
    public bool keyboardTestingShortcuts = true;

    [Header("Runtime Values - read only")]
    public int score;
    public int basketballShotsMade;
    public int basketballShotsTotal;
    public int baseballHits;
    public int baseballPitches;
    public float timer;
    public bool sessionRunning;

    BaseballPitcher cachedBaseballPitcher;

    void Awake()
    {
        if (autoSetupScene)
            AutoSetupScene();
    }

    void Start()
    {
        ShowLobby();
        UpdateHUD();
    }

    void Update()
    {
        if (keyboardTestingShortcuts)
            HandleKeyboardShortcuts();

        if (!sessionRunning) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = 0f;
            sessionRunning = false;
            SetInfo("Sesi selesai! Buka Rekap Skor atau Main Lagi.");
        }

        UpdateHUD();
    }

    void HandleKeyboardShortcuts()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) StartBasketball();
        if (Input.GetKeyDown(KeyCode.Alpha2)) StartBaseball();
        if (Input.GetKeyDown(KeyCode.L)) ShowLobby();
        if (Input.GetKeyDown(KeyCode.R)) RestartCurrentSession();
        if (Input.GetKeyDown(KeyCode.Space) && cachedBaseballPitcher != null && currentMode == SportsVRMode.Baseball)
            cachedBaseballPitcher.PitchOne();
    }

    void AutoSetupScene()
    {
        lobbyRoot = lobbyRoot != null ? lobbyRoot : FindOrCreateRoot("LOBBY_ROOT");
        basketballRoot = basketballRoot != null ? basketballRoot : FindOrCreateRoot("BASKETBALL_ROOT");
        baseballRoot = baseballRoot != null ? baseballRoot : FindOrCreateRoot("BASEBALL_ROOT");

        EnsureUniqueAudioListener();
        EnsureAudioSource();
        EnsureHaptics();

        if (createFallbackUI)
        {
            EnsureHUD();
            EnsureLobbyMenu();
            EnsurePauseMenu();
        }

        SetupBasketballScene();
        SetupBaseballScene();
    }

    public void ShowLobby()
    {
        currentMode = SportsVRMode.Lobby;
        sessionRunning = false;
        Time.timeScale = 1f;
        SetActiveSafe(lobbyRoot, true);
        SetActiveSafe(basketballRoot, false);
        SetActiveSafe(baseballRoot, false);
        SetInfo("Pilih mode: Basketball atau Baseball. Keyboard: 1=Basketball, 2=Baseball.");
        PlayClick();
        UpdateHUD();
    }

    public void StartBasketball()
    {
        currentMode = SportsVRMode.Basketball;
        score = 0;
        basketballShotsMade = 0;
        basketballShotsTotal = 0;
        timer = basketballSessionSeconds;
        sessionRunning = true;
        Time.timeScale = 1f;

        SetActiveSafe(lobbyRoot, false);
        SetActiveSafe(basketballRoot, true);
        SetActiveSafe(baseballRoot, false);

        SetInfo("Basketball: lempar satu bola dari 3 area. Dekat=1, Tengah=2, Jauh=3.");
        PlayClick();
        UpdateHUD();
    }

    public void StartBaseball()
    {
        currentMode = SportsVRMode.Baseball;
        score = 0;
        baseballHits = 0;
        baseballPitches = 0;
        timer = baseballSessionSeconds;
        sessionRunning = true;
        Time.timeScale = 1f;

        SetActiveSafe(lobbyRoot, false);
        SetActiveSafe(basketballRoot, false);
        SetActiveSafe(baseballRoot, true);

        SetInfo("Baseball: pilih kecepatan, pegang bat, tekan PITCH/Space, lalu pukul bola.");
        PlayClick();
        UpdateHUD();
    }

    public void RestartCurrentSession()
    {
        if (currentMode == SportsVRMode.Basketball) StartBasketball();
        else if (currentMode == SportsVRMode.Baseball) StartBaseball();
        else ShowLobby();
    }

    public void AddBasketballScore(int points)
    {
        if (currentMode != SportsVRMode.Basketball) return;
        if (!sessionRunning) return;

        score += points;
        basketballShotsMade++;
        SetInfo("Masuk! +" + points + " poin.");
        PlayOne(scoreClip);
        SimpleHaptics.Instance?.PulseBoth(0.55f, 0.12f);
        UpdateHUD();
    }

    public void RegisterBasketballShot()
    {
        if (currentMode != SportsVRMode.Basketball) return;
        if (!sessionRunning) return;

        basketballShotsTotal++;
        UpdateHUD();
    }

    public void RegisterBaseballPitch()
    {
        if (currentMode != SportsVRMode.Baseball) return;
        if (!sessionRunning) return;

        baseballPitches++;
        SetInfo("Pitch dilempar. Ayunkan bat!");
        UpdateHUD();
    }

    public void RegisterBaseballHit(float hitPower)
    {
        if (currentMode != SportsVRMode.Baseball) return;
        if (!sessionRunning) return;

        baseballHits++;
        int gained = Mathf.Clamp(Mathf.RoundToInt(hitPower * 0.5f), 1, 10);
        score += gained;

        SetInfo("Bagus! HIT +" + gained + " poin.");
        PlayOne(hitClip);
        SimpleHaptics.Instance?.PulseBoth(0.75f, 0.16f);
        UpdateHUD();
    }

    public void SetInfo(string message)
    {
        if (infoText != null) infoText.text = message;
        Debug.Log("[SportsVR] " + message);
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = Mathf.Clamp01(value);
    }

    public void QuitGame()
    {
        PlayClick();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void UpdateHUD()
    {
        if (scoreText != null)
        {
            if (currentMode == SportsVRMode.Basketball)
                scoreText.text = "SKOR\n" + score + "\nShot " + basketballShotsMade + "/" + basketballShotsTotal;
            else if (currentMode == SportsVRMode.Baseball)
                scoreText.text = "SKOR\n" + score + "\nHit " + baseballHits + "/" + baseballPitches;
            else
                scoreText.text = "SPORTS VR\n1 Basketball | 2 Baseball";
        }

        if (timeText != null)
        {
            int seconds = Mathf.CeilToInt(timer);
            int m = seconds / 60;
            int s = seconds % 60;
            timeText.text = "WAKTU\n" + m.ToString("00") + ":" + s.ToString("00");
        }
    }

    void SetupBasketballScene()
    {
        if (basketballRoot == null) return;

        if (createFallbackColliders)
            EnsureFloorCollider(basketballRoot.transform, "Basketball_FloorCollider", new Vector3(0, -0.05f, 0), new Vector3(30f, 0.1f, 42f));

        GameObject basketball = FindBasketballObject();
        if (basketball != null)
        {
            EnsureSpherePhysics(basketball, 0.6f, 0.24f);
            TryAddXRGrabInteractable(basketball);

            BasketballBallInfo info = AddOrGet<BasketballBallInfo>(basketball);
            info.gameManager = this;
            info.shotSpotDekat = FindOrCreatePoint(basketballRoot.transform, new[] { "ShotSpot_Dekat", "Marker_Dekat" }, new Vector3(-3f, 0.02f, -3f));
            info.shotSpotTengah = FindOrCreatePoint(basketballRoot.transform, new[] { "ShotSpot_Tengah", "Marker_Tengah" }, new Vector3(0f, 0.02f, -7f));
            info.shotSpotJauh = FindOrCreatePoint(basketballRoot.transform, new[] { "ShotSpot_Jauh", "Marker_Jauh" }, new Vector3(3f, 0.02f, -11f));

            BallResetter resetter = AddOrGet<BallResetter>(basketball);
            resetter.resetPoint = FindOrCreatePoint(basketballRoot.transform, new[] { "BallSpawn_Basketball", "ResetPoint" }, basketball.transform.position);
        }

        GameObject trigger = FindChildByNameContains(basketballRoot.transform, "HoopScoreTrigger");
        if (trigger == null)
        {
            Transform hoopParent = FindChildByNameContainsTransform(basketballRoot.transform, "Hoop") ?? basketballRoot.transform;
            trigger = new GameObject("HoopScoreTrigger");
            trigger.transform.SetParent(hoopParent, false);
            trigger.transform.localPosition = new Vector3(0f, 3f, 0f);
            trigger.transform.localScale = Vector3.one;
        }

        BoxCollider triggerCollider = AddOrGet<BoxCollider>(trigger);
        triggerCollider.isTrigger = true;
        if (triggerCollider.size == Vector3.one)
            triggerCollider.size = new Vector3(0.8f, 0.35f, 0.8f);

        BasketballHoopTrigger hoopTrigger = AddOrGet<BasketballHoopTrigger>(trigger);
        hoopTrigger.gameManager = this;
        hoopTrigger.requireBallToBeThrown = true;
    }

    void SetupBaseballScene()
    {
        if (baseballRoot == null) return;

        if (createFallbackColliders)
            EnsureFloorCollider(baseballRoot.transform, "Baseball_FloorCollider", new Vector3(0, -0.05f, 0), new Vector3(45f, 0.1f, 45f));

        GameObject bat = FindBatObject();
        if (bat != null)
        {
            BaseballBatMarker marker = AddOrGet<BaseballBatMarker>(bat);
            marker.hitMultiplier = 1f;
            EnsureBatPhysics(bat);
            TryAddXRGrabInteractable(bat);
            AddOrGet<BatSpeedTracker>(bat);
        }

        GameObject pitcherObject = FindChildByNameContains(baseballRoot.transform, "PitchingMachine")
                                   ?? FindChildByNameContains(baseballRoot.transform, "Pitcher")
                                   ?? new GameObject("PitchingMachine");

        if (pitcherObject.transform.parent == null)
            pitcherObject.transform.SetParent(baseballRoot.transform, false);

        cachedBaseballPitcher = AddOrGet<BaseballPitcher>(pitcherObject);
        cachedBaseballPitcher.gameManager = this;
        cachedBaseballPitcher.spawnPoint = FindOrCreatePoint(pitcherObject.transform, new[] { "PitchSpawnPoint", "BaseballSpawnPoint" }, new Vector3(0f, 0.8f, 0f));
        cachedBaseballPitcher.targetPoint = FindOrCreatePoint(baseballRoot.transform, new[] { "PitchTargetPoint", "HomePlate", "BatterArea" }, pitcherObject.transform.position + Vector3.forward * 12f + Vector3.up * 0.8f);

        EnsureBaseballSpeedUI(cachedBaseballPitcher);
    }

    void EnsureHUD()
    {
        if (scoreText != null && timeText != null && infoText != null) return;

        GameObject uiRoot = FindOrCreateRoot("UI_ROOT");
        GameObject canvasGO = FindChildByNameContains(uiRoot.transform, "HUDCanvas") ?? new GameObject("HUDCanvas");
        canvasGO.transform.SetParent(uiRoot.transform, false);

        Canvas canvas = AddOrGet<Canvas>(canvasGO);
        canvas.renderMode = RenderMode.WorldSpace;
        AddOrGet<GraphicRaycaster>(canvasGO);

        RectTransform rect = AddOrGet<RectTransform>(canvasGO);
        rect.sizeDelta = new Vector2(600, 260);
        canvasGO.transform.position = new Vector3(0f, 1.8f, 2f);
        canvasGO.transform.localScale = Vector3.one * 0.0025f;

        CanvasFollowHead follow = canvasGO.GetComponent<CanvasFollowHead>();
        if (follow == null) follow = canvasGO.AddComponent<CanvasFollowHead>();
        if (follow.headCamera == null && Camera.main != null) follow.headCamera = Camera.main.transform;
        follow.distanceFromHead = 1.8f;
        follow.heightOffset = 0.1f;
        follow.followEveryFrame = true;

        scoreText = scoreText != null ? scoreText : CreateTMPText(canvasGO.transform, "ScoreText", new Vector2(-200, 60), new Vector2(190, 120), "SPORTS VR", 28);
        timeText = timeText != null ? timeText : CreateTMPText(canvasGO.transform, "TimeText", new Vector2(200, 60), new Vector2(190, 120), "WAKTU", 28);
        infoText = infoText != null ? infoText : CreateTMPText(canvasGO.transform, "InfoText", new Vector2(0, -70), new Vector2(560, 100), "Pilih mode permainan.", 24);
    }

    void EnsureLobbyMenu()
    {
        if (lobbyRoot == null) return;
        if (FindChildByNameContains(lobbyRoot.transform, "LobbyMainMenuCanvas") != null) return;

        GameObject canvasGO = new GameObject("LobbyMainMenuCanvas");
        canvasGO.transform.SetParent(lobbyRoot.transform, false);
        canvasGO.transform.localPosition = new Vector3(0f, 1.6f, 2.2f);
        canvasGO.transform.localRotation = Quaternion.Euler(0, 180, 0);
        canvasGO.transform.localScale = Vector3.one * 0.003f;

        Canvas canvas = AddOrGet<Canvas>(canvasGO);
        canvas.renderMode = RenderMode.WorldSpace;
        AddOrGet<GraphicRaycaster>(canvasGO);
        RectTransform rect = AddOrGet<RectTransform>(canvasGO);
        rect.sizeDelta = new Vector2(700, 650);

        Image bg = canvasGO.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.55f);

        CreateTMPText(canvasGO.transform, "Title", new Vector2(0, 260), new Vector2(650, 80), "SPORTS VR", 46);
        CreateUIButton(canvasGO.transform, "Btn_Basketball", "Pilih Basketball", new Vector2(0, 150), StartBasketball);
        CreateUIButton(canvasGO.transform, "Btn_Baseball", "Pilih Baseball", new Vector2(0, 70), StartBaseball);
        CreateUIButton(canvasGO.transform, "Btn_Rules", "Peraturan", new Vector2(0, -10), () => SetInfo("Basketball: lempar dari dekat/tengah/jauh. Baseball: pilih speed lalu pukul bola."));
        CreateUIButton(canvasGO.transform, "Btn_ScoreRecap", "Rekap Skor", new Vector2(0, -90), () => SetInfo("Rekap: Skor " + score + " | Basket " + basketballShotsMade + "/" + basketballShotsTotal + " | Baseball " + baseballHits + "/" + baseballPitches));
        CreateUIButton(canvasGO.transform, "Btn_Quit", "Keluar", new Vector2(0, -170), QuitGame);
    }

    void EnsurePauseMenu()
    {
        GameObject pauseRoot = FindOrCreateRoot("PAUSE_MENU");
        PauseMenuController controller = pauseRoot.GetComponent<PauseMenuController>();
        if (controller == null) controller = pauseRoot.AddComponent<PauseMenuController>();
        controller.gameManager = this;

        GameObject panel = controller.pausePanel;
        if (panel == null)
        {
            panel = new GameObject("PauseCanvas");
            panel.transform.SetParent(pauseRoot.transform, false);
            panel.transform.position = new Vector3(0, 1.6f, 1.8f);
            panel.transform.localScale = Vector3.one * 0.003f;

            Canvas canvas = AddOrGet<Canvas>(panel);
            canvas.renderMode = RenderMode.WorldSpace;
            AddOrGet<GraphicRaycaster>(panel);
            RectTransform rect = AddOrGet<RectTransform>(panel);
            rect.sizeDelta = new Vector2(650, 520);

            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.72f);

            CreateTMPText(panel.transform, "PauseTitle", new Vector2(0, 190), new Vector2(600, 70), "PAUSE", 42);
            CreateUIButton(panel.transform, "Btn_Continue", "Lanjutkan", new Vector2(0, 95), controller.ContinueGame);
            CreateUIButton(panel.transform, "Btn_Restart", "Ulangi Sesi", new Vector2(0, 20), controller.RestartSession);
            CreateUIButton(panel.transform, "Btn_Lobby", "Kembali ke Lobby", new Vector2(0, -55), controller.BackToLobby);
            CreateUIButton(panel.transform, "Btn_Quit", "Keluar Game", new Vector2(0, -130), controller.QuitGame);
        }

        controller.pausePanel = panel;
        CanvasFollowHead follow = panel.GetComponent<CanvasFollowHead>();
        if (follow == null) follow = panel.AddComponent<CanvasFollowHead>();
        if (follow.headCamera == null && Camera.main != null) follow.headCamera = Camera.main.transform;
        follow.followEveryFrame = false;
        controller.menuFollow = follow;
        controller.SetPaused(false);
    }

    void EnsureBaseballSpeedUI(BaseballPitcher pitcher)
    {
        if (pitcher == null || baseballRoot == null) return;
        if (FindChildByNameContains(baseballRoot.transform, "BaseballSpeedCanvas") != null) return;

        GameObject canvasGO = new GameObject("BaseballSpeedCanvas");
        canvasGO.transform.SetParent(baseballRoot.transform, false);
        canvasGO.transform.localPosition = new Vector3(0f, 1.4f, 2.3f);
        canvasGO.transform.localRotation = Quaternion.Euler(0, 180, 0);
        canvasGO.transform.localScale = Vector3.one * 0.003f;

        Canvas canvas = AddOrGet<Canvas>(canvasGO);
        canvas.renderMode = RenderMode.WorldSpace;
        AddOrGet<GraphicRaycaster>(canvasGO);
        RectTransform rect = AddOrGet<RectTransform>(canvasGO);
        rect.sizeDelta = new Vector2(740, 360);

        Image bg = canvasGO.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.55f);

        CreateTMPText(canvasGO.transform, "BaseballTitle", new Vector2(0, 130), new Vector2(700, 60), "BASEBALL SPEED", 34);
        CreateUIButton(canvasGO.transform, "Btn_Slow", "Lambat", new Vector2(-230, 45), pitcher.SetSlow, new Vector2(180, 60));
        CreateUIButton(canvasGO.transform, "Btn_Medium", "Sedang", new Vector2(0, 45), pitcher.SetMedium, new Vector2(180, 60));
        CreateUIButton(canvasGO.transform, "Btn_Fast", "Cepat", new Vector2(230, 45), pitcher.SetFast, new Vector2(180, 60));
        CreateUIButton(canvasGO.transform, "Btn_Pitch", "PITCH / Space", new Vector2(-120, -55), pitcher.PitchOne, new Vector2(230, 70));
        CreateUIButton(canvasGO.transform, "Btn_Auto", "Auto Pitch", new Vector2(140, -55), pitcher.StartAutoPitch, new Vector2(220, 70));
        CreateUIButton(canvasGO.transform, "Btn_Stop", "Stop", new Vector2(0, -135), pitcher.StopAutoPitch, new Vector2(180, 55));
    }

    TextMeshProUGUI CreateTMPText(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, string text, float fontSize)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rect = AddOrGet<RectTransform>(go);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return tmp;
    }

    Button CreateUIButton(Transform parent, string name, string label, Vector2 anchoredPosition, UnityAction action, Vector2? size = null)
    {
        GameObject buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent, false);
        RectTransform rect = AddOrGet<RectTransform>(buttonGO);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size ?? new Vector2(360, 62);

        Image image = buttonGO.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.92f);

        Button button = buttonGO.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        TextMeshProUGUI text = CreateTMPText(buttonGO.transform, "Text (TMP)", Vector2.zero, rect.sizeDelta, label, 24);
        text.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        return button;
    }

    void EnsureAudioSource()
    {
        if (sfxSource != null) return;
        GameObject audioRoot = FindOrCreateRoot("AUDIO");
        sfxSource = audioRoot.GetComponent<AudioSource>();
        if (sfxSource == null) sfxSource = audioRoot.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
    }

    void EnsureHaptics()
    {
        if (SimpleHaptics.Instance != null) return;
        if (GetComponent<SimpleHaptics>() == null) gameObject.AddComponent<SimpleHaptics>();
    }

    void EnsureUniqueAudioListener()
    {
        AudioListener[] listeners = FindAll<AudioListener>();
        if (listeners.Length <= 1) return;

        AudioListener keep = null;
        if (Camera.main != null) keep = Camera.main.GetComponent<AudioListener>();
        if (keep == null) keep = listeners[0];

        foreach (AudioListener listener in listeners)
        {
            if (listener != keep) listener.enabled = false;
        }
    }

    void EnsureFloorCollider(Transform parent, string name, Vector3 localPosition, Vector3 localScale)
    {
        GameObject floor = FindChildByExactName(parent, name);
        if (floor == null)
        {
            floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = name;
            floor.transform.SetParent(parent, false);
            floor.transform.localPosition = localPosition;
            floor.transform.localScale = localScale;
            Renderer renderer = floor.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
        }

        Collider col = floor.GetComponent<Collider>();
        if (col != null) col.isTrigger = false;
    }

    void EnsureSpherePhysics(GameObject go, float mass, float radius)
    {
        SphereCollider collider = AddOrGet<SphereCollider>(go);
        if (collider.radius <= 0.01f || Mathf.Approximately(collider.radius, 0.5f)) collider.radius = radius;
        collider.isTrigger = false;

        Rigidbody rb = AddOrGet<Rigidbody>(go);
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.mass = mass;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void EnsureBatPhysics(GameObject bat)
    {
        CapsuleCollider collider = bat.GetComponent<CapsuleCollider>();
        if (collider == null) collider = bat.AddComponent<CapsuleCollider>();
        collider.isTrigger = false;
        collider.radius = collider.radius <= 0.01f ? 0.07f : collider.radius;
        collider.height = collider.height <= 0.2f ? 1.0f : collider.height;
        collider.direction = 2;

        Rigidbody rb = AddOrGet<Rigidbody>(bat);
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.mass = 1f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void TryAddXRGrabInteractable(GameObject go)
    {
        string[] names =
        {
            "UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable, Unity.XR.Interaction.Toolkit",
            "UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable, Unity.XR.Interaction.Toolkit"
        };

        foreach (string typeName in names)
        {
            Type type = Type.GetType(typeName);
            if (type == null) continue;
            if (go.GetComponent(type) == null) go.AddComponent(type);
            return;
        }
    }

    GameObject FindBasketballObject()
    {
        GameObject exact = FindChildByExactName(basketballRoot.transform, "Basketball");
        if (exact != null) return exact;

        foreach (Transform t in basketballRoot.GetComponentsInChildren<Transform>(true))
        {
            string n = t.name.ToLowerInvariant();
            if (n.Contains("basketball") && !n.Contains("court") && !n.Contains("root") && !n.Contains("floor") && !n.Contains("spot") && !n.Contains("marker") && !n.Contains("spawn"))
                return t.gameObject;
        }

        return null;
    }

    GameObject FindBatObject()
    {
        foreach (Transform t in baseballRoot.GetComponentsInChildren<Transform>(true))
        {
            string n = t.name.ToLowerInvariant();
            if (n.Contains("bat") && !n.Contains("canvas") && !n.Contains("button"))
                return t.gameObject;
        }

        return null;
    }

    Transform FindOrCreatePoint(Transform parent, string[] possibleNames, Vector3 localOrWorldPosition)
    {
        foreach (string n in possibleNames)
        {
            GameObject found = FindChildByExactName(parent, n) ?? FindGameObjectByName(n);
            if (found != null) return found.transform;
        }

        GameObject point = new GameObject(possibleNames[0]);
        point.transform.SetParent(parent, false);
        point.transform.localPosition = localOrWorldPosition;
        return point.transform;
    }

    GameObject FindOrCreateRoot(string name)
    {
        GameObject found = FindGameObjectByName(name) ?? FindGameObjectByName("[" + name + "]");
        if (found != null) return found;
        return new GameObject(name);
    }

    GameObject FindGameObjectByName(string name)
    {
        foreach (Transform t in FindAll<Transform>())
        {
            if (t.name == name) return t.gameObject;
        }
        return null;
    }

    GameObject FindChildByExactName(Transform parent, string name)
    {
        if (parent == null) return null;
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == name) return t.gameObject;
        }
        return null;
    }

    GameObject FindChildByNameContains(Transform parent, string part)
    {
        Transform t = FindChildByNameContainsTransform(parent, part);
        return t != null ? t.gameObject : null;
    }

    Transform FindChildByNameContainsTransform(Transform parent, string part)
    {
        if (parent == null) return null;
        string p = part.ToLowerInvariant();
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
        {
            if (t.name.ToLowerInvariant().Contains(p)) return t;
        }
        return null;
    }

    T AddOrGet<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        if (component == null) component = go.AddComponent<T>();
        return component;
    }

    static T[] FindAll<T>() where T : UnityEngine.Object
    {
#if UNITY_2023_1_OR_NEWER
        return UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        return UnityEngine.Object.FindObjectsOfType<T>(true);
#endif
    }

    void SetActiveSafe(GameObject go, bool state)
    {
        if (go != null) go.SetActive(state);
    }

    void PlayOne(AudioClip clip)
    {
        if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip);
    }

    void PlayClick()
    {
        PlayOne(clickClip);
    }
}