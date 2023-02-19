using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRefs : MonoBehaviour
{
    public TMPro.TMP_Text scoreText;
    public TMPro.TMP_Text livesText;

    public WaveUIManager waveUIManager;

    public GameObject deathCanvas;
    public TMPro.TMP_Text undistributedSkillPointsText;
    public SkillPointHandler frontHandler;
    public SkillPointHandler sideHandler;
    public SkillPointHandler backHandler;
    public SkillPointHandler lifeHandler;
    public SkillPointHandler zanyHandler;

    public Transform ScoreRoot;

    public GameObject PauseOverlay;
}