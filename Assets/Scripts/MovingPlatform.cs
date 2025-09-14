using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private bool moveHorizontal = true;      // 水平移動するか
    [SerializeField] private bool moveVertical = false;       // 垂直移動するか
    [SerializeField] private float moveSpeed = 2.0f;          // 移動速度
    [SerializeField] private float moveDistance = 5.0f;       // 移動距離
    
    [Header("往復設定")]
    [SerializeField] private bool usePingPong = true;         // 往復移動するか
    [SerializeField] private bool startMoving = true;         // 開始時に移動を開始するか
    [SerializeField] private float waitTime = 0.5f;           // 端での待機時間（秒）
    
    [Header("方向設定")]
    [SerializeField] private Vector2 moveDirection = Vector2.right; // 移動方向（正規化される）
    [SerializeField] private bool useCustomDirection = false; // カスタム方向を使用するか
    
    [Header("プレイヤー連携設定")]
    [SerializeField] private bool carryPlayer = true;         // プレイヤーを連れていくか
    [SerializeField] private string playerTag = "Player";     // プレイヤーのタグ
    [SerializeField] private bool carryOtherObjects = false;  // 他のオブジェクトも連れていくか
    [SerializeField] private string[] otherTags = { "Enemy", "Item" }; // 連れていく他のタグ
    [SerializeField] private bool useCollisionDetection = true; // コリジョン検知を使用するか
    [SerializeField] private float detectionOffset = 0.1f;    // 検知オフセット（プレイヤーの足元から）
    [SerializeField] private bool useFixedUpdate = true;     // FixedUpdateで処理するか
    
    [Header("物理設定")]
    [SerializeField] private bool useRigidbody = true;        // Rigidbodyを使用するか
    [SerializeField] private bool freezeRotation = true;      // 回転を固定するか
    [SerializeField] private bool isKinematic = true;         // Kinematicにするか
    
    [Header("デバッグ設定")]
    [SerializeField] private bool showDebugInfo = false;      // デバッグ情報を表示するか
    [SerializeField] private bool showGizmos = true;          // ギズモを表示するか
    [SerializeField] private Color gizmoColor = Color.green;  // ギズモの色
    
    // 内部変数
    private Vector3 startPosition;        // 開始位置
    private Vector3 currentDirection;     // 現在の移動方向
    private float currentDistance;        // 現在の移動距離
    private bool isMoving = false;        // 移動中かどうか
    private bool isWaiting = false;       // 待機中かどうか
    private Coroutine waitCoroutine;      // 待機コルーチンの参照
    
    // プレイヤー連携用
    private List<Transform> carriedObjects = new List<Transform>(); // 連れていくオブジェクトのリスト
    private Dictionary<Transform, Vector3> lastPositions = new Dictionary<Transform, Vector3>(); // 前フレームの位置
    private Dictionary<Transform, Vector3> lastPlatformPositions = new Dictionary<Transform, Vector3>(); // 前フレームのプラットフォーム位置
    
    // 物理コンポーネント
    private Rigidbody2D rb2D;
    private Rigidbody rb3D;
    
    // イベント
    public System.Action OnPlatformStart;     // プラットフォーム移動開始時
    public System.Action OnPlatformStop;      // プラットフォーム停止時
    public System.Action OnPlatformReverse;   // プラットフォーム方向転換時
    public System.Action OnPlayerEnter;       // プレイヤーが乗った時
    public System.Action OnPlayerExit;        // プレイヤーが降りた時

    void Start()
    {
        // 初期設定
        InitializePlatform();
        
        // 物理コンポーネントの設定
        SetupPhysics();
        
        // 移動開始
        if (startMoving)
        {
            StartMoving();
        }
    }

    void Update()
    {
        if (isMoving && !isWaiting)
        {
            MovePlatform();
        }
    }

    void FixedUpdate()
    {
        // プレイヤーを連れていく処理（物理更新後）
        if (carryPlayer && carriedObjects.Count > 0)
        {
            CarryObjects();
        }
    }

    // プラットフォームの初期化
    private void InitializePlatform()
    {
        startPosition = transform.position;
        
        // 移動方向の設定
        if (useCustomDirection)
        {
            currentDirection = moveDirection.normalized;
        }
        else
        {
            // 水平・垂直移動の設定
            if (moveHorizontal && moveVertical)
            {
                currentDirection = Vector2.right; // デフォルトは右方向
            }
            else if (moveHorizontal)
            {
                currentDirection = Vector2.right;
            }
            else if (moveVertical)
            {
                currentDirection = Vector2.up;
            }
            else
            {
                currentDirection = Vector2.right;
            }
        }
        
        currentDistance = 0f;
        
        if (showDebugInfo)
        {
            Debug.Log($"MovingPlatform初期化: 開始位置={startPosition}, 方向={currentDirection}, 距離={moveDistance}");
        }
    }

    // 物理コンポーネントの設定
    private void SetupPhysics()
    {
        if (useRigidbody)
        {
            // 2Dか3Dかを判定
            rb2D = GetComponent<Rigidbody2D>();
            rb3D = GetComponent<Rigidbody>();
            
            if (rb2D != null)
            {
                rb2D.isKinematic = isKinematic;
                if (freezeRotation)
                {
                    rb2D.freezeRotation = true;
                }
            }
            else if (rb3D != null)
            {
                rb3D.isKinematic = isKinematic;
                if (freezeRotation)
                {
                    rb3D.freezeRotation = true;
                }
            }
        }
    }

    // プラットフォームの移動
    private void MovePlatform()
    {
        Vector3 movement = currentDirection * moveSpeed * Time.deltaTime;
        Vector3 newPosition = transform.position + movement;
        
        // 移動距離を更新
        currentDistance += movement.magnitude;
        
        // 移動制限をチェック
        if (currentDistance >= moveDistance)
        {
            // 移動距離に達した場合
            float excessDistance = currentDistance - moveDistance;
            Vector3 excessMovement = currentDirection * excessDistance;
            newPosition = transform.position + movement - excessMovement;
            
            // 方向転換または停止
            if (usePingPong)
            {
                ReverseDirection();
            }
            else
            {
                StopMoving();
            }
        }
        
        // 位置を更新
        if (useRigidbody && rb2D != null)
        {
            rb2D.MovePosition(newPosition);
        }
        else if (useRigidbody && rb3D != null)
        {
            rb3D.MovePosition(newPosition);
        }
        else
        {
            transform.position = newPosition;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"MovingPlatform移動: 位置={transform.position}, 移動距離={currentDistance:F2}/{moveDistance}");
        }
    }

    // 方向転換
    private void ReverseDirection()
    {
        currentDirection = -currentDirection;
        currentDistance = 0f;
        
        OnPlatformReverse?.Invoke();
        
        if (showDebugInfo)
        {
            Debug.Log($"MovingPlatform方向転換: 新しい方向={currentDirection}");
        }
        
        // 待機時間がある場合は待機
        if (waitTime > 0f)
        {
            StartWait();
        }
    }

    // 待機開始
    private void StartWait()
    {
        isWaiting = true;
        isMoving = false;
        
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
        }
        waitCoroutine = StartCoroutine(WaitCoroutine());
    }

    // 待機コルーチン
    private IEnumerator WaitCoroutine()
    {
        yield return new WaitForSeconds(waitTime);
        
        isWaiting = false;
        isMoving = true;
        
        if (showDebugInfo)
        {
            Debug.Log("MovingPlatform待機終了: 移動再開");
        }
    }

    // 移動開始
    public void StartMoving()
    {
        isMoving = true;
        isWaiting = false;
        
        OnPlatformStart?.Invoke();
        
        if (showDebugInfo)
        {
            Debug.Log("MovingPlatform移動開始");
        }
    }

    // 移動停止
    public void StopMoving()
    {
        isMoving = false;
        isWaiting = false;
        
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }
        
        OnPlatformStop?.Invoke();
        
        if (showDebugInfo)
        {
            Debug.Log("MovingPlatform移動停止");
        }
    }

    // 移動再開
    public void ResumeMoving()
    {
        if (!isMoving && !isWaiting)
        {
            StartMoving();
        }
    }

    // 方向を手動で変更
    public void SetDirection(Vector2 newDirection)
    {
        currentDirection = newDirection.normalized;
        currentDistance = 0f;
        
        if (showDebugInfo)
        {
            Debug.Log($"MovingPlatform方向変更: {currentDirection}");
        }
    }

    // 位置をリセット
    public void ResetPosition()
    {
        transform.position = startPosition;
        currentDistance = 0f;
        currentDirection = moveDirection.normalized;
        
        if (showDebugInfo)
        {
            Debug.Log("MovingPlatform位置リセット");
        }
    }

    // オブジェクトを連れていく処理
    private void CarryObjects()
    {
        Vector3 currentPosition = transform.position;
        
        foreach (Transform obj in carriedObjects)
        {
            if (obj != null)
            {
                // プラットフォームの移動量を計算
                Vector3 lastPlatformPos = lastPlatformPositions.ContainsKey(obj) ? 
                    lastPlatformPositions[obj] : currentPosition;
                Vector3 platformMovement = currentPosition - lastPlatformPos;
                
                // プラットフォームが移動している場合のみ処理
                if (platformMovement.magnitude > 0.001f)
                {
                    // 床の移動量をキャラクターの位置に直接加算
                    obj.position += platformMovement;
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"MovingPlatform: {obj.name} を移動量 {platformMovement} で移動");
                    }
                }
                
                // 現在の位置を記録
                lastPlatformPositions[obj] = currentPosition;
            }
        }
    }

    // トリガーに入った時の処理
    void OnTriggerEnter2D(Collider2D other)
    {
        if (carryPlayer && other.CompareTag(playerTag))
        {
            AddCarriedObject(other.transform);
            OnPlayerEnter?.Invoke();
        }
        else if (carryOtherObjects)
        {
            foreach (string tag in otherTags)
            {
                if (other.CompareTag(tag))
                {
                    AddCarriedObject(other.transform);
                    break;
                }
            }
        }
    }
    
    // コリジョンに入った時の処理（物理的な接触）
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (useCollisionDetection)
        {
            if (carryPlayer && collision.gameObject.CompareTag(playerTag))
            {
                // プレイヤーが床の上にいるかチェック
                if (IsOnTopOfPlatform(collision))
                {
                    AddCarriedObject(collision.transform);
                    OnPlayerEnter?.Invoke();
                }
            }
            else if (carryOtherObjects)
            {
                foreach (string tag in otherTags)
                {
                    if (collision.gameObject.CompareTag(tag))
                    {
                        if (IsOnTopOfPlatform(collision))
                        {
                            AddCarriedObject(collision.transform);
                            break;
                        }
                    }
                }
            }
        }
    }
    
    // コリジョンから出た時の処理
    void OnCollisionExit2D(Collision2D collision)
    {
        if (useCollisionDetection)
        {
            if (carryPlayer && collision.gameObject.CompareTag(playerTag))
            {
                RemoveCarriedObject(collision.transform);
                OnPlayerExit?.Invoke();
            }
            else if (carryOtherObjects)
            {
                foreach (string tag in otherTags)
                {
                    if (collision.gameObject.CompareTag(tag))
                    {
                        RemoveCarriedObject(collision.transform);
                        break;
                    }
                }
            }
        }
    }
    
    // オブジェクトがプラットフォームの上にいるかチェック
    private bool IsOnTopOfPlatform(Collision2D collision)
    {
        // 接触点をチェック
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // プレイヤーの足元から検知オフセット分上にあるかチェック
            Vector3 playerBottom = collision.transform.position + Vector3.down * detectionOffset;
            if (contact.point.y >= playerBottom.y)
            {
                return true;
            }
        }
        return false;
    }

    // トリガーから出た時の処理
    void OnTriggerExit2D(Collider2D other)
    {
        if (carryPlayer && other.CompareTag(playerTag))
        {
            RemoveCarriedObject(other.transform);
            OnPlayerExit?.Invoke();
        }
        else if (carryOtherObjects)
        {
            foreach (string tag in otherTags)
            {
                if (other.CompareTag(tag))
                {
                    RemoveCarriedObject(other.transform);
                    break;
                }
            }
        }
    }

    // 連れていくオブジェクトを追加
    private void AddCarriedObject(Transform obj)
    {
        if (!carriedObjects.Contains(obj))
        {
            carriedObjects.Add(obj);
            lastPositions[obj] = obj.position;
            lastPlatformPositions[obj] = transform.position;
            
            if (showDebugInfo)
            {
                Debug.Log($"MovingPlatform: {obj.name}を連れていくリストに追加");
            }
        }
    }

    // 連れていくオブジェクトを削除
    private void RemoveCarriedObject(Transform obj)
    {
        if (carriedObjects.Contains(obj))
        {
            carriedObjects.Remove(obj);
            lastPositions.Remove(obj);
            lastPlatformPositions.Remove(obj);
            
            if (showDebugInfo)
            {
                Debug.Log($"MovingPlatform: {obj.name}を連れていくリストから削除");
            }
        }
    }

    // ギズモを描画
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Gizmos.color = gizmoColor;
        
        // 移動範囲を表示
        Vector3 start = Application.isPlaying ? startPosition : transform.position;
        Vector3 end = start + (Vector3)(currentDirection * moveDistance);
        
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(start, 0.5f);
        Gizmos.DrawWireSphere(end, 0.5f);
        
        // 現在の位置を表示
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }

    // 外部からアクセス可能なプロパティ
    public bool IsMoving => isMoving;
    public bool IsWaiting => isWaiting;
    public Vector3 StartPosition => startPosition;
    public Vector3 CurrentDirection => currentDirection;
    public float CurrentDistance => currentDistance;
    public int CarriedObjectCount => carriedObjects.Count;

    // デバッグ用：Inspectorでボタンから手動で移動開始
    [ContextMenu("Start Moving")]
    public void ManualStartMoving()
    {
        StartMoving();
    }
    
    // デバッグ用：Inspectorでボタンから手動で移動停止
    [ContextMenu("Stop Moving")]
    public void ManualStopMoving()
    {
        StopMoving();
    }
    
    // デバッグ用：Inspectorでボタンから手動で位置リセット
    [ContextMenu("Reset Position")]
    public void ManualResetPosition()
    {
        ResetPosition();
    }
    
    // デバッグ用：Inspectorでボタンから方向転換
    [ContextMenu("Reverse Direction")]
    public void ManualReverseDirection()
    {
        ReverseDirection();
    }
}
