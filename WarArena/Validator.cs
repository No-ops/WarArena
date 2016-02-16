using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarArena
{
    class Validator
    {
        public bool HasMinLength(string text, int minLength)
        {
            return text.Length >= minLength;
        }

        public bool IsKeyValid(ConsoleKey key, params ConsoleKey[] validKeys)
        {
            return validKeys.Contains(key);
        }
    }
}
