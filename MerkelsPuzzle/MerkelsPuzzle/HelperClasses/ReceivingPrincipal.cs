using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
using static MerkelsPuzzle.HelperClasses.Helpers;

namespace MerkelsPuzzle.HelperClasses
{
    public class ReceivingPrincipal
    {
        #region Fields
        private List<(string prePuzzleKey, Byte[] puzzle)> _receivedKeyedPuzzles;
        #endregion

        #region Properties

        #endregion

        #region Methods
        public void ReceiveKeyedPuzzles(List<(string prePuzzleKey, Byte[] puzzle)> keyedPuzzles) => _receivedKeyedPuzzles = keyedPuzzles;

        private byte[] GetDecryptedPuzzle()
        {
            var keyedPuzzle = GetRandomKeyedPuzzle();
            var puzzleKey = GetPuzzleKey(keyedPuzzle.prePuzzleKey);
            var puzzle = keyedPuzzle.puzzle;

            byte[] decryptedPuzzle = new byte[32];

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
                        cryptoStream.Read(decryptedPuzzle, 0, 32);
                    }
                }
            }
            return decryptedPuzzle;
        }

        public int GetIndex()
        {
            var decryptedPuzzle = GetDecryptedPuzzle();
            var indexBits = decryptedPuzzle.Take(16).ToArray();
            return BitConverter.ToInt32(indexBits, 12);
        }

        private (string prePuzzleKey, Byte[] puzzle) GetRandomKeyedPuzzle()
        {
            var randomNumberGenerator = new Random();
            return _receivedKeyedPuzzles[randomNumberGenerator.Next(_receivedKeyedPuzzles.Count)];
        }

        private byte[] GetPuzzleKey(string prePuzzleKey)
        {
            Encoding encoder = Encoding.ASCII;
            var bytes = encoder.GetBytes(prePuzzleKey.ToCharArray());
            return OneThousandTimesHasher(bytes);
        }
        
        #endregion
    }
}
