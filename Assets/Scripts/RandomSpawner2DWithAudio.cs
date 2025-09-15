using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RandomSpawner2DWithAudio : MonoBehaviour
{
	[Header("基本設定")]
	public GameObject prefab;
	public int spawnCount = 30;
	public float delayBetweenSpawns = 0.03f;

	[Header("生成先")]
	public Transform parentForSpawn;

	[Header("スポーン領域")]
	public bool useBoxCollider2DArea = true;
	public Vector2 areaCenter = Vector2.zero;
	public Vector2 areaSize = new Vector2(10f, 6f);

	[Header("ランダム化")]
	public bool randomRotationZ = true;
	public Vector2 randomUniformScaleMinMax = new Vector2(1f, 1f);

	[Header("実行")]
	public bool autoStart = false;
	public float startDelay = 0f;

	[Header("音声設定")]
	public AudioClip spawnLoopSound;
	public AudioSource audioSource;
	public float volume = 1.0f;
	public bool playSpawnLoopSound = true;
	public bool stopSoundOnSceneChange = true;

	[Header("生成完了後の処理")]
	public bool destroyAfterSpawn = false;
	public float destroyDelay = 5f;
	public bool changeSceneAfterSpawn = false;
	public string targetSceneName = "";
	public float changeSceneDelay = 2f;
	public GameObject[] objectsToActivate;

	BoxCollider2D _boxArea;
	private System.Collections.Generic.List<GameObject> spawnedObjects = new System.Collections.Generic.List<GameObject>();
	private bool isSoundPlaying = false;

	void Awake()
	{
		if (useBoxCollider2DArea)
		{
			_boxArea = GetComponent<BoxCollider2D>();
		}
	}

	void Start()
	{
		SetupAudioSource();
		
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
		yield return new WaitForSeconds(startDelay);
		StartSpawn();
	}

	[ContextMenu("Spawn Now")]
	public void StartSpawn()
	{
		if (prefab == null)
		{
			Debug.LogWarning("RandomSpawner2DWithAudio: prefab が未設定です。");
			return;
		}

		StopAllCoroutines();
		spawnedObjects.Clear();
		StartSpawnLoopSound();
		
		if (Application.isPlaying && delayBetweenSpawns > 0f)
		{
			StartCoroutine(SpawnRoutine());
		}
		else
		{
			SpawnImmediateOnce();
		}
	}

	IEnumerator SpawnRoutine()
	{
		for (int i = 0; i < spawnCount; i++)
		{
			Vector3 position = GetRandomPositionInArea();
			Quaternion rotation = GetRandomRotation2D();
			GameObject instance = Instantiate(prefab, position, rotation, parentForSpawn);
			ApplyRandomUniformScale(instance.transform);
			spawnedObjects.Add(instance);

			if (delayBetweenSpawns > 0f)
			{
				yield return new WaitForSeconds(delayBetweenSpawns);
			}
		}
		
		yield return StartCoroutine(OnSpawnComplete());
	}

	public void SpawnImmediateOnce()
	{
		for (int i = 0; i < spawnCount; i++)
		{
			Vector3 position = GetRandomPositionInArea();
			Quaternion rotation = GetRandomRotation2D();
			GameObject instance = Instantiate(prefab, position, rotation, parentForSpawn);
			ApplyRandomUniformScale(instance.transform);
			spawnedObjects.Add(instance);
		}
		
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
	
	IEnumerator OnSpawnComplete()
	{
		if (objectsToActivate != null && objectsToActivate.Length > 0)
		{
			foreach (GameObject obj in objectsToActivate)
			{
				if (obj != null)
				{
					obj.SetActive(true);
				}
			}
		}
		
		if (destroyAfterSpawn)
		{
			yield return new WaitForSeconds(destroyDelay);
			
			foreach (GameObject obj in spawnedObjects)
			{
				if (obj != null)
				{
					Destroy(obj);
				}
			}
			spawnedObjects.Clear();
		}
		
		if (changeSceneAfterSpawn)
		{
			yield return new WaitForSeconds(changeSceneDelay);
			
			if (stopSoundOnSceneChange)
			{
				StopSpawnLoopSound();
			}
			
			if (!string.IsNullOrEmpty(targetSceneName))
			{
				SceneManager.LoadScene(targetSceneName);
			}
		}
	}

	private void SetupAudioSource()
	{
		if (audioSource == null)
		{
			audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.playOnAwake = false;
		}
		
		if (audioSource != null)
		{
			audioSource.volume = volume;
		}
	}
	
	private void StartSpawnLoopSound()
	{
		if (playSpawnLoopSound && spawnLoopSound != null && audioSource != null)
		{
			audioSource.clip = spawnLoopSound;
			audioSource.loop = true;
			audioSource.Play();
			isSoundPlaying = true;
		}
	}
	
	private void StopSpawnLoopSound()
	{
		if (audioSource != null && isSoundPlaying)
		{
			audioSource.Stop();
			isSoundPlaying = false;
		}
	}
}

