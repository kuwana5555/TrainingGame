using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
	[Tooltip("アクティブになってから実行するまでの遅延時間（秒）")] public float startDelay = 0f;

	[Header("生成完了後の処理")]
	[Tooltip("生成完了後にオブジェクトを削除するか")] public bool destroyAfterSpawn = false;
	[Tooltip("生成完了から削除までの遅延時間（秒）")] public float destroyDelay = 5f;
	[Tooltip("生成完了後にシーン遷移するか")] public bool changeSceneAfterSpawn = false;
	[Tooltip("遷移先のシーン名")] public string targetSceneName = "";
	[Tooltip("シーン遷移までの遅延時間（秒）")] public float changeSceneDelay = 2f;
	[Tooltip("生成完了後にアクティブにするオブジェクト")] public GameObject[] objectsToActivate;

	BoxCollider2D _boxArea;
	
	// 生成されたオブジェクトを追跡するためのリスト
	private System.Collections.Generic.List<GameObject> spawnedObjects = new System.Collections.Generic.List<GameObject>();

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
			if (startDelay > 0f)
			{
				StartCoroutine(DelayedStart());
			}
			else
			{
				StartSpawn();
			}
		}
	}
	
	IEnumerator DelayedStart()
	{
		Debug.Log($"RandomSpawner2D: {startDelay}秒後に生成を開始します");
		yield return new WaitForSeconds(startDelay);
		StartSpawn();
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
		spawnedObjects.Clear(); // 生成リストをクリア
		
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
			
			// 生成されたオブジェクトをリストに追加
			spawnedObjects.Add(instance);

			if (delayBetweenSpawns > 0f)
			{
				yield return new WaitForSeconds(delayBetweenSpawns);
			}
		}
		
		// 生成完了後の処理
		Debug.Log($"RandomSpawner2D: {spawnCount}個のオブジェクト生成完了");
		yield return StartCoroutine(OnSpawnComplete());
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
			
			// 生成されたオブジェクトをリストに追加
			spawnedObjects.Add(instance);
		}
		
		// 生成完了後の処理（コルーチンで実行）
		Debug.Log($"RandomSpawner2D: {spawnCount}個のオブジェクト生成完了");
		if (Application.isPlaying)
		{
			StartCoroutine(OnSpawnComplete());
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
	
	/// <summary>
	/// 生成完了後の処理を実行します。
	/// </summary>
	IEnumerator OnSpawnComplete()
	{
		// オブジェクトをアクティブ化
		if (objectsToActivate != null && objectsToActivate.Length > 0)
		{
			foreach (GameObject obj in objectsToActivate)
			{
				if (obj != null)
				{
					obj.SetActive(true);
					Debug.Log($"RandomSpawner2D: {obj.name} をアクティブにしました");
				}
			}
		}
		
		// 削除処理
		if (destroyAfterSpawn)
		{
			Debug.Log($"RandomSpawner2D: {destroyDelay}秒後に生成オブジェクトを削除します");
			yield return new WaitForSeconds(destroyDelay);
			
			foreach (GameObject obj in spawnedObjects)
			{
				if (obj != null)
				{
					Destroy(obj);
				}
			}
			spawnedObjects.Clear();
			Debug.Log("RandomSpawner2D: 生成オブジェクトを削除しました");
		}
		
		// シーン遷移処理
		if (changeSceneAfterSpawn)
		{
			Debug.Log($"RandomSpawner2D: {changeSceneDelay}秒後にシーン遷移します");
			yield return new WaitForSeconds(changeSceneDelay);
			
			if (!string.IsNullOrEmpty(targetSceneName))
			{
				Debug.Log($"RandomSpawner2D: シーン遷移します: {targetSceneName}");
				SceneManager.LoadScene(targetSceneName);
			}
			else
			{
				Debug.LogWarning("RandomSpawner2D: 遷移先のシーン名が設定されていません");
			}
		}
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
	
	/// <summary>
	/// 生成されたオブジェクトを手動で削除します（デバッグ用）。
	/// </summary>
	[ContextMenu("Destroy Spawned Objects")]
	public void DestroySpawnedObjects()
	{
		foreach (GameObject obj in spawnedObjects)
		{
			if (obj != null)
			{
				Destroy(obj);
			}
		}
		spawnedObjects.Clear();
		Debug.Log("RandomSpawner2D: 生成オブジェクトを手動削除しました");
	}
	
	/// <summary>
	/// 生成完了後の処理を手動で実行します（デバッグ用）。
	/// </summary>
	[ContextMenu("Execute On Spawn Complete")]
	public void ExecuteOnSpawnComplete()
	{
		if (Application.isPlaying)
		{
			StartCoroutine(OnSpawnComplete());
		}
		else
		{
			Debug.LogWarning("RandomSpawner2D: 実行時のみ使用可能です");
		}
	}
	
	/// <summary>
	/// 現在の生成オブジェクト数を取得します。
	/// </summary>
	public int GetSpawnedObjectCount()
	{
		return spawnedObjects.Count;
	}
}


