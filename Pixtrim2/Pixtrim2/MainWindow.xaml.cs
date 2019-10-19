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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

using System.ComponentModel;


namespace Pixtrim2
{
    public enum SaveImageType
    {
        png = 0,
        jpg,
        bmp,
        gif,
        tiff
    }
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string CONFIG_FILE_NAME = "MyConfig.xml";
        private ClipboardWatcher ClipboardWatcher;
        private ObservableCollection<MyBitmapAndName> ListMyBitmapSource;
        private TrimThumb MyTrimThumb;//切り取り範囲
        private Window1 window1;//プレビュー用        
        private Config MyConfig;//設定
        //private BitmapSource PastBitmap;//前回クリップボードから取得した画像、比較用、不具合回避用→不具合じゃなかったので必要なくなった
        private ContextMenu MyListBoxContextMenu;
        private System.Media.SoundPlayer MySound;//画像取得時の音


        public MainWindow()
        {
            InitializeComponent();



            ButtonTest.Click += ButtonTest_Click;
            ButtonSave.Click += ButtonSave_Click;
            ButtonPreview.Click += ButtonPreview_Click;
            ButtonSaveDirSelect.Click += ButtonSaveDirSelect_Click;
            ButtonSaveDirOpen.Click += ButtonSaveDirOpen_Click;
            ButtonSaveDirPaste.Click += ButtonSaveDirPaste_Click;
            ButtonLoadConfig.Click += ButtonLoadConfig_Click;
            ButtonSaveConfig.Click += ButtonSaveConfig_Click;
            ButtonAddTrimSetting.Click += ButtonAddTrimSetting_Click;
            ButtonRemoveTrimSetting.Click += ButtonRemoveTrimSetting_Click;
            ButtonZoomIn.Click += ButtonZoomIn_Click;
            ButtonZoomOut.Click += ButtonZoomOut_Click;
            //scale
            ButtonScale1_9.Click += (s, e) => MyNumericSaveScale.MyValue = 1 / 9m;
            ButtonScale1_8.Click += (s, e) => MyNumericSaveScale.MyValue = 1 / 8m;
            ButtonScale1_7.Click += (s, e) => MyNumericSaveScale.MyValue = 1 / 7m;
            ButtonScale1_6.Click += (s, e) => MyNumericSaveScale.MyValue = 1 / 6m;
            ButtonScale1_5.Click += (s, e) => MyNumericSaveScale.MyValue = 1 / 5m;
            ButtonScale1_4.Click += (s, e) => MyNumericSaveScale.MyValue = 1 / 4m;
            ButtonScale1_3.Click += (s, e) => MyNumericSaveScale.MyValue = 1 / 3m;
            ButtonScale1_2.Click += (s, e) => MyNumericSaveScale.MyValue = 1 / 2m;
            ButtonScale1_1.Click += (s, e) => MyNumericSaveScale.MyValue = 1;

            //音声
            ButtonSoundSelect.Click += ButtonSoundSelect_Click;
            ButtonSoundPlay.Click += ButtonSoundPlay_Click;


            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            //this.Closed += MainWindow_Closed;
            CheckBoxIsClipboardWatch.Click += CheckBox_ClipCheck_Click;
            MyListBox.SelectionChanged += MyListBox_SelectionChanged;

            //ListBitmap = new List<BitmapSource>();

            ListMyBitmapSource = new ObservableCollection<MyBitmapAndName>();

            //リストボックス
            MyButtonRemoveSelectedImtem.Click += MyButtonRemoveSelectedImtem_Click;
            ButtonRemoveAllItems.Click += (s, e) => { ListMyBitmapSource.Clear(); };
            ButtonAddItemFromClipboard.Click += ButtonAddItemFromClipboard_Click;
            MyListBox.DataContext = ListMyBitmapSource;
            MakeContextMenu();


            //切り取り範囲Thumb初期化
            MyTrimThumb = new TrimThumb(MyCanvas, 20, (int)MyNumericX.MyValue2, 100, 100, 100);
            TextBoxDammy.PreviewKeyDown += MyTrimThumb_KeyDown;
            MyTrimThumb.PreviewMouseDown += (o, e) => { TextBoxDammy.Focus(); Keyboard.Focus(TextBoxDammy); };
            MyTrimThumb.MouseDown += (o, e) => { TextBoxDammy.Focus(); };
            MyTrimThumb.SetBackGroundColor(Color.FromArgb(100, 128, 128, 128));
            MyCanvas.Children.Add(MyTrimThumb);


            //画像形式コンボボックス初期化
            MyComboBoxSaveImageType.ItemsSource = Enum.GetValues(typeof(SaveImageType));
            //ComboBoxSaveImageType.SelectedIndex = 0;

            //切り抜き範囲コンボボックス初期化
            MyComboBoxTrimSetting.SelectionChanged += MyComboBoxTrimSetting_SelectionChanged;

            //            C# で実行ファイルのフォルダを取得
            //http://var.blog.jp/archives/66978870.html
            //実行ファイルのディレクトリ取得3種
            //string str = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //str = System.IO.Directory.GetCurrentDirectory();
            //str = Environment.CurrentDirectory;

            //前回終了時の設定ファイル読み込み、ファイルの場所はアプリと同じフォルダ
            MyConfig = new Config();
            string fullPath = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location) +
                "\\" + CONFIG_FILE_NAME;
            //ファイルの存在確認して読み込み
            if (System.IO.File.Exists(fullPath))
            {
                LoadConfig(fullPath);
                //音声ファイルの読み込み
                if (System.IO.File.Exists(MyConfig.SoundDir))
                {
                    MySound = new System.Media.SoundPlayer(MyConfig.SoundDir);
                }
                //音声ファイル指定なしの場合は内蔵音源使用
                else { MySound = new System.Media.SoundPlayer("pekowave2.wav"); }
            }
            //初回起動時は設定ファイルがないので初期値を指定する
            else
            {
                SetDefaultConfig();

            }


