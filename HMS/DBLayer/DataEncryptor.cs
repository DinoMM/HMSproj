using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer
{
    public static class DataEncryptor
    {
        private static IDataProtector _protector;

        public static void Init(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("StaticDataEncryptor"); //jednoduchy scope pre ochranu dat (skoro ako hashkey ale zevraj to neni rovnake)
        }

        public static string Protect(string data)
        {
            return _protector.Protect(data);
        }

        public static string Unprotect(string protectedData)
        {
            return _protector.Unprotect(protectedData);
        }
    }
}
