using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace Przelewy24
{
    public class Transaction
    {
        #region Static Fields

        public static ushort TransactionNumber = 1;

        #endregion


        #region Private fields

        private Przelewy24 parent;
        private List<Parameter> parameters;

        #endregion


        #region Properties
        
        //
        // Transaction data
        //
        // Basic parameters
        public string P24_session_id
        {
            get { return GetParameter ("p24_session_id"); }
            set { SetParameter ("p24_session_id", value); }
        }
        public string P24_amount 
        {
            get { return GetParameter ("p24_amount"); }
            set { SetParameter ("p24_amount", value); } 
        }
        public string P24_currency 
        {
            get { return GetParameter ("p24_currency"); }
            set { SetParameter ("p24_currency", value); }
        }
        public string P24_description 
        {
            get { return GetParameter ("p24_description"); }
            set { SetParameter ("p24_description", value); } 
        }
        public string P24_email 
        {
            get { return GetParameter ("p24_email"); }
            set { SetParameter ("p24_email", value); }
        }
        public string P24_country 
        {
            get { return GetParameter ("p24_country"); }
            set { SetParameter ("p24_country", value); }
        }
        public string P24_url_return 
        {
            get { return GetParameter ("p24_url_return"); }
            set { SetParameter ("p24_url_return", value); }
        }
        public string P24_api_version 
        { 
            get { return GetParameter("p24_api_version"); } 
        }

        // Parameters requied for credit card transactions
        public string P24_client 
        {
            get { return GetParameter ("p24_client"); }
            set { SetParameter ("p24_client", value); }
        }
        public string P24_address 
        {
            get { return GetParameter ("p24_address"); }
            set { SetParameter ("p24_address", value); }
        }
        public string P24_zip 
        {
            get { return GetParameter ("p24_zip"); }
            set { SetParameter ("p24_zip", value); }
        }
        public string P24_city 
        {
            get { return GetParameter ("p24_city"); }
            set { SetParameter ("p24_city", value); } 
        }

        // Non-basic parameters


        //
        // Additional data
        //
        // Additional merchant data
        public string SessionIdAdditionalData { get; set; }
        public ushort ThisTransactionNumber { get; set; }

        // Confirmation data
        public string ShortOrderId { get; private set;}
        public string FullOrderId { get; private set; }

        // Generated data
        public string RegisterSign 
        { 
            get 
            {
                return Przelewy24.CalculateRegisterSign (
                    this.P24_session_id, 
                    parent.MerchantId, 
                    this.P24_amount, 
                    this.P24_currency, 
                    parent.CrcKey
                 );
            } 
        }

        #endregion


        #region Constructors

        private Transaction()
        {
            this.parameters = new List<Parameter> ();
            this.SetParameter ("p24_api_version", "3.2");
        }

        public Transaction (
            // merchant account data
            Przelewy24 parent, 
            // transaction data
            SessionIdGenerationMode generationMode,
            string sessionId,
            string amount, 
            string currency, 
            string description,
            string email, 
            string country, 
            string urlReturn 
            )
            :this()
        {
            this.parent = parent;

            SetUniqueSessionId (generationMode, sessionId);
            this.P24_amount = amount;
            this.P24_currency = currency;
            this.P24_description = description;
            this.P24_email = email;
            this.P24_country = country;
            this.P24_url_return = urlReturn;
        }

        public Transaction (
            // merchant data
            string merchantId,
            string posId,
            string crcKey,
            bool sandboxMode,
            // transaction data
            SessionIdGenerationMode generationMode,
            string sessionId,
            string amount, 
            string currency, 
            string description,
            string email, 
            string country, 
            string urlReturn 
            )
            :this(
                new Przelewy24(merchantId, posId, crcKey, sandboxMode),
                generationMode, sessionId,amount,currency,description,email,country,urlReturn
            )
        { }

        

        #endregion


        #region Server connection methods

        public async Task<string> RegisterTransaction()
        {
            HttpClient client = new HttpClient();
            string sign = 
                Przelewy24.CalculateRegisterSign 
                (this.P24_session_id, parent.PosId, this.P24_amount, this.P24_currency, parent.CrcKey);
            var values = new Dictionary<string, string> ()
            {
                {"p24_merchant_id", parent.MerchantId },
                {"p24_pos_id", parent.PosId },
                {"p24_sign", sign}
            };

            foreach(Parameter param in this.parameters)
            {
                values.Add (param.Name, param.Value);
            }

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync (parent.UrlTrnRegister, content);

            string responseString = await response.Content.ReadAsStringAsync ();

            return responseString;
        }

        #endregion


        #region Session Id methods

        private string GetUniqueSessionId()
        {
            StringBuilder stb = new StringBuilder ();
            stb.Append (DateTime.Now.Ticks.ToString ());
            stb.Append ('|');
            stb.Append (this.ThisTransactionNumber);
            return stb.ToString ();
        }

        public void SetUniqueSessionId(SessionIdGenerationMode mode, string sessionIdAdditionalData)
        {
            this.SessionIdAdditionalData = sessionIdAdditionalData;
            switch(mode)
            {
                case SessionIdGenerationMode.time:
                {
                    this.P24_session_id = GetUniqueSessionId ();
                    break;
                }
                case SessionIdGenerationMode.addPostfix:
                {
                    this.P24_session_id = GetUniqueSessionId () + "|" + sessionIdAdditionalData;
                    break;
                }
                case SessionIdGenerationMode.addPrefix:
                {
                    this.P24_session_id = sessionIdAdditionalData + "|" + GetUniqueSessionId();
                    break;
                }
                case SessionIdGenerationMode.md5:
                {
                    this.P24_session_id = Przelewy24.CalculateMD5Hash (GetUniqueSessionId ());
                    break;
                }
                case SessionIdGenerationMode.plain:
                {
                    this.P24_session_id = sessionIdAdditionalData;
                    break;
                }
            }
        }

        #endregion
        

        #region Parameters methods

        public string GetParameter(string parameterName)
        {
            var result = this.parameters.Select (n => n).Where (n => n.Name == parameterName).FirstOrDefault ();
            if (result != null)
                return result.Value;
            else return null;
        }


        public void SetParameter(string parameterName, string parameterValue)
        {
            var result = this.parameters.Select (n => n).Where (n => n.Name == parameterName).FirstOrDefault ();
            if (result != null)
                result.Value = parameterValue;
            else
                this.parameters.Add (new Parameter (parameterName, parameterValue));
        }


        public void RemoveParameter (string parameterName)
        {
            var result = this.parameters.Select (n => n).Where (n => n.Name == parameterName).FirstOrDefault ();
            if (result != null)
                this.parameters.Remove (result);
        }
        
        #endregion
    }
}
