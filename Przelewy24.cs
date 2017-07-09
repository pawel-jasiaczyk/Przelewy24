// Author: Paweł Jasiaczyk

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net.Http;
using System.ComponentModel.DataAnnotations;

namespace Przelewy24
{
    public class Przelewy24
    {

        #region Static Fields

        [Obsolete]
        private static int numberOfINstances = 0;
        [Obsolete]
        public int InstanceNumber { get; private set; }

        private static string protocol = "https://";
        private static string sandbox = "sandbox";
        private static string secure = "secure";

        private static string testConnection = ".przelewy24.pl/testConnection";
        private static string trnRegister = ".przelewy24.pl/trnRegister";
        private static string trnRequest = ".przelewy24.pl/trnRequest";
        private static string trnVerify = ".przelewy24.pl/trnVerify";
        private static string trnDirect = ".przelewy24.pl/trnDirect";

        #endregion


        #region Properties

        /// <summary>
        /// Merchant Id - reqired
        /// It will be send in every transaction registration as p24_merchant_id
        /// This is a value of merchant's account identificator
        /// It should have same value as PosId
        /// </summary>
        [Required(ErrorMessage="MerchantId is nessessary")]
        public int MerchantId { get; set; }
        /// <summary>
        /// Pos Id - required
        /// It will be send in every transaction registration as p24_pos_id
        /// This is a value of merchant's account identificator
        /// It should have same value as Merchant Id 
        /// </summary>
        [Required(ErrorMessage="PosId is nessessary")]
        public int PosId { get; set; }
        /// <summary>
        /// Crc key
        /// It will be used for calculation of p24_sign in every
        /// transaction registration and transaction confirmation operation
        /// It can be get from Przelewy24 transaction pannel from "MyData" tab.
        /// </summary>
        [Required(ErrorMessage="CrcKey is nessessary")]
        public string CrcKey { get; set; }
        /// <summary>
        /// Determines if class will work with sandbox(true) or production(false) account
        /// </summary>
        public bool SandboxMode { get; set; }
        /// <summary>
        /// Describes an address of action for transaction verification.
        /// You should set here an address of where you will get verification data from Przelewy24
        /// and run VerifyTransaction method from this class with received parameters.
        /// </summary>
        public string P24_url_status { get; set; }

        // TODO
        // Przenieść ten badziew do klienta - do vievmodel
        // i pogadać o tym z Dawidem
        /// <summary>
        /// Determines, if client have to redirect automatically to registered transaction
        /// </summary>
        public bool AutomaticRedirection { get; set; }


        /// <summary>
        /// Transaction Register Url for current class mode
        /// It is used for register transaction and get TOKEN
        /// </summary>
        public string UrlTrnRegister { get { return GetFirstPartOfUrl() + trnRegister; } }
        /// <summary>
        /// Trasaction Request Ulr for current class mode
        /// It is used for redirect customer for transaction specified by TOKEN
        /// </summary>
        public string UrlTrnRequest { get { return GetFirstPartOfUrl() + trnRequest; } }
        /// <summary>
        /// Test connection Url
        /// It is used for verify, if PosId and CrcKey are correct for current class mode
        /// </summary>
        public string UrlTestConnection { get { return GetFirstPartOfUrl() + testConnection; } }
        /// <summary>
        /// Transacion verification Url.
        /// It is used for verification of transaction
        /// </summary>
        public string UrlTrnVerify  { get { return GetFirstPartOfUrl() + trnVerify; } }
        public string UrlDirect { get { return GetFirstPartOfUrl() + trnDirect; } }

        public IP24Db TransactionDb { get; set; }

        #endregion


        #region Constructors
        
        public Przelewy24()
        {
            this.MerchantId = 0;
            this.PosId = 0;
            this.CrcKey = "";
            this.SandboxMode = false;

            numberOfINstances++;
            this.InstanceNumber = numberOfINstances;
        }

        /// <summary>
        /// Create instance of Przelewy24 class
        /// This constructor was created in early version of project and will be removed
        /// in final version
        /// </summary>
        /// <param name="merchantId"></param>
        /// <param name="posId"></param>
        /// <param name="crcKey"></param>
        /// <param name="sandboxMode"></param>
        [Obsolete]
        public Przelewy24(string merchantId, string posId, string crcKey, bool sandboxMode)
            :this(int.Parse(merchantId), int.Parse(posId), crcKey, sandboxMode)
        { }

        
        /// <summary>
        /// Create instance of Przelewy24 class
        /// This constructor was created in early version of project and will be removed
        /// in final version
        /// </summary>
        /// <param name="merchantId"></param>
        /// <param name="posId"></param>
        /// <param name="crcKey"></param>
        [Obsolete]
        public Przelewy24(string merchantId, string posId, string crcKey)
            :this(merchantId, posId, crcKey, false)
        { }
       

        public Przelewy24(int merchantId, int posId, string crcKey)
            :this (merchantId, posId, crcKey, false)
        { }


        public Przelewy24(int merchantId, int posId, string crcKey, bool sandboxMode)
        {
            this.MerchantId = merchantId;
            this.PosId = posId;
            this.CrcKey = crcKey;
            this.SandboxMode = sandboxMode;
        }

        #endregion


        #region Public Methods

        #region TestConnection

