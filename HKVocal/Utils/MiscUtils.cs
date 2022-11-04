﻿namespace HKVocals;

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
}