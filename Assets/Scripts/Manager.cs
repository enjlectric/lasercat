using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    public static Manager instance;

    [HideInInspector] public Player playerInstance;
    [HideInInspector] public Camera mainCam;

    public References refs;

    public UIRefs uiReferences;

    public SpriteRenderer background;

    private int _lives = 9;
    private ulong _score = 0;
    private float _scoreMultiplier = 1.0f;
    private int _unassignedSkillPoints = 0;

    public static int enemyWaveCountAddition;
    public static float enemyHPAddition;

    private int _enemiesKilled = 0;
    private int _wave = 0;
    private int _waveScore = 0;
    private int _hitsTakenInWave = 0;

    private Vector2 _backgroundSpeed = Vector2.zero;
    private Vector2 _backgroundOffset = Vector2.zero;
    private bool _isPaused = false;

    public static WaveManager waveManager;

    public static float deltaTime = 0;

    private AudioSource pauseLoop;

    private Coroutine _nextWaveCor;

    [HideInInspector] public PowerupStage FrontShot = PowerupStage.Low;
    [HideInInspector] public PowerupStage SideShot = PowerupStage.Off;
    [HideInInspector] public PowerupStage BackShot = PowerupStage.Off;
    [HideInInspector] public PowerupStage Shields = PowerupStage.Off;
    [HideInInspector] public PowerupStage Zany = PowerupStage.Off;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        mainCam = Camera.main;

        uiReferences.frontHandler.SetValue(1);
        uiReferences.sideHandler.SetValue(0);
        uiReferences.backHandler.SetValue(0);
        uiReferences.lifeHandler.SetValue(0);
        uiReferences.zanyHandler.SetValue(0);
    }

    public static void SetWaveManager(WaveManager wm)
    {
        waveManager = wm;
    }

    public ParticleSystem SpawnEffect(EffectType type, Vector3 position)
    {
        ParticleSystem prt = refs.GetEffect(type);
        ParticleSystem prtInstance = Instantiate(prt, position, Quaternion.identity);
        prtInstance.Play();
        return prt;
    }

    private void Update()
    {
        deltaTime = Time.deltaTime;

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(StartNextWave());
        }
