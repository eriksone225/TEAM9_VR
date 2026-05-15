using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

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

    [Header("Runtime Values - read only")]
    public int score;
    public int basketballShotsMade;
    public int basketballShotsTotal;
    public int baseballHits;
    public int baseballPitches;
    public float timer;
    public bool sessionRunning;

    void Start()
    {
        ShowLobby();
        UpdateHUD();
    }

    void Update()
    {
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

    public void ShowLobby()
    {
        currentMode = SportsVRMode.Lobby;
        sessionRunning = false;
        SetActiveSafe(lobbyRoot, true);
        SetActiveSafe(basketballRoot, false);
        SetActiveSafe(baseballRoot, false);
        SetInfo("Pilih mode: Basketball atau Baseball.");
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

        SetActiveSafe(lobbyRoot, false);
        SetActiveSafe(basketballRoot, true);
        SetActiveSafe(baseballRoot, false);

        SetInfo("Basketball: lempar bola dari 3 posisi. Dekat=1, Tengah=2, Jauh=3.");
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

        SetActiveSafe(lobbyRoot, false);
        SetActiveSafe(basketballRoot, false);
        SetActiveSafe(baseballRoot, true);

        SetInfo("Baseball: pilih kecepatan pitch, pegang bat, lalu pukul bola.");
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
                scoreText.text = "SPORTS VR";
        }

        if (timeText != null)
        {
            int seconds = Mathf.CeilToInt(timer);
            int m = seconds / 60;
            int s = seconds % 60;
            timeText.text = "WAKTU\n" + m.ToString("00") + ":" + s.ToString("00");
        }
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