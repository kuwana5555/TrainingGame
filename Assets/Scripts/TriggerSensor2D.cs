using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 2D用トリガーセンサー。
/// ・このスクリプトを 2D トリガーを持つオブジェクトにアタッチ
/// ・Inspector で「検知役」となる複数の Collider2D を指定（自分自身も可）
/// ・検知方法は「指定タグのみ」または「全てのコライダー」から選択
/// ・接触時にログを出力（要件通り）。必要に応じてイベント拡張可
/// </summary>
public class TriggerSensor2D : MonoBehaviour
{
	public enum DetectionMode
	{
		All,
		TagOnly
	}

	[Header("検知設定")]
	[Tooltip("検知役となる Collider2D（複数指定可・自分自身可）")]
	public List<Collider2D> detectorColliders = new List<Collider2D>();

	[Tooltip("検知方法: 全て or 指定タグのみ")]
	public DetectionMode detectionMode = DetectionMode.TagOnly;

	[Tooltip("detectionMode=TagOnly のときに使用するタグ")]
	public string targetTag = "Player";

	[Header("動作")]
	[Tooltip("開始時に、自身のCollider2D(トリガー)を自動で検知役に追加する")]
	public bool includeSelfColliderOnStart = true;

	void Start()
	{
		if (includeSelfColliderOnStart)
		{
			var own = GetComponent<Collider2D>();
			if (own != null && own.isTrigger)
			{
				if (!detectorColliders.Contains(own)) detectorColliders.Add(own);
			}
		}
		ValidateDetectors();
	}

	void ValidateDetectors()
	{
		for (int i = detectorColliders.Count - 1; i >= 0; i--)
		{
			var c = detectorColliders[i];
			if (c == null)
			{
				detectorColliders.RemoveAt(i);
				continue;
			}
			if (!c.isTrigger)
			{
				Debug.LogWarning($"TriggerSensor2D: 検知役のCollider2Dは isTrigger=true が必要です -> {c.name}", c);
			}
		}
	}

	bool ShouldLog(Collider2D detector, Collider2D incoming)
	{
		if (detector == null || incoming == null) return false;
		if (!detectorColliders.Contains(detector)) return false;
		if (!incoming.isTrigger) return false; // 要件: トリガー同士
		switch (detectionMode)
		{
			case DetectionMode.All:
				return true;
			case DetectionMode.TagOnly:
				return !string.IsNullOrEmpty(targetTag) && incoming.CompareTag(targetTag);
		}
		return false;
	}

	// 自身に付いた複数のトリガーをみるため、共通で拾ってルーティング
	void OnTriggerEnter2D(Collider2D other)
	{
		// Unityのコールバックでは「どの検知役が反応したか」は直接渡されない。
		// 最も近い（同一GameObject上）の Collider2D を特定し、その反応元を detector とみなす。
		var candidate = FindMatchingDetectorFor(other);
		if (ShouldLog(candidate, other))
		{
			Debug.Log($"[TriggerSensor2D] Enter detector={NameOf(candidate)} other={NameOf(other)} tag={other.tag}", this);
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		var candidate = FindMatchingDetectorFor(other);
		if (ShouldLog(candidate, other))
		{
			Debug.Log($"[TriggerSensor2D] Exit  detector={NameOf(candidate)} other={NameOf(other)} tag={other.tag}", this);
		}
	}

	Collider2D FindMatchingDetectorFor(Collider2D other)
	{
		// 通常、同じ GameObject にある isTrigger Collider2D が反応元
		// 厳密に特定するAPIは無いが、本スクリプトの要件では
		// 「本オブジェクト上の検知役が反応した」とみなせばよい
		// → 自身に付いている検知役のうち、enabled なものを優先して返す
		for (int i = 0; i < detectorColliders.Count; i++)
		{
			var c = detectorColliders[i];
			if (c != null && c.enabled && c.gameObject == gameObject)
			{
				return c;
			}
		}
		// 自身以外も検知役として登録可能なため、最初の有効なものを返す
		for (int i = 0; i < detectorColliders.Count; i++)
		{
			var c = detectorColliders[i];
			if (c != null && c.enabled)
			{
				return c;
			}
		}
		return null;
	}

	static string NameOf(Object o)
	{
		return o == null ? "null" : o.name;
	}
}



