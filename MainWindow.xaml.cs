using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RPGcardsGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp|Tous les fichiers|*.*";
            dialog.Title = "Ouvrir l'image de fond pour cette carte";
            if (dialog.ShowDialog().Value)
            {
                var template = new Template();
                template.Background = new Bitmap(dialog.FileName);
                Visibility = Visibility.Collapsed;
                template.Widgets.Add((
                "test",
                new Template.Text
                {
                    Content = "test",
                    Font = "Roboto",
                    InnerColor = Color.Black,
                    Size = 25
                }));
                template.Widgets.Add((
                "test2",
                new Template.Text
                {
                    Location = new System.Numerics.Vector2(2),
                    Content = "test2",
                    Font = "Roboto",
                    InnerColor = Color.Red,
                    Size = 25
                }));
                App.StartEditor(template);
                Close();
            }
        }
    }
}