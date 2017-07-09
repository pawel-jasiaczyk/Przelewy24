// Author: Paweł Jasiaczyk

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.ComponentModel.DataAnnotations;

namespace Przelewy24
{
    public class Transaction
    {
        #region Static Fields

        public static ushort TransactionsNumber = 0;

        #endregion


        #region Private fields

        private Przelewy24 parent;
        private List<IParameter> parameters;

        #endregion

        //
        // Merchant data
        //
        #region Properties
        public int P24_merchant_id
        {
            get { return this.GetParameter<int>("p24_merchant_id"); }
            private set { this.SetParameter<int>("p24_merchant_id", value); }
        }
        public int P24_pos_id
        {
            get { return this.GetParameter<int>("p24_pos_id"); }
            private set { this.SetParameter<int>("p24_pos_id", value); }
        }

        //
        // Transaction data
        //
        // Basic parameters
        [Required(ErrorMessage="SesssionId is nessessary")]
        public string P24_session_id
        {
            get { return GetParameter<string> ("p24_session_id"); }
            set { SetParameter<string> ("p24_session_id", value); }
        }
        [Required(ErrorMessage="Amount is nessessary")]
        public int P24_amount 
        {
            get { return GetParameter<int> ("p24_amount"); }
            set { SetParameter<int> ("p24_amount", value); } 
        }
        [Required(ErrorMessage="Currency is nessessary")]
        public string P24_currency 
        {
            get { return GetParameter<string> ("p24_currency"); }
            set { SetParameter<string> ("p24_currency", value); }
        }
        [Required(ErrorMessage="description is nessessary")]
        public string P24_description 
        {
            get { return GetParameter<string> ("p24_description"); }
            set { SetParameter<string> ("p24_description", value); } 
        }
        [Required(ErrorMessage="Email is nessessary")]
        public string P24_email 
        {
            get { return GetParameter<string> ("p24_email"); }
            set { SetParameter<string> ("p24_email", value); }
        }
        [Required(ErrorMessage="Country is nessessary")]
        public string P24_country 
        {
            get { return GetParameter<string> ("p24_country"); }
            set { SetParameter<string> ("p24_country", value); }
        }
        [Required(ErrorMessage="return address is nessessary")]
        public string P24_url_return 
        {
            get { return GetParameter<string> ("p24_url_return"); }
            set { SetParameter<string> ("p24_url_return", value); }
        }
        public string P24_api_version 
        { 
            get { return GetParameter<string>("p24_api_version"); } 
        }
        public string P24_sign 
        { 
            get 
            {
                return this.SetRegisterSign();
            } 
            private set { this.SetParameter("p24_sign", value); }
        }

        // Parameters requied for credit card transactions
        public string P24_client 
        {
            get { return GetParameter<string> ("p24_client"); }
            set { SetParameter<string> ("p24_client", value); }
        }
        public string P24_address 
        {
            get { return GetParameter<string> ("p24_address"); }
            set { SetParameter<string> ("p24_address", value); }
        }
        public string P24_zip
        {
            get { return GetParameter<string> ("p24_zip"); }
            set { SetParameter<string> ("p24_zip", value); }
        }
        public string P24_city 
        {
            get { return GetParameter<string> ("p24_city"); }
            set { SetParameter<string> ("p24_city", value); } 
        }

        // Non-basic parameters
        public string P24_phone
        {
            get { return this.GetParameter<string>("p24_phone"); }
            set { this.SetParameter<string>("p24_phone", value); }
        } // string(12)
        public string P24_language
        {
            get { return this.GetParameter<string>("p24_language"); }
            set { this.SetParameter<string>("p24_language", value); }
        } // string(2)
        public int P24_method
        {
            get { return this.GetParameter<int>("p24_method"); }
            set { this.SetParameter<int>("p24_method", value); }
        } // int
        public string P24_url_status
        {
            get { return this.GetParameter<string>("p24_url_status"); }
            set
            {
                if (string.IsNullOrEmpty(value))
                    RemoveParameter("p24_url_status");
                else
                    this.SetParameter<string>("p24_url_status", value);
            }
        } // string(250)   STATUS
        public int P24_time_limit
        {
            get { return this.GetParameter<int>("p24_time_limit"); }
            set { this.SetParameter<int>("p24_time_limit", value); }
        } // int
        public int P24_wait_for_result
        {
            get { return this.GetParameter<int>("p24_wait_for_result"); }
            set { this.SetParameter<int>("p24_wait_for_result", value); }
        } // int
        public int P24_channel
        {
            get { return this.GetParameter<int>("p24_channel"); }
            set { this.SetParameter<int>("p24_channel", value); }
        } // int
        public string P24_transfer_label
        {
            get { return this.GetParameter<string>("p24_transfer_label"); }
            set { this.SetParameter<string>("p24_transfer_label", value); }
        } // string(20)
        public string P24_encoding
        {
            get { return this.GetParameter<string>("p24_encoding"); }
            set { this.SetParameter<string>("p24_encoding", value); }
        } // string(15)
        
