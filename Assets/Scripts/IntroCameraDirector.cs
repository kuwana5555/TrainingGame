using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// シーン開始直後: サブカメラで所定のズーム演出→真っ黒フェード→メインカメラに切替→入力解放。
/// ・導入カメラのFOV(またはOrthographic Size)を時間でズーム
/// ・所定のズーム率に到達したらフェードアウト(黒)
/// ・フェード完了でメインカメラに切替、入力を有効化
/// </summary>
public class IntroCameraDirector : MonoBehaviour
{
	[Header("Cameras")]
	public Camera introCamera;
	public Camera mainCamera;

	[Header("Zoom Settings")]
	[Tooltip("パースペクティブ時の開始FOV。Orthographicの場合は開始Size")]
	public float zoomStart = 60f;
	[Tooltip("パースペクティブ時の終了FOV。Orthographicの場合は終了Size")]
	public float zoomEnd = 30f;
	[Tooltip("ズーム完了までの時間(秒)")]
	public float zoomDuration = 2.5f;
	[Tooltip("ズームがこの割合(0-1)に到達したらフェードを開始")]
	[Range(0.05f, 1f)] public float fadeStartProgress = 0.7f;

	[Header("Fade Settings")]
	public ScreenFader screenFader;
	public float fadeOutDuration = 1.2f;
	public float fadeInDuration = 0.8f;

	[Header("Input / Control")]
	[Tooltip("イントロ中は無効化し、切替時に有効化するコンポーネント(例: PlayerInput, CharacterController拡張など)")]
	public Behaviour[] componentsToEnable;
	public UnityEvent onIntroFinished;

	bool _isOrthographic;
	bool _running;

	void Reset()
	{
		introCamera = Camera.main;
	}

	void Start()
	{
		if (introCamera == null || mainCamera == null)
		{
			Debug.LogError("IntroCameraDirector: カメラ参照が設定されていません。");
			return;
		}

		_isOrthographic = introCamera.orthographic;

		// 初期可視/入力状態の整備
		mainCamera.enabled = false;
		introCamera.enabled = true;
		SetZoom(zoomStart);
		SetComponentsEnabled(false);

		// Faderが未設定ならシーンから探す
		if (screenFader == null) screenFader = FindObjectOfType<ScreenFader>();
		if (screenFader != null)
		{
			screenFader.SetAlpha(0f); // イントロは見える状態で開始→途中で黒に
		}

		if (!_running) StartCoroutine(RunSequence());
	}

	IEnumerator RunSequence()
	{
		_running = true;
		float t = 0f;
		bool startedFade = false;
		while (t < zoomDuration)
		{
			t += Time.deltaTime;
			float p = Mathf.Clamp01(t / zoomDuration);
			float value = Mathf.Lerp(zoomStart, zoomEnd, Smooth01(p));
			SetZoom(value);

			if (!startedFade && p >= fadeStartProgress)
			{
				startedFade = true;
				if (screenFader != null) screenFader.FadeOut(fadeOutDuration);
			}
			yield return null;
		}

		// 念のため最終値を適用
		SetZoom(zoomEnd);

		// フェード完了待ち
		if (screenFader != null && screenFader.GetAlpha() < 1f)
		{
			float wait = 0f;
			while (wait < fadeOutDuration)
			{
				wait += Time.deltaTime;
				yield return null;
			}
		}

		// カメラ切替
		introCamera.enabled = false;
		mainCamera.enabled = true;

		// 入力/コンポーネント解放
		SetComponentsEnabled(true);

		// 黒からフェードイン
		if (screenFader != null) yield return screenFader.FadeIn(fadeInDuration);

		onIntroFinished?.Invoke();
		_running = false;
	}

	void SetZoom(float v)
	{
		if (_isOrthographic)
		{
			introCamera.orthographicSize = v;
		}
		else
		{
			introCamera.fieldOfView = v;
		}
	}

	void SetComponentsEnabled(bool enabled)
	{
		if (componentsToEnable == null) return;
		for (int i = 0; i < componentsToEnable.Length; i++)
		{
			var c = componentsToEnable[i];
			if (c == null) continue;
			c.enabled = enabled;
		}
	}

	static float Smooth01(float x)
	{
		// ゆっくり始まりゆっくり終わるS曲線
		return x * x * (3f - 2f * x);
	}
}


