using System;

namespace Arma3LauncherWPF.Extensions
{
    public static class StringExtensions
    {
        public static bool EqualIgnoreCase(this string source, string target)
        {
            return string.Compare(source, target, StringComparison.InvariantCultureIgnoreCase) == 0;
        }
    }
}