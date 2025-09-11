using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public float leftLimit = 0.0f;      // 左スクロールリミット
    public float rightLimit = 0.0f;     // 右スクロールリミット 
    public float topLimit = 0.0f;       // 上スクロールリミット 
    public float bottomLimit = 0.0f;    // 下スクロールリミット

    public GameObject subScreen;        // サブスクリーン

    public bool isForceScrollX = false;     // 強制スクロールフラグ
    public float forceScrollSpeedX = 0.5f;  // 1秒間で動かすX距離
    public bool isForceScrollY = false;     // Y軸強制スクロールフラグ 
    public float forceScrollSpeedY = 0.5f;  // 1秒間で動かすY距離

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player"); // プレイヤーを探す
        if (player != null)
        {
            // カメラの更新座標
            float x = player.transform.position.x;
            float y = player.transform.position.y;
            float z = transform.position.z;
            // 横同期させる
            if (isForceScrollX)
            {
                // 横強制スクロール
                x = transform.position.x + (forceScrollSpeedX * Time.deltaTime);
            }
            // 両端に移動制限を付ける
            if (x < leftLimit)
            {
                x = leftLimit;
            }
            else if (x > rightLimit)
            {
                x = rightLimit;
            }
            // 縦同期させる
            if (isForceScrollY)
            {
                // 縦強制スクロール
                y = transform.position.y + (forceScrollSpeedY * Time.deltaTime);
            }
            // 上下に移動制限を付ける
            if (y < bottomLimit)
            {
                y = bottomLimit;
            }
            else if (y > topLimit)
            {
                y = topLimit;
            }
            // カメラ位置のVector3を作る
            Vector3 v3 = new Vector3(x, y, z);
            transform.position = v3;

            // サブスクリーンスクロール
            if (subScreen != null)
            {
                y = subScreen.transform.position.y;
                z = subScreen.transform.position.z;
                Vector3 v = new Vector3(x / 2.0f, y, z);
                subScreen.transform.position = v;
            }
        }
    }
}
