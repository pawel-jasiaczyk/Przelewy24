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

    }
}
