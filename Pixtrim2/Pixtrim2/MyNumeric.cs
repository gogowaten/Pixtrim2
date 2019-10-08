using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using System.Text.RegularExpressions;


/// <summary>
/// Decimal型のNumericUpDownのようなもの
/// StackPanel
///     ┣TextBox    数値表示用
///     ┗ScrollBar  数値変化用
///     
/// 指定できる項目は
/// プロパティ名              内容
/// MyValue         数値(decimal型)、初期値は1.0
/// MyValue2        TextBoxに表示している数値(decimal型)外部からはgetのみ可能
/// MyMinimum       最小値、初期値 -10.0
/// MyMaximum       最大値、初期値 10.0
/// MySmallChange   変化値小、初期値 1.0
/// MyLargeChange   変化値大、初期値 10.0
/// MyWidth         TextBoxの横幅、初期値はdouble.NaNで自動調節
/// MyHeight        全体(StackPanel)の高さ、初期値はdouble.NaNで自動調節
/// MyFontSize      初期値はdouble.NaNで普通サイズ？
/// MyDigitsInteger  整数表示桁数、初期値は4
/// MyDigitsDecimal  小数点以下表示桁数、初期値は0
/// 
/// 値が 12.34 のとき
/// MyDigitsInteger = 4, MyDigitsDecimal = 0    0012
/// MyDigitsInteger = 4, MyDigitsDecimal = 2    0012.34
/// MyDigitsInteger = 1, MyDigitsDecimal = 2    12.34
/// MyDigitsInteger = 1, MyDigitsDecimal = 4    12.3400
/// 
/// 追加操作
/// TextBoxの上でマウスホイール回転でSmallChange
/// ScrollBarの上でマウスホイール回転でLargeChange
/// </summary>
namespace Pixtrim2
{
    class MyNumeric:StackPanel
    {

        private TextBox MyTextBox;
        private ScrollBar MyScrollBar;

        #region dependencyProperty依存関係プロパティ
        //内部数値
        public decimal MyValue
        {
            get => (decimal)GetValue(MyValueProperty);
            set => SetValue(MyValueProperty, value);
        }
        public static readonly DependencyProperty MyValueProperty =
            DependencyProperty.Register(nameof(MyValue), typeof(decimal), typeof(MyNumeric), new PropertyMetadata(1.0m, MyValuePropertyChanged));

        //表示している数値
        public decimal MyValue2
        {
            get => (decimal)GetValue(MyValue2Property);
            private set => SetValue(MyValue2Property, value);//外部からはsetできないように
        }
        public static readonly DependencyProperty MyValue2Property =
            DependencyProperty.Register(nameof(MyValue2), typeof(decimal), typeof(MyNumeric), new PropertyMetadata(1.0m));

        //値がセットされたときに上限加減のチェック
        private static void MyValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var nu = (MyNumeric)d;
            decimal v = nu.MyValue;
            if (v > nu.MyMaximum) { nu.MyValue = nu.MyMaximum; }
            else if (v < nu.MyMinimum) { nu.MyValue = nu.MyMinimum; }
        }


        public decimal MyMinimum
        {
            get => (decimal)GetValue(MyMinimumProperty);
            set => SetValue(MyMinimumProperty, value);
        }
        public static readonly DependencyProperty MyMinimumProperty =
            DependencyProperty.Register(nameof(MyMinimum), typeof(decimal), typeof(MyNumeric), new PropertyMetadata(-10.0m, MyValuePropertyChanged));

        public decimal MyMaximum
        {
            get => (decimal)GetValue(MyMaximumProperty);
            set => SetValue(MyMaximumProperty, value);
        }
        public static readonly DependencyProperty MyMaximumProperty =
            DependencyProperty.Register(nameof(MyMaximum), typeof(decimal), typeof(MyNumeric), new PropertyMetadata(10.0m, MyValuePropertyChanged));

        public decimal MySmallChange
        {
            get => (decimal)GetValue(MySmallChangeProperty);
            set => SetValue(MySmallChangeProperty, value);
        }
        public static readonly DependencyProperty MySmallChangeProperty =
            DependencyProperty.Register(nameof(MySmallChange), typeof(decimal), typeof(MyNumeric), new PropertyMetadata(1.0m));
        public decimal MyLargeChange
        {
            get => (decimal)GetValue(MyLargeChangeProperty);
            set => SetValue(MyLargeChangeProperty, value);
        }
        public static readonly DependencyProperty MyLargeChangeProperty =
            DependencyProperty.Register(nameof(MyLargeChange), typeof(decimal), typeof(MyNumeric), new PropertyMetadata(10.0m));

