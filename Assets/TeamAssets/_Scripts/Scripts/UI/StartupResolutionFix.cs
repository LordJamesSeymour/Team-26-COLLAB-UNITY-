using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-10000)]
public class StartupResolutionFix : MonoBehaviour
{
	[Header("Preferred Startup Resolution")]
	[SerializeField] private int preferredWidth = 1920;
	[SerializeField] private int preferredHeight = 1080;

	[Header("Behaviour")]
	[SerializeField] private bool forceFixOnEveryLaunchIfBadAspect = true;
	[SerializeField] private bool onlyRunOncePerInstall = false;

	private const string ResolutionFixAppliedKey = "StartupResolutionFix_Applied_v1";

	private const float TargetAspect = 16f / 9f;
	private const float AspectTolerance = 0.03f;

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}

	private IEnumerator Start()
	{
		bool alreadyApplied = PlayerPrefs.GetInt(ResolutionFixAppliedKey, 0) == 1;
		bool currentAspectIsBad = IsBadAspect(Screen.width, Screen.height);

		if (onlyRunOncePerInstall && alreadyApplied && !currentAspectIsBad)
			yield break;

		if (!forceFixOnEveryLaunchIfBadAspect && alreadyApplied)
			yield break;

		Vector2Int targetResolution = FindBestResolution();

		// This mimics your manual fix:
		// 1. Go windowed.
		// 2. Apply 1920x1080 or best 16:9 resolution.
		// 3. Return to fullscreen.
		Screen.fullScreenMode = FullScreenMode.Windowed;
		Screen.SetResolution(targetResolution.x, targetResolution.y, FullScreenMode.Windowed);

		yield return null;
		yield return new WaitForEndOfFrame();

		Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
		Screen.SetResolution(targetResolution.x, targetResolution.y, FullScreenMode.FullScreenWindow);
		Screen.fullScreen = true;

		PlayerPrefs.SetInt(ResolutionFixAppliedKey, 1);
		PlayerPrefs.Save();
	}

	private Vector2Int FindBestResolution()
	{
		Resolution[] resolutions = Screen.resolutions;

		// First priority: use the resolution that you already know fixes the game.
		for (int i = 0; i < resolutions.Length; i++)
		{
			if (resolutions[i].width == preferredWidth && resolutions[i].height == preferredHeight)
			{
				return new Vector2Int(preferredWidth, preferredHeight);
			}
		}

		// Second priority: find the largest available 16:9 resolution.
		Vector2Int best16By9 = Vector2Int.zero;

		for (int i = 0; i < resolutions.Length; i++)
		{
			int width = resolutions[i].width;
			int height = resolutions[i].height;

			if (!IsBadAspect(width, height))
			{
				if (width * height > best16By9.x * best16By9.y)
				{
					best16By9 = new Vector2Int(width, height);
				}
			}
		}

		if (best16By9 != Vector2Int.zero)
			return best16By9;

		// Final fallback.
		return new Vector2Int(preferredWidth, preferredHeight);
	}

	private bool IsBadAspect(int width, int height)
	{
		if (height <= 0)
			return true;

		float aspect = width / (float)height;
		return Mathf.Abs(aspect - TargetAspect) > AspectTolerance;
	}
}