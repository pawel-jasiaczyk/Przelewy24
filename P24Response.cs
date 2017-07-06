// Author: Paweł Jasiaczyk

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Przelewy24
{
    public class P24Response
    {
        public bool OK { get; private set;}
        public string Token { get; private set; }
        public Dictionary<string, string> Errors { get; private set; }
        public string ResponseString { get; private set; }
        public string Error { get; private set; }

        private P24Response()
        {
            this.Errors = new Dictionary<string, string>();
        }

        public P24Response(string responseString)
            :this()
        {
            if (!string.IsNullOrEmpty(responseString))
            {
                this.ResponseString = responseString;
                string[] splited = responseString.Split('&');
                string[] isOk = splited[0].Split('=');
                if (isOk.Length >= 2 && isOk[1] == "0")
                {
                    if (splited.Length >= 2)
                    {
                        this.OK = true;
                        this.Error = "0";
                        string[] tokenSource = splited[1].Split('=');
                        if (tokenSource.Length >= 2)
                            this.Token = tokenSource[1];
                        else this.OK = false;
                    }
                    else this.OK = false;
                }
                else
                {
                    this.OK = false;
                    if (isOk.Length >= 2)
                        this.Error = isOk[1];

                    string errorString = responseString.Substring(responseString.IndexOf('&'));
                    string errorDescString = errorString.Substring(errorString.IndexOf('=') + 1);
                    string[] errorDesc = errorDescString.Split('&');
                    foreach(string ed in errorDesc)
                    {
                        string[] temp = ed.Split(':');
                        if(temp.Length > 1)
                        {
                            this.Errors.Add(temp[0], temp[1]);
                        }
                    }
                }
            }
            else
            {
                this.OK = false;
                this.Error = "Wrong response string";
            }
        }
        
        public override string ToString()
        {
            StringBuilder stb = new StringBuilder();
            stb.AppendLine("Response");
            stb.AppendLine("[");
            stb.AppendLine(String.Format("\tOK = {0}", OK.ToString()));
            stb.AppendLine(String.Format("\tError = {0}", !String.IsNullOrEmpty(this.Error) ? this.Error.ToString() : ""));
            stb.AppendLine(String.Format("\tToken = {0}", !String.IsNullOrEmpty(this.Token) ? this.Token.ToString() : ""));
            stb.AppendLine(String.Format("\tResponseString = {0}", !String.IsNullOrEmpty(this.ResponseString) ? this.ResponseString : ""));
            stb.AppendLine("\tErrors:");
            if (this.Errors.Count > 0)
            {
                stb.AppendLine("\t[");
                foreach(KeyValuePair<string, string> pair in this.Errors)
                {
                    stb.AppendLine(String.Format("\t\t{0} = {1}", pair.Key, !String.IsNullOrEmpty(pair.Value) ? pair.Value : ""));
                }
                stb.AppendLine("\t]");
            }
            else
            {
                stb.AppendLine("\t\tEmpty list - there are no eroors");
            }
            stb.AppendLine("]");
            return stb.ToString();
        }
    }
}
