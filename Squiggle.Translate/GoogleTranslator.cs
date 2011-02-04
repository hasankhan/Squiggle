using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.IO;

namespace Squiggle.Translate
{
    class GoogleTranslator
    {
        string apiKey;

        public GoogleTranslator(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public string Translate(string fromLanguage, string toLanguage, string text)
        {
            string detectedLanguage;
            return Translate(fromLanguage, toLanguage, text, out detectedLanguage);
        }

        public string Translate(string toLanguage, string text, out string detectedLanguage)
        {
            return Translate(null, toLanguage, text, out detectedLanguage);
        }

        string Translate(string fromLanguage, string toLanguage, string text, out string detectedLanguage)
        {
            detectedLanguage = null;
            text = HttpUtility.UrlEncode(text);

            String apiUrl;
            if (String.IsNullOrEmpty(fromLanguage))
                apiUrl = "https://www.googleapis.com/language/translate/v2?key={0}&target={1}&q={2}";
            else
                apiUrl = "https://www.googleapis.com/language/translate/v2?key={0}&target={1}&q={2}&source={3}";

            string url = String.Format(apiUrl, apiKey, toLanguage, text, fromLanguage);

            WebRequest request = HttpWebRequest.Create(url);
            request.Method = "GET";

            using (WebResponse respone = request.GetResponse())
            using (var reader = new StreamReader(respone.GetResponseStream(), Encoding.UTF8))
                text = reader.ReadToEnd();

            string translatedText = String.Empty;

            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<Result>(text);
            if (result.data.translations.Any())
            {
                var translation = result.data.translations.FirstOrDefault();
                detectedLanguage = translation.detectedSourceLanguage;
                translatedText = translation.translatedText;
            }

            return translatedText;
        }

        class Result
        {
            public Data data { get; set; }
        }

        class Data
        {
            public List<Translation> translations { get; set; }
        }

        class Translation
        {
            public string translatedText { get; set; }
            public string detectedSourceLanguage { get; set; }
        }
    }

}
