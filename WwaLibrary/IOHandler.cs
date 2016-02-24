using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WarArena
{
    public class IOHandler
    {
        public void SetCursorPosition(Coords coordinates)
        {
            Console.SetCursorPosition(coordinates.X, coordinates.Y);
        }

        public void SetCursorPosition(int x, int y)
        {
            Console.SetCursorPosition(x, y);
        }

        public ConsoleKeyInfo ReadKey()
        {
            return Console.ReadKey();
        }

        public string ReadString()
        {
            return Console.ReadLine();
        }

        public void Clear()
        {
            Console.Clear();
        }

        public void Write(string text)
        {
            Console.Write(text);
        }

        public void Write(char character)
        {
            Console.Write(character);
        }

        public void WriteLine(string text)
        {
            Console.WriteLine(text);
        }

        public void ChangeTextColor(string color)
        {
            Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof (ConsoleColor), color);
        }

        public void WriteBlock(string color)
        {
            var oldColor = Console.BackgroundColor;
            Console.BackgroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), color);
            Console.Write(" ");
            Console.BackgroundColor = oldColor;
        }
    }
}
