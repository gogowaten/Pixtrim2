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
    /// Window1.xaml の相互作用ロジック
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            //Closing += Window1_Closing;
            //            [WPF, xaml] 要素のサイズに合わせてWindowの大きさを自動的にリサイズ調整する方法 │ Web備忘録
            //https://webbibouroku.com/Blog/Article/wpf-auto-resize

            SizeToContent = SizeToContent.WidthAndHeight;
            //ShowInTaskbar = false;
            MinWidth = 300;


            RenderOptions.SetBitmapScalingMode(MyPreview, BitmapScalingMode.NearestNeighbor);

        }

        //private void Window1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    if (MainWindow.IsClosing == false)
        //    {
        //        //ウィンドウは閉じないで隠して見えなくする            
        //        e.Cancel = true;
        //        this.Hide();
        //    }
        //}
    }
}
