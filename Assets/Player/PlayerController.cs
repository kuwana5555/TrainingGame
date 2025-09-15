using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rbody;              //Rigidbody2D型の変数
    float axisH = 0.0f;             //入力
    public float speed = 3.0f;      //移動速度
    public float jump = 9.0f;       //ジャンプ力
    public LayerMask groundLayer;   //着地できるレイヤー
    bool goJump = false;            //ジャンプ開始フラグ
    // アニメーション対応
    Animator animator; // アニメーター
    public string stopAnime = "PlayerStop";
    public string moveAnime = "PlayerMove";
    public string jumpAnime = "PlayerJump";
    public string goalAnime = "PlayerGoal";
    public string deadAnime = "PlayerOver";
    string nowAnime = "";
    string oldAnime = "";
    public static string gameState = "playing"; // ゲームの状態

    public int score = 0;       // スコア
    public static Vector3 CheckPoint = new Vector3();
    
    [Header("リスポーン設定")]
    public float respawnDelay = 2.0f;    // リスポーンまでの遅延時間（秒）
    public bool useCheckPoint = true;    // チェックポイントを使用するか
    public Vector3 respawnPosition = Vector3.zero; // リスポーン位置（チェックポイント未使用時）
    
    [Header("カメラ・フェード設定")]
    public bool useCameraControl = true;     // カメラ制御を使用するか
    public bool useFadeEffect = true;        // フェード効果を使用するか
    public float fadeOutTime = 1.0f;         // フェードアウト時間（秒）
    public float fadeInTime = 1.0f;          // フェードイン時間（秒）
    public float cameraFollowDelay = 0.5f;   // カメラ追従再開までの遅延（秒）
    
    [Header("ダッシュ設定")]
    public bool dashEnabled = false;         // ダッシュ機能が有効かどうか
    public float dashSpeed = 6.0f;          // ダッシュ時の最大速度
    public float dashAcceleration = 2.0f;   // ダッシュの加速速度
    public float dashDeceleration = 3.0f;   // ダッシュの減速速度
    public KeyCode dashKey = KeyCode.LeftShift; // ダッシュキー
    public bool requireGroundForDash = true; // ダッシュに地面が必要か
    
    [Header("キック設定")]
    public bool enableKick = true;          // キック機能が有効かどうか
    public float kickForceMultiplier = 1.0f; // キック力の倍率
    public Vector2 kickDirection = new Vector2(0.5f, 1.0f); // キック方向（正規化前）
    public float minKickForce = 5.0f;       // 最小キック力
    public float maxKickForce = 15.0f;      // 最大キック力
    public string kickableTag = "Kickable"; // キック可能なオブジェクトのタグ
    
    // リスポーン制御用の変数
    private bool isRespawning = false;   // リスポーン中かどうか
    private Coroutine respawnCoroutine;  // リスポーンコルーチンの参照
    
    // カメラ・フェード制御用の変数
    private CameraManager cameraManager; // カメラマネージャーの参照
    private ScreenFader screenFader;     // フェード用のScreenFader
    private bool originalCameraFollow;   // 元のカメラ追従状態

    // タッチスクリーン対応追加
    bool isMoving = false;
    [Header("Player Jump Sound")]
    [SerializeField] AudioClip JumpSE;
    [Header("Player Item Get Sound")]
    [SerializeField] AudioClip ItemGetSE;
    [Header("Player Flag Get Sound")]
    [SerializeField] AudioClip MiddleSE;
    
    // ダッシュ制御用の変数
    private bool isDashing = false;         // ダッシュ中かどうか
    private float currentDashSpeed = 0f;    // 現在のダッシュ速度
    private bool dashKeyPressed = false;    // ダッシュキーが押されているか
    
    // キック制御用の変数
    private bool canKick = false;           // キック可能かどうか
    private float lastKickTime = 0f;        // 最後にキックした時間
    public float kickCooldown = 0.1f;       // キックのクールダウン時間

    // Start is called before the first frame update
    void Start()
    {
        rbody = this.GetComponent<Rigidbody2D>();   //Rigidbody2Dを取ってくる
        animator = GetComponent<Animator>();        //Animator を取ってくる
        nowAnime = stopAnime;                       //停止から開始する
        oldAnime = stopAnime;                       //停止から開始する
        gameState = "playing";                      // ゲーム中にする
        
        // カメラ・フェード関連の初期化
        InitializeCameraAndFade();
        
        if (CheckPoint != Vector3.zero)
        {
            transform.position = CheckPoint;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gameState != "playing")
        {
            return;
        }
        // 移動
        if (isMoving == false)
        {
            //水平方向の入力をチェックする
            axisH = Input.GetAxisRaw("Horizontal");
        }
        //向きの調整
        if (axisH > 0.0f)
        {
            //右移動
            Debug.Log("右移動");
            transform.localScale = new Vector2(1, 1);
        }
        else if (axisH < 0.0f)
        {
            Debug.Log("左移動");
            transform.localScale = new Vector2(-1, 1);
        }

        //キャラクターをジャンプさせる
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }
        
        // ダッシュキーの入力処理
        if (dashEnabled)
        {
            dashKeyPressed = Input.GetKey(dashKey);
        }
        
        // キック可能状態の更新
        UpdateKickAvailability();
    }

    void FixedUpdate()
    {
        if (gameState != "playing")
        {
            return;
        }
        //地上判定
        bool onGround = Physics2D.CircleCast(transform.position,    //発射位置
                                             0.2f,                  //円の半径
                                             Vector2.down,          //発射方向
                                             1.0f,                  //発射距離
                                             groundLayer);          //検出するレイヤー
        if (onGround || axisH != 0)
        {
            // ダッシュ処理
            if (dashEnabled)
            {
                UpdateDashSpeed(onGround);
                float currentSpeed = speed + currentDashSpeed;
                rbody.velocity = new Vector2(axisH * currentSpeed, rbody.velocity.y);
            }
            else
            {
                //速度を更新する
                rbody.velocity = new Vector2(axisH * speed, rbody.velocity.y);
            }
        }
        if (onGround && goJump)
        {
            //地面の上でジャンプキーが押された
            //ジャンプさせる
            Vector2 jumpPw = new Vector2(0, jump);          //ジャンプさせるベクトルを作る
            rbody.AddForce(jumpPw, ForceMode2D.Impulse);    //瞬間的な力を加える
            goJump = false;
        }
        //アニメーション更新
        if (onGround)
        {
            // 地面の上
            if (axisH == 0)
            {
                nowAnime = stopAnime; 		// 停止中
            }
            else
            {
                // ダッシュ中かどうかでアニメーションを分岐
                if (dashEnabled && isDashing)
                {
                    nowAnime = moveAnime;   // ダッシュ時も通常の移動アニメーション
                }
                else
                {
                    nowAnime = moveAnime;   // 移動
                }
            }
        }
        else
        {
            // 空中
            nowAnime = jumpAnime;
        }
        if(nowAnime != oldAnime)
        {
            oldAnime = nowAnime;
            animator.Play(nowAnime);        // アニメーション再生
        }

    }
    //ジャンプ
    public void Jump()
    {
        goJump = true;
        AudioSource.PlayClipAtPoint(JumpSE, Camera.main.transform.position, 0.5f);//ジャンプフラグを立てる
    }
    // 接触開始
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Goal")
        {
            Goal();        // ゴール！！
        }
        else if (collision.gameObject.tag == "Dead")
        {
            GameOver();     // ゲームオーバー
        }
        else if (collision.gameObject.tag == "ScoreItem")
        {
            // スコアアイテム
            // ItemDataを得る
            ItemData item = collision.gameObject.GetComponent<ItemData>();
            // スコアを得る
            score = item.value;
            // アイテム削除する
            Destroy(collision.gameObject);
            AudioSource.PlayClipAtPoint(ItemGetSE, Camera.main.transform.position, 0.5f);
        }
        else if (collision.gameObject.tag == "Flag")
        {
            CheckPoint = transform.position;
            Destroy(collision.gameObject);
            Debug.Log("CheckPoint : " + transform.position);
            AudioSource.PlayClipAtPoint(MiddleSE, Camera.main.transform.position, 0.5f);
        }
        else if (collision.gameObject.tag == kickableTag)
        {
            // キック可能なオブジェクトに触れた場合
            TryKickObject(collision.gameObject);
        }
    }
    // ゴール
    public void Goal()
    {
        animator.Play(goalAnime);
        gameState = "gameclear";
        GameStop();             // ゲーム停止
    }
    // ゲームオーバー
    public void GameOver()
    {
        // 既にリスポーン中の場合は処理をスキップ
        if (isRespawning)
        {
            Debug.Log("既にリスポーン処理中のため、新しいリスポーン処理をスキップしました");
            return;
        }
        
        animator.Play(deadAnime);
        gameState = "gameover";
        GameStop();
        
        // ゲーム停止（ゲームオーバー演出）
        // プレイヤー当たりを消す
        GetComponent<CapsuleCollider2D>().enabled = false;
        // プレイヤーを上に少し跳ね上げる演出
        rbody.AddForce(new Vector2(0, 5), ForceMode2D.Impulse);
        
        // リスポーン処理を開始
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
        }
        respawnCoroutine = StartCoroutine(RespawnCoroutine());
    }
    
    // リスポーン処理
    private IEnumerator RespawnCoroutine()
    {
        // リスポーン中フラグを設定
        isRespawning = true;
        
        Debug.Log("リスポーン処理を開始しました");
        
        // カメラ追従を停止
        if (useCameraControl)
        {
            DisableCameraFollow();
        }
        
        // フェードアウト
        if (useFadeEffect)
        {
            yield return StartCoroutine(FadeOutCoroutine());
        }
        
        // 遅延時間を待機
        yield return new WaitForSeconds(respawnDelay);
        
        // リスポーン位置を決定
        Vector3 respawnPos = useCheckPoint && CheckPoint != Vector3.zero ? 
            CheckPoint : respawnPosition;
        
        // プレイヤーをリスポーン位置に移動
        transform.position = respawnPos;
        
        // コライダーを再有効化
        GetComponent<CapsuleCollider2D>().enabled = true;
        
        // 速度をリセット
        rbody.velocity = Vector2.zero;
        
        // ゲーム状態をプレイ中に戻す
        gameState = "playing";
        
        // アニメーションを通常に戻す
        nowAnime = stopAnime;
        oldAnime = stopAnime;
        animator.Play(stopAnime);
        
        // カメラ追従を再開（遅延あり）
        if (useCameraControl)
        {
            yield return new WaitForSeconds(cameraFollowDelay);
            EnableCameraFollow();
        }
        
        // フェードイン
        if (useFadeEffect)
        {
            yield return StartCoroutine(FadeInCoroutine());
        }
        
        // リスポーン中フラグを解除
        isRespawning = false;
        respawnCoroutine = null;
        
        Debug.Log($"プレイヤーをリスポーンしました: {respawnPos}");
    }
    // ゲーム停止
    void GameStop()
    {
        // Rigidbody2Dを取ってくる
        Rigidbody2D rbody = GetComponent<Rigidbody2D>();
        // 速度を 0 にして強制停止
        rbody.velocity = new Vector2(0, 0);
    }
    // タッチスクリーン対応追加
    public void SetAxis(float h, float v)
    {
        axisH = h;
        if (axisH == 0)
        {
            isMoving = false;
        }
        else
        {
            isMoving = true;
        }
    }
    
    // 手動でリスポーン（デバッグ用）
    [ContextMenu("Manual Respawn")]
    public void ManualRespawn()
    {
        if (gameState == "gameover" && !isRespawning)
        {
            if (respawnCoroutine != null)
            {
                StopCoroutine(respawnCoroutine);
            }
            respawnCoroutine = StartCoroutine(RespawnCoroutine());
        }
        else if (isRespawning)
        {
            Debug.Log("既にリスポーン処理中のため、手動リスポーンをスキップしました");
        }
    }
    
    // リスポーン処理を強制停止（デバッグ用）
    [ContextMenu("Force Stop Respawn")]
    public void ForceStopRespawn()
    {
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }
        isRespawning = false;
        Debug.Log("リスポーン処理を強制停止しました");
    }
    
    // リスポーン位置を設定
    public void SetRespawnPosition(Vector3 position)
    {
        respawnPosition = position;
        Debug.Log($"リスポーン位置を設定しました: {position}");
    }
    
    // チェックポイントを設定
    public void SetCheckPoint(Vector3 position)
    {
        CheckPoint = position;
        Debug.Log($"チェックポイントを設定しました: {position}");
    }
    
    // カメラ・フェード関連の初期化
    private void InitializeCameraAndFade()
    {
        // CameraManagerを検索
        cameraManager = FindObjectOfType<CameraManager>();
        if (cameraManager == null)
        {
            Debug.LogWarning("CameraManagerが見つかりません。カメラ制御機能は無効になります。");
        }
        
        // ScreenFaderを検索
        screenFader = FindObjectOfType<ScreenFader>();
        if (screenFader == null)
        {
            Debug.LogWarning("ScreenFaderが見つかりません。フェード効果は無効になります。");
        }
    }
    
    // カメラ追従を停止
    private void DisableCameraFollow()
    {
        if (cameraManager != null)
        {
            // CameraManagerのenabledを一時的に無効化
            originalCameraFollow = cameraManager.enabled;
            cameraManager.enabled = false;
            Debug.Log("カメラ追従を停止しました");
        }
    }
    
    // カメラ追従を再開
    private void EnableCameraFollow()
    {
        if (cameraManager != null)
        {
            cameraManager.enabled = originalCameraFollow;
            Debug.Log("カメラ追従を再開しました");
        }
    }
    
    // フェードアウト処理
    private IEnumerator FadeOutCoroutine()
    {
        if (screenFader != null)
        {
            Debug.Log("フェードアウト開始");
            Coroutine fadeCoroutine = screenFader.FadeOut(fadeOutTime);
            yield return fadeCoroutine;
            Debug.Log("フェードアウト完了");
        }
        else
        {
            Debug.LogWarning("ScreenFaderが設定されていません。フェードアウトをスキップします。");
        }
    }
    
    // フェードイン処理
    private IEnumerator FadeInCoroutine()
    {
        if (screenFader != null)
        {
            Debug.Log("フェードイン開始");
            Coroutine fadeCoroutine = screenFader.FadeIn(fadeInTime);
            yield return fadeCoroutine;
            Debug.Log("フェードイン完了");
        }
        else
        {
            Debug.LogWarning("ScreenFaderが設定されていません。フェードインをスキップします。");
        }
    }
    
    /// <summary>
    /// ダッシュ速度を更新
    /// </summary>
    void UpdateDashSpeed(bool onGround)
    {
        // ダッシュキーが押されているかチェック
        bool canDash = dashKeyPressed && (!requireGroundForDash || onGround);
        
        if (canDash && axisH != 0)
        {
            // ダッシュ中
            isDashing = true;
            currentDashSpeed = Mathf.MoveTowards(currentDashSpeed, dashSpeed, dashAcceleration * Time.fixedDeltaTime);
        }
        else
        {
            // ダッシュ終了
            isDashing = false;
            currentDashSpeed = Mathf.MoveTowards(currentDashSpeed, 0f, dashDeceleration * Time.fixedDeltaTime);
        }
    }
    
    /// <summary>
    /// ダッシュ機能を有効/無効にする
    /// </summary>
    public void SetDashEnabled(bool enabled)
    {
        dashEnabled = enabled;
        
        // 無効にした場合はダッシュ状態をリセット
        if (!enabled)
        {
            isDashing = false;
            currentDashSpeed = 0f;
            dashKeyPressed = false;
        }
        
        Debug.Log($"ダッシュ機能を{(enabled ? "有効" : "無効")}にしました");
    }
    
    /// <summary>
    /// ダッシュ機能が有効かどうかを取得
    /// </summary>
    public bool IsDashEnabled()
    {
        return dashEnabled;
    }
    
    /// <summary>
    /// 現在ダッシュ中かどうかを取得
    /// </summary>
    public bool IsDashing()
    {
        return isDashing;
    }
    
    /// <summary>
    /// 現在のダッシュ速度を取得
    /// </summary>
    public float GetCurrentDashSpeed()
    {
        return currentDashSpeed;
    }
    
    /// <summary>
    /// ダッシュ設定を更新
    /// </summary>
    public void UpdateDashSettings(float newDashSpeed, float newAcceleration, float newDeceleration)
    {
        dashSpeed = newDashSpeed;
        dashAcceleration = newAcceleration;
        dashDeceleration = newDeceleration;
        
        Debug.Log($"ダッシュ設定を更新しました - 速度: {dashSpeed}, 加速: {dashAcceleration}, 減速: {dashDeceleration}");
    }
    
    /// <summary>
    /// キック可能状態を更新
    /// </summary>
    private void UpdateKickAvailability()
    {
        if (!enableKick)
        {
            canKick = false;
            return;
        }
        
        // ダッシュ中で、クールダウンが終了している場合のみキック可能
        canKick = isDashing && (Time.time - lastKickTime >= kickCooldown);
    }
    
    /// <summary>
    /// オブジェクトをキックする
    /// </summary>
    private void TryKickObject(GameObject targetObject)
    {
        if (!canKick)
        {
            return;
        }
        
        // Rigidbody2Dを取得
        Rigidbody2D targetRigidbody = targetObject.GetComponent<Rigidbody2D>();
        if (targetRigidbody == null)
        {
            Debug.LogWarning($"キック対象オブジェクトにRigidbody2Dがありません: {targetObject.name}");
            return;
        }
        
        // キック実行
        KickObject(targetRigidbody);
        
        // クールダウン開始
        lastKickTime = Time.time;
        canKick = false;
    }
    
    /// <summary>
    /// オブジェクトにキック力を加える
    /// </summary>
    private void KickObject(Rigidbody2D targetRigidbody)
    {
        // 現在のダッシュ速度に基づいてキック力を計算
        float speedRatio = Mathf.Clamp01(currentDashSpeed / dashSpeed);
        float kickForce = Mathf.Lerp(minKickForce, maxKickForce, speedRatio) * kickForceMultiplier;
        
        // キック方向を正規化
        Vector2 normalizedKickDirection = kickDirection.normalized;
        
        // プレイヤーの向きに応じてキック方向を調整
        if (transform.localScale.x < 0) // 左向きの場合
        {
            normalizedKickDirection.x = -normalizedKickDirection.x;
        }
        
        // キック力を適用
        Vector2 kickForceVector = normalizedKickDirection * kickForce;
        targetRigidbody.AddForce(kickForceVector, ForceMode2D.Impulse);
        
        Debug.Log($"キック実行: 力={kickForce:F2}, 方向={normalizedKickDirection}, 速度比={speedRatio:F2}");
    }
    
    /// <summary>
    /// キック機能を有効/無効にする
    /// </summary>
    public void SetKickEnabled(bool enabled)
    {
        enableKick = enabled;
        if (!enabled)
        {
            canKick = false;
        }
        Debug.Log($"キック機能を{(enabled ? "有効" : "無効")}にしました");
    }
    
    /// <summary>
    /// キック機能が有効かどうかを取得
    /// </summary>
    public bool IsKickEnabled()
    {
        return enableKick;
    }
    
    /// <summary>
    /// 現在キック可能かどうかを取得
    /// </summary>
    public bool CanKick()
    {
        return canKick;
    }
    
    /// <summary>
    /// キック設定を更新
    /// </summary>
    public void UpdateKickSettings(float newMinForce, float newMaxForce, Vector2 newDirection, float newMultiplier)
    {
        minKickForce = newMinForce;
        maxKickForce = newMaxForce;
        kickDirection = newDirection;
        kickForceMultiplier = newMultiplier;
        
        Debug.Log($"キック設定を更新しました - 最小力: {minKickForce}, 最大力: {maxKickForce}, 方向: {kickDirection}, 倍率: {kickForceMultiplier}");
    }
    
    // デバッグ用：Inspectorでボタンからキック機能を有効化
    [ContextMenu("Enable Kick")]
    public void ManualEnableKick()
    {
        SetKickEnabled(true);
    }
    
    // デバッグ用：Inspectorでボタンからキック機能を無効化
    [ContextMenu("Disable Kick")]
    public void ManualDisableKick()
    {
        SetKickEnabled(false);
    }
    
    // デバッグ用：Inspectorでボタンからキック状態を表示
    [ContextMenu("Show Kick Status")]
    public void ManualShowKickStatus()
    {
        Debug.Log($"キック機能: {(enableKick ? "有効" : "無効")}, キック可能: {canKick}, ダッシュ中: {isDashing}, クールダウン残り: {Mathf.Max(0, kickCooldown - (Time.time - lastKickTime)):F2}秒");
    }
}
