namespace HKVocals;

public static class MiscUtils
{
    private static int[] GetRange(int start, int end)
    {
        int[] array = new int[end - start + 1];
        for (int i = 0; i < array.Length; i++)
            array[i] = start + i;
        return array;
    }
}