using FluentAssertions;

namespace Interview;

static class BinarySequence
{
    public static int FindSequences(string input)
    {
        var result = 0;
        var tracker = new Dictionary<char, int>
        {
            { '0', 0 },
            { '1', 0 }
        };
        var ar = input.ToCharArray();

        for (var index = 0; index < ar.Length - 1; index++)
        {
            tracker[ar[index]]++;
            Console.WriteLine($"Index: {index}, Char: {ar[index]}, Tracker: {tracker['0']}, {tracker['1']}");
            if (index < ar.Length - 1 && ar[index + 1] != ar[index] || index == ar.Length - 1)
            {
                var subCount = Math.Min(tracker['0'], tracker['1']);
                result += subCount;
                tracker[ar[index + 1]] = 0;
            }
        }

        return result;
    }

    public static void FindSequencesTest()
    {
        var tests = new Tuple<string, int>[]
        {
            new("00110011", 6),
            new("10101", 4)
        };

        foreach (var testcase in tests)
        {
            var result = FindSequences(testcase.Item1);
            result.Should().Be(testcase.Item2);
        }
    }
   
}
