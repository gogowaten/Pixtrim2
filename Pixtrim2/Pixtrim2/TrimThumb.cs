using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Input;

using System.Windows.Media;
using System.Windows.Shapes;

namespace Pixtrim2
{
    class TrimThumb:FlatThumb
    {
        public Canvas ParentCanvas { get; set; }
        public double Area { get; set; }//カーソル変化範囲の幅
        private double PastHorizontalChange;//直前の横幅変更量記録用
        private double PastVerticalChange;


        public TrimThumb(Canvas parent, double area, double left, double top, double width, double height)
        {
            ParentCanvas = parent;
            Area = area;
            Canvas.SetLeft(this, left);
            Canvas.SetTop(this, top);
            this.Width = width;
            this.Height = height;

            this.DragDelta += TrimThumb_DragDelta;
            this.DragCompleted += TrimThumb_DragCompleted;
            this.MouseMove += TrimThumb_MouseMove;
        }

        //マウス移動イベント時
        //マウスの位置によって形状を変える
        private void TrimThumb_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(ParentCanvas);
            double left = Canvas.GetLeft(this);
            double top = Canvas.GetTop(this);
            double w = this.Width;
            double h = this.Height;

            if (p.X > left + w - Area && p.Y > top + h - Area)
            {
                this.Cursor = Cursors.SizeNWSE;//右下のときは斜め＼
            }
            else if (p.X > left + w - Area)
            {
                this.Cursor = Cursors.SizeWE;//右側なら左右矢印↔
            }
            else if (p.Y > top + h - Area)
            {
                this.Cursor = Cursors.SizeNS;//下側なら上下矢印↕
            }
            else
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        //ドラッグイベント終了時
        //直前の変更量をリセット
        private void TrimThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            PastHorizontalChange = 0;
            PastVerticalChange = 0;
        }

        //ドラッグイベント中
        //カーソルの形でサイズ変更と移動を分けている
        //サイズ変更はe.HorizontalChangeやe.VerticalChangeをWidthに加算すると
        //思った以上に伸縮するので、直前の変更量を引き算したのを加算している
        private void TrimThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            //横幅変更
            void ChangeWidthSize()
            {
                //変更後の幅が小さくなりすぎないように調整
                //もし、小さすぎなら変更しない
                double w = this.Width + e.HorizontalChange - PastHorizontalChange;
                if (Area > w)
                {
                    PastHorizontalChange = 0;
                }
                else
                {
                    this.Width = w;//変更
                    PastHorizontalChange = e.HorizontalChange;//変更量を記録
                }
            }
            //縦幅変更
            void ChangeHeightSize()
            {
                double h = this.Height + e.VerticalChange - PastVerticalChange;
                if (Area > h)
                {
                    PastVerticalChange = 0;
                }
                else
                {
                    this.Height = h;
                    PastVerticalChange = e.VerticalChange;
                }
            }

            //カーソルの形で判定
            if (this.Cursor == Cursors.SizeWE)//左右
            {
                ChangeWidthSize();
            }
            else if (this.Cursor == Cursors.SizeNS)//上下
            {
                ChangeHeightSize();
            }
            else if (this.Cursor == Cursors.SizeNWSE)//斜め
            {
                ChangeHeightSize();
                ChangeWidthSize();
            }
            else if (this.Cursor == Cursors.Arrow)//通常の矢印
            {
                //ドラッグ移動
                Canvas.SetLeft(this, Canvas.GetLeft(this) + e.HorizontalChange);
                Canvas.SetTop(this, Canvas.GetTop(this) + e.VerticalChange);
            }
        }
    }

    public class FlatThumb : Thumb
    {

        private Rectangle MyRectangle;
        public FlatThumb()
        {
            //template作成
            ControlTemplate template = new ControlTemplate(typeof(Thumb));

            //Templateの見た目の作成、ベースはRectangleに変更した
            //任意の名前をつけておくと、これをもとに検索して要素を取得できる
            template.VisualTree = new FrameworkElementFactory(typeof(Rectangle), "tempRectangle");
            this.Template = template;

            this.ApplyTemplate();//再構築、これで名前検索で取得できる

            //名前検索で取得して背景色指定
            MyRectangle = (Rectangle)this.Template.FindName("tempRectangle", this);
            MyRectangle.Fill = new SolidColorBrush(SystemColors.ControlColor);
        }

        //破線の枠の色と太さ指定、非表示は色をTransparentにするか幅を0
        //実線にするにはdoublecollectionを0,1とかにすればいいけど
        public void SetOutLine(Color color, double lineWidth)
        {
            MyRectangle.StrokeDashArray = new DoubleCollection(new List<double> { 4, 2 });
            MyRectangle.StrokeThickness = lineWidth;
            MyRectangle.Stroke = new SolidColorBrush(color);
        }
        public void SetBackGroundColor(Color color)
        {
            MyRectangle.Fill = new SolidColorBrush(color);
        }
    }
}
