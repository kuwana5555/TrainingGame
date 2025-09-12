using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // IconのUI可視性制御用
using TMPro;          // TextMeshPro対応

public class ObjectGrabber : MonoBehaviour
{
    [Header("掴み設定")]
    [SerializeField] private string grabbableTag = "Grabbable";  // 掴めるオブジェクトのタグ
    [SerializeField] private string grabberTag = "Grabber";      // 掴むコライダーのタグ
    [SerializeField] private KeyCode grabKey = KeyCode.E;        // 掴みキー
    [SerializeField] private float grabDistance = 2.0f;          // 掴める距離（2D）
    [SerializeField] private float followSpeed = 10.0f;          // 追従速度
    [SerializeField] private Vector2 grabOffset2D = Vector2.zero; // 掴んだ時のオフセット（2D）
    [SerializeField] private bool ignoreZAxis = true;            // Z軸を無視するか（2Dゲーム用）
    
    [Header("物理設定")]
    [SerializeField] private bool freezeRotation = true;         // 掴み中に回転を固定する
    [SerializeField] private bool useGravity = true;             // 掴み中も重力を使う（falseで掴み中は重力無効）
    
    [Header("UI設定")]
    [SerializeField] private string grabbableIconTag = "GrabbableIcon"; // 掴みアイコンのタグ
    [SerializeField] private bool hideIconOnGrab = true;         // 掴んだ時にアイコンを非表示にするか
    
    private GameObject grabbedObject = null;                     // 掴んでいるオブジェクト
    private Rigidbody grabbedRigidbody = null;                   // 掴んでいるオブジェクトのRigidbody(3D)
    private Rigidbody2D grabbedRigidbody2D = null;               // 掴んでいるオブジェクトのRigidbody(2D)
    private bool isGrabbing = false;                             // 掴んでいるかどうか
    private bool canGrab = false;                                // 掴める状態かどうか
    private GameObject targetGrabbable = null;                   // 掴み対象のオブジェクト
    private GameObject grabbableIcon = null;                     // 掴みアイコン
    private Transform grabbableIconOriginalParent = null;        // アイコンの元の親
    private Vector3 grabbableIconLocalPos;                       // アイコンの元のローカル位置
    private Quaternion grabbableIconLocalRot;                    // アイコンの元のローカル回転
    private Vector3 grabbableIconLocalScale;                     // アイコンの元のローカルスケール

    // アイコンの可視性復元用キャッシュ
    private List<Renderer> iconRenderers;
    private List<bool> iconRenderersEnabled;
    private List<Graphic> iconGraphics;
    private List<bool> iconGraphicsEnabled;
    private List<TMP_Text> iconTMPTexts;
    private List<bool> iconTMPTextsEnabled;
    private CanvasGroup iconCanvasGroup;
    // 浮遊スクリプト一時停止用
    private List<Behaviour> floatingBehaviours;                 // 無効化した浮遊系Behaviour
    private List<bool> floatingBehavioursEnabled;               // その元のenabled状態
    
