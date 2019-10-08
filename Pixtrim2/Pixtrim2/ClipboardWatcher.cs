using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Pixtrim2
{
    class ClipboardWatcher
    {
        //[DllImport("user32.dll")]
        //private static extern bool AddClipboardFormatListener(IntPtr hWnd);

        //[DllImport("user32.dll")]
        //private static extern bool RemoveClipboardFormatListener(IntPtr hWnd);

        //別バージョン
        [DllImport("user32.dll", SetLastError = true)]
        private extern static void AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private extern static void RemoveClipboardFormatListener(IntPtr hwnd);


        private const int WM_CLIPBOARDUPDATE = 0x031D;
        //private const int WM_DRAWCLIPBOARD = 0x308;

        IntPtr handle;
        HwndSource hwndSource = null;


        public event EventHandler DrawClipboard;
        //イベント起動
        private void raiseClipboardUpdata()
        {
            DrawClipboard?.Invoke(this, EventArgs.Empty);
        }
        //↑は↓と同じ意味
        //private void raiseDrawClipboard()
        //{
        //    if (DrawClipboard != null)
        //    {
        //        DrawClipboard(this, EventArgs.Empty);
        //    }
        //}


        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_CLIPBOARDUPDATE)
            {
                this.raiseClipboardUpdata();
                handled = true;
            }
            return IntPtr.Zero;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="handle">System.Windows.Interop.WindowInteropHelper(this).Handleとかで取得</param>
        public ClipboardWatcher(IntPtr handle)
        {
            hwndSource = HwndSource.FromHwnd(handle);
            hwndSource.AddHook(WndProc);
            //↑でも↓でも変わらない？
            //hwndSource.AddHook(new HwndSourceHook(WndProc));
            this.handle = handle;
            //AddClipboardFormatListener(handle);
        }

        //クリップボード監視開始
        public void Start()
        {
            AddClipboardFormatListener(handle);
        }

        //クリップボード監視停止
        public void Stop()
        {
            RemoveClipboardFormatListener(handle);
        }
    }
}
