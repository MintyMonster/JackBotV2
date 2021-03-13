using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;

namespace JackBotV2.API_Stuff
{
    public class APIHelper
    {
        public static HttpClient APIClient { get; set; }

        public static void InitialiseClient()
        {
            APIClient = new HttpClient();
            APIClient.DefaultRequestHeaders.Accept.Clear();
            APIClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}