    // 元の物理設定を保存
    private bool originalUseGravity = false;                     // 3D用
    private bool originalFreezeRotation = false;                 // 3D用
    private RigidbodyConstraints originalConstraints;            // 3D用
    private float originalGravityScale2D = 0f;                   // 2D用
    private bool originalFreezeRotation2D = false;               // 2D用
    private RigidbodyConstraints2D originalConstraints2D;        // 2D用

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleGrabInput();
        UpdateGrabbedObject();
    }

    // 掴み入力の処理
    private void HandleGrabInput()
    {
        if (Input.GetKeyDown(grabKey))
        {
            if (!isGrabbing && canGrab && targetGrabbable != null)
            {
                // 掴み開始
                GrabObject(targetGrabbable);
            }
            else if (isGrabbing)
            {
                // 掴み解除
                ReleaseObject();
            }
        }
    }

    // 掴んでいるオブジェクトの更新（2D対応）
    private void UpdateGrabbedObject()
    {
        if (isGrabbing && grabbedObject != null)
        {
            // 掴んでいるオブジェクトを追従させる（2D対応）
            Vector3 targetPosition = transform.position + new Vector3(grabOffset2D.x, grabOffset2D.y, 0);
            
            // Z軸の処理
            if (ignoreZAxis)
            {
                // Z軸は掴んだオブジェクトの元の位置を維持
                targetPosition.z = grabbedObject.transform.position.z;
            }
            
            Vector3 direction = targetPosition - grabbedObject.transform.position;
            
            if (grabbedRigidbody != null)
            {
                // Rigidbodyを使用した滑らかな追従（2D対応）
                if (ignoreZAxis)
                {
                    Vector2 velocity2D = new Vector2(direction.x, direction.y) * followSpeed;
                    grabbedRigidbody.velocity = new Vector3(velocity2D.x, velocity2D.y, 0);
                }
                else
                {
                    grabbedRigidbody.velocity = direction * followSpeed;
                }
            }
            else
            {
                // Transformを使用した直接的な追従（2D対応）
                Vector3 currentPos = grabbedObject.transform.position;
                Vector3 newPos = Vector3.Lerp(currentPos, targetPosition, followSpeed * Time.deltaTime);
                
                if (ignoreZAxis)
                {
                    // Z軸は変更しない
                    newPos.z = currentPos.z;
                }
                
                grabbedObject.transform.position = newPos;
            }
        }
    }

    // オブジェクトを掴む
    private void GrabObject(GameObject obj)
    {
        grabbedObject = obj;
        grabbedRigidbody = obj.GetComponent<Rigidbody>();
        grabbedRigidbody2D = obj.GetComponent<Rigidbody2D>();
        isGrabbing = true;
        
        // 掴みアイコンを検索して非アクティブにする
        if (hideIconOnGrab)
        {
            grabbableIcon = FindGrabbableIcon(obj);
            if (grabbableIcon != null)
            {
                // 親とローカル変換を保存
                grabbableIconOriginalParent = grabbableIcon.transform.parent;
                grabbableIconLocalPos = grabbableIcon.transform.localPosition;
                grabbableIconLocalRot = grabbableIcon.transform.localRotation;
                grabbableIconLocalScale = grabbableIcon.transform.localScale;
                // GameObjectは非アクティブにせず描画のみ無効化して追従を維持
                HideIcon();
                Debug.Log($"掴みアイコンを非表示にしました: {grabbableIcon.name}");
            }
        }
        
        // 物理設定を保存
        if (grabbedRigidbody != null)
        {
            // 3D Rigidbody の保存と適用
            originalUseGravity = grabbedRigidbody.useGravity;
            originalFreezeRotation = grabbedRigidbody.freezeRotation;
            originalConstraints = grabbedRigidbody.constraints;

            // 掴み中は: useGravity=false なら重力を切る
            grabbedRigidbody.useGravity = useGravity;
            // freezeRotation=true なら固定
            grabbedRigidbody.freezeRotation = freezeRotation;
        }
        if (grabbedRigidbody2D != null)
        {
            // 2D Rigidbody の保存と適用
            originalGravityScale2D = grabbedRigidbody2D.gravityScale;
            originalFreezeRotation2D = grabbedRigidbody2D.freezeRotation;
            originalConstraints2D = grabbedRigidbody2D.constraints;

            // 掴み中は: useGravity=false なら重力を切る
            grabbedRigidbody2D.gravityScale = useGravity ? originalGravityScale2D : 0f;
            // freezeRotation=true なら固定
            grabbedRigidbody2D.freezeRotation = freezeRotation;
        }
        
        Debug.Log($"オブジェクトを掴みました: {obj.name}");
    }

    // オブジェクトを離す
    private void ReleaseObject()
    {
        if (grabbedObject != null)
        {
            // 掴みアイコンを再表示する
            if (hideIconOnGrab && grabbableIcon != null)
            {
                // 位置は現状を維持（ローカル変換は復元しない）
                // 描画を元に戻す
                ShowIcon();
                // 再開前に現在位置を開始位置として再初期化
                AlignFloatingStartToCurrent(grabbableIcon);
                // 浮遊系スクリプトを元に戻す
                RestoreFloatingScripts();
                Debug.Log($"掴みアイコンを表示しました: {grabbableIcon.name}");
            }
            
            // 物理設定を復元（3D）
            if (grabbedRigidbody != null)
            {
                grabbedRigidbody.useGravity = originalUseGravity;
                grabbedRigidbody.freezeRotation = originalFreezeRotation;
                grabbedRigidbody.constraints = originalConstraints;
            }
            // 物理設定を復元（2D）
            if (grabbedRigidbody2D != null)
            {
                grabbedRigidbody2D.gravityScale = originalGravityScale2D;
                grabbedRigidbody2D.freezeRotation = originalFreezeRotation2D;
                grabbedRigidbody2D.constraints = originalConstraints2D;
            }
            
            Debug.Log($"オブジェクトを離しました: {grabbedObject.name}");
        }
        
        grabbedObject = null;
        grabbedRigidbody = null;
        grabbedRigidbody2D = null;
        grabbableIcon = null;
        isGrabbing = false;
    }

    // 掴み可能なオブジェクトとの接触開始（2D対応）
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(grabbableTag) && !isGrabbing)
        {
            // 2D距離チェック（Z軸を無視）
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.y);
            Vector2 otherPos2D = new Vector2(other.transform.position.x, other.transform.position.y);
            float distance = Vector2.Distance(pos2D, otherPos2D);
            
            if (distance <= grabDistance)
            {
                canGrab = true;
                targetGrabbable = other.gameObject;
                Debug.Log($"掴み可能なオブジェクトに接触: {other.name}");
            }
        }
    }

    // 掴み可能なオブジェクトとの接触終了（2D対応）
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(grabbableTag))
        {
            if (targetGrabbable == other.gameObject)
            {
                canGrab = false;
                targetGrabbable = null;
                Debug.Log($"掴み可能なオブジェクトから離脱: {other.name}");
            }
        }
    }

    // デバッグ用：Inspectorでボタンから手動で掴み
    [ContextMenu("Grab Object")]
    public void ManualGrab()
    {
        if (!isGrabbing && canGrab && targetGrabbable != null)
        {
            GrabObject(targetGrabbable);
        }
        else
        {
            Debug.LogWarning("掴めるオブジェクトがありません");
        }
    }

    // デバッグ用：Inspectorでボタンから手動で離す
    [ContextMenu("Release Object")]
    public void ManualRelease()
    {
        if (isGrabbing)
        {
            ReleaseObject();
        }
        else
        {
            Debug.LogWarning("掴んでいるオブジェクトがありません");
        }
    }

    // 掴みアイコンを検索する
    private GameObject FindGrabbableIcon(GameObject grabbableObject)
    {
        // 子オブジェクトから指定したタグを持つオブジェクトを検索
        Transform[] children = grabbableObject.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child.CompareTag(grabbableIconTag))
            {
                // 掴み中は浮遊スクリプトを停止して追従ずれを防ぐ
                CacheAndDisableFloatingScripts(child.gameObject);
                return child.gameObject;
            }
        }
        
        Debug.LogWarning($"掴みアイコンが見つかりません: {grabbableObject.name} の子オブジェクトにタグ '{grabbableIconTag}' を持つオブジェクトがありません");
        return null;
    }

    // 浮遊スクリプト（FloatingAnimator / FloatingAnimationなど）を検出して無効化
    private void CacheAndDisableFloatingScripts(GameObject icon)
    {
        floatingBehaviours = new List<Behaviour>();
        floatingBehavioursEnabled = new List<bool>();

        // Icon配下の全てのBehaviourを走査
        var behaviours = icon.GetComponentsInChildren<Behaviour>(true);
        foreach (var b in behaviours)
        {
            var typeName = b.GetType().Name;
            if (typeName == "FloatingAnimator" || typeName == "FloatingAnimation")
            {
                floatingBehaviours.Add(b);
                floatingBehavioursEnabled.Add(b.enabled);
                b.enabled = false;
            }
        }
    }

    // 無効化した浮遊スクリプトのenabledを元に戻す
    private void RestoreFloatingScripts()
    {
        if (floatingBehaviours == null || floatingBehavioursEnabled == null) return;
        for (int i = 0; i < floatingBehaviours.Count; i++)
        {
            if (floatingBehaviours[i] != null)
            {
                bool enabled = (i < floatingBehavioursEnabled.Count) ? floatingBehavioursEnabled[i] : true;
                floatingBehaviours[i].enabled = enabled;
            }
        }
        floatingBehaviours = null;
        floatingBehavioursEnabled = null;
    }

    // 浮遊開始位置を現在座標へ合わせる（対応: FloatingAnimation/任意のReset/UpdateStartPosition相当）
    private void AlignFloatingStartToCurrent(GameObject icon)
    {
        if (icon == null) return;

        // FloatingAnimation対応: publicメソッドがあれば呼ぶ
        var floatingAnimations = icon.GetComponentsInChildren<Component>(true);
        foreach (var c in floatingAnimations)
        {
            var typeName = c.GetType().Name;
            if (typeName == "FloatingAnimation")
            {
                var m = c.GetType().GetMethod("UpdateStartPosition");
                if (m != null)
                {
                    m.Invoke(c, null);
                }
            }
            else if (typeName == "FloatingAnimator")
            {
                // 想定: ResetStartやSetStartPositionなどがあれば呼ぶ
                var m = c.GetType().GetMethod("UpdateStartPosition");
                if (m == null) m = c.GetType().GetMethod("ResetStartPosition");
                if (m == null) m = c.GetType().GetMethod("SetStartPositionToCurrent");
                if (m != null)
                {
                    m.Invoke(c, null);
                }
            }
        }
    }

    // アイコンの描画を無効化（追従は維持）
    private void HideIcon()
    {
        if (grabbableIcon == null) return;

        // 初回時にコンポーネントを収集
        iconRenderers = new List<Renderer>(grabbableIcon.GetComponentsInChildren<Renderer>(true));
        iconRenderersEnabled = new List<bool>(iconRenderers.Count);
        foreach (var r in iconRenderers)
        {
            iconRenderersEnabled.Add(r.enabled);
            r.enabled = false;
        }

        iconGraphics = new List<Graphic>(grabbableIcon.GetComponentsInChildren<Graphic>(true));
        iconGraphicsEnabled = new List<bool>(iconGraphics.Count);
        foreach (var g in iconGraphics)
        {
            iconGraphicsEnabled.Add(g.enabled);
            g.enabled = false;
        }

        iconTMPTexts = new List<TMP_Text>(grabbableIcon.GetComponentsInChildren<TMP_Text>(true));
        iconTMPTextsEnabled = new List<bool>(iconTMPTexts.Count);
        foreach (var t in iconTMPTexts)
        {
            iconTMPTextsEnabled.Add(t.enabled);
            t.enabled = false;
        }

        iconCanvasGroup = grabbableIcon.GetComponentInChildren<CanvasGroup>(true);
        if (iconCanvasGroup != null)
        {
            iconCanvasGroup.alpha = 0f;
            iconCanvasGroup.blocksRaycasts = false;
            iconCanvasGroup.interactable = false;
        }
    }

    // アイコンの描画を復元
    private void ShowIcon()
    {
        if (grabbableIcon == null) return;

        if (iconRenderers != null && iconRenderersEnabled != null)
        {
            for (int i = 0; i < iconRenderers.Count; i++)
            {
                if (iconRenderers[i] != null)
                {
                    bool enabled = (i < iconRenderersEnabled.Count) ? iconRenderersEnabled[i] : true;
                    iconRenderers[i].enabled = enabled;
                }
            }
        }

        if (iconGraphics != null && iconGraphicsEnabled != null)
        {
            for (int i = 0; i < iconGraphics.Count; i++)
            {
                if (iconGraphics[i] != null)
                {
                    bool enabled = (i < iconGraphicsEnabled.Count) ? iconGraphicsEnabled[i] : true;
                    iconGraphics[i].enabled = enabled;
                }
            }
        }

        if (iconTMPTexts != null && iconTMPTextsEnabled != null)
        {
            for (int i = 0; i < iconTMPTexts.Count; i++)
            {
                if (iconTMPTexts[i] != null)
                {
                    bool enabled = (i < iconTMPTextsEnabled.Count) ? iconTMPTextsEnabled[i] : true;
                    iconTMPTexts[i].enabled = enabled;
                }
            }
        }

        if (iconCanvasGroup != null)
        {
            iconCanvasGroup.alpha = 1f;
            iconCanvasGroup.blocksRaycasts = true;
            iconCanvasGroup.interactable = true;
        }

        // 一度復元したらキャッシュはクリア（次回のために取り直す）
        iconRenderers = null; iconRenderersEnabled = null;
        iconGraphics = null; iconGraphicsEnabled = null;
        iconTMPTexts = null; iconTMPTextsEnabled = null;
        iconCanvasGroup = null;
    }

    // デバッグ用：現在の状態を表示
    [ContextMenu("Show Status")]
    public void ShowStatus()
    {
        Debug.Log($"掴み状態: {isGrabbing}");
        Debug.Log($"掴める状態: {canGrab}");
        Debug.Log($"掴んでいるオブジェクト: {(grabbedObject != null ? grabbedObject.name : "なし")}");
        Debug.Log($"掴み対象: {(targetGrabbable != null ? targetGrabbable.name : "なし")}");
        Debug.Log($"掴みアイコン: {(grabbableIcon != null ? grabbableIcon.name : "なし")}");
    }

    // 掴んでいるオブジェクトを取得（外部から参照用）
    public GameObject GetGrabbedObject()
    {
        return grabbedObject;
    }

    // 掴んでいるかどうかを取得（外部から参照用）
    public bool IsGrabbing()
    {
        return isGrabbing;
    }
}
