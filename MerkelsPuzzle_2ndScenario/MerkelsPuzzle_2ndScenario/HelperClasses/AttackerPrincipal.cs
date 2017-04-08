using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using static MerkelsPuzzle_2nd.HelperClasses.Helpers;

namespace MerkelsPuzzle_2nd.HelperClasses
{
    public class AttackerPrincipal
    {
        #region Field
        private int _stolenIndex;
        private List<Byte[]> _stolenPuzzles;
        private List<string> _stolenPrePuzzleKeys;
        #endregion

        #region Properties

        #endregion

        #region Methods

        public void StealIndex(int index) => _stolenIndex = index;

        public void StealKeyedPuzzles((List<string> prePuzzleKeys, List<Byte[]> puzzles) keyedPuzzles)
        {
            _stolenPrePuzzleKeys = keyedPuzzles.prePuzzleKeys;
            _stolenPuzzles = keyedPuzzles.puzzles;
        }


        public string MineKeyedPuzzles()
        {
            string returnValue = "";
            foreach (var _stolenPuzzle in _stolenPuzzles)
            {
                var index_SecretKey = GetIndex_SecretKey(_stolenPuzzle);
                if (index_SecretKey.index == _stolenIndex)
                {
                    return index_SecretKey.secretKey;
                }
            }

            return returnValue;
        }

        public (int index, string secretKey) GetIndex_SecretKey(byte[] puzzle)
        {
            (int index, string secretKey) index_SecretKey = (0, "");
            var stolenPuzzleKeys = _stolenPrePuzzleKeys.Select(prePuzzleKey => GetPuzzleKey(prePuzzleKey));
            foreach (var stolenPuzzleKey in stolenPuzzleKeys)
            {
                var decryptedData = GetDecryptedPuzzle(puzzle, stolenPuzzleKey);
                if (decryptedData.puzzleKey.SequenceEqual(stolenPuzzleKey))
                {
                    return (decryptedData.index, decryptedData.secretKey);
                }
            }
            return index_SecretKey;
        }

        private (int index, string secretKey, byte[] puzzleKey) GetDecryptedPuzzle(byte[] puzzle, byte[] stolenPuzzleKey)
        {
            byte[] decryptedPuzzle = new byte[48];

            using (Aes aes = Aes.Create())
            {
                aes.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                aes.Mode = CipherMode.ECB;
                aes.BlockSize = 128;
                aes.KeySize = 128;
                aes.Padding = PaddingMode.None;

                aes.Key = stolenPuzzleKey;
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
                    GetSecretKeyDecryptedData(decryptedPuzzle),
                    GetPuzzleKeyFromDecryptedData(decryptedPuzzle));
        }

        public int GetIndexFromDecryptedData(byte[] decryptedPuzzle)
        {
            var indexBits = decryptedPuzzle.Take(16).ToArray();
            return BitConverter.ToInt32(indexBits, 12);
        }

        public string GetSecretKeyDecryptedData(byte[] decryptedPuzzle) => Encoding.ASCII.GetString(decryptedPuzzle.Skip(16).Take(16).ToArray());

        public byte[] GetPuzzleKeyFromDecryptedData(byte[] decryptedPuzzle) => decryptedPuzzle.Skip(32).Take(16).ToArray();



        private byte[] GetPuzzleKey(string prePuzzleKey)
        {
            Encoding encoder = Encoding.ASCII;
            var bytes = encoder.GetBytes(prePuzzleKey.ToCharArray());
            return OneThousandTimesHasher(bytes);
        }


        #endregion
    }
}
