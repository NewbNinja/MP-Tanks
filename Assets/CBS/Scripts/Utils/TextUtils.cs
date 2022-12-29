using System;
using System.Linq;


namespace CBS.Utils
{
    public class TextUtils
    {
        public static bool ContainSpecialSymbols(string input)
        {
            return input.Any(ch => !Char.IsLetterOrDigit(ch));
        }
    }
}
