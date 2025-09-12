using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 画面全体を黒でフェードさせるユーティリティ。
/// インスタンスはシーンに1つ想定。AwakeでフルスクリーンのCanvasとImageを自動生成します。
/// </summary>
public class ScreenFader : MonoBehaviour
{
	[Tooltip("開始時に黒で覆う (Alpha=1)")]
	public bool startBlack = false;

	[Tooltip("生成するCanvasのSort Order。大きいほど最前面")] 
	public int sortingOrder = 5000;

	[Tooltip("フェードに使う色 (通常は黒)")]
	public Color fadeColor = Color.black;

	Canvas _canvas;
	Image _image;
	Coroutine _running;

	void Awake()
	{
		CreateFullScreenOverlay();
		SetAlpha(startBlack ? 1f : 0f);
	}

	void CreateFullScreenOverlay()
	{
		var go = new GameObject("ScreenFader_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
		go.layer = gameObject.layer;
		_canvas = go.GetComponent<Canvas>();
		_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		_canvas.sortingOrder = sortingOrder;
		var scaler = go.GetComponent<CanvasScaler>();
		scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		scaler.referenceResolution = new Vector2(1920, 1080);

		var imageGO = new GameObject("ScreenFader_Image", typeof(Image));
		imageGO.transform.SetParent(go.transform, false);
		_image = imageGO.GetComponent<Image>();
		_image.color = fadeColor;
		var rect = _image.rectTransform;
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.offsetMin = Vector2.zero;
		rect.offsetMax = Vector2.zero;
	}

	public void SetAlpha(float a)
	{
		if (_image == null) return;
		var c = _image.color;
		c.a = Mathf.Clamp01(a);
		_image.color = c;
	}

	public float GetAlpha()
	{
		return _image != null ? _image.color.a : 0f;
	}

	public Coroutine FadeIn(float duration)
	{
		return StartFade(1f, 0f, duration);
	}

	public Coroutine FadeOut(float duration)
	{
		return StartFade(0f, 1f, duration);
	}

	Coroutine StartFade(float from, float to, float duration)
	{
		if (_running != null) StopCoroutine(_running);
		_running = StartCoroutine(FadeRoutine(from, to, duration));
		return _running;
	}

	IEnumerator FadeRoutine(float from, float to, float duration)
	{
		SetAlpha(from);
		if (duration <= 0f)
		{
			SetAlpha(to);
			yield break;
		}
		float t = 0f;
		while (t < duration)
		{
			t += Time.deltaTime;
			float a = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
			SetAlpha(a);
			yield return null;
		}
		SetAlpha(to);
	}
}


