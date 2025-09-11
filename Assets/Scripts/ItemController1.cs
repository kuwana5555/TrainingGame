using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController1 : MonoBehaviour
{

    [Header("Variety of Item")]
    [SerializeField] int itemType = 0;

    [Header("Item of Parameter")]
    [SerializeField] int addScore = 1000;

    [Header("Item Move Speed")]
    [SerializeField] float itemMoveSpeed = 1.5f;

    [Header("Item Spin Speed")]
    [SerializeField] float itemRotateSpeed = 10f;

    [Header("Variety Get Sound")]
    [SerializeField] AudioClip getItemSE;

    [Header("Item Get Volume")]
    [SerializeField] float getItemSEVolume = 0.5f;

    [Header("Item Effect:Heal")]
    [SerializeField] int healItemVolume;

    [Header("Item Effect:Speed")]
    [SerializeField] float speedUpItemVolume = 5f;

    enum ItemType { addScore, addRemainingCount, HPHeal, shotPowerUp, speedUp, shield }
    Rigidbody2D capsuleRB;
    int shotPowerMax = 3;
    GameManager1 gameManager;
    PlayerController1 player;
    // Start is called before the first frame update
    void Start()
    {
        capsuleRB = GetComponent<Rigidbody2D>();
        gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager1>();
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController1>();
        
    }

    // Update is called once per frame
    void Update()
    {
        capsuleRB.velocity = new Vector2(-itemMoveSpeed, 0);
        capsuleRB.transform.Rotate(0, 0, itemRotateSpeed);
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ItemEffects(itemType);
            Destroy(gameObject);
            AudioSource.PlayClipAtPoint(getItemSE, Camera.main.transform.position, getItemSEVolume);
        }
    }

    void ItemEffects(int iType)
    {
        switch (iType)
        {
            case (int)ItemType.addScore:
                gameManager.ScoreAdd(addScore);
                break;

            case (int)ItemType.addRemainingCount:
                player.RemainingCounter((int)PlayerController1.RemainingCountType.add);
                break;

            case (int)ItemType.HPHeal:
                player.PlayerHPChanged((int)PlayerController1.HPCalcType.heal, healItemVolume);
                break;

            case (int)ItemType.shotPowerUp:
                PlayerController1.playerShotPower++;
                if (PlayerController1.playerShotPower > shotPowerMax)
                {
                    PlayerController1.playerShotPower = shotPowerMax;
                }
                break;

            case (int)ItemType.speedUp:
                player.PlayerSpeedUp(speedUpItemVolume);
                break;

            case (int)ItemType.shield:
                player.ShieldActive();
                break;

            default:
                break;
        }
    }
}
