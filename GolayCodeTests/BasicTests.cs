using System.Diagnostics;
using System.Text;
using Golay;
using NUnit.Framework;

namespace GolayCodeTests;

[TestFixture]
public class BasicTests
{
    [Test]
    public void can_encode_data()
    {
        var original = MakeData(32);
        
        var encoded = GolayCodec.Encode(original, out var length);
        
        Console.WriteLine($"Received {encoded.Length} bytes, encoder claimed {length}");
    }

    [Test]
    public void can_decode_data()
    {
        var original = Encoding.UTF8.GetBytes("HELLO");//MakeData(32);
        Console.WriteLine(string.Join(" ", original.Select(i=>i.ToString("X2"))));
        
        var encoded = GolayCodec.Encode(original, out var length);
        
        Console.WriteLine($"Received {encoded.Length} bytes, encoder claimed {length}");
        Console.WriteLine(string.Join(" ", encoded.Select(i=>i.ToString("X2"))));
        
        var result = GolayCodec.Decode(encoded, out var totalErrors);
        Console.WriteLine(string.Join(" ", result.Select(i=>i.ToString("X2"))));

        Console.WriteLine($"Total errors found: {totalErrors}");
        Assert.That(result.Length, Is.GreaterThanOrEqualTo(original.Length), "Length");
        
        var readable = Encoding.UTF8.GetString(result);
        Console.WriteLine(readable);
        
        for (int i = 0; i < original.Length; i++)
        {
            Assert.That(result[i], Is.EqualTo(original[i]), $"Byte at position {i}");
        }
    }
    
    [Test]
    public void can_decode_random_data()
    {
        var original = MakeData(1024);
        
        var encoded = GolayCodec.Encode(original, out var length);
        
        Console.WriteLine($"Received {encoded.Length} bytes, encoder claimed {length}");
        
        var result = GolayCodec.Decode(encoded, out var totalErrors);

        Console.WriteLine($"Total errors found: {totalErrors}");
        Assert.That(result.Length, Is.GreaterThanOrEqualTo(original.Length), "Length");
        Console.WriteLine($"Decoded {result.Length} bytes, from {original.Length}");
        
        for (int i = 0; i < original.Length; i++)
        {
            Assert.That(result[i], Is.EqualTo(original[i]), $"Byte at position {i}");
        }
    }
    
    [Test]
    public void can_decode_large_data()
    {
        var sw = new Stopwatch();
        sw.Start();
        var original = MakeData(1024*1024); // 1MiB... large for embedded systems
        sw.Stop();
        Console.WriteLine($"Generating data took {sw.Elapsed}");
        
        sw.Restart();
        var encoded = GolayCodec.Encode(original, out var length);
        sw.Stop();
        Console.WriteLine($"Encoding took {sw.Elapsed}");
        
        Console.WriteLine($"Received {encoded.Length} bytes, encoder claimed {length}");
        
        sw.Restart();
        var result = GolayCodec.Decode(encoded, out var totalErrors);
        sw.Stop();
        Console.WriteLine($"Decoding took {sw.Elapsed}");

        Console.WriteLine($"Total errors found: {totalErrors}");
        Assert.That(result.Length, Is.GreaterThanOrEqualTo(original.Length), "Length");
        Console.WriteLine($"Decoded {result.Length} bytes, from {original.Length}");
        
        for (int i = 0; i < original.Length; i++)
        {
            Assert.That(result[i], Is.EqualTo(original[i]), $"Byte at position {i} of {original.Length-1}");
        }
    }

    
    [Test]
    public void can_decode_damaged_data()
    {
        var rnd = new Random();
        var original = MakeData(1024);
        
        var encoded = GolayCodec.Encode(original, out var length);
        
        
        // Randomly flip 1 bit in every 3rd byte (right at the threshold of what Golay can handle)
        for (int i = 0; i < encoded.Length; i+=3)
        {
            var b = rnd.Next(0,8);
            encoded[i] ^= (byte)(1 << b);
        }
        
        Console.WriteLine($"Received {encoded.Length} bytes, encoder claimed {length}");
        
        var result = GolayCodec.Decode(encoded, out var totalErrors);

        Console.WriteLine($"Total errors found: {totalErrors}");
        Assert.That(result.Length, Is.GreaterThanOrEqualTo(original.Length), "Length");
        Console.WriteLine($"Decoded {result.Length} bytes, from {original.Length}");
        
        for (int i = 0; i < original.Length; i++)
        {
            Assert.That(result[i], Is.EqualTo(original[i]), $"Byte at position {i} of {original.Length-1}");
        }
    }

    private byte[] MakeData(int size)
    {
        var rnd = new Random();
        var data = new byte[size];
        rnd.NextBytes(data);
        return data;
    }
}