        public double MyWidth
        {
            get => (double)GetValue(MyWidthProperty);
            set => SetValue(MyWidthProperty, value);
        }
        public static readonly DependencyProperty MyWidthProperty =
            DependencyProperty.Register(nameof(MyWidth), typeof(double), typeof(MyNumeric), new PropertyMetadata(double.NaN));

        public double MyHeight
        {
            get => (double)GetValue(MyHeightProperty);
            set => SetValue(MyHeightProperty, value);
        }
        public static readonly DependencyProperty MyHeightProperty =
            DependencyProperty.Register(nameof(MyHeight), typeof(double), typeof(MyNumeric), new PropertyMetadata(double.NaN));

        public double MyFontSize
        {
            get => (double)GetValue(MyFontSizeProperty);
            set => SetValue(MyFontSizeProperty, value);
        }
        public static readonly DependencyProperty MyFontSizeProperty =
            DependencyProperty.Register(nameof(MyFontSize), typeof(double), typeof(MyNumeric), new PropertyMetadata(double.NaN));

        //整数部分の0埋め桁数
        public int MyDigitsInteger
        {
            get => (int)GetValue(MyDigitsIntegerProperty);
            set => SetValue(MyDigitsIntegerProperty, value);
        }
        public static readonly DependencyProperty MyDigitsIntegerProperty =
            DependencyProperty.Register(nameof(MyDigitsInteger), typeof(int), typeof(MyNumeric),
                new PropertyMetadata(4, MyDigitsPropertyChanged));

        //小数点以下部分の表示(0埋め)桁数
        public int MyDigitsDecimal
        {
            get => (int)GetValue(MyDigitsDecimalProperty);
            set => SetValue(MyDigitsDecimalProperty, value);
        }
        public static readonly DependencyProperty MyDigitsDecimalProperty =
            DependencyProperty.Register(nameof(MyDigitsDecimal), typeof(int), typeof(MyNumeric),
                new PropertyMetadata(0, MyDigitsPropertyChanged));

        //表示桁数変更時に表示を更新
        private static void MyDigitsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var nu = (MyNumeric)d;

