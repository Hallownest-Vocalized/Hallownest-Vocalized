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

    public static int GetIndexOf<T>(this T[] arr, Func<T,bool> predicate) where T: FsmStateAction
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
}