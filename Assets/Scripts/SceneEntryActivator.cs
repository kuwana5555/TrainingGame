using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 指定した「前シーン」から遷移してきた場合に、複数の対象をアクティブ/非アクティブ化する。
/// </summary>
public class SceneEntryActivator : MonoBehaviour
{
    [System.Serializable]
    public class EntryActivationRule
    {
        [Tooltip("このシーンに入る直前のシーン名がこれらのいずれかと一致した場合に発動")] public string[] allowedPreviousScenes;
        [Tooltip("条件を反転（allowedPreviousScenes以外から来た時に発動）")] public bool invertCondition = false;
        [Header("対象（アクティブ化）")] public GameObject[] objectsToActivate;
        [Header("対象（非アクティブ化）")] public GameObject[] objectsToDeactivate;
    }

    [Header("条件設定")]
    [Tooltip("このシーンに入る直前のシーン名がこれらのいずれかと一致した場合に発動（単一条件・下位互換）")]
    public string[] allowedPreviousScenes;
    [Tooltip("複数条件で制御したい場合はこちらを使用（設定があればこちらを優先）")]
    public EntryActivationRule[] rules;

    [Header("対象設定（アクティブ化）")]
    public GameObject[] objectsToActivate;

    [Header("対象設定（非アクティブ化）")]
    public GameObject[] objectsToDeactivate;

    [Header("動作設定")]
    public bool runOnStart = true;          // Startで即判定するか
    public bool runOnce = true;             // 一度実行したら自動で無効化
    public bool invertCondition = false;    // 条件を反転（allowedPreviousScenes以外から来た時に発動）
    [Tooltip("最初に一致したルールだけ適用し、以降は適用しない")]
    public bool applyFirstMatchOnly = false;

    [Header("Debug")]
    public bool showDebugLogs = false;      // デバッグログを表示するか

    void Start()
    {
        if (runOnStart)
        {
            TryApply();
        }
    }

    /// <summary>
    /// 条件に合えばアクティブ/非アクティブを適用
    /// </summary>
    [ContextMenu("Apply Now")] 
    public void TryApply()
    {
        string prev = SceneTransitionTracker.Instance.PreviousSceneName;
        bool executed = false;

        // ルール優先（設定がある場合）
        if (rules != null && rules.Length > 0)
        {
            for (int i = 0; i < rules.Length; i++)
            {
                var rule = rules[i];
                if (rule == null) continue;

                bool matched = false;
                if (rule.allowedPreviousScenes != null && rule.allowedPreviousScenes.Length > 0)
                {
                    foreach (var name in rule.allowedPreviousScenes)
                    {
                        if (!string.IsNullOrEmpty(name) && name == prev)
                        {
                            matched = true;
                            break;
                        }
                    }
                }

                bool shouldRunRule = rule.invertCondition ? !matched : matched;
                if (showDebugLogs)
                {
                    Debug.Log($"[SceneEntryActivator] Rule #{i}: prev='{prev}', matched={matched}, invert={rule.invertCondition}, shouldRun={shouldRunRule}");
                }
                if (!shouldRunRule) continue;

                if (showDebugLogs)
                {
                    Debug.Log($"[SceneEntryActivator] Applying Rule #{i}: Activate={Count(rule.objectsToActivate)}, Deactivate={Count(rule.objectsToDeactivate)}");
                }
                SetActive(rule.objectsToActivate, true);
                SetActive(rule.objectsToDeactivate, false);
                executed = true;

                if (applyFirstMatchOnly)
                {
                    if (showDebugLogs)
                    {
                        Debug.Log("[SceneEntryActivator] applyFirstMatchOnly=true -> stop after first match");
                    }
                    break;
                }
            }
        }
        else
        {
            // 既存の単一条件（下位互換）
            bool matched = false;
            if (allowedPreviousScenes != null && allowedPreviousScenes.Length > 0)
            {
                foreach (var name in allowedPreviousScenes)
                {
                    if (!string.IsNullOrEmpty(name) && name == prev)
                    {
                        matched = true;
                        break;
                    }
                }
            }

            bool shouldRun = invertCondition ? !matched : matched;
            if (showDebugLogs)
            {
                Debug.Log($"[SceneEntryActivator] Legacy: prev='{prev}', matched={matched}, invert={invertCondition}, shouldRun={shouldRun}");
            }
            if (shouldRun)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[SceneEntryActivator] Applying Legacy: Activate={Count(objectsToActivate)}, Deactivate={Count(objectsToDeactivate)}");
                }
                SetActive(objectsToActivate, true);
                SetActive(objectsToDeactivate, false);
                executed = true;
            }
        }

        if (showDebugLogs)
        {
            Debug.Log($"[SceneEntryActivator] executed={executed}, runOnce={runOnce}");
        }

        if (executed && runOnce)
        {
            enabled = false;
        }
    }

    private void SetActive(GameObject[] targets, bool active)
    {
        if (targets == null) return;
        foreach (var go in targets)
        {
            if (go != null)
            {
                go.SetActive(active);
            }
        }
    }

    private int Count(GameObject[] targets)
    {
        if (targets == null) return 0;
        int c = 0;
        foreach (var go in targets) { if (go != null) c++; }
        return c;
    }
}


