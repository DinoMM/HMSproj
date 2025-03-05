using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMSModels
{
    public static class DataEncryptor
    {
        private static IDataProtector _protector;

        /// <summary>
        /// Inicializácia, nutnost spustit pred pouzitim
        /// </summary>
        /// <param name="provider"></param>
        public static void Init(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("StaticDataEncryptor"); //jednoduchy scope pre ochranu dat (skoro ako hashkey ale zevraj to neni rovnake)
        }

        /// <summary>
        /// Vracia zasifrovane data
        /// </summary>
        /// <param name="data"></param>
        public static string Protect(string data)
        {
            return _protector.Protect(data);
        }

        /// <summary>
        /// Vracia odsifrovane data
        /// </summary>
        /// <param name="protectedData"></param>
        public static string Unprotect(string protectedData)
        {
            return string.IsNullOrEmpty(protectedData) ? "" : _protector.Unprotect(protectedData);
        }
    }
}
