using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleMenu
{
    class MenuItemBase
    {
        public Menu Menu { get; set; }
        public string Text { get; set; }
        public int Height { get; set; } = 1;
        public bool Selected { get; set; } = false;
        public virtual bool Selectable => true;
        public virtual void Action() => throw new NotImplementedException();
        public override string ToString() => Text;
    }

    class MenuTextItem : MenuItemBase
    {
        public override bool Selectable => false;

        public MenuTextItem(string text)
        {
            Text = text;
        }

        public override string ToString()
        {
            return Text;
        }
    }

    class MenuActionItem : MenuItemBase
    {
        Action _action;

        public MenuActionItem(string text, Action action)
        {
            Text = text;
            _action = action;
        }

        public MenuActionItem(string text, Menu submenu)
        {
            Text = text;
            _action = () =>
            {
                submenu.ParentMenu = Menu;
                Menu.GetMainMenu().SetCurrentMenu(submenu);
            };
        }

        public override void Action()
        {
            _action();
        }
    }

    class MenuCheckboxItem : MenuItemBase
    {
        public bool Checked { get; set; } = false;
        public int Group { get; set; }

        public MenuCheckboxItem(string text, int groupID = 0)
        {
            Text = text;
            Group = groupID;
        }

        public override void Action()
        {
            Checked = !Checked;
        }

        public override string ToString() => (Checked ? "[*] " : "[ ] ") + Text;
    }

    class MenuInputOption<T> : MenuItemBase
    {
        public T Value { get; set; }

        public MenuInputOption(string text, T value)
        {
            Text = text;
            Value = value;
        }

        public override void Action()
        {
            int t = 1;
            foreach (MenuItemBase item in Menu.MenuItems)
            {
                t += item.Height;
                if (item == this)
                    break;
            }

            Console.SetCursorPosition(Menu.Margin + Text.Length + 2, t);
            for (int l = Console.CursorLeft; l < Console.WindowWidth; l++)
                Console.Write(' ');
            Console.SetCursorPosition(Menu.Margin + Text.Length + 2, t);

            Menu.GetMainMenu().ReverseColors();
            Console.CursorVisible = true;
            string input = Console.ReadLine();
            Console.CursorVisible = false;
            Menu.GetMainMenu().ReverseColors();

            try
            {
                Value = (T)Convert.ChangeType(input, Value.GetType());
            }
            catch (FormatException)
            {
                Console.Beep();
            }
        }

        public override string ToString() => $"{Text}: {Value}";
    }

    class Menu
    {
        public string Name { get; set; }
        public int Margin { get; set; } = 2;
        public Menu ParentMenu { get; set; }
        public MainMenu MainMenu { get; set; } = null;
        public List<MenuItemBase> MenuItems { get; } = new List<MenuItemBase>();
        public List<MenuItemBase> SelectableMenuItems => MenuItems.Where(x => x.Selectable).ToList();

        public Menu()
        {
            Name = "New Menu";
            ParentMenu = null;
        }

        public Menu(string name, Menu parent = null)
        {
            Name = name;
            ParentMenu = parent;
        }

        public Menu(string name, IEnumerable<MenuItemBase> items, Menu parent = null)
        {
            Name = name;
            ParentMenu = parent;
            MenuItems.AddRange(items);
        }

        public void AddItem(MenuItemBase item)
        {
            item.Menu = this;
            MenuItems.Add(item);
        }

        public void RemoveItem(MenuItemBase item)
        {
            item.Menu = null;
            MenuItems.Remove(item);
        }

        public MainMenu GetMainMenu()
        {
            Menu m = this;
            while (true)
            {
                if (m.MainMenu != null)
                    return m.MainMenu;
                if (m.ParentMenu == null)
                    return null;
                m = m.ParentMenu;
            }
        }
    }

    class MainMenu 
    {
        public Menu TopMenu { get; private set; }
        public Menu CurrentMenu { get; set; }
        public ConsoleColor BackgroundColor { get; set; } = Console.BackgroundColor;
        public ConsoleColor ForegroundColor { get; set; } = Console.ForegroundColor;

        public MainMenu(Menu menu)
        {
            menu.MainMenu = this;
            TopMenu = menu;
            SetCurrentMenu(menu);
        }

        public void Clear()
        {
            for (int i = 0; i < Console.WindowWidth * Console.WindowHeight - Console.CursorTop * Console.WindowWidth - Console.CursorLeft; i++)
                Console.Write(' ');
        }

        public void Show()
        {
            Console.CursorVisible = false;
            Console.ForegroundColor = ForegroundColor;
            Console.BackgroundColor = BackgroundColor;
            Print();
            GetInput();
        }

        public void ReverseColors()
        {
            ConsoleColor c = BackgroundColor;
            BackgroundColor = ForegroundColor;
            ForegroundColor = c;
            Console.ForegroundColor = ForegroundColor;
            Console.BackgroundColor = BackgroundColor;
        }

        void PrintItem(MenuItemBase item, string margin)
        {
            if (item.Selected) ReverseColors();
            Console.Write(margin + item.ToString());
            if (item.Selected) ReverseColors();
        }

        public void Print()
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(CurrentMenu.Name + '\n');

            string margin = string.Empty;
            for (int i = 0; i < CurrentMenu.Margin; i++)
                margin += ' ';

            foreach (MenuItemBase item in CurrentMenu.MenuItems.SkipLast(1))
            {
                PrintItem(item, margin);
                Console.WriteLine();
            }
            if (CurrentMenu.MenuItems.Count > 0)
                PrintItem(CurrentMenu.MenuItems.Last(), margin);
            Clear();
        }

        public void SetCurrentMenu(Menu menu)
        {
            if (menu.SelectableMenuItems.Count > 0)
            {
                menu.SelectableMenuItems[0].Selected = true;
                foreach (MenuItemBase item in menu.SelectableMenuItems.Skip(1))
                    item.Selected = false;
            }
            CurrentMenu = menu;
            Print();
        }

        void GetInput()
        {
            while (true)
            {
                ConsoleKeyInfo cki = Console.ReadKey();
                switch (cki.Key)
                {
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.Escape:
                    case ConsoleKey.Backspace:
                        if (cki.Modifiers.HasFlag(ConsoleModifiers.Control) || CurrentMenu.ParentMenu == null)
                        {
                            Console.Clear();
                            Console.ResetColor();
                            Console.CursorVisible = true;
                            return;
                        }
                        SetCurrentMenu(CurrentMenu.ParentMenu);
                        break;

                    case ConsoleKey.UpArrow:
                        for (int i = 0; i < CurrentMenu.SelectableMenuItems.Count; i++)
                            if (CurrentMenu.SelectableMenuItems[i].Selected && i > 0)
                            {
                                CurrentMenu.SelectableMenuItems[i].Selected = false;
                                CurrentMenu.SelectableMenuItems[i - 1].Selected = true;
                                Print();
                                break;
                            }
                        break;

                    case ConsoleKey.RightArrow:
                    case ConsoleKey.Enter:
                        if (CurrentMenu.SelectableMenuItems.Count > 0)
                            CurrentMenu.SelectableMenuItems.First(x => x.Selected).Action();
                        Print();
                        break;

                    case ConsoleKey.DownArrow:
                        for (int i = 0; i < CurrentMenu.SelectableMenuItems.Count - 1; i++)
                            if (CurrentMenu.SelectableMenuItems[i].Selected)
                            {
                                CurrentMenu.SelectableMenuItems[i].Selected = false;
                                CurrentMenu.SelectableMenuItems[i + 1].Selected = true;
                                Print();
                                break;
                            }
                        break;
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Menu menu2 = new Menu("Test Menu 2");
            menu2.AddItem(new MenuTextItem("Hello There!"));
            menu2.AddItem(new MenuActionItem("Exit", () => { Environment.Exit(1); }));
            menu2.AddItem(new MenuInputOption<int>("Number", 12345));

            Menu menu1 = new Menu("Test Menu 1");
            menu1.AddItem(new MenuTextItem("Test text"));
            menu1.AddItem(new MenuTextItem("More test text"));
            menu1.AddItem(new MenuCheckboxItem("Random check box"));
            menu1.AddItem(new MenuActionItem("Menu 2", menu2));

            MainMenu mainMenu = new MainMenu(menu1);
            mainMenu.Show();
        }
    }
}
