using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController1 : MonoBehaviour
{
    [Header("PlayerSpeed")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float marginX = 1f, marginY = 1f;
    [SerializeField] Transform playerRespawnPos;

    [Header("PlayerShot")]
    [SerializeField] float playerShotSpeed = 5f;
    [SerializeField] GameObject[] playerBullet;
    [SerializeField] Transform firePos;
    [SerializeField] float shotThreshold = 1f;

    [Header("PlayerExplosionEffect")]
    [SerializeField] GameObject playerExplosion;

    [Header("GameManager")]
    [SerializeField] GameManager1 gameManager;


    [SerializeField] VariableJoystick joyStick;

    [Header("PlayerRemaining")]
    [SerializeField] int remainingCount = 3;
    [Header("PlayerRemaining UI")]
    [SerializeField] TMP_Text UI_RemainingCount;
    [SerializeField] GameObject[] UI_RemainingCountIcon;
    [SerializeField] float invincibleTime = 2f;
    [SerializeField] int invincibleVal = 10;

    [Header("Player HP")]
    [SerializeField] int playerHPMax = 10;
    [Header("Player HP UI(Num)")]
    [SerializeField] TMP_Text playerHPText;
    [Header("Player HP UI(Gauge)")]
    [SerializeField] Slider playerHPGauge;
    [Header("Player Damaged Sound")]
    [SerializeField] AudioClip playerDamagedSE;
    [Header("Player Damaged Sound Volume")]
    [SerializeField] float playerDamagedSEVolume = 0.3f;

    [Header("Shield")]
    [SerializeField] GameObject shield;
    [Header("Shield HP")]
    [SerializeField] int shieldDurableValue = 5;
    [Header("Shield Damage Sound")]
    [SerializeField] AudioClip shieldDamagedSE;
    [Header("Shield Damage Sound Volume")]
    [SerializeField] float shieldDamagedSEVolume = 0.5f;
    [Header("Shield Break Sound")]
    [SerializeField] AudioClip shieldBrokenSE;
    [Header("Shield Break Sound Volume")]
    [SerializeField] float shieldBrokenSEVolume = 0.5f;

    

    Rigidbody2D playerRB;
    SpriteRenderer playerSR;
    Vector2 moveDirection, screenMin, screenMax;
    Vector2 playerPos;
    public static int playerHPCurrent { get; set; }
    public static int playerShotPower { get; set; } = 1;
    int playerHPMin = 0;
    float shotDelayReset = 0, shotDelay = 0;
    public int remainingCountCurrent, remainingCountMin = 0, remainingCountMax = 5;
    float playerColorValue = 1f;
    bool isInvincible = false;
    bool isButtonPressed = false;
    int shotPowerInitVal = 1;
    float speedInit = 12f;
    int shieldDurableValueCurrent;

    public enum RemainingCountType { add, minus};

    public enum HPCalcType { damage, heal, display, shieldDamaged };
    // Start is called before the first frame update
    void Start()
    {
        playerRB = GetComponent<Rigidbody2D>();
        playerSR = GetComponent<SpriteRenderer>();
        gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager1>();
        RemainingCountInit();
        playerHPCurrent = playerHPMax;
        PlayerHPDisplay();
        


    }

    // Update is called once per frame
    void Update()
    {
        InputProcess();

        if (joyStick.Direction != Vector2.zero)
        {
            moveDirection = joyStick.Direction.normalized;
        }

        shotDelay += Time.deltaTime;
        if (shotDelay <= shotThreshold)
        {
            return;
        }
        if (Input.GetKey(KeyCode.Space) || isButtonPressed)
        {
            PlayerShot(firePos);
        }

        shotDelay = shotDelayReset;

        if (isInvincible)
        {
            float invincibleLevel = Mathf.Abs(Mathf.Sin(Time.time * invincibleVal));
            playerSR.color = new Color(playerColorValue, playerColorValue, playerColorValue, invincibleLevel);
        }
        else
        {
            playerSR.color = new Color(playerColorValue, playerColorValue, playerColorValue, playerColorValue);
        }

        
        
    }

    void FixedUpdate()
    {
        PlayerMove();
        
    }

    void InputProcess()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector2(x, y).normalized;
    }

    void PlayerMove()
    {
        MoveClamp();
        playerRB.velocity = moveDirection * moveSpeed;
    }

    void MoveClamp()
    {
        playerPos = transform.position;
        screenMin = Camera.main.ViewportToWorldPoint(Vector2.zero);
        screenMax = Camera.main.ViewportToWorldPoint(Vector2.one);
        playerPos.x = Mathf.Clamp(playerPos.x, screenMin.x + marginX, screenMax.x - marginX);
        playerPos.y = Mathf.Clamp(playerPos.y, screenMin.y + marginY, screenMax.y - marginY);
        transform.position = playerPos;
    }

    void PlayerShot(Transform firePos)
    {
        GameObject bulletClone = Instantiate(playerBullet[playerShotPower - shotPowerInitVal], firePos.position, Quaternion.identity);
        bulletClone.GetComponent<Rigidbody2D>().velocity = new Vector2(playerShotSpeed, 0);
    }

    public void OnPointerDown()
    {
        isButtonPressed = true;
    }

    public void OnPointerUp()
    {
        isButtonPressed = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isInvincible)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (shield.activeInHierarchy)
            {
                PlayerHPChanged((int)HPCalcType.shieldDamaged, EnemyController1.enemyAttackDamage, collision);
            } 
            else
            {
                PlayerHPChanged((int)HPCalcType.damage, EnemyController1.enemyAttackDamage, collision);
            }
            
        }
        else if (collision.gameObject.CompareTag("EnemyBullet"))
        {
            if (shield.activeInHierarchy)
            {
                PlayerHPChanged((int)HPCalcType.shieldDamaged, EnemyController1.enemyShotPower, collision);
            }
            else
            {
                PlayerHPChanged((int)HPCalcType.damage, EnemyController1.enemyShotPower, collision);
            }
        }
            
    }

    void PlayerExplosion(Collider2D collision)
    {
        Instantiate(playerExplosion, playerPos, Quaternion.identity);
        gameObject.SetActive(false);
    }

    void Continue()
    {
        gameManager.VisibleUI_Continue();
        
    }

    void RemainingCountInit()
    {
        remainingCountCurrent = remainingCount;
        RemainingCountDisplay();
    }

    public void RemainingCounter(int countType)
    {
        switch (countType)
        {
            case (int)RemainingCountType.minus:
                remainingCountCurrent--;
                if (remainingCountCurrent < remainingCountMin)
                {
                    remainingCountCurrent = remainingCountMin;
                    Continue();
                    return;
                }
                    RemainingCountDisplay();
                    PlayerRespawn(playerRespawnPos);
                    StartCoroutine(Invincible());
                    break;

            case (int)RemainingCountType.add:
                remainingCountCurrent++;
                if (remainingCountCurrent > remainingCountMax)
                {
                    remainingCountCurrent = remainingCountMax;
                }
                RemainingCountDisplay();
                break;

            default:
                break;
        }

    }

    void RemainingCountDisplay()
    {
        UI_RemainingCount.SetText(remainingCountCurrent.ToString() + " / " + remainingCountMax.ToString());
        for (int i = 0; i < remainingCountMax; i++)
        {
            if (remainingCountCurrent <= i)
            {
                UI_RemainingCountIcon[i].SetActive(false);
            }
            else
            {
                UI_RemainingCountIcon[i].SetActive(true);
            }
        }
    }

    void PlayerRespawn(Transform respawnPosition)
    {
        gameObject.transform.position = respawnPosition.position;
        playerHPCurrent = playerHPMax;
        PlayerHPDisplay();
        gameObject.SetActive(true);
    }

    void PlayerHPDisplay()
    {
        playerHPText.SetText(playerHPCurrent + " / " + playerHPMax);
        playerHPGauge.value = playerHPCurrent;
    }

    public void PlayerHPChanged(int calcType, int volume, Collider2D collider = null)
    {
        switch (calcType)
        {
            case (int)HPCalcType.heal:
                playerHPCurrent += volume;
                if (playerHPCurrent >= playerHPMax)
                {
                    playerHPCurrent = playerHPMax;
                }
                PlayerHPDisplay();
                break;

            case (int)HPCalcType.damage:
                AudioSource.PlayClipAtPoint(playerDamagedSE, Camera.main.transform.position, playerDamagedSEVolume);
                playerHPCurrent -= volume;
                if (collider.gameObject.CompareTag("EnemyBullet"))
                {
                    Destroy(collider.gameObject);
                }

                if (playerHPCurrent <= playerHPMin)
                {
                    playerHPCurrent = playerHPMin;
                    PlayerExplosion(collider);
                    RemainingCounter((int)RemainingCountType.minus);
                    ResetPlayerStatus();
                }
                PlayerHPDisplay();
                break;

            case (int)HPCalcType.shieldDamaged:
                AudioSource.PlayClipAtPoint(shieldDamagedSE, Camera.main.transform.position, shieldDamagedSEVolume);
                shieldDurableValueCurrent -= volume;
                if (collider.gameObject.CompareTag("EnemyBullet"))
                {
                    Destroy(collider.gameObject);
                }

                if (shieldDurableValueCurrent <= 0)
                {
                    ShieldInActive();
                }
                break;

            default:
                break;


        }
    }

    public void PlayerSpeedUp(float speedUpVolume)
    {
        moveSpeed += speedUpVolume;
    }

    void ResetPlayerStatus()
    {
        playerShotPower = shotPowerInitVal;
        moveSpeed = speedInit;
    }
    public void ShieldActive()
    {
        shieldDurableValueCurrent = shieldDurableValue;
        shield.SetActive(true);
    }

    void ShieldInActive()
    {
        AudioSource.PlayClipAtPoint(shieldBrokenSE, Camera.main.transform.position, shieldBrokenSEVolume);
        shield.SetActive(false);
    }

    IEnumerator Invincible()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("EnemyBoss"))
        {
            if (shield.activeInHierarchy)
            {
                PlayerHPChanged((int)HPCalcType.shieldDamaged, EnemyController1.enemyAttackDamage, collision);
            }
            else
            {
                PlayerHPChanged((int)HPCalcType.damage, EnemyController1.enemyAttackDamage, collision);
            }

        }
        
    }

}
