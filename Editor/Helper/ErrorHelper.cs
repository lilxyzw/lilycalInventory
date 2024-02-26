using System;
using System.Linq;

#if LIL_NDMF
using nadena.dev.ndmf.localization;
using nadena.dev.ndmf;
#endif

namespace jp.lilxyzw.lilycalinventory
{
    internal static class ErrorHelper
    {
        internal static void Report(string key, params object[] args)
        {
            #if LIL_NDMF
            var list = Localization.GetCodes().Select(code => (code, LocalizationFunction(code))).ToList();
            var localizer = new Localizer("en-us", () => list);
            ErrorReport.ReportError(localizer, ErrorSeverity.Error, key, args);
            #else
            throw new Exception(Localization.S(key));
            #endif
        }

        private static Func<string, string> LocalizationFunction(string code)
        {
            return key => Localization.S(key, code);
        }
    }
}
