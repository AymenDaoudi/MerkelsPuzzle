using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using static MerkelsPuzzle.HelperClasses.Helpers;

namespace MerkelsPuzzle.HelperClasses
{
    public class AttackerPrincipal
    {
        #region Field
        private int _stolenIndex;
        private List<(string prePuzzleKey, Byte[] puzzle)> _stolenKeyedPuzzles;
        #endregion

        #region Properties

        #endregion

        #region Methods

        public void StealIndex(int index) => _stolenIndex = index;

        public void StealKeyedPuzzles(List<(string prePuzzleKey, Byte[] puzzle)> keyedPuzzles) => _stolenKeyedPuzzles = keyedPuzzles;

        


        public string MineKeyedPuzzles()
        {
            string returnValue = "";
            foreach (var keyedPuzzle in _stolenKeyedPuzzles)
            {
                var indexAndSecretKey = GetIndexAndSecretKey(keyedPuzzle);
                if (indexAndSecretKey.index == _stolenIndex)
                {
                    var i = _stolenKeyedPuzzles.IndexOf(keyedPuzzle);
                    return indexAndSecretKey.secretKey;
                }
            }
            return returnValue;
        }

        private ( int index, string secretKey) GetIndexAndSecretKey((string prePuzzleKey, Byte[] puzzle) keyedPuzzle)
        {
            var decryptedPuzzle = GetDecryptedPuzzle(keyedPuzzle);
            var indexBits = decryptedPuzzle.Take(16).ToArray();
            var secretKeyBits = decryptedPuzzle.Skip(16).Take(16).ToArray();
            return (BitConverter.ToInt32(indexBits, 12), Encoding.ASCII.GetString(secretKeyBits));
        }

        private byte[] GetDecryptedPuzzle((string prePuzzleKey, Byte[] puzzle) keyedPuzzle)
        {
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

        private byte[] GetPuzzleKey(string prePuzzleKey)
        {
            Encoding encoder = Encoding.ASCII;
            var bytes = encoder.GetBytes(prePuzzleKey.ToCharArray());
            return OneThousandTimesHasher(bytes);
        }


        #endregion
    }
}
