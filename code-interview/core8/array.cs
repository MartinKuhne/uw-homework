internal static class ArrayUtilities
{
    // https://leetcode.com/explore/featured/card/top-interview-questions-easy/92/array/727/
    internal static Tuple<int[], int> RemoveDuplicates(int[] input)
    {
        int removeCount = 0;

        for (int i = 1; i < input.Length; i++)
        {
            if (input[i] == input[i - 1])
            {
                removeCount++;
            }
            else
            {
                input[i - removeCount] = input[i];
            }
        }

        return new(input, input.Length - removeCount);
    }
}

internal class Dto
{
    public int[]? Array { get; set; }
    public int Count { get; set; }

    internal Dto(int[] array, int count)
    {
        Array = array;
        Count = count;
    }
}

internal class Node<T>
{
    public T Value { get; set; }
    public Node<T>? Next { get; set; }

    public Node(T value)
    {
        Value = value;
        Next = null;
    }

    public Node(T value, Node<T>? next)
    {
        Value = value;
        Next = next;
    }
}
