using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net.Http;

namespace Przelewy24
{
    public class Przelewy24
    {
        #region Static Fields

        private static string protocol = "https://";
        private static string sandbox = "sandbox";
        private static string secure = "secure";

        private static string testConnection = ".przelewy24.pl/testConnection";
        private static string trnRegister = ".przelewy24.pl/trnRegister";
        private static string trnRequest = ".przelewy24.pl/trnRequest";

        #endregion

        #region Constructors
        
        protected Przelewy24()
        {

        }


        public Przelewy24(string merchantId, string posId, string crcKey, bool sandboxMode)
        {
            this.MerchantId = merchantId;
            this.PosId = posId;
            this.CrcKey = crcKey;
            this.SandboxMode = sandboxMode;
        }


        public Przelewy24(string merchantId, string posId, string crcKey)
            :this(merchantId, posId, crcKey, false)
        { }
       

        public Przelewy24(int merchantId, int posId, string crcKey)
            :this (merchantId.ToString(), posId.ToString(), crcKey)
        { }

        public Przelewy24(int merchantId, int posId, string crcKey, bool sandboxMode)
            :this (merchantId.ToString(), posId.ToString(), crcKey, sandboxMode)
        { }

        #endregion


        #region Properties

        public string MerchantId { get; set; }
        public string PosId { get; set; }
        public string CrcKey { get; set; }

        public bool SandboxMode { get; set; }

        public string UrlTrnRegister { get { return GetFirstPartOfUrl() + trnRegister; } }
        public string UrlTrnRequest { get { return GetFirstPartOfUrl() + trnRequest; } }
        public string UrlTestConnection { get { return GetFirstPartOfUrl() + testConnection; } }

        #endregion

        #region Public Methods

        public async Task<string> TestConnection()
        {
            HttpClient client = new HttpClient();
            string[] parameters = new string[2] { this.PosId, this.CrcKey };
            string sign = Przelewy24.CalculateSign(parameters);

            var values = new Dictionary<string, string>()
            {
                { "p24_merchant_id", this.MerchantId },
                { "p24_pos_id", this.PosId },
                { "p24_sign", sign }
            };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync(this.UrlTestConnection, content);

            var responseString = await response.Content.ReadAsStringAsync ();

            return responseString;
        }

        #endregion

        #region Private Methods

        private string GetFirstPartOfUrl()
        {
            if (this.SandboxMode) return protocol + sandbox;
            else return protocol + secure;
        }

        #endregion

        #region Static Methods

        #region Sign Methods

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

        #endregion

        #endregion
    }
}
