﻿using System;
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
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void PoolCat_Click(object sender, RoutedEventArgs e)
        {
            //var parent = (ListView)((TextBox)sender).Parent;
            (sender as LevelPoolCategory).Enabled = !(sender as LevelPoolCategory).Enabled;
            //PoolList.GetBindingExpression(ListBox.VisibilityProperty).UpdateTarget();
        }
        private void PoolList_Click(object sender, RoutedEventArgs e)
        {
            var selected = (sender as ListBox).SelectedItem;
            (selected as LevelPool).Active = !(selected as LevelPool).Active;
        }
        //private void Mod_Click(object sender, RoutedEventArgs e)
        //{
        //    var mod = (sender as StackPanel);
        //}

        private void SelectRunButton_Click(object sender, RoutedEventArgs e)
        {
            //var element = ((StackPanel)sender);
            //var window = element.
            (sender as Mod).Active = true;
            //foreach (var mod in Mods)
        }
        
        private void NoClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void SwitchAll(object sender, MouseButtonEventArgs e)
        {
            var grid = ((Grid)((Canvas)sender).Parent);
            var list = (ListBox)grid.Children[1];
            foreach (var pool in list.ItemsSource)
            {
                (pool as LevelPool).Active = false;
                //(pool as ListBoxItem).GetBindingExpression(ListBox.).UpdateTarget();
            }
        }
    }
}