        public async Task<string> TestConnection()
        {
            HttpClient client = new HttpClient();
            string[] parameters = new string[2] { this.PosId.ToString(), this.CrcKey };
            string sign = Przelewy24.CalculateSign(parameters);

            var values = new Dictionary<string, string>()
            {
                { "p24_merchant_id", this.MerchantId.ToString() },
                { "p24_pos_id", this.PosId.ToString() },
                { "p24_sign", sign }
            };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync(this.UrlTestConnection, content);

            var responseString = await response.Content.ReadAsStringAsync ();

            return responseString;
        }


        #endregion


        #region Verify Transaction

        public async Task<bool> VerifyTransaction(
            string p24_merchant_id, 
            string p24_pos_id, 
            string p24_session_id, 
            string p24_amount,
            string p24_currency,
            string p24_order_id,
            string p24_method,
            string p24_statent,
            string p24_sign)
        {
            Transaction tr = this.TransactionDb.GetTransaction(p24_session_id);
            string[] forSign = new string[] 
            {
                p24_session_id,
                p24_order_id,
                tr.P24_amount.ToString(),
                tr.P24_currency,
                this.CrcKey
            };
            string sign = CalculateSign(forSign);
            if(sign == p24_sign)
            {
                HttpClient client = new HttpClient();

                var values = new Dictionary<string, string>()
                {
                    { "p24_merchant_id", this.MerchantId.ToString() },
                    { "p24_pos_id", this.PosId.ToString() },
                    { "p24_session_id", tr.P24_session_id },
                    { "p24_amount" , tr.P24_amount.ToString() },
                    { "p24_currency", tr.P24_currency },
                    { "p24_order_id", p24_order_id },
                    { "p24_sign", sign }
                };
                
                var content = new FormUrlEncodedContent(values);

                var response = await client.PostAsync(this.UrlTrnVerify, content);

                string responseString = await response.Content.ReadAsStringAsync();

                if(responseString.ToLower().Equals("error=0"))
                    return true;
            }
            return false;
        }


        #endregion


        #region Register Transaction

        /// <summary>
        /// Register transaction with mode set in this Przelewy24 object
        /// It uses only parameters given in input IDictionary
        /// Do not check if Merchant ID and POS ID are the same as in this Przelewy24 object
        /// This method do not create Transaction Object
        /// </summary>
        /// <param name="parametares">Set of parameters</param>
        /// <returns>Response from Przelewy24. String contains TOKEN or list of errors</returns>
        public async Task<string> RegisterTransaction(IDictionary<string, string> parametares)
        {
            HttpClient client = new HttpClient();
            var content = new FormUrlEncodedContent(parametares);
            var response = await client.PostAsync(this.UrlTrnRegister, content);
            string responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }


        /// <summary>
        /// Register transaction with mode set in this Przelewy24 object
        /// It uses only parameters given in input IEnumerable parameters
        /// Do not check if Merchant ID and POS ID are the same as in this Przelewy24 object
        /// This method do not create transaction object
        /// </summary>
        /// <param name="parameters">Set of parameters</param>
        /// <returns>Response from Przelewy24. String contains TOKEN or list of errors</returns>
        public async Task<string> RegisterTransaction(IEnumerable<IParameter> parameters)
        {
            Dictionary<string, string> DParams = new Dictionary<string, string>();
            foreach (IParameter param in parameters)
            {
                DParams.Add(param.Name, param.StringValue);
            }
            return await RegisterTransaction(DParams);
        }


        /// <summary>
        /// Register specified Transaction in Przelewy24 for mode specified in this Przelewy24 object
        /// This method uses only parameters from give transaction
        /// It does not chec if Merchant ID nad POS ID are the same as in this Przelewy24 object
        /// </summary>
        /// <param name="transaction">Transaction to register</param>
        /// <returns>P24Response object with data about Token or Errors</returns>
        public async Task<P24Response> RegisterTransaction(Transaction transaction)
        {
            transaction.SetRegisterSign();
            string responseString =  await RegisterTransaction(transaction.GetParameters());
            P24Response response = new P24Response(responseString);
            transaction.P24Response = response;
            return response;
        }


        /// <summary>
        /// Register specified Transaction in Przelewy24 for mode specified in this Przelewy24 object
        /// This method uses only parameters from give transaction
        /// It does not chec if Merchant ID nad POS ID are the same as in this Przelewy24 object
        /// Allow to save transaction to database specified in TransactionDb property
        /// </summary>
        /// <param name="transaction">Transaction to register</param>
        /// <param name="saveToDatabase">If sets, transaction will be saved in TransactionDb</param>
        /// <param name="saveOnlyCorrecteTransaction">If sets, only Correct transactions will be saved - will ignore transaction with errors</param>
        /// <returns>P24Response object with data about Token or Errors</returns>
        public async Task<P24Response> RegisterTransaction(Transaction transaction, bool saveToDatabase, bool saveOnlyCorrecteTransaction)
        {
            P24Response response = await RegisterTransaction(transaction);
            if (saveToDatabase)
            {
                if (response.OK || !saveOnlyCorrecteTransaction)
                {
                   try
                    {
                        this.TransactionDb.SaveTransaction(transaction);
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
            return response;
        }


        #endregion


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


        #region Register Sign

        public static string CalculateRegisterSign
                   (string sessionId, string merchantId, string amount, string currency, string crcKey)
        {
            string[] data = new string[5] { sessionId, merchantId, amount, currency, crcKey };
            return CalculateSign(data);
        }


        public static string CalculateRegisterSign
                   (string sessionId, int merchantId, int amount, string currency, string crcKey)
        {
            return CalculateRegisterSign(sessionId, merchantId.ToString(), amount.ToString(), currency, crcKey);
        }


        #endregion

        #endregion


        #endregion
    }
}
