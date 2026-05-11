using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

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

        public string Translate(string toLanguage, string text, out string? detectedLanguage)
        {
            return Translate(null, toLanguage, text, out detectedLanguage);
        }

        string Translate(string? fromLanguage, string toLanguage, string text, out string? detectedLanguage)
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
            using (var reader = new StreamReader(respone.GetResponseStream()!, Encoding.UTF8))
                text = reader.ReadToEnd();

            string translatedText = String.Empty;

            var result = JsonSerializer.Deserialize(text, GoogleTranslateJsonContext.Default.TranslateResult)!;
            if (result.Data.Translations.Any())
            {
                var translation = result.Data.Translations.FirstOrDefault();
                detectedLanguage = translation.DetectedSourceLanguage;
                translatedText = translation.TranslatedText;
            }

            return translatedText;
        }
    }

    internal class TranslateResult
    {
        [JsonPropertyName("data")]
        public TranslateData Data { get; set; } = null!;
    }

    internal class TranslateData
    {
        [JsonPropertyName("translations")]
        public List<TranslateTranslation> Translations { get; set; } = null!;
    }

    internal class TranslateTranslation
    {
        [JsonPropertyName("translatedText")]
        public string TranslatedText { get; set; } = null!;
        [JsonPropertyName("detectedSourceLanguage")]
        public string? DetectedSourceLanguage { get; set; }
    }

    [JsonSerializable(typeof(TranslateResult))]
    internal partial class GoogleTranslateJsonContext : JsonSerializerContext
    {
    }
}
