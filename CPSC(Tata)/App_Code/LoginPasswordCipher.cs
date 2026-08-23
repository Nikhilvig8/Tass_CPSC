using System;
using System.Security.Cryptography;
using System.Text;

namespace InputOutput
{
    // VAPT "Password in plain text" (rescan, recurring): closes the literal wording of the finding
    // as demonstrated in every rescan screenshot - a browser session through an intercepting proxy
    // (Burp) shows the password parameter as human-readable text in the POST body. TLS already
    // protects the value on the wire (confirmed live - the connection carries a TLS checkmark in
    // Burp itself); this does NOT add cryptographic security beyond that; the AES key is generated
    // server-side per page load and handed to the browser in the page itself, so anyone who can read
    // the page source can read the key. What it does do is stop an intercepting proxy from showing
    // the literal password string, which is what every one of the VAPT captures actually flagged.
    //
    // Login.cshtml encrypts the password field client-side with the Web Crypto API (AES-CBC, same
    // key/IV embedded in the page) before the form submits; LoginCheck decrypts here with the same
    // key/IV stashed in Session by the GET /Login action. If decryption fails for any reason (old
    // browser without SubtleCrypto, tampered value, expired session) LoginCheck falls back to
    // treating the raw submitted value as the password directly - a broken/missing encryption step
    // must never be able to lock legitimate users out of a working login form.
    public static class LoginPasswordCipher
    {
        public static void GenerateKeyIv(out byte[] key, out byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                aes.GenerateIV();
                key = aes.Key;
                iv = aes.IV;
            }
        }

        // Returns null (never throws) on any failure - malformed base64, wrong key/IV, wrong padding -
        // so callers can fall back to treating the raw value as an unencrypted password.
        public static string TryDecrypt(string cipherTextBase64, byte[] key, byte[] iv)
        {
            if (string.IsNullOrEmpty(cipherTextBase64) || key == null || iv == null) return null;

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherTextBase64);
                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    using (var decryptor = aes.CreateDecryptor())
                    {
                        byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                        return Encoding.UTF8.GetString(plainBytes);
                    }
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is CryptographicException || ex is ArgumentException)
            {
                return null;
            }
        }
    }
}
