using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Threading;
using System;

public class GameManager1 : MonoBehaviour
{

    [Header("Mobile Controll UI")]
    [SerializeField] GameObject forMobileControl;
    [Header("Continue UI")]
    [SerializeField] GameObject UI_Continue;
    [Header("Pause Controll UI")]
    [SerializeField] GameObject UI_Pause;
    [Header("DisPlay Score")]
    [SerializeField] TMP_Text UI_score;
    [Header("DisPlay Clear")]
    [SerializeField] GameObject UI_Clear;

    [Header("Boss Object")]
    [SerializeField] GameObject enemyBoss;
    [Header("Boss Spawn Point")]
    [SerializeField] Transform bossSpawnPoint;
    [Header("Boss Spawn Score")]
    [SerializeField] int bossSpawnScore = 10000;

    [Header("Boss HP Max")]
    [SerializeField] int bossHPMax = 100;
    [Header("Boss HP UI")]
    [SerializeField] GameObject UI_BossStatus;
    [Header("Boss HP(Text)")]
    [SerializeField] TMP_Text UI_BossHPText;
    [Header("Boss HP(Gauge)")]
    [SerializeField] Slider UI_BossHPGauge;

    [Header("GameClear Sound")]
    [SerializeField] AudioClip ClearSE;

    [Header("GameClear Volume")]
    [SerializeField] float ClearSEVolume = 0.5f;
    // Start is called before the first frame update
    EnemyGenerator1 enemyGenerator;
    SoundManager1 soundManager;
    PlayerController1 controller;
    int pause = 0, pauseRelease = 1;
    public static int totalscore = 0, scoreMin = 0, scoreMax = 99999999, stockscore = 0;
    bool isBossSpawn = false;
    int bossHPMin = 0;
    public int bossHPCurrent { get; set; }
    void Start()
    {
        enemyGenerator = GameObject.FindWithTag("EnemyGenerator").GetComponent<EnemyGenerator1>();
        soundManager = GameObject.FindWithTag("SoundManager").GetComponent<SoundManager1>();
        controller = GameObject.FindWithTag("Player").GetComponent<PlayerController1>();
        GamePauseRelease();
        ScoreInit();
        if (stockscore < 3000)
        {
            stockscore = 0;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            VisibleUI_Pause();
        }

        if (stockscore >= bossSpawnScore)
        {
            if (!isBossSpawn)
            {
                if (enemyBoss.activeSelf)
                {
                    isBossSpawn = true;
                    Instantiate(enemyBoss, bossSpawnPoint.position, Quaternion.identity);
                    enemyGenerator.CancelInvoke();
                    bossHPCurrent = bossHPMax;
                    UI_BossStatus.SetActive(true);
                    EnemyBossHPDisplay();
                    soundManager.ChangeBGMStart();
                }
                
                
            }
        }
        
    }

    void VisibleUI_Pause()
    {
        GamePause();
        UI_Pause.SetActive(true);
    }

    public void PauseActionRelease()
    {
        UI_Pause.SetActive(false);
        GamePauseRelease();
    }

    void GamePause()
    {
        Time.timeScale = pause;
    }

    void GamePauseRelease()
    {
        Time.timeScale = pauseRelease;
    }

    public void VisibleUI_Continue()
    {
        GamePause();
        UI_Continue.SetActive(true);
    }

    
    void ScoreInit()
    {
        totalscore = GameManager.totalScore;
        UI_score.SetText(totalscore.ToString());
    }

    public void ScoreAdd(int addScore)
    {
        totalscore += addScore;
        stockscore += addScore;
        if (totalscore >= scoreMax)
        {
            totalscore = scoreMax;
        }
        UI_score.SetText(totalscore.ToString());
    }

    public void EnemyBossHPDisplay()
    {
        UI_BossHPText.SetText(bossHPCurrent + "/" + bossHPMax);
        UI_BossHPGauge.maxValue = bossHPMax;
        if (bossHPCurrent <= bossHPMin)
        {
            bossHPCurrent = 0;
        }
        UI_BossHPGauge.value = bossHPCurrent;

        if (bossHPCurrent <= bossHPMin)
        {
            UI_Clear.SetActive(true);
            AudioSource.PlayClipAtPoint(ClearSE, Camera.main.transform.position, ClearSEVolume);
            totalscore += controller.remainingCountCurrent * 2000;


        }
    }
}