            //MyCanvasの初期設定
            MyCanvas.RenderTransform = new ScaleTransform(1, 1);
            //MyCanvasの拡大表示方式をニアレストネイバー法に指定
            RenderOptions.SetBitmapScalingMode(MyCanvas, BitmapScalingMode.NearestNeighbor);


        }

        //既定値に設定
        private void SetDefaultConfig()
        {
            MyConfig.Height = 100;
            MyConfig.JpegQuality = 97;
            MyConfig.Left = 100;
            MyConfig.SavaDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            MyConfig.SaveImageType = SaveImageType.png;
            MyConfig.SaveScale = 1;
            MyConfig.Top = 100;
            MyConfig.Width = 100;
            MyConfig.IsAutoRemoveSavedItem = false;
            MyConfig.IsClipboardWatch = false;
            MyConfig.IsPlaySound = false;
            MyConfig.IsAutoSave = false;

            //内蔵音源
            MySound = new System.Media.SoundPlayer("pekowave2.wav");

            MySetBinding();
        }

        private void ButtonZoomOut_Click(object sender, RoutedEventArgs e)
        {
            //表示の縮小、等倍以下にはしない
            ScaleTransform scale = (ScaleTransform)MyCanvas.RenderTransform;
            if (scale.ScaleX == 1) return;
            scale.ScaleX--;
            scale.ScaleY--;
            CanvasSizeSuitable();//MyCanvasを適切なサイズに変更
        }

        private void ButtonZoomIn_Click(object sender, RoutedEventArgs e)
        {
            //表示の拡大、整数倍
            ScaleTransform scale = (ScaleTransform)MyCanvas.RenderTransform;
            scale.ScaleX++;
            scale.ScaleY++;
            CanvasSizeSuitable();//MyCanvasを適切なサイズに変更
        }
        //MyCanvasを適切なサイズに変更
        private void CanvasSizeSuitable()
        {
            var bn = (MyBitmapAndName)MyListBox.SelectedItem;
            if (bn == null) return;
            //Canvasのサイズ変更
            ScaleTransform st = (ScaleTransform)MyCanvas.RenderTransform;
            double scale = st.ScaleX;
            MyCanvas.Width = bn.Source.PixelWidth * scale + 10;
            MyCanvas.Height = bn.Source.PixelHeight * scale + 10;
        }


        //切り抜き範囲コンボボックスから選択項目削除
        private void ButtonRemoveTrimSetting_Click(object sender, RoutedEventArgs e)
        {
            RemoveTrimSetting();
        }
        private void RemoveTrimSetting()
        {
            var config = MyComboBoxTrimSetting.SelectedItem;
            if (config == null) return;
            MyConfig.TrimConfigList.Remove((TrimConfig)config);
            //コンボボックスをリフレッシュ、これで削除が反映される
            MyComboBoxTrimSetting.Items.Refresh();
            //MyComboBoxTrimSetting.SelectedIndex = 0;
        }


        //切り抜き範囲の設定をコンボボックスに追加
        private void ButtonAddTrimSetting_Click(object sender, RoutedEventArgs e)
        {
            AddTrimSetting();
        }
        //名前を付けてリストに追加、設定保存
        private void AddTrimSetting()
        {
            //名前入力ダイアログボックス表示
            MyDialogWindow dialog = new MyDialogWindow(this);
            dialog.ShowDialog();

            if (dialog.DialogResult == true)
            {
                if (dialog.Answer == "") return;//空白なら何もしないで終了
                //設定作成
                TrimConfig trimConfig = new TrimConfig
                {
                    Name = dialog.Answer,
                    Top = MyConfig.Top,
                    Left = MyConfig.Left,
                    Width = MyConfig.Width,
                    Height = MyConfig.Height,
                    SaveScale = MyConfig.SaveScale,
                };
                //リストに追加
                MyConfig.TrimConfigList.Add(trimConfig);
                //MyComboBoxTrimSetting.Items.Add(trimConfig);//コレだとエラーになる
                //追加したあとにリフレッシュするとリストに表示される
                MyComboBoxTrimSetting.Items.Refresh();
                MyComboBoxTrimSetting.SelectedItem = trimConfig;

            }
        }

        //切り抜き範囲コンボボックスの選択項目変更時
        //設定を反映
        private void MyComboBoxTrimSetting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MyComboBoxTrimSetting.SelectedIndex < 0) return;
            SetTrimConfig(MyConfig.TrimConfigList[MyComboBoxTrimSetting.SelectedIndex]);
        }
        private void SetTrimConfig(TrimConfig config)
        {
            MyConfig.Top = config.Top;
            MyConfig.Left = config.Left;
            MyConfig.Width = config.Width;
            MyConfig.Height = config.Height;
            MyConfig.SaveScale = config.SaveScale;

        }

        //今のクリップボードから画像を追加
        //この場合もし自動保存設定なら一時停止する、追加したら戻すようにしようとしたけどやっぱやめた
        private void ButtonAddItemFromClipboard_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource bitmap = GetBitmapSource();
            if (bitmap == null) return;
            //if (MyConfig.IsAutoSave)
            //{
            //    MyConfig.IsAutoSave = false;
            //    AddBitmapToList(bitmap);
            //    MyConfig.IsAutoSave = true;
            //}
            AddBitmapToList(bitmap);
        }



        #region 右クリックメニュー
        //リストボックスの右クリックメニュー
        //削除
        //保存
        //切り抜かないで保存

        private void MakeContextMenu()
        {
            MyListBoxContextMenu = new ContextMenu();
            MyListBox.ContextMenu = MyListBoxContextMenu;
            var item = new MenuItem();
            item.Header = "削除(_D)";
            item.Click += Item_Click;
            MyListBoxContextMenu.Items.Add(item);
            //メニューを表示するとき、アイテムがなければ直前でキャンセルして表示しない
            MyListBox.ContextMenuOpening += (s, e) => { if (MyListBox.Items.Count == 0) e.Handled = true; };


            //
            var cm = new ContextMenu();

            MyCanvas.ContextMenu = cm;
            item = new MenuItem();
            item.Header = "レイアウト1";
            item.Click += (s, e) => { Column0.Width = new GridLength(250); };
            cm.Items.Add(item);
            item = new MenuItem();
            item.Header = "レイアウト2";
            item.Click += (s, e) => { Column0.Width = new GridLength(0); };
            cm.Items.Add(item);

            item = new MenuItem();
            cm.Items.Add(item);
            item.Header = "切り抜いてクリップボードにコピー";
            item.Click += Item_Click1;

        }


        //クリップボードへ切り抜き画像をコピー、スケールも反映される
        private void Item_Click1(object sender, RoutedEventArgs e)
        {
            MyBitmapAndName item = (MyBitmapAndName)MyListBox.SelectedItem;
            if (item == null) return;
            if (CheckCropRect(item.Source) == false)
            {
                MessageBox.Show("切り抜き範囲が画像内に収まっていないので処理できませんでした");
                return;
            }
            ClipboardWatcher.Stop();//監視を停止

            BitmapSource bitmap = MakeSaveBitmap(item.Source, false);
            Clipboard.Clear();//クリップボードクリア(おまじない)
            //Clipboard.SetDataObject(item.Source);//コレだとなぜかコピーされない
            //Clipboard.SetImage(bitmap);//コピー、たまに失敗する
            if (MySetImageClipboard(bitmap))
            {
                MessageBox.Show("コピーしました");
            }
            else
            {
                MessageBox.Show("コピーに失敗しました");
            }

            ClipboardWatcher.Start();//監視を再開
        }


        private bool MySetImageClipboard(BitmapSource bitmap)
        {
            int count = 1;
            int limit = 5;//試行回数、5あれば十分
            do
            {
                try
                {
                    //MySleep(10);//10ミリ秒アプリを停止、コレがあると成功率が上がる気がする
                    Clipboard.SetImage(bitmap);//稀にエラー
                    //MessageBox.Show($"{count}");
                    return true;
                }
                catch (Exception)
                {
                    count++;
                }
                finally { }
            } while (limit >= count);

            return false;
        }

        //指定時間アプリを停止、ミリ秒
        private async void MySleep(int millisecond)
        {
            await Task.Delay(millisecond);
        }

        private void Item_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelectedListItem();
        }


        #endregion



        //アプリ終了時
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            //クリップボード監視を停止
            ClipboardWatcher.Stop();
            //今の設定をファイルに保存
            string fullpath = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\" + CONFIG_FILE_NAME;
            SaveConfig(fullpath);
        }


        //設定を読み込んだあとはこれを実行
        private void MySetBinding()
        {
            //MyNumericX.SetBinding(MyNumeric.MyValueProperty, new Binding(nameof(MyConfig.Left)));
            //↑これだとBindingにならないので↓
            Binding b;
            b = MakeBinding(nameof(MyConfig.Left));
            MyNumericX.SetBinding(MyNumeric.MyValueProperty, b);
            MyTrimThumb.SetBinding(LeftProperty, b);
            b = MakeBinding(nameof(MyConfig.Top));
            MyNumericY.SetBinding(MyNumeric.MyValueProperty, b);
            MyTrimThumb.SetBinding(TopProperty, b);
            b = MakeBinding(nameof(MyConfig.Width));
            MyNumericW.SetBinding(MyNumeric.MyValueProperty, b);
            MyTrimThumb.SetBinding(WidthProperty, b);
            b = MakeBinding(nameof(MyConfig.Height));
            MyNumericH.SetBinding(MyNumeric.MyValueProperty, b);
            MyTrimThumb.SetBinding(HeightProperty, b);


            //MyConfig.SaveScale;//切り抜き後のサイズ変更
            MyNumericSaveScale.SetBinding(MyNumeric.MyValueProperty, MakeBinding(nameof(Config.SaveScale)));
            //MyConfig.JpegQuality;
            MyNumericJpegQuality.SetBinding(MyNumeric.MyValueProperty, MakeBinding(nameof(Config.JpegQuality)));
            //MyConfig.IsPlaySound;
            CheckBoxSoundPlay.SetBinding(CheckBox.IsCheckedProperty, MakeBinding(nameof(Config.IsPlaySound)));
            //MyConfig.SaveImageType;
            MyComboBoxSaveImageType.SetBinding(ComboBox.SelectedValueProperty, MakeBinding(nameof(Config.SaveImageType)));
            //MyConfig.SoundDir;
            TextBoxSoundDir.SetBinding(TextBox.TextProperty, MakeBinding(nameof(Config.SoundDir)));
            //MyConfig.FileName;
            //TextBoxFileName.SetBinding(TextBox.TextProperty, MakeBinding(nameof(Config.FileName)));
            //MyConfig.SavaDir;
            TextBoxSaveDir.SetBinding(TextBox.TextProperty, MakeBinding(nameof(Config.SavaDir)));
            //MyConfig.IsAutoRemoveSavedItem;
            CheckBoxIsAutoRemoveSavedItem.SetBinding(CheckBox.IsCheckedProperty, MakeBinding(nameof(Config.IsAutoRemoveSavedItem)));
            //MyConfig.IsClipboardWatch;
            CheckBoxIsClipboardWatch.SetBinding(CheckBox.IsCheckedProperty, MakeBinding(nameof(Config.IsClipboardWatch)));
            //MyConfig.IsAutoSave;
            CheckBoxIsAutoSave.SetBinding(CheckBox.IsCheckedProperty, MakeBinding(nameof(Config.IsAutoSave)));

            //切り抜き範囲設定リスト
            MyComboBoxTrimSetting.ItemsSource = MyConfig.TrimConfigList;
            //var neko = MyConfig.TrimConfigList[0];

            //this.DataContext = MyConfig;//要らないみたい
        }

        private Binding MakeBinding(string config)
        {
            var b = new Binding()
            {
                Source = MyConfig,
                Path = new PropertyPath(config),
                Mode = BindingMode.TwoWay,
            };
            return b;
        }

        #region 設定の読み書き

        //設定の保存
        private void ButtonSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.DefaultExt = ".config";
            dialog.Filter = "*.config|*.config";
            dialog.AddExtension = true;

            if (dialog.ShowDialog() == true)
            {
                if (SaveConfig(dialog.FileName)) MessageBox.Show("保存できたよ");
            }
        }
        private bool SaveConfig(string fullPath)
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(Config));
            try
            {
                using (var stream = new System.IO.StreamWriter(fullPath, false, new System.Text.UTF8Encoding(false)))
                {
                    serializer.Serialize(stream, MyConfig);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"なんかエラーで設定の保存できんかったわ\n" +
                    $"{ex.Message}");
                return false;
            }
        }

        //設定の読み込み
        private void ButtonLoadConfig_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "*.config|*.config";
            if (dialog.ShowDialog() == true)
            {
                LoadConfig(dialog.FileName);
            }
        }
        private bool LoadConfig(string fullPath)
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(Config));
            try
            {
                using (var stream = new System.IO.StreamReader(fullPath, new UTF8Encoding(false)))
                {
                    MyConfig = (Config)serializer.Deserialize(stream);
                }
                MySetBinding();//再Binding
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"なんかエラーで設定の読み込みできんかったわ\n" +
                    $"{ex.Message}");
                //既定値に設定する
                SetDefaultConfig();
                return false;
            }
        }
        #endregion

        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            var neko = MyComboBoxTrimSetting.SelectedItem;
            var index = MyComboBoxTrimSetting.SelectedIndex;
            var con = MyConfig;
            var items = MyComboBoxTrimSetting.Items;
            var sc = MyComboBoxTrimSetting.Items.SourceCollection;
            MyComboBoxTrimSetting.Items.Refresh();
            var top = MyNumericY.MyValue;
            var nu = MyNumericY.MyValue2;
            var left = MyNumericX.MyValue;

        }


        #region 音
        private void ButtonSoundPlay_Click(object sender, RoutedEventArgs e)
        {
            PlaySoundFile();
        }
        //音声ファイル再生
        private void PlaySoundFile()
        {
            try
            {
                MySound.Play();
            }
            catch (Exception)
            {
                //MessageBox.Show($"指定されたファイルは再生できなかったよ\n" +
                //    $"再生できる音声ファイルは、wav形式だけ");
            }
        }
        //音声ファイルの選択
        private void ButtonSoundSelect_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "(wav)|*.wav";
            if (dialog.ShowDialog() == true)
            {
                //TextBoxSoundDir.Text = dialog.FileName;//これだとBindingが解けてしまうので
                MyConfig.SoundDir = dialog.FileName;
                MySound = new System.Media.SoundPlayer(dialog.FileName);
            }
        }
        #endregion

        #region その他

        ////        WPF、2つのBitmapSource比較をMD5ハッシュ値で行ってみた - 午後わてんのブログ
        ////https://gogowaten.hatenablog.com/entry/2019/10/03/205130
        //#region BitmapSourceの比較
        ///// <summary>
        ///// 2つのBitmapSourceが同じ画像(のすべてのピクセルの色)なのか判定する、MD5のハッシュ値を作成して比較
        ///// </summary>
        ///// <param name="bmp1"></param>
        ///// <param name="bmp2"></param>
        ///// <returns></returns>
        //private bool IsBitmapEqual(BitmapSource bmp1, BitmapSource bmp2)
        //{
        //    if (bmp1 == null || bmp2 == null) return false;
        //    //それぞれのハッシュ値を作成
        //    var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        //    byte[] h1 = md5.ComputeHash(MakeBitmapByte(bmp1));
        //    byte[] h2 = md5.ComputeHash(MakeBitmapByte(bmp2));
        //    md5.Clear();
        //    //ハッシュ値を比較
        //    return IsArrayEquals(h1, h2);

        //}
        ////2つのハッシュ値を比較
        //private bool IsArrayEquals(byte[] h1, byte[] h2)
        //{
        //    for (int i = 0; i < h1.Length; i++)
        //    {
        //        if (h1[i] != h2[i])
        //        {
        //            return false;
        //        }
        //    }
        //    return true;
        //}
        ////BitmapSourceをbyte配列に変換
        //private byte[] MakeBitmapByte(BitmapSource bitmap)
        //{
        //    int w = bitmap.PixelWidth;
        //    int h = bitmap.PixelHeight;
        //    int stride = w * bitmap.Format.BitsPerPixel / 8;
        //    byte[] pixels = new byte[h * stride];
        //    bitmap.CopyPixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
        //    return pixels;
        //}
        //#endregion

        //フォルダの存在確認、なければマイドキュメントのパスを返す
        private string CheckDir(string path)
        {
            if (System.IO.Directory.Exists(path) == false)
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            return path;
        }
        //切り抜き範囲を画像内に収める、移動させる     
        private void TrimAreaCorrection()
        {
            var right = MyNumericW.MyValue2 + MyNumericX.MyValue2;
            var bottom = MyNumericH.MyValue2 + MyNumericY.MyValue2;
            BitmapSource bmp = (BitmapSource)MyImage.Source;
            var imgWidht = bmp.PixelWidth;
            var imgHeight = bmp.PixelHeight;
            var xDiff = right - imgWidht;//範囲内ならマイナス値
            var yDiff = bottom - imgHeight;
            if (xDiff > 0)
            {
                if (MyNumericX.MyValue - xDiff < 0)
                {
                    MyNumericW.MyValue = imgWidht;
                }
                MyNumericX.MyValue -= xDiff;
            }
            if (yDiff > 0)
            {
                if (MyNumericY.MyValue - yDiff < 0)
                {
                    MyNumericH.MyValue = imgHeight;
                }
                MyNumericY.MyValue -= yDiff;
            }
        }


        //  画像の拡縮、最近傍補間法
        private BitmapSource NearestnaverScale(BitmapSource bitmap, decimal scale)
        {
            int w = bitmap.PixelWidth;
            int h = bitmap.PixelHeight;
            int stride = w * 4;
            byte[] pixels = new byte[h * stride];
            bitmap.CopyPixels(pixels, stride, 0);


            int ww = (int)Math.Round(w * scale);
            int hh = (int)Math.Round(h * scale);
            int stride2 = ww * 4;
            byte[] pixels2 = new byte[hh * stride2];
            int motoP, pp;
            decimal rate = 1 / scale;
            for (int y = 0; y < hh; y++)
            {
                for (int x = 0; x < ww; x++)
                {
                    //元の画像の座標は小数点以下切り捨てが正解、四捨五入だとあふれる
                    int motoX = (int)Math.Floor(x * rate);// (int)Math.Round(x * rate) ;
                    int motoY = (int)Math.Floor(y * rate);
                    motoP = motoY * stride + motoX * 4;

                    pp = y * stride2 + x * 4;
                    pixels2[pp] = pixels[motoP];
                    pixels2[pp + 1] = pixels[motoP + 1];
                    pixels2[pp + 2] = pixels[motoP + 2];
                    pixels2[pp + 3] = pixels[motoP + 3];
                }
            }

            return BitmapSource.Create(ww, hh, 96, 96, bitmap.Format, null, pixels2, stride2);
        }




        //プレビュー
        private void ButtonPreview_Click(object sender, RoutedEventArgs e)
        {
            if (ListMyBitmapSource.Count == 0) return;
            var bs = (MyBitmapAndName)MyListBox.SelectedItem;
            if (CheckCropRect(bs.Source))
            {
                //画像を切り抜いて拡大
                BitmapSource img = NearestnaverScale(MakeCroppedBitmap(bs.Source, false), MyConfig.SaveScale);
                //表示ウィンドウの設定して表示
                window1 = new Window1();
                window1.MaxWidth = this.ActualWidth;
                window1.MaxHeight = this.ActualHeight;
                window1.MyPreview.Source = img;// bmp;            
                window1.ShowDialog();
                window1.Owner = this;
                window1.SizeToContent = SizeToContent.WidthAndHeight;//ウィンドウ自動サイズ
            }
            else { MessageBox.Show("切り抜き範囲が画像に収まっていないのでプレビューできない"); };
            //切り抜いた画像を拡縮表示は別ウィンドウに表示して、それをBitmapレンダーで保存？
            //できたけど、別ウィンドウを表示した状態visivleじゃないと画像が更新されない
            //自分で拡縮したほうがいいかも？
        }

        #endregion


        #region キーボードで操作


        private void MyTrimThumb_KeyDown(object sender, KeyEventArgs e)
        {
            //ctrl+Shift、サイズ変更、ラージ
            if (e.KeyboardDevice.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                switch (e.Key)
                {
                    case Key.Left:
                        MyNumericW.MyValue -= MyNumericW.MyLargeChange;
                        break;
                    case Key.Up:
                        MyNumericH.MyValue -= MyNumericH.MyLargeChange;
                        break;
                    case Key.Right:
                        MyNumericW.MyValue += MyNumericW.MyLargeChange;
                        break;
                    case Key.Down:
                        MyNumericH.MyValue += MyNumericH.MyLargeChange;
                        break;
                    default:
                        break;
                }
            }
            //ctrl+、移動、ラージ
            else if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.Left:
                        MyNumericX.MyValue -= MyNumericY.MyLargeChange;
                        break;
                    case Key.Up:
                        MyNumericY.MyValue -= MyNumericY.MyLargeChange;
                        break;
                    case Key.Right:
                        MyNumericX.MyValue += MyNumericY.MyLargeChange;
                        break;
                    case Key.Down:
                        MyNumericY.MyValue += MyNumericY.MyLargeChange;
                        break;
                    case Key.P:
                        break;
                    case Key.S:
                        break;
                    default:
                        break;
                }
            }
            //shift+、サイズ変更、スモール
            else if (e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
            {
                switch (e.Key)
                {
                    case Key.Left:
                        MyNumericW.MyValue -= MyNumericW.MySmallChange;
                        break;
                    case Key.Up:
                        MyNumericH.MyValue -= MyNumericH.MySmallChange;
                        break;
                    case Key.Right:
                        MyNumericW.MyValue += MyNumericW.MySmallChange;
                        break;
                    case Key.Down:
                        MyNumericH.MyValue += MyNumericH.MySmallChange;
                        break;
                    default:
                        break;
                }
            }
            //移動、スモール
            else
            {
                switch (e.Key)
                {
                    case Key.Left:
                        MyNumericX.MyValue -= MyNumericY.MySmallChange;
                        break;
                    case Key.Up:
                        MyNumericY.MyValue -= MyNumericY.MySmallChange;
                        break;
                    case Key.Right:
                        MyNumericX.MyValue += MyNumericY.MySmallChange;
                        break;
                    case Key.Down:
                        MyNumericY.MyValue += MyNumericY.MySmallChange;
                        break;
                    default:
                        break;
                }
            }
        }


        #endregion


        #region 保存フォルダ
        //パスの貼り付け
        //        文字列から特定の文字列を取り除くには？［C#／VB］：.NET TIPS - ＠IT
        //https://www.atmarkit.co.jp/ait/articles/0711/15/news142.html

        private void ButtonSaveDirPaste_Click(object sender, RoutedEventArgs e)
        {
            //エクスプローラーのパスのコピーからだと " がついているので取り除く
            string str = Clipboard.GetText();
            string target = "\"";
            str = str.Replace(target, "");
            if (System.IO.Directory.Exists(str))
            {
                //TextBoxSaveDir.Text = str;//これだとBindingが解けてしまうので
                MyConfig.SavaDir = str;

            }
            else
            {
                MessageBox.Show(str.ToString() + "というフォルダは見当たらない");
            }

        }


        //保存フォルダを開く
        private void ButtonSaveDirOpen_Click(object sender, RoutedEventArgs e)
        {
            string dir = MyConfig.SavaDir;
            if (System.IO.Directory.Exists(dir))
            {
                System.Diagnostics.Process.Start(dir);//フォルダを開く
            }
            else
            {
                MessageBox.Show("\"" + dir.ToString() + "\"" + "というフォルダは見当たらない");
            }
        }

        //保存フォルダ選択
        private void ButtonSaveDirSelect_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderDialog(CheckDir(MyConfig.SavaDir), this);
            dialog.ShowDialog();
            if (dialog.DialogResult == true)
            {
                //TextBoxSaveDir.Text = dialog.FileName;//これだとBindingが解けてしまう
                MyConfig.SavaDir = dialog.GetFullPath();
            }

        }

        #endregion


        #region 画像保存

        //画像の保存
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveImage();
        }

        //画像の保存
        private void SaveImage()
        {
            if (ListMyBitmapSource.Count == 0) return;
            //保存フォルダの存在チェック
            if (CheckSaveFolderExists() == false)
            {
                MessageBox.Show("指定された保存場所が見つからないので保存できないよ");
                return;
            }

            //切り抜き範囲チェック
            //一つでも範囲外なら保存しない
            foreach (var item in ListMyBitmapSource)
            {
                if (CheckCropRect(item.Source) == false)
                {
                    string str = $"切り抜き範囲が画像範囲外なので保存できないよ\n" +
                        $"{item.Name}";
                    MyListBox.SelectedItem = item;
                    MyListBox.ScrollIntoView(item);
                    MessageBox.Show(str);
                    return;
                }
            }

            var savedItems = new List<MyBitmapAndName>();
            //リストの画像全部を保存
            for (int i = 0; i < ListMyBitmapSource.Count; i++)
            {
                //保存と自動削除モードなら成功したアイテムをリスト化
                if (SaveBitmap(ListMyBitmapSource[i]) && MyConfig.IsAutoRemoveSavedItem)
                {
                    savedItems.Add(ListMyBitmapSource[i]);
                }
            }
            //自動削除
            foreach (var item in savedItems)
            {
                ListMyBitmapSource.Remove(item);
            }
            MessageBox.Show("保存完了");
        }

        //保存フォルダの存在チェック
        private bool CheckSaveFolderExists()
        {
            return System.IO.Directory.Exists(MyConfig.SavaDir);
        }


        /// <summary>
        /// 画像保存の前段階、切り抜きとスケールを行う、切り抜き範囲が適合しなかった場合はnullを返す
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="IsRectCheck">切り抜き範囲チェックの有無、falseならチェックしないで切り抜く</param>
        /// <returns></returns>
        private BitmapSource MakeSaveBitmap(BitmapSource bitmap, bool IsRectCheck)
        {
            //切り抜き
            if (IsRectCheck)
            {
                if (CheckCropRect(bitmap) == false) return null;
            }
            bitmap = MakeCroppedBitmap(bitmap, false);
            //スケール
            if (MyConfig.SaveScale != 1)
            {
                bitmap = NearestnaverScale(bitmap, MyConfig.SaveScale);
            }
            return bitmap;
        }

        /// <summary>
        /// リストの画像を保存
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool SaveBitmap(MyBitmapAndName data)
        {
            return SaveBitmap(MakeSaveBitmap(data.Source, false), data.Name);
        }

        /// <summary>
        /// 切り抜きとスケール済みのBitmapをファイルに保存
        /// </summary>
        /// <param name="bitmap">切り抜きとスケール済のBitmap</param>
        /// <param name="fileName">拡張子付きのファイル名</param>
        /// <returns></returns>
        private bool SaveBitmap(BitmapSource bitmap, string fileName)
        {
            //CroppedBitmapで切り抜いた画像でBitmapFrame作成して保存
            BitmapEncoder encoder = GetEncoder();
            //メタデータ作成、アプリ名記入
            BitmapMetadata meta = MakeMetadata();
            encoder.Frames.Add(BitmapFrame.Create(bitmap, null, meta, null));
            try
            {
                using (var fs = new System.IO.FileStream(
                    MakeFullPath(fileName), System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    encoder.Save(fs);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{fileName}はファイル保存できなかったよ\n" +
                    $"{ex.Message}");
                return false;
            }

        }



        //メタデータ作成
        private BitmapMetadata MakeMetadata()
        {
            BitmapMetadata data = null;
            switch (MyConfig.SaveImageType)
            {
                case SaveImageType.png:
                    data = new BitmapMetadata("png");
                    data.SetQuery("/tEXt/Software", "Pixtrim2");
                    break;
                case SaveImageType.jpg:
                    data = new BitmapMetadata("jpg");
                    data.SetQuery("/app1/ifd/{ushort=305}", "Pixtrim2");
                    break;
                case SaveImageType.bmp:

                    break;
                case SaveImageType.gif:
                    data = new BitmapMetadata("Gif");
                    //data.SetQuery("/xmp/xmp:CreatorTool", "Pixtrim2");
                    //data.SetQuery("/XMP/XMP:CreatorTool", "Pixtrim2");
                    break;
                case SaveImageType.tiff:
                    data = new BitmapMetadata("tiff");
                    data.ApplicationName = "Pixtrim2";
                    break;
                default:
                    break;
            }

            return data;
        }

        //ファイル名の重複を回避、拡張子の前に"_"を付け足す
        private string MakeFullPath(string fileName)
        {
            var dir = System.IO.Path.Combine(MyConfig.SavaDir, fileName);
            var ex = "." + MyComboBoxSaveImageType.SelectedValue.ToString();
            var fullPath = dir + ex;

            string bar = "";
            while (System.IO.File.Exists(fullPath))
            {
                bar += "_";
                fullPath = dir + bar + ex;
            }
            return fullPath;
        }


        ////CroppedBitmapを使って切り抜いた画像を作成
        //private BitmapSource MakeCroppedBitmap(BitmapSource bitmap)
        //{
        //    ////切り抜き範囲適合チェック
        //    //if (CheckCropRect(bitmap) == false) { return null; }
        //    //切り抜き範囲取得
        //    var rect = MakeCropRect();
        //    return new CroppedBitmap(bitmap, rect);
        //}
        /// <summary>
        /// CroppedBitmapを使って切り抜いた画像を作成、切り抜き範囲チェックありならチェックして範囲外だったらnullを返す
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="IsRectCheck">切り抜き範囲チェックの有無、falseならチェックしないで切り抜く</param>
        /// <returns></returns>
        private BitmapSource MakeCroppedBitmap(BitmapSource bitmap, bool IsRectCheck)
        {
            //切り抜き範囲適合チェック
            if (IsRectCheck)
            {
                if (CheckCropRect(bitmap) == false) { return null; }
            }
            //切り抜き範囲取得
            var rect = MakeCropRect();
            return new CroppedBitmap(bitmap, rect);
        }

        private Int32Rect MakeCropRect()
        {
            return new Int32Rect((int)MyNumericX.MyValue2,
                (int)MyNumericY.MyValue2, (int)MyNumericW.MyValue2, (int)MyNumericH.MyValue2);
        }

        //画像ファイル形式によるEncoder取得
        private BitmapEncoder GetEncoder()
        {
            var type = MyComboBoxSaveImageType.SelectedItem;

            switch (type)
            {
                case SaveImageType.png:
                    return new PngBitmapEncoder();
                case SaveImageType.jpg:
                    var jpeg = new JpegBitmapEncoder();
                    jpeg.QualityLevel = (int)MyConfig.JpegQuality;// (int)MyNumericJpegQuality.MyValue2;
                    return jpeg;
                case SaveImageType.bmp:
                    return new BmpBitmapEncoder();
                case SaveImageType.gif:
                    return new GifBitmapEncoder();
                case SaveImageType.tiff:
                    return new TiffBitmapEncoder();
                default:
                    throw new Exception();
            }
        }

        //切り抜き範囲が画像範囲内にあるかどうかのチェック
        //private bool CheckCropRect()
        //{
        //    //切り抜き範囲チェック
        //    BitmapSource bitmap;
        //    Int32Rect intRect = MakeCropRect();
        //    Rect crop = new Rect(intRect.X, intRect.Y, intRect.Width, intRect.Height);
        //    for (int i = 0; i < ListMyBitmapSource.Count; i++)
        //    {
        //        bitmap = ListMyBitmapSource[i].Source;
        //        if (CheckCropRect(bitmap, crop) == false)
        //        {
        //            string str = $"切り抜き範囲が画像範囲外なので保存できない\n" +
        //                $"{ListMyBitmapSource[i].Name.ToString()}";
        //            MyListBox.SelectedItem = MyListBox.Items[i];
        //            MyListBox.ScrollIntoView(MyListBox.Items[i]);
        //            MessageBox.Show(str);
        //            return false;
        //        }
        //    }
        //    return true;
        //}
        private bool CheckCropRect(BitmapSource bitmap)
        {
            Int32Rect intRect = MakeCropRect();
            Rect crop = new Rect(intRect.X, intRect.Y, intRect.Width, intRect.Height);
            return CheckCropRect(bitmap, crop);
        }
        private bool CheckCropRect(BitmapSource bitmap, Rect crop)
        {
            Rect r2 = new Rect(crop.X, crop.Y, crop.Width, crop.Height);
            Rect r1 = new Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);
            return r1.Contains(r2);
        }

        #endregion



        #region リストボックス





        private void MyButtonRemoveSelectedImtem_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelectedListItem();
        }
        //選択アイテム削除
        private void RemoveSelectedListItem()
        {
            var items = new List<MyBitmapAndName>();

            foreach (MyBitmapAndName item in MyListBox.SelectedItems)
            {
                items.Add(item);
            }
            foreach (var item in items)
            {
                ListMyBitmapSource.Remove(item);
            }
        }


        private void MyListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MyBitmapAndName myBitmap = (MyBitmapAndName)MyListBox.SelectedItem;
            if (myBitmap == null)
            {
                MyImage.Source = null;
            }
            else
            {

                MyImage.Source = myBitmap.Source;
            }
        }
        #endregion



        #region クリップボードから画像取得
        //クリップボード更新時に画像取得してリストに追加、名前もつける
        //なぜか更新がなくても5分間隔で通知が来るので、前回の画像と比較してから追加している
        private void ClipboardWatcher_DrawClipboard(object sender, EventArgs e)
        {
            //if (Clipboard.ContainsImage())
            //{
            //    BitmapSource bitmap = null;
            //    int count = 1;
            //    int limit = 5;
            //    do
            //    {
            //        try
            //        {
            //            bitmap = Clipboard.GetImage();//ここで取得できない時がある

            //            //エクセルコピーテストここから
            //            //var data = Clipboard.GetDataObject();
            //            //var mStream = (System.IO.MemoryStream)data.GetData("PNG");//これがいい
            //            ////png = (System.IO.MemoryStream)data.GetData("PNG+Office Art");//null
            //            ////png = (System.IO.MemoryStream)data.GetData("GIF");//少し劣化
            //            ////mStream = (System.IO.MemoryStream)data.GetData("JFIF");//jpgになる？劣化
            //            ////png = (System.IO.MemoryStream)data.GetData("BMP");//null

            //            //var em = data.GetData(DataFormats.EnhancedMetafile);//system.drawing.imaging.metafile、参照が必要
            //            //var bitm = data.GetData(DataFormats.Bitmap);//system.windows.interop.interopBitmap、普通に取得した場合と同じ
            //            //var dib = data.GetData(DataFormats.Dib);//memoryStreamだけどBitmapFrame.Createでエラー
            //            ////sa = data.GetData(DataFormats.Tiff);//null
            //            ////sa = data.GetData(DataFormats.MetafilePicture);//error or null
            //            //bitmap = (BitmapSource)bitm;

            //            //if (mStream != null)
            //            //{
            //            //    BitmapSource bmp = BitmapFrame.Create(mStream);
            //            //    bitmap = bmp;
            //            //}
            //            //エクセルコピーテストここまで


            BitmapSource bitmap = GetBitmapSource();
            if (bitmap == null) return;
            AddBitmapToList(bitmap);
        }

        //Bitmapをリストに追加
        private void AddBitmapToList(BitmapSource bitmap)
        {
            ////画像比較、同じなら何もしないでreturn、違ったらリストに追加
            //if (IsBitmapEqual(bitmap, PastBitmap)) return;
            //PastBitmap = bitmap;

            string name = GetStringNowTime();
            //string name = MyConfig.FileName + GetStringNowTime();

            //自動保存モードならファイルに保存
            if (CheckBoxIsAutoSave.IsChecked == true)
            {
                //切り抜き範囲チェック
                if (CheckCropRect(bitmap) == false)
                {
                    MessageBox.Show("切り抜き範囲が画像内に収まっていないので保存できませんでした");
                }
                else
                {
                    SaveBitmap(MakeSaveBitmap(bitmap, false), name);
                }
            }
            //音声ファイル再生
            if (MyConfig.IsPlaySound == true) { PlaySoundFile(); }

            //自動保存and自動削除モードならここまで
            if (MyConfig.IsAutoSave && MyConfig.IsAutoRemoveSavedItem) return;

            //画像と名前をリストに追加
            var source = new MyBitmapAndName(bitmap, name);
            ListMyBitmapSource.Add(source);
            MyListBox.SelectedItem = source;
            MyListBox.ScrollIntoView(source);//選択アイテムまでスクロール

            //Canvasのサイズを画像のサイズに合わせる、これがないとスクロールバーが出ない
            CanvasSizeSuitable();

        }

        //クリップボードから画像取得
        //画像取得時に失敗することがあるので指定回数連続トライしている
        private BitmapSource GetBitmapSource()
        {
            BitmapSource bitmap = null;
            if (Clipboard.ContainsImage())
            {
                int count = 1;
                int limit = 5;//試行回数、5あれば十分だけど、失敗するようなら10とかにする
                do
                {
                    try
                    {
                        //MySleep(10);//10ミリ秒待機、意味ないかも
                        bitmap = Clipboard.GetImage();//ここで取得できない時がある
                    }
                    catch (Exception ex)
                    {
                        if (count == limit)
                        {
                            string str = $"{limit}回試したけど画像の取得に失敗\n{ex.Message}";
                            MessageBox.Show(str);
                        }
                    }
                    finally { count++; }
                } while (limit >= count && bitmap == null);
            }
            return bitmap;
        }

        #endregion


        //        【C#入門】現在時刻を取得する方法(DateTime.Now/UtcNow) | 侍エンジニア塾ブログ（Samurai Blog） - プログラミング入門者向けサイト
        //https://www.sejuku.net/blog/51208

        //今の日時をStringで作成
        private string GetStringNowTime()
        {
            DateTime dt = DateTime.Now;
            string str = dt.ToString("yyyyMMdd" + "_" + "HHmmss" + "_" + dt.Millisecond.ToString("000"));
            return str;
        }

        private void CheckBox_ClipCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBoxIsClipboardWatch.IsChecked == true) { ClipboardWatcher.Start(); }
            else ClipboardWatcher.Stop();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ClipboardWatcher = new ClipboardWatcher(
                new System.Windows.Interop.WindowInteropHelper(this).Handle);
            ClipboardWatcher.DrawClipboard += ClipboardWatcher_DrawClipboard;
            if (CheckBoxIsClipboardWatch.IsChecked == true) ClipboardWatcher.Start();
        }



    }

    public class MyBitmapAndName
    {
        public BitmapSource Source { get; }
        public string Name { get; }
        public MyBitmapAndName(BitmapSource source, string name)
        {
            Source = source;
            Name = name;
        }
    }






    /// <summary>
    /// 切り抜き範囲の設定データ用、アプリの設定ファイルにリストとして追加する
    /// 項目は設定名、位置、サイズ、保存時の拡大率
    /// </summary>
    [Serializable]
    public class TrimConfig : System.ComponentModel.INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //設定の名前
        private string _Name;
        public string Name
        {
            get => _Name;
            set
            {
                if (_Name == value)
                    return;
                _Name = value;
                RaisePropertyChanged();
            }
        }

        private int _Width;
        public int Width
        {
            get => _Width;
            set
            {
                if (_Width == value)
                    return;
                _Width = value;
                RaisePropertyChanged();
            }
        }

        private int _Height;
        public int Height
        {
            get => _Height;
            set
            {
                if (_Height == value)
                    return;
                _Height = value;
                RaisePropertyChanged();
            }
        }

        private int _Left;
        public int Left
        {
            get => _Left;
            set
            {
                if (_Left == value)
                    return;
                if (value < 0) { value = 0; }
                _Left = value;
                RaisePropertyChanged();
            }
        }

        private int _Top;
        public int Top
        {
            get => _Top;
            set
            {
                if (_Top == value)
                    return;
                if (value < 0) value = 0;
                _Top = value;
                RaisePropertyChanged();
            }
        }
        private decimal _SaveScale;
        public decimal SaveScale
        {
            get => _SaveScale;
            set
            {
                if (_SaveScale == value)
                    return;
                _SaveScale = value;
                RaisePropertyChanged();
            }
        }

    }

    [Serializable]
    public class Config : System.ComponentModel.INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //private List<TrimConfig> TrimConfigList = new List<TrimConfig>();

        private int _Width;
        public int Width
        {
            get => _Width;
            set
            {
                if (_Width == value)
                    return;
                _Width = value;
                RaisePropertyChanged();
            }
        }

        private int _Height;
        public int Height
        {
            get => _Height;
            set
            {
                if (_Height == value)
                    return;
                _Height = value;
                RaisePropertyChanged();
            }
        }

        private int _Left;
        public int Left
        {
            get => _Left;
            set
            {
                if (_Left == value)
                    return;
                if (value < 0) { value = 0; }
                _Left = value;
                RaisePropertyChanged();
            }
        }

        private int _Top;
        public int Top
        {
            get => _Top;
            set
            {
                if (_Top == value)
                    return;
                if (value < 0) value = 0;
                _Top = value;
                RaisePropertyChanged();
            }
        }

        //private string _FileName;
        //public string FileName
        //{
        //    get => _FileName;
        //    set
        //    {
        //        if (_FileName == value)
        //            return;
        //        _FileName = value;
        //        RaisePropertyChanged();
        //    }
        //}

        private string _SavaDir;
        public string SavaDir
        {
            get => _SavaDir;
            set
            {
                if (_SavaDir == value)
                    return;
                _SavaDir = value;
                RaisePropertyChanged();
            }
        }


        private string _SoundDir;
        public string SoundDir
        {
            get => _SoundDir;
            set
            {
                if (_SoundDir == value)
                    return;
                _SoundDir = value;
                RaisePropertyChanged();
            }
        }

        private SaveImageType _SaveImageType;
        public SaveImageType SaveImageType
        {
            get => _SaveImageType;
            set
            {
                if (_SaveImageType == value)
                    return;
                _SaveImageType = value;
                RaisePropertyChanged();
            }
        }


        private int _JpegQuality;
        public int JpegQuality
        {
            get => _JpegQuality;
            set
            {
                if (_JpegQuality == value)
                    return;
                _JpegQuality = value;
                RaisePropertyChanged();
            }
        }

        private bool _PlaySound;
        public bool IsPlaySound
        {
            get => _PlaySound;
            set
            {
                if (_PlaySound == value)
                    return;
                _PlaySound = value;
                RaisePropertyChanged();
            }
        }

        private decimal _SaveScale;
        public decimal SaveScale
        {
            get => _SaveScale;
            set
            {
                if (_SaveScale == value)
                    return;
                _SaveScale = value;
                RaisePropertyChanged();
            }
        }

        private bool _IsClipboardWatch;
        public bool IsClipboardWatch
        {
            get => _IsClipboardWatch;
            set
            {
                if (_IsClipboardWatch == value)
                    return;
                _IsClipboardWatch = value;
                RaisePropertyChanged();
            }
        }

        private bool _IsAutoRemoveSavedItem;
        public bool IsAutoRemoveSavedItem
        {
            get => _IsAutoRemoveSavedItem;
            set
            {
                if (_IsAutoRemoveSavedItem == value)
                    return;
                _IsAutoRemoveSavedItem = value;
                RaisePropertyChanged();
            }
        }

        private bool _IsAutoSave;
        public bool IsAutoSave
        {
            get => _IsAutoSave;
            set
            {
                if (_IsAutoSave == value)
                    return;
                _IsAutoSave = value;
                RaisePropertyChanged();
            }
        }

        //切り抜き範囲の設定リスト
        private List<TrimConfig> _TrimConfigList = new List<TrimConfig>();
        public List<TrimConfig> TrimConfigList
        {
            get => _TrimConfigList;
            set
            {
                if (_TrimConfigList == value)
                    return;
                _TrimConfigList = value;
                RaisePropertyChanged();
            }
        }


    }
}
//効果音メーカー : WEBブラウザ上で効果音を作成できる無料ツール - PEKO STEP
//https://www.peko-step.com/tool/soundeffect/#pInTopMenu
