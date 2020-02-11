using System;
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
        private static EditorProperties Instance;

        public EditorProperties() : base()
        {
            Instance = this;
            InitializeComponent();
            UpdateWidgetList();
        }

        public static void UpdateWidgetList()
        {
            Instance.widgets.Children.Clear();
            foreach (var widget in App.CurrentFile.Widgets)
                Instance.widgets.Children.Add(new WidgetElement(App.Preview.ToDraw.First((set) => set.Link == widget.Item2)));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new Options().ShowDialog();
        }
    }
}