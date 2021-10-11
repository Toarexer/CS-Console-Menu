using System;
using System.Collections.Generic;
using System.Linq;

namespace MenuFramework
{
    public class MenuItem
    {
        public readonly int ID;
        public string Text;
        public readonly Action<object> func;
        public readonly object args;

        public MenuItem(int ID, string text, Action<object> function, object args)
        {
            this.ID = ID;
            Text = text;
            func = function;
            this.args = args;
        }
    }

    public partial class Menu
    {
        public bool SetItems(List<MenuItem> items)
        {
            if (items.GroupBy(x => x.ID).Any(g => g.Count() > 1))
                return false;
            _items = items;
            return true;
        }

        public bool AddItem(MenuItem item)
        {
            if (Items.Any(x => x.ID == item.ID))
                return false;
            Items.Add(item);
            return true;
        }

        public bool RemoveItem(int ID)
        {
            return Items.Remove(Items.First(x => x.ID == ID));
        }

        public void OrderByID(bool descending = false)
        {
            if (!descending)
                Items.OrderBy(x => x.ID);
            else
                Items.OrderByDescending(x => x.ID);
        }
    }
}