            string str = "0";
            for (int i = 1; i < nu.MyDigitsInteger; i++)
            {
                str += "0";
            }
            str += ".";
            for (int i = 0; i < nu.MyDigitsDecimal; i++)
            {
                str += "0";
            }
            str = nu.MyValue.ToString(str);
            nu.MyTextBox.Text = str;
        }

        #endregion

        public MyNumeric()
        {
            Orientation = Orientation.Horizontal;
            HorizontalAlignment = HorizontalAlignment.Center;

            this.Loaded += MyNumeric_Loaded;

            MyTextBox = new TextBox
            {
                HorizontalContentAlignment = HorizontalAlignment.Right,
                VerticalContentAlignment = VerticalAlignment.Center,
            };

            MyScrollBar = new ScrollBar
            {
                RenderTransform = new RotateTransform(180),
                RenderTransformOrigin = new Point(0.5, 0.5),
            };

            this.Children.Add(MyTextBox);
            this.Children.Add(MyScrollBar);



            //Binding設定
            void MyBindingTwoWay(DependencyProperty source, FrameworkElement targetElement, DependencyProperty target)
            {
                Binding bb = new Binding()
                {
                    Source = this,
                    Path = new PropertyPath(source),
                    Mode = BindingMode.TwoWay,
                };
                targetElement.SetBinding(target, bb);
            }
            void MyBindingOneWay(DependencyProperty source, FrameworkElement targetElement, DependencyProperty target)
            {
                Binding bb = new Binding()
                {
                    Source = this,
                    Path = new PropertyPath(source),
                    Mode = BindingMode.OneWay,
                };
                targetElement.SetBinding(target, bb);
            }
            MyBindingTwoWay(MyValueProperty, MyScrollBar, ScrollBar.ValueProperty);
            MyBindingTwoWay(MyMaximumProperty, MyScrollBar, ScrollBar.MaximumProperty);
            MyBindingTwoWay(MyMinimumProperty, MyScrollBar, ScrollBar.MinimumProperty);
            MyBindingOneWay(MySmallChangeProperty, MyScrollBar, ScrollBar.SmallChangeProperty);
            MyBindingOneWay(MyLargeChangeProperty, MyScrollBar, ScrollBar.LargeChangeProperty);
            MyBindingTwoWay(MyWidthProperty, MyTextBox, TextBox.WidthProperty);
            MyBindingTwoWay(MyHeightProperty, MyTextBox, StackPanel.HeightProperty);
            MyBindingTwoWay(MyFontSizeProperty, MyTextBox, TextBox.FontSizeProperty);

            //textboxのBindingはConveterを使うので別に設定
            Binding b = new Binding
            {
                Source = this,
                Path = new PropertyPath(MyValueProperty),
                Mode = BindingMode.TwoWay,
                Converter = new MyConvert(),
                ConverterParameter = this,
            };
            MyTextBox.SetBinding(TextBox.TextProperty, b);

            //TextBoxの値をソースに、MyValue2をターゲット
            b = new Binding
            {
                Source = MyTextBox,
                Path = new PropertyPath(TextBox.TextProperty),
                Mode = BindingMode.OneWay,
            };
            BindingOperations.SetBinding(this, MyValue2Property, b);
        }

        #region イベント関連
        private void MyNumeric_Loaded(object sender, RoutedEventArgs e)
        {
            MyTextBox.GotFocus += MyTextBox_GotFocus;
            MyTextBox.TextChanged += MyTextBox_TextChanged;
            MyTextBox.MouseWheel += MyTextBox_MouseWheel;
            MyScrollBar.MouseWheel += MyScrollBar_MouseWheel;
        }

        //Scrollbarの上でマウスホイール回転で数値をLargeChange
        private void MyScrollBar_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollBar sb = (ScrollBar)sender;
            if (e.Delta > 0)
            {
                MyValue += this.MyLargeChange;
            }
            else
            {
                MyValue -= this.MyLargeChange;
            }
        }

        //TextBoxの上でマウスホイール回転でSmallChange
        private void MyTextBox_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (e.Delta > 0)
            {

                decimal i = MyValue + this.MySmallChange;
                if (MyMaximum < i) i = MyMaximum;
                MyValue = i;
            }
            else
            {
                decimal i = MyValue - this.MySmallChange;
                if (this.MyMinimum > i) i = MyMinimum;
                MyValue = i;
            }
        }

        //のぶろぐ[WPF] テキストボックスに数字以外は受け付けない簡単な方法
        //http://shen7113.blog.fc2.com/blog-entry-22.html
        //正規表現で数値以外は削除using System.Text.RegularExpressions;
        private void MyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            double d;
            if (!double.TryParse(box.Text, out d))
            {
                box.Text = Regex.Replace(box.Text, "[^0-9-]", "");
            }
        }

        //[WPF] TEXTBOX でフォーカス時にマウスクリックでもテキストを全選択する v2 | rksoftware
        //https://rksoftware.wordpress.com/2016/09/06/001-48/
        //[WPF] TextBox でフォーカス時にマウスクリックでもテキストを全選択する | rksoftware
        //https://rksoftware.wordpress.com/2016/06/17/001-38/
        //TextBoxフォーカス時にテキスト全選択
        private void MyTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            //box.SelectAll();
            this.Dispatcher.InvokeAsync(() =>
            {
                Task.Delay(10); box.SelectAll();
            });
        }
        #endregion


        //TextBoxに表示されている値を取得
        public string GetText()
        {
            return MyTextBox.Text;
        }

    }


    //TextBoxの文字列値と数値のBindingに必要なConverter
    //書式設定と最大値、最小値を超えていないかチェックもする
    public class MyConvert : IValueConverter
    {
        /// <summary>
        /// 数値を文字列に変換、指定桁数分を0で埋めた書式で返す
        /// 
        /// ToStringの書式作成
        /// 整数部分と小数点以下部分を指定された桁ぶんを表示、溢れた部分は0埋めか切り捨てになる
        /// 整数桁3、少数桁4なら書式は"000.0000"で
        /// 3.14の表示は 003.1400 になる
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter">MyNumeric自身</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var param = (MyNumeric)parameter;
            string str = "0";//書式作成
            for (int i = 1; i < param.MyDigitsInteger; i++)
            {
                str += "0";
            }
            str += ".";
            for (int i = 0; i < param.MyDigitsDecimal; i++)
            {
                str += "0";
            }
            decimal v = (decimal)value;
            //上限下限チェック
            if (v > param.MyMaximum) v = param.MyMaximum;
            else if (v < param.MyMinimum) v = param.MyMinimum;
            //
            //int ii = (int)v;//切り捨て
            str = v.ToString(str);
            return str;
        }

        /// <summary>
        /// stringをdoubleに変換して返す
        /// doubleに変換したあとは最大値、最小値を超えていないかチェック
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter">MyNumeric</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MyNumeric param = (MyNumeric)parameter;
            string str = (string)value;
            decimal v = decimal.Parse(str);
            if (v > param.MyMaximum) v = param.MyMaximum;
            else if (v < param.MyMinimum) v = param.MyMinimum;
            return v;
        }
    }
}
