using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace MvcQuiniela.Models
{

    public class GoogleUserOutputData
    {
        public string id { get; set; }
        public string name { get; set; }
        public string given_name { get; set; }
        public string email { get; set; }
        public string picture { get; set; }
    }

    public class GooglePlusAccessToken
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string id_token { get; set; }
        public string refresh_token { get; set; }
    }

    public class GoogleLogin
    {
        public string googleplus_client_id = "173582404619-rgk3mtkbas4qu927lj9steu13b3stnbc.apps.googleusercontent.com";    // Replace this with your Client ID
        private string googleplus_client_sceret = "tQzvtBajNUpsdkgQCn-UF-rT";                                               // Replace this with your Client Secret
        public string googleplus_redirect_url = "http://localhost:54012/Quiniela/Login";                                    // Replace this with your Redirect URL; Your Redirect URL from your developer.google application should match this URL.
       // Replace this with your Redirect URL; Your Redirect URL from your developer.google application should match this URL.

        private string Parameters;
        private string accessToken;

        public GoogleUserOutputData ObtenerCorreo(bool succesLogin, string url)
        {
            GoogleUserOutputData user = new GoogleUserOutputData();

            if (succesLogin)
            {
                try
                {
                    
                        string queryString = url.ToString();
                        char[] delimiterChars = { '=' };
                        string[] words = queryString.Split(delimiterChars);
                        string code = words[1];

                        if (code != null)
                        {
                            //get the access token 
                            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://accounts.google.com/o/oauth2/token");
                            webRequest.Method = "POST";
                            Parameters = "code=" + code + "&client_id=" + googleplus_client_id + "&client_secret=" + googleplus_client_sceret + "&redirect_uri=" + googleplus_redirect_url + "&grant_type=authorization_code";
                            byte[] byteArray = Encoding.UTF8.GetBytes(Parameters);
                            webRequest.ContentType = "application/x-www-form-urlencoded";
                            webRequest.ContentLength = byteArray.Length;
                            Stream postStream = webRequest.GetRequestStream();
                            // Add the post data to the web request
                            postStream.Write(byteArray, 0, byteArray.Length);
                            postStream.Close();

                            WebResponse response = webRequest.GetResponse();
                            postStream = response.GetResponseStream();
                            StreamReader reader = new StreamReader(postStream);
                            string responseFromServer = reader.ReadToEnd();

                            GooglePlusAccessToken serStatus = JsonConvert.DeserializeObject<GooglePlusAccessToken>(responseFromServer);

                            if (serStatus != null)
                            {
                                accessToken = string.Empty;
                                accessToken = serStatus.access_token;

                                if (!string.IsNullOrEmpty(accessToken))
                                {
                                    WebRequest request = WebRequest.Create("https://www.googleapis.com/oauth2/v1/userinfo?access_token=" + accessToken);

                                    //Obtiene la data del request
                                    WebResponse responseDataUser = request.GetResponse();
                                    Stream dataStream = responseDataUser.GetResponseStream();
                                    StreamReader readerDataUser = new StreamReader(dataStream);
                                    string responseFromServerDataUser = readerDataUser.ReadToEnd();

                                    //Crea objeto con la data
                                    user = JsonConvert.DeserializeObject<GoogleUserOutputData>(responseFromServerDataUser);

                                    reader.Close();
                                    response.Close();
                                    readerDataUser.Close();
                                    responseDataUser.Close();
                                }
                            }

                        }
                }
                catch (Exception ex)
                {
                    //throw new Exception(ex.Message, ex);
                }
            }

            return user;
        }
    }

    public class User
    {
        public int id_alias { get; set; }
        public string nickname { get; set; }
        public string email { get; set; }
        public string picture { get; set; }
        public bool login { get; set; }
        public int id_login { get; set; }
        public int id_rol { get; set; }
        public int id_menu { get; set; }
        public List<int> permisos { get; set; }
        public string nombre { get; set; }
    }
}