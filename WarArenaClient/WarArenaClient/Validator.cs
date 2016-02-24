using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarArena
{
    static class Validator
    {
        public static bool HasMinLength(string text, int minLength)
        {
            return text.Length >= minLength;
        }

        public static bool IsKeyValid(ConsoleKey key, params ConsoleKey[] validKeys)
        {
            return validKeys.Contains(key);
        }
    }
}
