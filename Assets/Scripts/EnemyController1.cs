using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController1 : MonoBehaviour
{
    [Header("EnemyMove")]
    [SerializeField] float enemyMoveSpeed = 3f;
    [Header("Boss Stop Point")]
    [SerializeField] Transform enemyBossStopPos;
    [Header("EnemyBullet")]
    [SerializeField] GameObject enemyBullet;
    [Header("EnemyBulletSpeed")]
    [SerializeField] float enemyShotSpeed = 5f;
    [Header("EnemyBulletPosition")]
    [SerializeField] Transform enemyFirePos;
    [Header("ShotRate")]
    [SerializeField] float enemyShotThreshold = 1f;

    [Header("Rapid Shot Count")]
    [SerializeField] int rapidShotCount = 5;
    [Header("Rapid Shot Rate")]
    [SerializeField] float rapidShotInterval = 2f;

    [Header("Rotate Angle")]
    [SerializeField] float rotateAngle = 5f;
    [Header("Rolling Shot Rate")]
    [SerializeField] float rollingShotInterval = 4f;


    [Header("EnemyExplosionEffect")]
    [SerializeField] GameObject enemyExplosion;

    [Header("EnemyMove Version")]
    [SerializeField] int moveType = 0;

    [Header("EnemyBullet Pattern")]
    [SerializeField] int enemyShotType = 0;

    [Header("Amount of EnemyBullet")]
    [SerializeField] int enemyShotCount = 1;

    [Header("MultiWay Shot Angle")]
    [SerializeField] float multiWayShotAngle = 15f;

    [Header("EnemyBullet Shot Volume")]
    [SerializeField] float shotVolume = 0.2f;

    [Header("Enemy HP")]
    [SerializeField] int enemyHP = 1;
    [Header("Enemy Attack Damage")]
    [SerializeField] int enemyDamage = 1;
    [Header("Enemy Boss Damage")]
    [SerializeField] int enemyBossDamaged = 1;

    [Header("Enemy Damaged Sign")]
    [SerializeField] float blinkTime = 0.15f;
    [Header("Enemy Attack Damage")]
    [SerializeField] int blinkCount = 3;


    Rigidbody2D enemyRB;

    float enemyShotDelayReset = 0, enemyShotDelay = 0;

    int score_MOB = 100;
    int score_Boss = 10000;
    int enemyHPMin = 0;
    int shotCount = 0, shotCountReset = 0;
    bool isShot = true;
    GameManager1 gameManager;

    Rigidbody2D player;
    Transform target;
    SpriteRenderer enemySP;
    Animator animator;
    public string moveAnime = "BossMove";
    string nowAnime = "";
    public static int enemyAttackDamage { get; set; }
    public static int enemyShotPower = 1;
    enum EnemyMoveType { Normal, PlayerFollow, PlayerAttack, EnemyBoss, NoMove }

    enum EnemyShotPattern { Normal, playerAim, multiWay, PAMultiWay, PARapidShot, RollingShot }

    enum EnemyDamageType { heal, damage }
    enum EnemyAttackDamage { Normal = 1, PlayerFollow = 2, PlayerAttack = 3, EnemyBoss = 10 }

    // Start is called before the first frame update
    void Start()
    {
        enemyRB = GetComponent<Rigidbody2D>();

        enemySP = GetComponent<SpriteRenderer>();

        player = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();

        gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager1>();

        target = GameObject.FindWithTag("Player").GetComponent<Transform>();

        enemyBossStopPos = GameObject.FindWithTag("EnemyBossStopPos").GetComponent<Transform>();

        animator = GetComponent<Animator>();


    }

    // Update is called once per frame
    void Update()
    {
        enemyShotDelay += Time.deltaTime;

        if (enemyShotType == (int)EnemyShotPattern.PARapidShot)
        {
            if (isShot)
            {
                EnemyBulletShot();
                if (shotCount >= rapidShotCount)
                {
                    isShot = false;
                    StartCoroutine(ShotInterval(rapidShotInterval));
                }
            }
        }
        else if (enemyShotType == (int)EnemyShotPattern.RollingShot)
        {
            if (isShot)
            {
                EnemyBulletShot();
                if (shotCount >= rapidShotCount)
                {
                    isShot = false;
                    StartCoroutine(ShotInterval(rollingShotInterval));
                }
                else
                {
                    enemyFirePos.transform.Rotate(Vector3.zero);
                }
            }
            
        }
        else
        {
            EnemyBulletShot();
        }


    }

    void FixedUpdate()
    {
        EnemyMove(moveType);
        if (gameObject.CompareTag("EnemyBoss"))
        {
            nowAnime = moveAnime;
            animator.Play(nowAnime);
        }
        
    }

    void EnemyMove(int type_Move)
    {
        switch (type_Move)
        {
            case (int)EnemyMoveType.Normal:
                enemyRB.velocity = new Vector2(-enemyMoveSpeed, 0);
                break;

            case (int)EnemyMoveType.PlayerFollow:      
                enemyRB.velocity = new Vector2(-enemyMoveSpeed, player.transform.position.y - transform.position.y);           
                break;

            case (int)EnemyMoveType.PlayerAttack:
                enemyRB.velocity = new Vector2(-enemyMoveSpeed + player.transform.position.x, player.transform.position.y - transform.position.y);
                break;

            case (int)EnemyMoveType.EnemyBoss:
                enemyRB.velocity = new Vector2(-enemyMoveSpeed , 0);
                if (enemyBossStopPos.position.x >= gameObject.transform.position.x)
                {
                    enemyRB.velocity = Vector2.zero;
                }
                break;

            case (int)EnemyMoveType.NoMove:
                break;

            default:
                break;
        }

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        switch (enemyDamage)
        {
            case (int)EnemyAttackDamage.Normal:
                enemyAttackDamage = (int)EnemyAttackDamage.Normal;
                break;

            case (int)EnemyAttackDamage.PlayerFollow:
                enemyAttackDamage = (int)EnemyAttackDamage.PlayerFollow;
                break;

            case (int)EnemyAttackDamage.PlayerAttack:
                enemyAttackDamage = (int)EnemyAttackDamage.PlayerAttack;
                break;

            case (int)EnemyAttackDamage.EnemyBoss:
                enemyAttackDamage = (int)EnemyAttackDamage.EnemyBoss;
                break;

            default:
                break;


        }

        if (collision.gameObject.CompareTag("Player"))
        {
            if (gameObject.CompareTag("EnemyBoss"))
            {
                EnemyHPChanged((int)EnemyDamageType.damage, enemyBossDamaged, collision);
            }
            else
            {
                EnemyHPChanged((int)EnemyDamageType.damage, enemyHP, collision);
            }
        }


        else if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            EnemyHPChanged((int)EnemyDamageType.damage, PlayerController1.playerShotPower, collision);
        }
        else if (collision.gameObject.CompareTag("Shield"))
        {
            if (gameObject.CompareTag("EnemyBoss"))
            {
                EnemyHPChanged((int)EnemyDamageType.damage, enemyBossDamaged, collision);
            }
            else
            {
                EnemyHPChanged((int)EnemyDamageType.damage, enemyHP, collision);
            }
        }

    }

    void EnemyExplosion()
    {
        Instantiate(enemyExplosion, gameObject.transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    void EnemyHPChanged(int damageType, int volume, Collider2D collider)
    {
        switch (damageType)
        {
            case (int)EnemyDamageType.heal:
                break;

            case (int)EnemyDamageType.damage:
                enemyHP -= volume;
                if (collider.gameObject.CompareTag("PlayerBullet"))
                {
                    Destroy(collider.gameObject);
                }

                if (gameObject.CompareTag("EnemyBoss"))
                {
                    gameManager.bossHPCurrent = enemyHP;
                    gameManager.EnemyBossHPDisplay();
                }

                if (enemyHP <= enemyHPMin)
                {
                    if (gameObject.CompareTag("EnemyBoss"))
                    {
                        gameManager.bossHPCurrent = 0;
                        gameManager.ScoreAdd(score_Boss);
                        EnemyExplosion();
                    }
                    else
                    {
                        gameManager.ScoreAdd(score_MOB);
                        EnemyExplosion();
                    }
                }
                
                StartCoroutine(EnemyDamagedBlink());
                break;

            default:
                break;


        }
    }

    void EnemyShot(Transform enemyFirePos, int sType)
    {
        
        switch (sType)
        {
            case (int)EnemyShotPattern.Normal:
                GameObject enemyBulletClone = Instantiate(enemyBullet, enemyFirePos.position, Quaternion.identity);
                enemyBulletClone.GetComponent<Rigidbody2D>().velocity = new Vector2(-enemyShotSpeed, 0);
                break;

            case (int)EnemyShotPattern.playerAim:
                GameObject enemyBulletClone_PlayerAim = Instantiate(enemyBullet, enemyFirePos.position, Quaternion.identity);
                enemyBulletClone_PlayerAim.GetComponent<Rigidbody2D>().velocity = new Vector2(-enemyShotSpeed, target.position.y- transform.position.y);
                break;

            case (int)EnemyShotPattern.multiWay:
                for (int i = 0; i < enemyShotCount; i++)
                {
                    GameObject enemyBulletClone_MultiWay = Instantiate(enemyBullet, enemyFirePos.position, Quaternion.identity);
                    enemyBulletClone_MultiWay.GetComponent<AudioSource>().volume = shotVolume;
                    enemyBulletClone_MultiWay.GetComponent<Rigidbody2D>().velocity = new Vector2(-enemyShotSpeed, (multiWayShotAngle / enemyShotCount) - (multiWayShotAngle / enemyShotCount) * i);
                }
                break;

            case (int)EnemyShotPattern.PAMultiWay:
                for (int i = 0; i < enemyShotCount; i++)
                {
                    GameObject enemyBulletClone_PAMultiWay = Instantiate(enemyBullet, enemyFirePos.position, Quaternion.identity);
                    enemyBulletClone_PAMultiWay.GetComponent<AudioSource>().volume = shotVolume;
                    enemyBulletClone_PAMultiWay.GetComponent<Rigidbody2D>().velocity = new Vector2(-enemyShotSpeed, (target.position.y - transform.position.y) * i);
                }
                    break;

            case (int)EnemyShotPattern.PARapidShot:
                PARapidShot();
                break;

            case (int)EnemyShotPattern.RollingShot:
                RollingShot();
                break;

            default:
                break;


        }
    }

    IEnumerator EnemyDamagedBlink()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            yield return new WaitForSeconds(blinkTime);
            enemySP.enabled = false;
            yield return new WaitForSeconds(blinkTime);
            enemySP.enabled = true;
        }
    }

    void EnemyBulletShot()
    {
        if (enemyShotDelay <= enemyShotThreshold)
        {
            return;
        }

        else
        {
            EnemyShot(enemyFirePos, enemyShotType);
        }

        enemyShotDelay = enemyShotDelayReset;
    }

    void PARapidShot()
    {
        GameObject enemyBulletClone_PARapidShot = Instantiate(enemyBullet, enemyFirePos.position, Quaternion.identity);
        enemyBulletClone_PARapidShot.GetComponent<Rigidbody2D>().velocity = new Vector2(-enemyShotSpeed, target.position.y - transform.position.y);
        shotCount++;
    }

    void RollingShot()
    {
        GameObject enemyBulletClone_RollingShot = Instantiate(enemyBullet, enemyFirePos.position, gameObject.transform.rotation);
        enemyBulletClone_RollingShot.GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Sin(gameObject.transform.rotation.z * rotateAngle) * enemyShotSpeed, Mathf.Cos(gameObject.transform.rotation.z * rotateAngle) * enemyShotSpeed);
        gameObject.transform.Rotate(0, 0, rotateAngle);
        shotCount++;
    }

    IEnumerator ShotInterval(float interval)
    {
        yield return new WaitForSeconds(interval);
        isShot = true;
        shotCount = shotCountReset;
    }

}
