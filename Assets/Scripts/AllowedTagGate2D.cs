using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 指定したタグを持つコライダーだけが通過できる2Dゲート。
/// ゲート側のCollider2Dは「非Trigger」で配置してください（物理的にブロック）。
/// 許可タグの相手に対してのみ一時的に衝突を無効化して通過させます。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AllowedTagGate2D : MonoBehaviour
{
    [Header("許可タグ設定")]
    [Tooltip("このタグのオブジェクトのみ通過可能（複数指定可）")]
    public string[] allowedTags;
    [Tooltip("通過可否判定で相手のルート(最上位)のタグも参照する")]
    public bool checkRootTag = true;

    [Header("動作設定")]
    [Tooltip("初期チェック：ゲート側Collider2DがTriggerだとブロックできません")] public bool warnIfTrigger = true;
    [Tooltip("IgnoreCollision復帰の最短ディレイ（秒）。通過中に即復帰して再度めり込むのを防止")]
    public float minRestoreDelay = 0.05f;

    [Header("デバッグ")]
    public bool showDebugLogs = false;

    private Collider2D gateCollider;
    private Collider2D[] gateCollidersAll;
    // 誤解除を防ぐため、ゲートが無視設定したコライダーのみ管理
    private readonly HashSet<Collider2D> ignoredColliders = new HashSet<Collider2D>();

    void Awake()
    {
        gateCollider = GetComponent<Collider2D>();
        gateCollidersAll = GetComponentsInChildren<Collider2D>(true);
    }

    void Start()
    {
        if (warnIfTrigger && gateCollider.isTrigger)
        {
            Debug.LogWarning($"AllowedTagGate2D({gameObject.name}): ゲートCollider2DがTriggerです。物理ブロックには非Triggerが必要です。");
        }
    }

    // 非Trigger同士の衝突を許可タグなら無視
    void OnCollisionEnter2D(Collision2D collision)
    {
        TryAllowThrough(collision.collider);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        // 衝突が継続していても、まだ許可していなければ許可を適用
        TryAllowThrough(collision.collider);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        RestoreCollisionIfNeeded(collision.collider);
    }

    // 片方がTriggerの場合にも対応（ゲートが非Trigger推奨だが、相手がTriggerの場合の通過サポート）
    void OnTriggerEnter2D(Collider2D other)
    {
        TryAllowThrough(other);
    }
    void OnTriggerStay2D(Collider2D other)
    {
        TryAllowThrough(other);
    }
    void OnTriggerExit2D(Collider2D other)
    {
        RestoreCollisionIfNeeded(other);
    }

    private void TryAllowThrough(Collider2D other)
    {
        if (other == null) return;
        if (IsAllowedTag(other))
        {
            if (!ignoredColliders.Contains(other))
            {
                // ゲート側の全Colliderと、相手側ルート配下の全Colliderの組み合わせでIgnore設定
                var otherAll = GetAllColliders(other);
                foreach (var gc in gateCollidersAll)
                {
                    if (gc == null) continue;
                    foreach (var oc in otherAll)
                    {
                        if (oc == null) continue;
                        Physics2D.IgnoreCollision(gc, oc, true);
                    }
                }
                ignoredColliders.Add(other);
                // 最低ディレイ後に復帰可
                restoreAfterTime[other] = Time.time + minRestoreDelay;
                if (showDebugLogs)
                {
                    Debug.Log($"[AllowedTagGate2D] Allow pass: {other.name} (tag={other.tag}, rootTag={other.transform.root.tag}) through {gameObject.name}");
                }
            }
        }
        else
        {
            // 不許可は何もしない（衝突継続でブロック）
            if (showDebugLogs)
            {
                Debug.Log($"[AllowedTagGate2D] Block: {other.name} (tag={other.tag}, rootTag={other.transform.root.tag}) at {gameObject.name}");
            }
        }
    }

    private void RestoreCollisionIfNeeded(Collider2D other)
    {
        if (other == null) return;
        if (ignoredColliders.Contains(other))
        {
            // 復帰ディレイ中 or まだ接触中なら復帰しない
            if (restoreAfterTime.TryGetValue(other, out float t) && Time.time < t)
            {
                return;
            }
            if (IsStillTouchingGate(other))
            {
                return;
            }
            // 全ペア復帰
            var otherAll = GetAllColliders(other);
            foreach (var gc in gateCollidersAll)
            {
                if (gc == null) continue;
                foreach (var oc in otherAll)
                {
                    if (oc == null) continue;
                    Physics2D.IgnoreCollision(gc, oc, false);
                }
            }
            ignoredColliders.Remove(other);
            restoreAfterTime.Remove(other);
            if (showDebugLogs)
            {
                Debug.Log($"[AllowedTagGate2D] Restore collision: {other.name} (tag={other.tag}) with {gameObject.name}");
            }
        }
    }

    private bool IsAllowedTag(Collider2D other)
    {
        if (allowedTags == null || allowedTags.Length == 0) return false;
        // 1) 自身のタグ
        string selfTag = other.tag;
        if (!string.IsNullOrEmpty(selfTag))
        {
            for (int i = 0; i < allowedTags.Length; i++)
            {
                var t = allowedTags[i];
                if (!string.IsNullOrEmpty(t) && t == selfTag) return true;
            }
        }
        // 2) ルートのタグ（子コライダーが Untagged の場合に備える）
        if (checkRootTag && other.transform != null)
        {
            string rootTag = other.transform.root != null ? other.transform.root.tag : string.Empty;
            if (!string.IsNullOrEmpty(rootTag))
            {
                for (int i = 0; i < allowedTags.Length; i++)
                {
                    var t = allowedTags[i];
                    if (!string.IsNullOrEmpty(t) && t == rootTag) return true;
                }
            }
        }
        // 3) アタッチされたRigidbodyのゲームオブジェクトのタグ
        var rb = other.attachedRigidbody;
        if (rb != null)
        {
            string rbTag = rb.gameObject.tag;
            if (!string.IsNullOrEmpty(rbTag))
            {
                for (int i = 0; i < allowedTags.Length; i++)
                {
                    var t = allowedTags[i];
                    if (!string.IsNullOrEmpty(t) && t == rbTag) return true;
                }
            }
        }
        return false;
    }

    // 復帰ディレイ管理
    private readonly Dictionary<Collider2D, float> restoreAfterTime = new Dictionary<Collider2D, float>();

    private bool IsStillTouchingGate(Collider2D other)
    {
        if (other == null) return false;
        // いずれかのゲートColliderと相手Colliderが接触中ならtrue
        foreach (var gc in gateCollidersAll)
        {
            if (gc == null) continue;
            if (gc.IsTouching(other)) return true;
        }
        return false;
    }

    private static List<Collider2D> GetAllColliders(Collider2D any)
    {
        var list = new List<Collider2D>();
        if (any == null) return list;
        // ルート配下を優先
        var root = any.transform != null ? any.transform.root : null;
        if (root != null)
        {
            root.GetComponentsInChildren(true, list);
        }
        else
        {
            list.Add(any);
        }
        return list;
    }

    void OnDisable()
    {
        // ゲートが無効化されるとき、無視設定を戻す
        RestoreAllIgnored();
    }

    void OnDestroy()
    {
        RestoreAllIgnored();
    }

    private void RestoreAllIgnored()
    {
        if (gateCollider == null) return;
        foreach (var col in ignoredColliders)
        {
            if (col != null)
            {
                Physics2D.IgnoreCollision(gateCollider, col, false);
            }
        }
        ignoredColliders.Clear();
    }
}


