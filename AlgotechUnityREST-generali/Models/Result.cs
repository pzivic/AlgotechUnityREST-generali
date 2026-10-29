// Predrag Živić
// 18.09.2022

namespace AlgotechUnityREST_ckb.Models
{
    /// <summary>
    /// Rezultat izvršavanja metode
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Povratna vrednost
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Poruka
        /// </summary>
        public string Message { get; set;}

        /// <summary>
        /// Konstruktor
        /// </summary>
        public Result()
        {
            Status = string.Empty;
            Message = string.Empty;
        }
    }
}