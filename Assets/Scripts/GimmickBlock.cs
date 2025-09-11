using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GimmickBlock : MonoBehaviour
{
    public float length = 0.0f;     // 自動落下検知距離
    public bool isDelete = false;   // 落下後に削除するフラグ
    public GameObject deadObj;      // 死亡当たり

    bool isFell = false;            // 落下フラグ
    float fadeTime = 0.5f;          // フェードアウト時間

    // Start is called before the first frame update
    void Start()
    {
        // Rigidbody2Dの物理挙動を停止
        Rigidbody2D rbody = GetComponent<Rigidbody2D>();
        rbody.bodyType = RigidbodyType2D.Static;
        deadObj.SetActive(false);    //死亡あたりを非表示
    }

    // Update is called once per frame
    void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player"); // プレイヤーを探す
        if (player != null)
        {
            // プレイヤーとの距離計測
            float d = Vector2.Distance(transform.position, player.transform.position);
            if (length >= d)
            {
                Rigidbody2D rbody = GetComponent<Rigidbody2D>();
                if (rbody.bodyType == RigidbodyType2D.Static)
                {
                    // Rigidbody2Dの物理挙動を開始
                    rbody.bodyType = RigidbodyType2D.Dynamic;
                    deadObj.SetActive(true);    //死亡あたりを表示
                }
            }
        }
        if (isFell)
        {
            // 落下した
            // 透明値を変更してフェードアウトさせる
            fadeTime -= Time.deltaTime; // 前フレームの差分秒マイナス
            Color col = GetComponent<SpriteRenderer>().color;   // カラーを取り出す
            col.a = fadeTime;   // 透明値を変更
            GetComponent<SpriteRenderer>().color = col; // カラーを再設定する
            if (fadeTime <= 0.0f)
            {
                // 0以下(透明)になったら消す
                Destroy(gameObject);
            }
        }
    }

    // 接触開始
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDelete)
        {
            isFell = true; // 落下フラグオン
        }
    }
    //範囲表示
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, length);
    }

    
}