#endif

        if (Input.GetButtonDown("Pause"))
        {
            if (_isPaused)
            {
                UnpauseGame();
            }
            else
            {
                PauseGame();
            }
        }

        _backgroundOffset -= _backgroundSpeed * Manager.deltaTime * 0.05f;
        background.material.SetVector("_BGOffset", _backgroundOffset);
    }

    public void PauseGame()
    {
        uiReferences.PauseOverlay.SetActive(true);
        _isPaused = true;
        Time.timeScale = 0;
        AudioManager.PlaySFX(SFX.Pause);
        pauseLoop = AudioManager.PlaySFX(SFX.Pauseloop);
        AudioManager.PauseMusic();
    }

    public void UnpauseGame()
    {
        uiReferences.PauseOverlay.SetActive(false);
        _isPaused = false;
        pauseLoop.Stop();
        AudioManager.PlaySFX(SFX.Resume);
        Time.timeScale = 1;
        AudioManager.UnpauseMusic();
    }

    public bool GetIsPaused()
    {
        return _isPaused;
    }

    public void Restart()
    {
        Time.timeScale = 1;
        PersistentManager.instance.LoadScene(1);
    }

    public void GiveUp()
    {
        Time.timeScale = 1;
        PersistentManager.instance.LoadScene(0);
    }

    public void DoGameOver()
    {
        _isPaused = true;
        AudioManager.StopMusic();
        SpawnEffect(EffectType.PlayerDeath, playerInstance.transform.position);
        AudioManager.PlaySFX(SFX.BossKill);
        StartCoroutine(GameOverDelay());
    }

    private IEnumerator GameOverDelay()
    {
        yield return new WaitForSeconds(2);
        PersistentManager.instance.LoadScene(0);
    }

    public void AddScore(int value, Vector3 position)
    {
        value = Mathf.FloorToInt(value * _scoreMultiplier);
        _waveScore = _waveScore + value;
        _score = _score + System.Convert.ToUInt64(value);
        TextEffect effect = Instantiate(refs.scorePointsText, uiReferences.ScoreRoot);
        effect.transform.position = mainCam.worldToCameraMatrix * position;
        effect.SetScore(value);
        uiReferences.scoreText.SetText(_score.ToString("00000000000"));
    }

    public int SubtractLife()
    {
        _lives = _lives - 1;
        _unassignedSkillPoints = _unassignedSkillPoints + 1;
        uiReferences.livesText.SetText("@_" + _lives.ToString());
        return _lives;
    }

    public int GetWaveLoop()
    {
        return Mathf.CeilToInt((float)_wave / refs.waves.Count);
    }

    public string BuildFullWaveName(string waveName)
    {
        string suffix = refs.waveRepeatNameSuffix[(GetWaveLoop() - 1) % refs.waveRepeatNameSuffix.Count];
        string prefix = refs.waveRepeatNamePrefix[Mathf.FloorToInt((GetWaveLoop() - 1) / refs.waveRepeatNameSuffix.Count) % refs.waveRepeatNamePrefix.Count];

        int totalIteration = Mathf.FloorToInt(Mathf.FloorToInt((GetWaveLoop() - 1) / refs.waveRepeatNameSuffix.Count / refs.waveRepeatNamePrefix.Count));

        string totalIterationAddition = string.Empty;

        if (totalIteration >= 1)
        {
            totalIterationAddition = (totalIteration + 1) + " ";
        }

        return $"{prefix} {waveName} {totalIterationAddition}{suffix}".Trim(' ');
    }

    public void UpdateSkillPointsText()
    {
        string text = "";
        for (int i = 0; i < _unassignedSkillPoints; i++)
        {
            text += "@";
        }
        uiReferences.undistributedSkillPointsText.SetText(text);
    }

    public void NotifyEnemyKilled(Transform bonusSpawnLocation)
    {
        _enemiesKilled++;
        if (_enemiesKilled % 20 == 0)
        {
            Collectible bonus = Instantiate(refs.collectibles.GetRandom());
            bonus.SetSpeed(new Vector2(Random.Range(-4, 4), 4));
            bonus.transform.position = bonusSpawnLocation.position;
        }
    }

    public int GetUnassignedSkillPoints()
    {
        return _unassignedSkillPoints;
    }

    public void UpdateUnassignedSkillPoints(int change)
    {
        _unassignedSkillPoints += change;
        UpdateSkillPointsText();
    }

    public void OpenSkillUI()
    {
        Time.timeScale = 0;
        AudioManager.PauseMusic();
        _hitsTakenInWave++;
        UpdateSkillPointsText();
        uiReferences.deathCanvas.SetActive(true);
    }

    public void NextWave()
    {
        if (_nextWaveCor != null)
        {
            StopCoroutine(_nextWaveCor);
            _nextWaveCor = null;
        }

        _nextWaveCor = StartCoroutine(StartNextWave());
    }

    public IEnumerator StartNextWave()
    {
        int lastWave = _wave;
        _wave++;

        AudioManager.FadeOut();

        if (lastWave > 0)
        {
            WaveConfig oldWave = refs.waves[(lastWave - 1) % refs.waves.Count];
            enemyWaveCountAddition += oldWave.countIncreaseAfterBeat;
            enemyHPAddition += oldWave.hpIncreaseAfterBeat;
            _scoreMultiplier += oldWave.addToScoreMultiplierAfterBeat;
            yield return uiReferences.waveUIManager.ShowWaveClear(_waveScore, _hitsTakenInWave);

            yield return SceneManager.UnloadSceneAsync(oldWave.sceneName);
            yield return new WaitForSeconds(1);
        }

        _hitsTakenInWave = 0;
        _waveScore = 0;

        WaveConfig newWave = refs.waves[(_wave - 1) % refs.waves.Count];

        SceneManager.LoadSceneAsync(newWave.sceneName, LoadSceneMode.Additive); // Load while continuing function execution

        if (background != null)
        {
            Color oldColor = background.color;

            float i = 0;
            while (i < 1)
            {
                yield return null;
                i += deltaTime;
                background.color = Color.Lerp(oldColor, newWave.backgroundTint, i);
            }
        }

        yield return uiReferences.waveUIManager.ShowWaveStart("Wave " + _wave, BuildFullWaveName(newWave.flavourName));
    }

    public void OnDestroy()
    {
        if (_score > PersistentManager.instance._highScore)
        {
            PersistentManager.instance._highScore = _score;
            PlayerPrefs.SetInt("HighScore", System.Convert.ToInt32(_score));
        }
        if (_wave > PersistentManager.instance._bestWave)
        {
            PersistentManager.instance._bestWave = _wave;
            PlayerPrefs.SetInt("BestWave", _wave);
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            PauseGame();
        }
    }

    public void CloseSkillUIAndContinueGame()
    {
        FrontShot = (PowerupStage)uiReferences.frontHandler.GetValue();
        SideShot = (PowerupStage)uiReferences.sideHandler.GetValue();
        BackShot = (PowerupStage)uiReferences.backHandler.GetValue();
        Shields = (PowerupStage)uiReferences.lifeHandler.GetValue();
        Zany = (PowerupStage)uiReferences.zanyHandler.GetValue();
        uiReferences.deathCanvas.SetActive(false);
        playerInstance.hp = playerInstance.hpMax;
        AudioManager.UnpauseMusic();
        Time.timeScale = 1;
    }

    public void SetBackgroundSpeed(Vector2 speed)
    {
        _backgroundSpeed = speed;
    }
}