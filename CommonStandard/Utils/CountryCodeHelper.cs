using Common.EntityModels;
using Common.ExtensionMethods;
using System.Collections.Generic;
using System.Linq;

namespace Common.Utils
{
    /// <summary>
    /// Used to get country codes and currency codes from country names
    /// </summary>
    public class CountryCodeHelper
    {
        private readonly Dictionary<string, string> _countryCodes;
        private readonly Dictionary<string, string> _currencyCodes;

        public CountryCodeHelper(List<Country> countries)
        {
            _countryCodes = countries.ToDictionary(x => x.Name, x => x.CountryCode);
            _currencyCodes = countries
                .Distinct((x, y) => x.CountryCode == y.CountryCode)
                .ToDictionary(x => x.CountryCode, x => x.CurrencyCode);
        }

        public string GetCountryCode(string countryName) => _countryCodes[countryName];

        public string GetCurrencyCode(string countryCode) => _currencyCodes[countryCode];
    }
}