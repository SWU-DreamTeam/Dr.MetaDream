using System;
using System.Text;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Engines;
using UnityEngine;

public static class AESGCMEncryption
{
    private const int NonceSize = 12; // AES-GCM Nonce 크기
    private const int TagSize = 16; // AES-GCM 인증 태그 크기

    public static string Encrypt(string plaintext, string password)
    {
        // 비밀번호로 AES 키 생성
        byte[] key = DeriveKeyFromPassword(password, 32); // 256-bit 키
        byte[] nonce = GenerateRandomNonce();

        // 평문을 바이트 배열로 변환
        byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

        // AES-GCM 암호화 수행
        var cipher = new GcmBlockCipher(new AesEngine());
        var parameters = new AeadParameters(new KeyParameter(key), TagSize * 8, nonce);

        cipher.Init(true, parameters); // true: 암호화 모드
        byte[] ciphertext = new byte[cipher.GetOutputSize(plaintextBytes.Length)];
        int len = cipher.ProcessBytes(plaintextBytes, 0, plaintextBytes.Length, ciphertext, 0);
        cipher.DoFinal(ciphertext, len);

        // 결과: Nonce + Ciphertext
        byte[] result = new byte[nonce.Length + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(ciphertext, 0, result, nonce.Length, ciphertext.Length);

        return Convert.ToBase64String(result);
    }

    public static string Decrypt(string encryptedData, string password)
    {
        byte[] encryptedBytes = Convert.FromBase64String(encryptedData);

        // Nonce와 Ciphertext 분리
        byte[] nonce = new byte[NonceSize];
        byte[] ciphertext = new byte[encryptedBytes.Length - NonceSize];

        Buffer.BlockCopy(encryptedBytes, 0, nonce, 0, NonceSize);
        Buffer.BlockCopy(encryptedBytes, NonceSize, ciphertext, 0, ciphertext.Length);

        // 비밀번호로 AES 키 생성
        byte[] key = DeriveKeyFromPassword(password, 32);

        // AES-GCM 복호화 수행
        var cipher = new GcmBlockCipher(new AesEngine());
        var parameters = new AeadParameters(new KeyParameter(key), TagSize * 8, nonce);

        cipher.Init(false, parameters); // false: 복호화 모드
        byte[] plaintextBytes = new byte[cipher.GetOutputSize(ciphertext.Length)];
        int len = cipher.ProcessBytes(ciphertext, 0, ciphertext.Length, plaintextBytes, 0);
        cipher.DoFinal(plaintextBytes, len);

        return Encoding.UTF8.GetString(plaintextBytes).TrimEnd('\0');
    }

    private static byte[] DeriveKeyFromPassword(string password, int keySize)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] hash = sha256.ComputeHash(passwordBytes);

            // 키 크기를 초과하지 않도록 자릅니다.
            return hash.Length >= keySize ? hash[..keySize] : hash;
        }
    }

    private static byte[] GenerateRandomNonce()
    {
        byte[] nonce = new byte[NonceSize];
        using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
        {
            rng.GetBytes(nonce);
        }
        return nonce;
    }
}
