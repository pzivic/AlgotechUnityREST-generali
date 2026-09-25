using System;
using System.Security.Cryptography;

namespace AlgotechUnityREST_ckb.Shared
{
    /// <summary>
    /// 
    /// </summary>
    public static class Util
    {
        #region GetUniqueFileName
        /// <summary>Obezbeđuje jedinstveni naziv datoteke.</summary>
        /// <param name="fileNameContext">Vrsta datoteke.</param>
        /// <returns>Vraća naziv daoteke</returns>

        public static string GetUniqueFileName(string fileNameContext)
        {
            return fileNameContext + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + Guid.NewGuid();
        }
        #endregion

        #region GetFileNameExtension
        public static string GetFileNameExtension(string fileName)
        {
            var files = fileName.Split('.');
            return files[files.Length - 1];
        }
        #endregion

        #region Left
        public static string Left(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            maxLength = Math.Abs(maxLength);

            return (value.Length <= maxLength
                   ? value
                   : value.Substring(0, maxLength)
                   );
        }
        #endregion

        #region ToBanka
        public static string ToBanka(this string value)
        {
            return value.Substring(0, 3);
        }
        #endregion

        #region ToPartija
        public static string ToPartija(this string value)
        {
            return value.Substring(4, 13);
        }
        #endregion

        #region ToKontrolniBroj
        public static string ToKontrolniBroj(this string value)
        {
            return value.Substring(18, 2);
        }
        #endregion

        #region FormatBrojRacuna
        public static string FormatBrojRacuna(this string brojRacuna)
        {
            return brojRacuna.Substring(0, 3) + "-" + brojRacuna.Substring(3, 13) + "-" + brojRacuna.Substring(16, 2);
        }
        #endregion

        #region KillChars
        public static string KillChars(string strWords)
        {

            var badChars = new string[32];
            int i;

            badChars[0] = "select";
            badChars[1] = "insert";
            badChars[2] = "delete";
            badChars[3] = "drop";
            badChars[4] = "create";
            badChars[5] = "rename";
            badChars[6] = "replace";
            badChars[7] = ";";
            badChars[8] = ";--";
            badChars[9] = "--";
            badChars[10] = "xp_";
            badChars[11] = "@";
            badChars[12] = "@@";
            badChars[13] = @"\";
            badChars[14] = "alter";
            badChars[15] = "script";
            badChars[16] = "/*";
            badChars[17] = "*/";
            badChars[18] = "cast";
            badChars[19] = "cursor";
            badChars[20] = "declare";
            badChars[21] = "exec";
            badChars[22] = "execute";
            badChars[23] = "fetch";
            badChars[24] = "kill";
            badChars[25] = "open";
            badChars[26] = "sys";
            badChars[27] = "sysobjects";
            badChars[28] = "syscolumns";
            badChars[29] = "table";
            badChars[30] = "<";
            badChars[31] = ">";

            var newChars = strWords;

            for (i = 0; i <= 29; i++)
            {
                newChars = newChars.Replace(badChars[i], "");
            }
            return newChars;
        }
        #endregion

        #region KillCharsSaveAt
        public static string KillCharsSaveAt(string strWords)
        {

            var badChars = new string[29];
            int i;

            badChars[0] = "select";
            badChars[1] = "insert";
            badChars[2] = "delete";
            badChars[3] = "drop";
            badChars[4] = "create";
            badChars[5] = "rename";
            badChars[6] = "replace";
            badChars[7] = ";";
            badChars[8] = ";--";
            badChars[9] = "--";
            badChars[10] = "xp_";
            badChars[11] = "@@";
            badChars[12] = @"\";
            badChars[13] = "alter";
            badChars[14] = "script";
            badChars[15] = "/*";
            badChars[16] = "*/";
            badChars[17] = "cast";
            badChars[18] = "cursor";
            badChars[19] = "declare";
            badChars[20] = "exec";
            badChars[21] = "execute";
            badChars[22] = "fetch";
            badChars[23] = "kill";
            badChars[24] = "open";
            badChars[25] = "sys";
            badChars[26] = "sysobjects";
            badChars[27] = "syscolumns";
            badChars[28] = "table";

            var newChars = strWords;

            for (i = 0; i <= 28; i++)
            {
                if (i != 24 && !newChars.Contains("@open.telekom"))
                    newChars = newChars.Replace(badChars[i], "");
            }
            return newChars;
        }
        #endregion

        #region SafeSQL
        public static string SafeSQL(this string var)
        {
            string ret;
            if (var == null)
            {
                ret = string.Empty;
            }
            else
            {
                var = var.Replace("'", "''");

                const string quote = "\"";
                ret = var.Replace(quote, string.Empty);

                ret = KillChars(ret);
            }
            return ret;
        }
        #endregion

        #region SafeSQLSaveAt
        public static string SafeSQLSaveAt(this string var)
        {
            string ret;
            if (var == null)
            {
                ret = string.Empty;
            }
            else
            {
                ret = var.Replace("'", "''");
                ret = KillCharsSaveAt(ret);
            }
            return ret;
        }
        #endregion

        /// <summary>
        /// Generisanje klijentskog tajnog ključa. Primer 5p7X7n9d1d8mV1Tq9LJmQJ7m7cVdD2xY2t3Xg2w8nLo=
        /// </summary>
        /// <returns></returns>
        public static string GenerateClientSecret()
        {
            byte[] Buffer = new byte[32];

            using (var Rng = RandomNumberGenerator.Create())
            {
                Rng.GetBytes(Buffer);
            }

            return Convert.ToBase64String(Buffer);
        }

        /// <summary>
        /// Heširanje klijentskog tajnog ključa. Primer 100000.P2xfrh9Es63grzk+fLueug==.pEHfLjUXJXl69mFxPJ3DqEIMwKtn8FyRVzdtAzTfNCQ=
        /// </summary>
        /// <param name="clientSecret"></param>
        /// <returns></returns>
        public static string HashClientSecret(string clientSecret)
        {
            byte[] salt = new byte[16];

            using (var Rng = RandomNumberGenerator.Create())
            {
                Rng.GetBytes(salt);
            }

            const int Iterations = 100000;

            using (var Pbkdf2 = new Rfc2898DeriveBytes(
                clientSecret,
                salt,
                Iterations,
                HashAlgorithmName.SHA256))
            {
                byte[] hash = Pbkdf2.GetBytes(32);

                return string.Format(
                    "{0}.{1}.{2}",
                    Iterations,
                    Convert.ToBase64String(salt),
                    Convert.ToBase64String(hash));
            }
        }

        public static bool VerifyClientSecret(string clientSecret, string storedHash)
        {
            string[] parts = storedHash.Split('.');

            if (parts.Length != 3)
                return false;

            int Iterations = int.Parse(parts[0]);

            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] expectedHash = Convert.FromBase64String(parts[2]);

            using (var Pbkdf2 = new Rfc2898DeriveBytes(
                clientSecret,
                salt,
                Iterations,
                HashAlgorithmName.SHA256))
            {
                byte[] ActualHash = Pbkdf2.GetBytes(expectedHash.Length);

                return FixedTimeEquals(
                    ActualHash,
                    expectedHash);
            }
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null)
                return false;

            if (a.Length != b.Length)
                return false;

            int Difference = 0;

            for (int i = 0; i < a.Length; i++)
            {
                Difference |= a[i] ^ b[i];
            }

            return Difference == 0;
        }
    }


}