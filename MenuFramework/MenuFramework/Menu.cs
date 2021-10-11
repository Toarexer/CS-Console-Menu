using System;
using System.Collections.Generic;

namespace MenuFramework
{
    public partial class Menu
    {
        public string Text { get; set; }

        public int _index;
        public int Index
        {
            get => _index;
            set
            {
                if (value < 0)
                    _index = 0;
                else if (value >= _items.Count)
                    _index = _items.Count - 1;
                else
                    _index = value;
            }
        }

        List<MenuItem> _items;
        public List<MenuItem> Items => _items;
        public int Count => _items.Count;

        public Menu(string text)
        {
            Text = text;
            _items = new List<MenuItem>();
        }

        public Menu(string text, List<MenuItem> items)
        {
            Text = text;
            if (!SetItems(items))
                _items = new List<MenuItem>();
        }


        public void Print()
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(Text);
            for (int i = 0; i < Count; i++)
                if (i == Index)
                {
                    InvertColors();
                    Console.WriteLine("  " + Items[i].Text);
                    InvertColors();
                }
                else
                    Console.WriteLine("  " + Items[i].Text);
        }

        int oldbh;
        public void Redraw()
        {
            oldbh = Console.BufferHeight;
            Console.BufferHeight = Console.WindowHeight;
            Console.CursorVisible = false;
            Console.Clear();
            Print();
        }

        public void Run()
        {
            Redraw();
            while (true)
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.UpArrow:
                        Index--;
                        Print();
                        break;
                    case ConsoleKey.DownArrow:
                        Index++;
                        Print();
                        break;
                    case ConsoleKey.Enter:
                        Items[Index].func(Items[Index].args);
                        break;
                    case ConsoleKey.F5:
                        Redraw();
                        break;
                    case ConsoleKey.Escape:
                        Console.BufferHeight = oldbh;
                        Console.CursorVisible = true;
                        return;
                    default:
                        Console.SetCursorPosition(0, Console.BufferHeight - 1);
                        Console.Write(' ');
                        Console.SetCursorPosition(0, Console.BufferHeight - 1);
                        break;
                }
        }

        public void GetInput(string text, out string input)
        {
            Console.Clear();
            Console.Write(text);
            input = Console.ReadLine();
            Print();
        }

        public static void InvertColors()
        {
            ConsoleColor fc = Console.ForegroundColor;
            Console.ForegroundColor = Console.BackgroundColor;
            Console.BackgroundColor = fc;
        }

        public static Menu NewEmpty() => new Menu(string.Empty);
    }
}
