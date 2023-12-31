using NLog;
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace RedfishViewer.Services
{
    /// <summary>
    /// 暗号＆複合化
    /// </summary>
    public static class CryptoAes
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static bool IsInited;
        private static readonly byte[] _aesKey = new byte[32];     // キー生成用
        private static readonly byte[] _aesIv = new byte[16];      // 初期化ベクトル生成用

        /// <summary>
        /// 初期化
        /// </summary>
        public static void Initialize()
        {
            if (IsInited)
                return;
            IsInited = true;
            var asm = Assembly.GetExecutingAssembly();
            var filename = string.Concat(asm.Location.AsSpan(0, asm.Location.Length - 4), ".dat");
            var fileInfo = new FileInfo(filename);
            if (!fileInfo.Exists || fileInfo.Length < 48)
                WriteKeyIv(filename);
            ReadKeyIv(filename);
        }

        /// <summary>
        /// キー＆初期化ベクトル書き込み
        /// </summary>
        /// <param name="filename"></param>
        private static void WriteKeyIv(string filename)
        {
            using var witer = new BinaryWriter(new FileStream(filename, FileMode.Create));
            witer.Write(Encoding.UTF8.GetBytes(DateTime.Now.Ticks.ToString()));
            witer.Write(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString().Replace("-", "")));
        }

        /// <summary>
        /// キー＆初期化ベクトル読み込み
        /// </summary>
        /// <param name="filename"></param>
        private static void ReadKeyIv(string filename)
        {
            using var reader = new BinaryReader(File.OpenRead(filename));
            reader.Read(_aesKey, 0, _aesKey.Length);
            reader.Read(_aesIv, 0, _aesIv.Length);
        }

        /// <summary>
        /// 暗号化
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string? Encrypt(string? plainText)
        {
            if (plainText == null)
                return null;
            Initialize();
            using var cryptoTransform = Aes.Create().CreateEncryptor(_aesKey, _aesIv);
            using MemoryStream memoryStream = new();
            using (StreamWriter streamWriter = new(new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write)))
                streamWriter.Write(plainText);
            var base64_text = Convert.ToBase64String(memoryStream.ToArray());
            return base64_text;
        }

        /// <summary>
        /// 複合化
        /// </summary>
        /// <param name="base64Text"></param>
        /// <returns></returns>
        public static string? Decrypt(string? base64Text)
        {
            if (base64Text == null)
                return null;
            try
            {
                Initialize();
                using var cipherTransform = Aes.Create().CreateDecryptor(_aesKey, _aesIv);
                using MemoryStream memoryStream = new(Convert.FromBase64String(base64Text));
                using StreamReader streamReader = new(new CryptoStream(memoryStream, cipherTransform, CryptoStreamMode.Read));
                var plain_text = streamReader.ReadToEnd();
                return plain_text;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Could not decrypt '{base64Text}'.");
                return base64Text;
            }
        }
    }
}
