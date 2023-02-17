using System.Security.Cryptography;

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
        List<T> removeList = new ();
        foreach (var (key, value) in dict)
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
    public static void RemoveNullValues<T>(this List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null)
            {
                list.RemoveAt(i);
                i--;
            }
        }
    }
    
    public static void TryInvokeActions(this Action<PlayMakerFSM> action, PlayMakerFSM fsm)
    {
        if (action != null)
        {
            Delegate[] invocationList = action.GetInvocationList();
            for (int i = 0; i < invocationList.Length; i++)
            {
                try
                {
                    (invocationList[i] as Action<PlayMakerFSM>)!.Invoke(fsm);
                }
                catch (Exception e)
                {
                    HKVocals.instance.LogError(e);
                }
            }
        }
    }

    public static GameObject Find(this Behaviour obj, string name)
    {
        return obj.transform.Find(name).gameObject;
    }

    public static float GetDecibelVolume(float value)
    {
        // 0 is max volume on this scale
        return value <= 9
            ? RefVanillaMenu.MusicVolumeSlider.GetComponent<MenuAudioSlider>().Reflect().GetVolumeLevel(value)
            : 0.0f;
    }

    public static string GetFileHash(string file)
    {
        var sha1 = SHA1.Create();
        var stream = File.OpenRead(file);
        var hashBytes = sha1.ComputeHash(stream);
        var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        stream.Close();
        sha1.Clear();
        return $"{hash.Substring(0, 6)}";
    }
}