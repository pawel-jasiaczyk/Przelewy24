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
        private Przelewy24 parent;

        public static ushort TransactionNumber = 1;
        
        public string MerchantId { get; set; }
        public string PosId { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string SessionId { get; set; }
        public string CrcKey { get; set; }
        public string Country { get; set; }
        public string UrlReturn { get; set; }
        public string ApiVersion { get { return "3.2"; } }
        public string Description { get; set; }
        public string Email { get; set; }

        public string SessionIdAdditionalData { get; set; }
        public ushort ThisTransactionNumber { get; set; }

        private List<Parameter> parameters;

        public Transaction(string id, string amount, string currency, string crcKey, string country, string urlReturn, string email)
            :this(id,amount,currency,"",crcKey, country, urlReturn, email)
        {

        }

        public Transaction(Przelewy24 parent, string amount, string currency, string country, string urlReturn, string email, string sessionId)
        {

        }

        public Transaction
            (string id, string amount, string currency, string sessionId, string crcKey, string country, string urlReturn, string email)
            : this()
        {
            this.MerchantId = id;
            this.PosId = id;
            this.Amount = amount;
            this.Currency = currency;
            this.SessionId = sessionId;
            this.CrcKey = crcKey;
            this.Email = email;
            this.UrlReturn = urlReturn;
            this.ThisTransactionNumber = Transaction.TransactionNumber;
            TransactionNumber++;
        }

        private Transaction()
        {
            this.parameters = new List<Parameter> ();
        }

        public async Task<string> RegisterTransaction()
        {
            HttpClient client = new HttpClient();
            string sign = 
                Przelewy24.CalculateRegisterSign 
                (this.SessionId, this.MerchantId, this.Amount, this.Currency, this.CrcKey);
            var values = new Dictionary<string, string> ()
            {
                {"p24_merchant_id", this.MerchantId },
                {"p24_pos_id", this.PosId },
                {"p24_session_id", this.SessionId },
                {"p24_amount", this.Amount },
                {"p24_currency", this.Currency },
                {"p24_description", this.Description },
                {"p24_email", this.Email },
                {"p24_country", this.Country },
                {"p24_url_return", this.UrlReturn },
                {"p24_api_version", this.ApiVersion },
                {"p24_sign", sign}
            };
            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync (parent.UrlTrnRegister, content);

            string responseString = await response.Content.ReadAsStringAsync ();

            return responseString;
        }

        public void SetUniqueSessionId(SessionIdGenerationMode mode)
        {
            StringBuilder stb = new StringBuilder ();
            switch(mode)
            {
                case SessionIdGenerationMode.random:
                {
                    stb.Append (DateTime.Now.Ticks.ToString ());
                    stb.Append ('_');
                    stb.Append (this.ThisTransactionNumber);
                    this.SessionId = stb.ToString ();
                    break;
                }
            }
        }

        public void SetUniqueSessionId(SessionIdGenerationMode mode, string sessionIdAdditionalData)
        {
            this.SessionIdAdditionalData = sessionIdAdditionalData;
        }
    }
}
