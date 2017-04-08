using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using static MerkelsPuzzle.HelperClasses.Helpers;


namespace MerkelsPuzzle.HelperClasses
{
    public class SendingPrincipal
    {
        #region Properties

        #endregion

        #region Fields
       
        #endregion

        #region Constructors
        public SendingPrincipal()
        {

        }
        #endregion

        #region Methods

        public List<(string prePuzzleKey, Byte[] puzzle)> GetKeyedPuzzles(int length)
        {
            var prePuzzleKeys_SecreteKeysList = GetPrePuzzleKeys_SecreteKeysList(length);
            List<Byte[]> puzzles = GetPuzzles(prePuzzleKeys_SecreteKeysList);
            var nonShuffledKeyedPuzzles = puzzles.Select(puzzle => (prePuzzleKeys_SecreteKeysList.ElementAt(puzzles.IndexOf(puzzle)).prePuzzleKey, puzzle)).ToList();
            var s = ShuffleList<(string prePuzzleKey, Byte[] puzzle)>(nonShuffledKeyedPuzzles);
            return s;
        }

        private List<Byte[]> GetPuzzles(List<(string prePuzzleKey, string secreteKey)> prePuzzleKeys_SecreteKeysList)
        {
            return prePuzzleKeys_SecreteKeysList.Select(item => GetPuzzle(prePuzzleKeys_SecreteKeysList.IndexOf(item), item.secreteKey, item.prePuzzleKey)).ToList();
        }

        private byte[] GetPuzzle(int index, string secretKey, string prePuzzleKey)
        {
            var secret = ToASCIIBytes(secretKey);
            var message = GetMessage(index, secretKey);
            byte[] encreptedData;
            using (Aes aes = Aes.Create())
            {
                aes.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                aes.Mode = CipherMode.ECB;
                aes.BlockSize = 128;
                aes.KeySize = 128;
                aes.Padding = PaddingMode.None;

                aes.Key = SetPuzzleKey(prePuzzleKey);
                aes.GenerateIV();
                ICryptoTransform aesEncryptor = aes.CreateEncryptor();
                using (var memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesEncryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(message, 0, message.Length);
                    }
                    encreptedData = memoryStream.ToArray();
                }
            }
            return encreptedData;
        }

        private byte[] GetMessage(int index, string secretKey) => PadderTo128bit(BitConverter.GetBytes(index)).Concat(ToASCIIBytes(secretKey)).ToArray();

        private byte[] SetPuzzleKey(string prePuzzleKey)
        {
            Encoding encoder = Encoding.ASCII;
            var bytes = encoder.GetBytes(prePuzzleKey);
            return OneThousandTimesHasher(bytes);
        }

        private List<(string prePuzzleKey, string secreteKey)> GetPrePuzzleKeys_SecreteKeysList(int length)
        {
            var prePuzzleKeys_SecreteKeys = new List<(string prePuzzleKey, string secreteKey)>();
            for (int i = 0; i < length; i++)
            {
                prePuzzleKeys_SecreteKeys.Add((GetPrePuzzleKey(), GetSecretKey()));
            }
            return prePuzzleKeys_SecreteKeys;
        }

        private string GetPrePuzzleKey() => Random128bitsString(16);

        private string GetSecretKey() => Random128bitsString(16);


        #endregion





    }
}
