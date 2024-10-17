using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UctovnyModul.ViewModels
{
    public class Currency
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("symbol_native")]
        public string SymbolNative { get; set; }

        [JsonPropertyName("decimal_digits")]
        public int DecimalDigits { get; set; }

        [JsonPropertyName("rounding")]
        public int Rounding { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("name_plural")]
        public string NamePlural { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("countries")]
        public List<string> Countries { get; set; }


        public static List<Currency>? GetListOfCurrenciesFromJsonString(string jsonString)
        {
            var currencyData = JsonSerializer.Deserialize<CurrencyData>(jsonString);
            return currencyData?.Data.Values.ToList() ?? null;
        }
    }

    public class CurrencyData
    {
        [JsonPropertyName("data")]
        public Dictionary<string, Currency> Data { get; set; }
    }

    public class Meta
    {
        [JsonPropertyName("last_updated_at")]
        public DateTime LastUpdatedAt { get; set; }
    }

    public class CurrencyValue
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("value")]
        public decimal Value { get; set; }
    }

    public class CurrencyApiResponse
    {
        [JsonPropertyName("meta")]
        public Meta Meta { get; set; }

        [JsonPropertyName("data")]
        public Dictionary<string, CurrencyValue> Data { get; set; }

        public static CurrencyApiResponse? GetCurrencyApiResponse(string jsonString)
        {
            var response = JsonSerializer.Deserialize<CurrencyApiResponse>(jsonString);
            return response;
        }
    }


}
