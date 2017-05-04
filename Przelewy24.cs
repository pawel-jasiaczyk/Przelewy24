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

        public static string CalculateSign (string[] inputFields, bool ignonreNulls)
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

        public static string CalculateSign(string[] inputFields)
        {
            return CalculateSign(inputFields, false);
        }

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

            return stb.ToString ();
        }
    }
}
