using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Pixtrim2
{
    /// <summary>
    /// MyDialogWindw.xaml の相互作用ロジック
    /// </summary>
    public partial class MyDialogWindow : Window
    {
        public MyDialogWindow(Window owner)
        {
            InitializeComponent();
            this.Owner = owner;
            MyTextBox.Focus();
            MyTextBox.KeyDown += MyTextBox_KeyDown;
        }

        private void MyTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.DialogResult = true;
            }
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void ButtonCancel_Click_1(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
        public string Answer => MyTextBox.Text;
    }
}
//Creating a custom input dialog - The complete WPF tutorial
//https://www.wpf-tutorial.com/dialogs/creating-a-custom-input-dialog/