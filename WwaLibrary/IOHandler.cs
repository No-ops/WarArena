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

        public void Write(string text, int x, int y)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(text);
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

        public void ClearLine(int startColumn, int startRow)
        {            
            Console.SetCursorPosition(startColumn, startRow);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(startColumn, startRow);
        }

        public void ClearArea(int x1, int y1, int x2, int y2)
        {
            for (int i = y1; i <= y2; i++)
            {
                Console.SetCursorPosition(x1, i);
                Console.Write(new string(' ', y1 + 1));
            }            
        }
    }
}
