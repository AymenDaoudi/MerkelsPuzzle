using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
using static MerkelsPuzzle_2nd.HelperClasses.Helpers;

namespace MerkelsPuzzle_2nd.HelperClasses
{
    public class ReceivingPrincipal
    {
        #region Fields
        private List<Byte[]> _receivedPuzzles;
        private List<string> _receivedPrePuzzleKeys;
        #endregion

        #region Properties

        #endregion

        #region Methods
        public void ReceiveKeyedPuzzles((List<string> prePuzzleKeys, List<Byte[]> puzzles) keyedPuzzles)
        {
            _receivedPrePuzzleKeys = keyedPuzzles.prePuzzleKeys;
            _receivedPuzzles = keyedPuzzles.puzzles;
        }

        public int GetIndex()
        {
            int index = 0;
            var puzzle = GetRandomPuzzle();
            var _receivedPuzzleKeys = _receivedPrePuzzleKeys.Select(prePuzzleKey => GetPuzzleKey(prePuzzleKey));
            foreach (var receivedPuzzleKeys in _receivedPuzzleKeys)
            {
                var decryptedData = GetDecryptedPuzzle(puzzle, receivedPuzzleKeys);
                if (decryptedData.puzzleKey.SequenceEqual(receivedPuzzleKeys))
                {
                    return decryptedData.index;
                }
            }
            return index;
        }

        private (int index, string secretKey, byte[] puzzleKey) GetDecryptedPuzzle(byte[] puzzle, byte[] puzzleKey)
        {         
            byte[] decryptedPuzzle = new byte[48];

            using (Aes aes = Aes.Create())
            {
                aes.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                aes.Mode = CipherMode.ECB;
                aes.BlockSize = 128;
                aes.KeySize = 128;
                aes.Padding = PaddingMode.None;

                aes.Key = puzzleKey;
                aes.GenerateIV();
                ICryptoTransform aesDecryptor = aes.CreateDecryptor();
                using (var memoryStream = new MemoryStream(puzzle))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesDecryptor, CryptoStreamMode.Read))
                    {
                        cryptoStream.Read(decryptedPuzzle, 0, 48);
                    }
                }
            }

            return (GetIndexFromDecryptedData(decryptedPuzzle),
                    GetSecretKeyFromDecryptedData(decryptedPuzzle),
                    GetPuzzleKeyFromDecryptedData(decryptedPuzzle)) ;

        }

        public int GetIndexFromDecryptedData(byte[] decryptedPuzzle)
        {
            var indexBits = decryptedPuzzle.Take(16).ToArray();
            return BitConverter.ToInt32(indexBits, 12);
        }

        public string GetSecretKeyFromDecryptedData(byte[] decryptedPuzzle) => Encoding.ASCII.GetString(decryptedPuzzle.Skip(16).Take(16).ToArray());

        public byte[] GetPuzzleKeyFromDecryptedData(byte[] decryptedPuzzle) => decryptedPuzzle.Skip(32).Take(16).ToArray();

        private byte[] GetPuzzleKey(string prePuzzleKey)
        {
            Encoding encoder = Encoding.ASCII;
            var bytes = encoder.GetBytes(prePuzzleKey.ToCharArray());
            return OneThousandTimesHasher(bytes);
        }

        private byte[] GetRandomPuzzle()
        {
            var randomNumberGenerator = new Random();
            return _receivedPuzzles[randomNumberGenerator.Next(_receivedPuzzles.Count)];
        }

        #endregion
    }
}
