using System.Collections;
using UnityEngine;

/// <summary>
/// 指定したプレファブを、指定範囲内にランダム位置で連続生成する 2D 用スクリプト。
/// ウイルスの大量ポップアップ演出などに使用できます。
/// - worldSpace の長方形領域内にスポーン
/// - 個数、間隔、ランダム回転・スケール、親 Transform を指定可能
/// - 同一 GameObject にある BoxCollider2D を領域として使うことも可能
/// </summary>
public class RandomSpawner2D : MonoBehaviour
{
	[Header("基本設定")]
	[Tooltip("生成するプレファブ。必須")] public GameObject prefab;
	[Tooltip("生成する個数")] public int spawnCount = 30;
	[Tooltip("1 体ずつ生成する間隔(秒)。0 で一度に生成")] public float delayBetweenSpawns = 0.03f;

	[Header("生成先")]
	[Tooltip("生成したオブジェクトの親にする Transform。未指定ならルート直下")] public Transform parentForSpawn;

	[Header("スポーン領域(ワールド座標)")]
	[Tooltip("この GameObject の BoxCollider2D を領域として使用するか")] public bool useBoxCollider2DArea = true;
	[Tooltip("領域の中心(ワールド座標)。BoxCollider2D を使わないとき有効")] public Vector2 areaCenter = Vector2.zero;
	[Tooltip("領域のサイズ。BoxCollider2D を使わないとき有効")] public Vector2 areaSize = new Vector2(10f, 6f);

	[Header("ランダム化")]
	[Tooltip("Z 軸周りにランダム回転を付与(2D 用)")] public bool randomRotationZ = true;
	[Tooltip("ランダムスケールの最小・最大(等方)。(1,1) なら固定")] public Vector2 randomUniformScaleMinMax = new Vector2(1f, 1f);

	[Header("実行")]
	[Tooltip("Start で自動開始")] public bool autoStart = false;

	BoxCollider2D _boxArea;

	void Awake()
	{
		if (useBoxCollider2DArea)
		{
			_boxArea = GetComponent<BoxCollider2D>();
		}
	}

	void Start()
	{
		if (autoStart)
		{
			StartSpawn();
		}
	}

	/// <summary>
	/// 生成を開始します。
	/// </summary>
	[ContextMenu("Spawn Now")]
	public void StartSpawn()
	{
		if (prefab == null)
		{
			Debug.LogWarning("RandomSpawner2D: prefab が未設定です。生成を中止します。");
			return;
		}

		StopAllCoroutines();
		bool canUseCoroutine = Application.isPlaying && delayBetweenSpawns > 0f;
		if (canUseCoroutine)
		{
			StartCoroutine(SpawnRoutine());
		}
		else
		{
			// Edit モードや遅延なしのときは一括生成
			SpawnImmediateOnce();
		}
	}

	IEnumerator SpawnRoutine()
	{
		int remaining = Mathf.Max(0, spawnCount);
		for (int i = 0; i < remaining; i++)
		{
			Vector3 position = GetRandomPositionInArea();
			Quaternion rotation = GetRandomRotation2D();
			GameObject instance = Instantiate(prefab, position, rotation, parentForSpawn);
			ApplyRandomUniformScale(instance.transform);

			if (delayBetweenSpawns > 0f)
			{
				yield return new WaitForSeconds(delayBetweenSpawns);
			}
		}
	}

	/// <summary>
	/// 遅延なしで一括スポーン（Edit/Play 両対応）。
	/// </summary>
	public void SpawnImmediateOnce()
	{
		int remaining = Mathf.Max(0, spawnCount);
		for (int i = 0; i < remaining; i++)
		{
			Vector3 position = GetRandomPositionInArea();
			Quaternion rotation = GetRandomRotation2D();
			GameObject instance = Instantiate(prefab, position, rotation, parentForSpawn);
			ApplyRandomUniformScale(instance.transform);
		}
	}

	Vector3 GetRandomPositionInArea()
	{
		if (useBoxCollider2DArea && _boxArea != null)
		{
			Bounds b = _boxArea.bounds;
			float x = Random.Range(b.min.x, b.max.x);
			float y = Random.Range(b.min.y, b.max.y);
			return new Vector3(x, y, transform.position.z);
		}
		else
		{
			Vector2 half = areaSize * 0.5f;
			float x = Random.Range(areaCenter.x - half.x, areaCenter.x + half.x);
			float y = Random.Range(areaCenter.y - half.y, areaCenter.y + half.y);
			return new Vector3(x, y, transform.position.z);
		}
	}

	Quaternion GetRandomRotation2D()
	{
		if (!randomRotationZ)
		{
			return Quaternion.identity;
		}
		float z = Random.Range(0f, 360f);
		return Quaternion.Euler(0f, 0f, z);
	}

	void ApplyRandomUniformScale(Transform target)
	{
		float min = Mathf.Min(randomUniformScaleMinMax.x, randomUniformScaleMinMax.y);
		float max = Mathf.Max(randomUniformScaleMinMax.x, randomUniformScaleMinMax.y);
		float s = Random.Range(min, max);
		target.localScale = new Vector3(s, s, target.localScale.z);
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
		if (useBoxCollider2DArea)
		{
			var box = Application.isPlaying ? _boxArea : GetComponent<BoxCollider2D>();
			if (box != null)
			{
				Bounds b = box.bounds;
				Vector3 center = b.center;
				Vector3 size = b.size;
				Gizmos.DrawCube(center, size);
				Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.9f);
				Gizmos.DrawWireCube(center, size);
			}
		}
		else
		{
			Vector3 center = new Vector3(areaCenter.x, areaCenter.y, transform.position.z);
			Vector3 size = new Vector3(areaSize.x, areaSize.y, 0.1f);
			Gizmos.DrawCube(center, size);
			Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.9f);
			Gizmos.DrawWireCube(center, size);
		}
	}
}