        // Basket

        // p24_shipping int KOSZYK

        // p24_name_X string(127)
        // p24_description string(127)
        // p24_quantity_X int
        // p24_price_X int
        // p24_number_X int


        //
        // Additional data
        //
        // Additional merchant data
        public string SessionIdAdditionalData { get; set; }
        public ushort ThisTransactionNumber { get; set; }

        // Confirmation data
        public int ShortOrderId { get; set;}
        public int FullOrderId { get; set; }

        // Generated data

        public Przelewy24 P24 
        { 
            get { return this.parent; }
            set { this.parent = value; }
        }
        
        // TODO
        // To jest register response. Zmienić to.
        // Zmienić całą klasę
        public P24Response P24Response { get; set; }

        // TODO
        // Umożliwić rozpoznanie statusu transakcji i odczyt różnych błędów
        // Specjalny obiekt do tego

        #endregion


        #region Constructors

        private void AllConstuctorsInitialOperations()
        {
            this.parameters = new List<IParameter> ();
            this.SetParameter ("p24_api_version", "3.2");
            TransactionsNumber++;
            this.ThisTransactionNumber = TransactionsNumber;
        }


        private void AllConstructorsFinalOperations()
        {
            if (this.parent != null)
            {
                this.P24_merchant_id = parent.MerchantId;
                this.P24_pos_id = parent.PosId;
                this.P24_url_status = parent.P24_url_status;
            }
        }


        // Must move nessessary operations to another function
        // and leave Default Constructor for MVC
        public Transaction()
            :this(new Przelewy24())
        {
        }
        
        public Transaction(Przelewy24 przelewy24)
        {
            AllConstuctorsInitialOperations();
            this.parent = przelewy24; 
            SetUniqueSessionId(SessionIdGenerationMode.time, "");
            this.AllConstructorsFinalOperations();
        }

        public Transaction (
            // merchant account data
            Przelewy24 parent, 
            // transaction data
            SessionIdGenerationMode generationMode,
            string sessionId,
            int amount, 
            string currency, 
            string description,
            string email, 
            string country, 
            string urlReturn 
            )
        {
            AllConstuctorsInitialOperations();
            this.parent = parent;
            this.AllConstructorsFinalOperations();

            SetUniqueSessionId (generationMode, sessionId);
            this.P24_amount = amount;
            this.P24_currency = currency;
            this.P24_description = description;
            this.P24_email = email;
            this.P24_country = country;
            this.P24_url_return = urlReturn;
        }


