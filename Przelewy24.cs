using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Przelewy24
{
    public class Przelewy24
    {
        public static string TrnRegister = "https://sandbox.przelewy24.pl/trnRegister";

        protected static string CalculateSign (string[] inputFields, bool ignonreNulls)
        {
            StringBuilder stb = new StringBuilder ();

            for (int i = 0; i < inputFields.Length; i++)
            {
                if (inputFields[i] == null)
                    if (ignonreNulls)
                        continue;
                    else
                        throw new NullReferenceException("Parameter inputFields position: " + i);
                stb.Append (inputFields[i]);
                if (i < inputFields.Length - 1)
                    stb.Append ("|");
            }

            return CalculateMD5Hash (stb.ToString ());
        }

        /// <summary>
        /// Calculate sign in p24 style.
        /// I.E. create string from input fields separated by '|' character
        /// and calculate MD5 from this string.
        /// Null fields are not allowed
        /// Epmty strgins are allowed.
        /// </summary>
        /// <param name="inputFields">Parameters used to calculate sign</param>
        /// <returns>P24_sign or crc</returns>
        /// <exception>NullReferenceException</exception>
        public static string CalculateSign(string[] inputFields)
        {
            return CalculateSign(inputFields, false);
        }

        public static string CalculateRegisterSign(string sessionId, string merchantId, string amount, string currency, string crcKey)
        {
            string[] data = new string[5] { sessionId, merchantId, amount, currency, crcKey };
            return CalculateSign (data);
        }

        /// <summary>
        /// Calclate MD5 hash from input string.
        /// </summary>
        /// <param name="input">String to calculate</param>
        /// <returns></returns>
        public static string CalculateMD5Hash (string input)
        {
            MD5 md5 = MD5.Create ();
            byte[] inputBytes = Encoding.ASCII.GetBytes (input);

            byte[] hash = md5.ComputeHash (inputBytes);

            StringBuilder stb = new StringBuilder ();
            for (int i = 0; i < hash.Length; i++)
            {
                stb.Append (hash[i].ToString ("X2"));
            }

            return stb.ToString ().ToLower();
        }
    }
}
