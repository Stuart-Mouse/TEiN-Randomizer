using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using TEiNRandomizer.Properties;

namespace TEiNRandomizer
{
    public class PoolCategory : IComparable<PoolCategory>
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public bool Enabled { get; set; } = true;
        public int CompareTo(PoolCategory other) => Name.CompareTo(other.Name);
        public IEnumerable<Pool> Pools { get; set; }
    }
}
