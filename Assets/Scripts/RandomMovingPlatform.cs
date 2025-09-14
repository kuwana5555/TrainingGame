using System.Collections;
using UnityEngine;

/// <summary>
/// ランダムで移動し続けるプラットフォーム
/// 指定した範囲内でランダムな方向に移動し続けます
/// </summary>
public class RandomMovingPlatform : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 2.0f;          // 移動速度
    [SerializeField] private float moveDistance = 5.0f;       // 移動距離
    [SerializeField] private float directionChangeInterval = 3.0f; // 方向変更間隔（秒）
    [SerializeField] private float waitTimeAtEdge = 0.5f;     // 端での待機時間（秒）
    [SerializeField] private bool useSimpleMovement = true;   // シンプルな移動方式を使用するか
    
    [Header("移動範囲設定")]
    [SerializeField] private bool useBoxCollider2DArea = true; // BoxCollider2Dを領域として使用するか
    [SerializeField] private Vector2 areaCenter = Vector2.zero; // 領域の中心（BoxCollider2Dを使わないとき）
    [SerializeField] private Vector2 areaSize = new Vector2(10f, 6f); // 領域のサイズ
    [SerializeField] private float edgeBuffer = 0.5f;         // 端からのバッファ距離
    
    [Header("ランダム化設定")]
    [SerializeField] private bool randomizeSpeed = false;     // 速度をランダム化するか
    [SerializeField] private Vector2 speedRange = new Vector2(1.0f, 3.0f); // 速度の範囲
    [SerializeField] private bool randomizeInterval = false;  // 間隔をランダム化するか
    [SerializeField] private Vector2 intervalRange = new Vector2(2.0f, 5.0f); // 間隔の範囲
    
    [Header("プレイヤー連携設定")]
    [SerializeField] private bool carryPlayer = true;         // プレイヤーを連れていくか
    [SerializeField] private string playerTag = "Player";     // プレイヤーのタグ
    [SerializeField] private bool carryOtherObjects = false;  // 他のオブジェクトも連れていくか
    [SerializeField] private string[] otherTags = { "Enemy", "Item" }; // 連れていく他のタグ
    
    [Header("物理設定")]
    [SerializeField] private bool useRigidbody = true;        // Rigidbodyを使用するか
    [SerializeField] private bool freezeRotation = true;      // 回転を固定するか
    [SerializeField] private bool isKinematic = true;         // Kinematicにするか
    
    [Header("デバッグ設定")]
    [SerializeField] private bool showDebugInfo = false;      // デバッグ情報を表示するか
    [SerializeField] private bool showGizmos = true;          // ギズモを表示するか
    [SerializeField] private Color gizmoColor = Color.cyan;   // ギズモの色
    
    // 内部変数
    private Vector3 startPosition;        // 開始位置
    private Vector3 currentDirection;     // 現在の移動方向
    private Vector3 targetPosition;       // 目標位置
    private bool isMoving = false;        // 移動中かどうか
    private bool isWaiting = false;       // 待機中かどうか
    private Coroutine moveCoroutine;      // 移動コルーチンの参照
    private Coroutine waitCoroutine;      // 待機コルーチンの参照
    
    // プレイヤー連携用
    private System.Collections.Generic.List<Transform> carriedObjects = new System.Collections.Generic.List<Transform>();
    private System.Collections.Generic.Dictionary<Transform, Vector3> lastPlatformPositions = new System.Collections.Generic.Dictionary<Transform, Vector3>();
    
    // 物理コンポーネント
    private Rigidbody2D rb2D;
    private Rigidbody rb3D;
    private BoxCollider2D boxArea;
    
    // イベント
    public System.Action OnPlatformStart;     // プラットフォーム開始時
    public System.Action OnPlatformStop;      // プラットフォーム停止時
    public System.Action OnDirectionChange;   // 方向変更時
    public System.Action OnPlayerEnter;       // プレイヤーが乗った時
    public System.Action OnPlayerExit;        // プレイヤーが降りた時

    void Start()
    {
        InitializePlatform();
        SetupPhysics();
        
        // ランダム移動を開始
        StartRandomMovement();
    }

    void OnEnable()
    {
        // オブジェクトがアクティブになった時も移動を開始
        if (isMoving)
        {
            StartRandomMovement();
        }
    }

    void OnDisable()
    {
        // オブジェクトが非アクティブになった時は移動を停止
        StopRandomMovement();
    }

    void Update()
    {
        // シンプルな移動方式の場合
        if (useSimpleMovement && isMoving && !isWaiting)
        {
            SimpleRandomMovement();
        }
        
        // オブジェクトを連れていく処理
        if (carriedObjects.Count > 0)
        {
            CarryObjects();
        }
    }

    // プラットフォームの初期化
    private void InitializePlatform()
    {
        startPosition = transform.position;
        
        if (useBoxCollider2DArea)
        {
            boxArea = GetComponent<BoxCollider2D>();
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"RandomMovingPlatform初期化: 開始位置={startPosition}");
        }
    }

    // 物理コンポーネントの設定
    private void SetupPhysics()
    {
        if (useRigidbody)
        {
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

    // ランダム移動を開始
    public void StartRandomMovement()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        
        isMoving = true;
        
        if (useSimpleMovement)
        {
            // シンプルな移動方式
            Debug.Log("RandomMovingPlatform: シンプル移動を開始しました");
        }
        else
        {
            // コルーチン方式
            moveCoroutine = StartCoroutine(RandomMovementCoroutine());
            Debug.Log("RandomMovingPlatform: コルーチン移動を開始しました");
        }
        
        OnPlatformStart?.Invoke();
    }

    // ランダム移動を停止
    public void StopRandomMovement()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }
        
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }
        
        isMoving = false;
        isWaiting = false;
        
        OnPlatformStop?.Invoke();
        
        if (showDebugInfo)
        {
            Debug.Log("RandomMovingPlatform: ランダム移動を停止しました");
        }
    }

    // ランダム移動コルーチン
    private IEnumerator RandomMovementCoroutine()
    {
        Debug.Log("RandomMovingPlatform: 移動コルーチン開始");
        
        while (isMoving)
        {
            Debug.Log("RandomMovingPlatform: 新しい移動サイクル開始");
            
            // ランダムな方向を生成
            Vector3 randomDirection = GetRandomDirection();
            currentDirection = randomDirection;
            
            // ランダムな速度を設定
            float currentSpeed = moveSpeed;
            if (randomizeSpeed)
            {
                currentSpeed = Random.Range(speedRange.x, speedRange.y);
            }
            
            // 目標位置を計算
            targetPosition = GetRandomPositionInArea();
            
            Debug.Log($"RandomMovingPlatform: 目標位置={targetPosition}, 速度={currentSpeed}");
            
            // 移動を実行
            yield return StartCoroutine(MoveToTarget(targetPosition, currentSpeed));
            
            Debug.Log("RandomMovingPlatform: 移動完了");
            
            // 待機時間
            if (waitTimeAtEdge > 0f)
            {
                Debug.Log($"RandomMovingPlatform: {waitTimeAtEdge}秒待機");
                yield return StartCoroutine(WaitAtPosition());
            }
            
            // 次の方向変更までの間隔
            float interval = directionChangeInterval;
            if (randomizeInterval)
            {
                interval = Random.Range(intervalRange.x, intervalRange.y);
            }
            
            Debug.Log($"RandomMovingPlatform: {interval}秒間隔で待機");
            yield return new WaitForSeconds(interval);
        }
        
        Debug.Log("RandomMovingPlatform: 移動コルーチン終了");
    }

    // ランダムな方向を取得
    private Vector3 GetRandomDirection()
    {
        Vector3 randomPos = GetRandomPositionInArea();
        Vector3 direction = (randomPos - transform.position).normalized;
        
        if (showDebugInfo)
        {
            Debug.Log($"RandomMovingPlatform: 新しい方向={direction}");
        }
        
        OnDirectionChange?.Invoke();
        return direction;
    }

    // 領域内のランダムな位置を取得
    private Vector3 GetRandomPositionInArea()
    {
        Vector3 randomPos;
        
        if (useBoxCollider2DArea && boxArea != null)
        {
            Bounds bounds = boxArea.bounds;
            randomPos = new Vector3(
                Random.Range(bounds.min.x + edgeBuffer, bounds.max.x - edgeBuffer),
                Random.Range(bounds.min.y + edgeBuffer, bounds.max.y - edgeBuffer),
                transform.position.z
            );
        }
        else
        {
            Vector2 half = areaSize * 0.5f;
            randomPos = new Vector3(
                Random.Range(areaCenter.x - half.x + edgeBuffer, areaCenter.x + half.x - edgeBuffer),
                Random.Range(areaCenter.y - half.y + edgeBuffer, areaCenter.y + half.y - edgeBuffer),
                transform.position.z
            );
        }
        
        return randomPos;
    }

    // 目標位置まで移動
    private IEnumerator MoveToTarget(Vector3 target, float speed)
    {
        Vector3 startPos = transform.position;
        float distance = Vector3.Distance(startPos, target);
        float duration = distance / speed;
        float elapsed = 0f;
        
        Debug.Log($"RandomMovingPlatform: 移動開始 - 開始位置={startPos}, 目標位置={target}, 距離={distance}, 時間={duration}");
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            Vector3 newPosition = Vector3.Lerp(startPos, target, t);
            
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
                Debug.Log($"RandomMovingPlatform: 移動中 - 現在位置={transform.position}, 進行率={t:F2}");
            }
            
            yield return null;
        }
        
        // 最終位置を設定
        if (useRigidbody && rb2D != null)
        {
            rb2D.MovePosition(target);
        }
        else if (useRigidbody && rb3D != null)
        {
            rb3D.MovePosition(target);
        }
        else
        {
            transform.position = target;
        }
        
        Debug.Log($"RandomMovingPlatform: 移動完了 - 最終位置={transform.position}");
    }

    // 位置で待機
    private IEnumerator WaitAtPosition()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTimeAtEdge);
        isWaiting = false;
    }

    // シンプルなランダム移動
    private void SimpleRandomMovement()
    {
        // 現在の方向が設定されていない場合は初期化
        if (currentDirection == Vector3.zero)
        {
            currentDirection = GetRandomDirection();
            Debug.Log($"RandomMovingPlatform: 初期方向設定={currentDirection}");
        }
        
        // 現在の速度を取得
        float currentSpeed = moveSpeed;
        if (randomizeSpeed)
        {
            currentSpeed = Random.Range(speedRange.x, speedRange.y);
        }
        
        // 移動
        Vector3 movement = currentDirection * currentSpeed * Time.deltaTime;
        Vector3 newPosition = transform.position + movement;
        
        // 範囲内かチェック
        if (IsPositionInArea(newPosition))
        {
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
        }
        else
        {
            // 範囲外の場合は新しい方向を設定
            currentDirection = GetRandomDirection();
            Debug.Log($"RandomMovingPlatform: 範囲外のため方向変更={currentDirection}");
        }
    }

    // 位置が領域内かチェック
    private bool IsPositionInArea(Vector3 position)
    {
        if (useBoxCollider2DArea && boxArea != null)
        {
            Bounds bounds = boxArea.bounds;
            return position.x >= bounds.min.x + edgeBuffer && 
                   position.x <= bounds.max.x - edgeBuffer &&
                   position.y >= bounds.min.y + edgeBuffer && 
                   position.y <= bounds.max.y - edgeBuffer;
        }
        else
        {
            Vector2 half = areaSize * 0.5f;
            return position.x >= areaCenter.x - half.x + edgeBuffer && 
                   position.x <= areaCenter.x + half.x - edgeBuffer &&
                   position.y >= areaCenter.y - half.y + edgeBuffer && 
                   position.y <= areaCenter.y + half.y - edgeBuffer;
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
                        Debug.Log($"RandomMovingPlatform: {obj.name} を移動量 {platformMovement} で移動");
                    }
                }
                
                // 現在の位置を記録
                lastPlatformPositions[obj] = currentPosition;
            }
        }
    }

    // 2Dコリジョンに入った時の処理
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (carryPlayer && collision.gameObject.CompareTag(playerTag))
        {
            AddCarriedObject(collision.transform);
            OnPlayerEnter?.Invoke();
        }
        else if (carryOtherObjects)
        {
            foreach (string tag in otherTags)
            {
                if (collision.gameObject.CompareTag(tag))
                {
                    AddCarriedObject(collision.transform);
                    break;
                }
            }
        }
    }

    // 2Dコリジョンから出た時の処理
    void OnCollisionExit2D(Collision2D collision)
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

    // 3Dコリジョンに入った時の処理
    void OnCollisionEnter(Collision collision)
    {
        if (carryPlayer && collision.gameObject.CompareTag(playerTag))
        {
            AddCarriedObject(collision.transform);
            OnPlayerEnter?.Invoke();
        }
        else if (carryOtherObjects)
        {
            foreach (string tag in otherTags)
            {
                if (collision.gameObject.CompareTag(tag))
                {
                    AddCarriedObject(collision.transform);
                    break;
                }
            }
        }
    }

    // 3Dコリジョンから出た時の処理
    void OnCollisionExit(Collision collision)
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

    // 連れていくオブジェクトを追加
    private void AddCarriedObject(Transform obj)
    {
        if (!carriedObjects.Contains(obj))
        {
            carriedObjects.Add(obj);
            lastPlatformPositions[obj] = obj.position;
            
            if (showDebugInfo)
            {
                Debug.Log($"RandomMovingPlatform: {obj.name}を連れていくリストに追加");
            }
        }
    }

    // 連れていくオブジェクトを削除
    private void RemoveCarriedObject(Transform obj)
    {
        if (carriedObjects.Contains(obj))
        {
            carriedObjects.Remove(obj);
            lastPlatformPositions.Remove(obj);
            
            if (showDebugInfo)
            {
                Debug.Log($"RandomMovingPlatform: {obj.name}を連れていくリストから削除");
            }
        }
    }

    // ギズモを描画
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Gizmos.color = gizmoColor;
        
        // 移動範囲を表示
        if (useBoxCollider2DArea && boxArea != null)
        {
            Bounds bounds = boxArea.bounds;
            Vector3 center = bounds.center;
            Vector3 size = bounds.size;
            Gizmos.DrawCube(center, size);
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
            Gizmos.DrawWireCube(center, size);
        }
        else
        {
            Vector3 center = new Vector3(areaCenter.x, areaCenter.y, transform.position.z);
            Vector3 size = new Vector3(areaSize.x, areaSize.y, 0.1f);
            Gizmos.DrawCube(center, size);
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
            Gizmos.DrawWireCube(center, size);
        }
        
        // 現在の方向を表示
        if (Application.isPlaying && isMoving)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, currentDirection * 2f);
        }
    }

    // デバッグ用：Inspectorでボタンから手動で開始
    [ContextMenu("Start Random Movement")]
    public void ManualStartMovement()
    {
        StartRandomMovement();
    }

    // デバッグ用：Inspectorでボタンから手動で停止
    [ContextMenu("Stop Random Movement")]
    public void ManualStopMovement()
    {
        StopRandomMovement();
    }

    // 外部からアクセス可能なプロパティ
    public bool IsMoving => isMoving;
    public bool IsWaiting => isWaiting;
    public Vector3 CurrentDirection => currentDirection;
    public Vector3 TargetPosition => targetPosition;
    public int CarriedObjectCount => carriedObjects.Count;
}
