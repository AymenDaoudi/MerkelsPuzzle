using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MerkelsPuzzle_2nd.HelperClasses
{
    public static class Helpers
    {
        #region Consts
        const string AplhaNums = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        #endregion
        
        #region Fields
        private static Random _random = new Random();
        #endregion

        public static string Random128bitsString(int length)
        {
            return new string(Enumerable.Repeat(AplhaNums, length).Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public static byte[] OneThousandTimesHasher(byte[] bytes)
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            for (int i = 0; i < 1000; i++)
            {
                bytes = sha1.ComputeHash(bytes);
            }
            
            return PadderTo128bit(bytes);
        }

        public static string PadderTo128bit(string input)
        {
            while (input.Length < 16)
            {
                input = 0 + input;
            }
            while (input.Length > 16)
            {
                input = input.Remove(0, 1);
            }
            return input;
        }

        public static byte[] PadderTo128bit(byte[] input)
        {
            var output = new List<byte>();
            output = input.ToList();
            while (output.Count < 16)
            {
                output.Insert(0, byte.MinValue);
            }
            while (output.Count > 16)
            {
                output = output.Take(output.Count - 1).ToList();
            }
            return output.ToArray();
        }

        public static byte[] ToASCIIBytes(string input)
        {
            Encoding encoder = Encoding.ASCII;
            return encoder.GetBytes(input);
        }

        public static List<T> ShuffleList<T>(List<T> input)
        {
            var randomizer = new Random();
            return input.OrderBy(item => randomizer.Next()).ToList();
        }

       
    }
}
