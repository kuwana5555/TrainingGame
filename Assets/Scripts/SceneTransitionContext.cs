using UnityEngine;

/// <summary>
/// シーン遷移元/先の名前を一時的に保持する静的コンテキスト。
/// シーンロード間で値を共有するために使用する。
/// </summary>
public static class SceneTransitionContext
{
    public static string FromSceneName { get; private set; }
    public static string ToSceneName { get; private set; }

    public static void Set(string fromSceneName, string toSceneName)
    {
        FromSceneName = fromSceneName;
        ToSceneName = toSceneName;
    }

    public static void Clear()
    {
        FromSceneName = null;
        ToSceneName = null;
    }
}



