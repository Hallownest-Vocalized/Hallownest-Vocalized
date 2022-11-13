namespace HKVocals;

public static class MiscUtils
{
    public static int[] GetRange(int start, int end)
    {
        int[] array = new int[end - start + 1];
        for (int i = 0; i < array.Length; i++)
            array[i] = start + i;
        return array;
    }

    public static int GetIndexOf<T>(this T[] arr, Func<T, bool> predicate)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            if (predicate(arr[i]))
            {
                return i;
            }
        }

        throw new ArgumentOutOfRangeException();
    }

    public static void WaitForFramesBeforeInvoke(int numFrames, Action codeToRun)
    {
        HKVocals.CoroutineHolder.StartCoroutine(WaitBeforeInvokeRoutine(numFrames, codeToRun));
    }

    public static void WaitForSecondsBeforeInvoke(float seconds, Action codeToRun)
    {
        HKVocals.CoroutineHolder.StartCoroutine(WaitBeforeInvokeRoutine(seconds, codeToRun));
    }
    
    private static IEnumerator WaitBeforeInvokeRoutine(int numFrames, Action codeToRun)
    {
        for (int i = 0; i < numFrames; i++)
        {
            yield return null;
        }

        codeToRun();
    }

    private static IEnumerator WaitBeforeInvokeRoutine(float seconds, Action codeToRun)
    {
        yield return new WaitForSeconds(seconds);

        codeToRun();
    }

    public static string GetCurrentSceneName() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

    public static void ForEach<T>(this IEnumerable<T> ienumarable, Action<T> action)
    {
        var arr = ienumarable.ToArray();
        foreach (var t in arr)
        {
            action(t);
        }
    }
    
    public static void RemoveValues<T,V>(this Dictionary<T,V> dict, Func<V, bool> condition)
    {
        List<T> removeList = new List<T>();
        foreach ((T key, V value) in dict)
        {
            if (condition(value))
            {
                removeList.Add(key);
            }
        }

        foreach (var key in removeList)
        {
            dict.Remove(key);
        }
    }
}