        /// <summary>
        /// Create transaction and associated Przelewy24 class
        /// This Constructor was created in early version of project and
        /// will be removed in final version
        /// </summary>
        /// <param name="merchantId"></param>
        /// <param name="posId"></param>
        /// <param name="crcKey"></param>
        /// <param name="sandboxMode"></param>
        /// <param name="generationMode"></param>
        /// <param name="sessionId"></param>
        /// <param name="amount"></param>
        /// <param name="currency"></param>
        /// <param name="description"></param>
        /// <param name="email"></param>
        /// <param name="country"></param>
        /// <param name="urlReturn"></param>
        [Obsolete]
        public Transaction (
            // merchant data
            string merchantId,
            string posId,
            string crcKey,
            bool sandboxMode,
            // transaction data
            SessionIdGenerationMode generationMode,
            string sessionId,
            int amount, 
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


        public Transaction (
            // merchant data
            int merchantId,
            int posId,
            string crcKey,
            bool sandboxMode,
            // transaction data
            SessionIdGenerationMode generationMode,
            string sessionId,
            int amount, 
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

        // Need to test

        public async Task<string> RegisterTransaction()
        {
            P24Response response =  await parent.RegisterTransaction(this);
            return response.ResponseString;
        }

        public async Task<P24Response> RegisterTransaction(bool saveToDatabase)
        {
            return await RegisterTransaction(saveToDatabase, false);
        }

        public async Task<P24Response> RegisterTransaction(bool saveToDatabase, bool saveOnlyCorrectTransactions)
        {
            string respString = await RegisterTransaction();
            if (saveToDatabase)
            {
                if (this.P24.TransactionDb == null)
                {
                    throw new ApplicationException("Database is not set");
                }
                else
                {
                    try
                    {
                        if (this.P24Response.OK || !saveOnlyCorrectTransactions)
                        {
                            this.P24.TransactionDb.SaveTransaction(this);
                        }
                    }
                    catch (Exception e)
                    {
                        throw;
                    }
                }
            }
            return this.P24Response;
        }

        public string GetRequestLink()
        {
            return this.P24.UrlTrnRequest + "/" + this.P24Response.Token;
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

        public T GetParameter<T>(string parameterName)
        {
            var result = this.parameters.Select (n => n).Where (n => n.Name == parameterName).FirstOrDefault ();
            if (result != null)
            {
                return ((Parameter<T>)result).Value;
            }
            else
            {
                return default(T);
            }
        }


        public void SetParameter<T>(string parameterName, T parameterValue)
        {
            var result = this.parameters.Select (n => n).Where (n => n.Name == parameterName).FirstOrDefault ();
            if (result != null)
            {
                var r = (Parameter<T>)result;
                r.Value = parameterValue;
            }
            else
                this.parameters.Add (new Parameter<T> (parameterName, parameterValue));
        }


        public void RemoveParameter (string parameterName)
        {
            var result = this.parameters.Select(n => n).Where(n => n.Name == parameterName);
            foreach (IParameter p in result)
                this.parameters.Remove(p);
        }

        
        public IParameter[] GetParameters()
        {
            return this.parameters.ToArray();
        }

        #endregion


        #region Sign methods

        public string CalculateRegisterSign()
        {
             string registerSign = Przelewy24.CalculateRegisterSign (
                this.P24_session_id, 
                parent.MerchantId, 
                this.P24_amount, 
                this.P24_currency, 
                parent.CrcKey
             );

            return registerSign;
        }

        public string SetRegisterSign()
        {
            string registerSign = CalculateRegisterSign();
            this.P24_sign = registerSign;
            return registerSign;
        }


        #endregion


        #region Override Methods

        public override string ToString()
        {
            StringBuilder stb = new StringBuilder();

            stb.AppendLine("Transaction:");
            stb.AppendLine("[");

            stb.AppendLine("\tAdditional settinsg");
            stb.AppendLine("\t[");
            stb.AppendLine(string.Format("\t\t\"SessionIdAdditionalData\" = \"{0}\"", this.SessionIdAdditionalData));
            stb.AppendLine(string.Format("\t\t\"ThisTransactionNumber\" = \"{0}\"", this.ThisTransactionNumber));
            stb.AppendLine(string.Format("\t\t\"ShortOrderId\" = \"{0}\"", this.ShortOrderId));
            stb.AppendLine(string.Format("\t\t\"FullOrderId\" = \"{0}\"", this.FullOrderId));
            stb.AppendLine("\t]");

            stb.AppendLine();

            stb.AppendLine("\tParameters");
            stb.AppendLine("\t[");
            for(int i = 0; i < parameters.Count; i++)
            {
                IParameter parameter = parameters[i];
                stb.AppendLine(string.Format("\t\t{0}", parameter.ToString()));
            }
            stb.AppendLine("\t]");

            if (this.P24Response != null)
                stb.AppendLine(String.Format("P24Response = {0}", this.P24Response.ToString()));

            stb.AppendLine("]");



            return stb.ToString();
        }


        #endregion

    }
}
