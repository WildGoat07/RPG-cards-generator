﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RPGcardsGenerator
{
    /// <summary>
    /// Logique d'interaction pour EditorProperties.xaml
    /// </summary>
    public partial class EditorProperties : Window
    {
        public EditorProperties() : base()
        {
            InitializeComponent();
            foreach (var widget in App.CurrentFile.Widgets)
                widgets.Children.Add(new WidgetElement(App.Preview.ToDraw.First((set) => set.Link == widget.Item2)));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new Options().ShowDialog();
        }
    }
}