using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Automation.Text;
using System.Windows.Forms;

[assembly: System.Reflection.AssemblyTitle("键位提示")]
[assembly: System.Reflection.AssemblyProduct("键位提示")]
[assembly: System.Reflection.AssemblyVersion("1.0.0.0")]
[assembly: System.Reflection.AssemblyFileVersion("1.0.0.0")]

namespace ShuangpinHint
{
    internal static class Native
    {
        public const int WH_KEYBOARD_LL = 13;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_SYSKEYDOWN = 0x0104;
        public const int WM_HOTKEY = 0x0312;
        public const int WM_IME_CONTROL = 0x0283;
        public const int IMC_GETCONVERSIONMODE = 0x0001;
        public const int IME_CMODE_NATIVE = 0x0001;
        public const int GCS_COMPSTR = 0x0008;
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const uint SMTO_ABORTIFHUNG = 0x0002;
        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;

        [StructLayout(LayoutKind.Sequential)]
        public struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct COMPOSITIONFORM
        {
            public int dwStyle;
            public POINT ptCurrentPos;
            public RECT rcArea;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GUITHREADINFO
        {
            public uint cbSize;
            public uint flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public RECT rcCaret;
        }

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO lpgui);

        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        // MSAA：从窗口的 OBJID_CARET 取插入点，很多编辑器/浏览器都实现了
        [DllImport("oleacc.dll")]
        public static extern int AccessibleObjectFromWindow(IntPtr hwnd, uint dwId, ref Guid riid,
                                                            [MarshalAs(UnmanagedType.Interface)] out object ppvObject);

        public const uint OBJID_CARET = 0xFFFFFFF8;

        [DllImport("imm32.dll")]
        public static extern bool ImmGetCompositionWindow(IntPtr hIMC, out COMPOSITIONFORM lpCompForm);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("imm32.dll")]
        public static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);

        [DllImport("imm32.dll")]
        public static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("imm32.dll")]
        public static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);

        [DllImport("imm32.dll")]
        public static extern bool ImmGetConversionStatus(IntPtr hIMC, out int lpfdwConversion, out int lpfdwSentence);

        // 只取组字串长度（不读取内容），用于判断候选词框是否正在显示
        [DllImport("imm32.dll", CharSet = CharSet.Unicode, EntryPoint = "ImmGetCompositionStringW")]
        public static extern int ImmGetCompositionString(IntPtr hIMC, int dwIndex, IntPtr lpBuf, int dwBufLen);

        // 带超时，避免输入法进程卡住时阻塞界面
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam,
                                                       uint flags, uint timeout, out IntPtr result);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern uint ExtractIconEx(string file, int index, IntPtr[] large, IntPtr[] small, uint count);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        public static extern bool SetProcessDPIAware();

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int cx, int cy);

        [DllImport("user32.dll")]
        public static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetProcessWorkingSetSize(IntPtr hProcess, IntPtr dwMin, IntPtr dwMax);
    }

    internal class KeyDef
    {
        public readonly string Key;      // 键名
        public readonly string Sound;    // 该键对应的声母（无则 null）
        public readonly string[] Vowels; // 该键对应的韵母

        public KeyDef(string key, string sound, params string[] vowels)
        {
            Key = key;
            Sound = sound;
            Vowels = vowels;
        }
    }

    internal class WubiDef
    {
        public readonly string Key;    // 字母键
        public readonly string Code;   // 区位号，如 11
        public readonly string Name;   // 键名字
        public readonly string Roots;  // 该键上的主要字根

        public WubiDef(string key, string code, string name, string roots)
        {
            Key = key;
            Code = code;
            Name = name;
            Roots = roots;
        }
    }

    // 关于窗口：应用信息 + 赞赏码
    internal class AboutForm : Form
    {
        private static readonly Font TitleFont = new Font("Microsoft YaHei", 16f, FontStyle.Bold);
        private static readonly Font InfoFont = new Font("Microsoft YaHei", 9f);
        private static readonly Font TipFont = new Font("Microsoft YaHei", 8f);
        private static readonly Font CloseFont = new Font("Segoe UI", 8.5f, FontStyle.Bold);

        private readonly Rectangle _closeRect;
        private readonly Image _payImage;

        private bool _dragging;
        private Point _dragCursor;
        private Point _dragForm;

        public AboutForm()
        {
            Text = "关于";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(24, 26, 32);
            DoubleBuffered = true;
            TopMost = true;
            ShowInTaskbar = false;
            KeyPreview = true;
            ClientSize = new Size(340, 452);

            _closeRect = new Rectangle(ClientSize.Width - 30, 9, 21, 21);

            int textX = 22;
            int textW = ClientSize.Width - 44;

            AddText("键位提示", TitleFont, Color.FromArgb(236, 241, 250), textX, 20, textW, 34);

            Version version = typeof(AboutForm).Assembly.GetName().Version;
            AddText("版本 " + version.ToString(3), TipFont,
                    Color.FromArgb(132, 140, 154), textX, 54, textW, 18);
            AddText("作者：jensen", InfoFont, Color.FromArgb(202, 210, 222), textX, 84, textW, 20);
            AddText("邮箱：594844561@qq.com", InfoFont, Color.FromArgb(202, 210, 222), textX, 108, textW, 20);

            PictureBox pic = new PictureBox();
            pic.SetBounds(38, 136, 264, 264);
            pic.BackColor = Color.FromArgb(32, 35, 43);
            pic.SizeMode = PictureBoxSizeMode.Zoom;

            _payImage = LoadPayImage();
            pic.Image = _payImage;

            AttachDrag(pic);
            Controls.Add(pic);

            if (pic.Image == null)
            {
                Label missing = new Label();
                missing.Text = "赞赏码图片未找到";
                missing.Font = TipFont;
                missing.ForeColor = Color.FromArgb(120, 128, 142);
                missing.BackColor = Color.Transparent;
                missing.TextAlign = ContentAlignment.MiddleCenter;
                missing.SetBounds(38, 250, 264, 22);
                AttachDrag(missing);
                Controls.Add(missing);
                missing.BringToFront();
            }

            AddText("微信扫码赞赏，感谢支持", TipFont, Color.FromArgb(140, 148, 162), textX, 410, textW, 18);
        }

        // 赞赏码直接用编译时内嵌进 exe 的那份，不依赖本机文件
        private static Image LoadPayImage()
        {
            try
            {
                using (Stream stream = typeof(AboutForm).Assembly.GetManifestResourceStream("PayQr.jpg"))
                {
                    if (stream != null)
                    {
                        using (Image temp = Image.FromStream(stream))
                            return new Bitmap(temp);
                    }
                }
            }
            catch
            {
                // 资源缺失时返回 null，窗口显示文字提示
            }

            return null;
        }

        private void AddText(string text, Font font, Color color, int x, int y, int w, int h)
        {
            Label label = new Label();
            label.Text = text;
            label.Font = font;
            label.ForeColor = color;
            label.BackColor = Color.Transparent;
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.UseMnemonic = false;
            label.SetBounds(x, y, w, h);

            AttachDrag(label);
            Controls.Add(label);
        }

        // 无边框窗口，按住任意位置都能拖动
        private void AttachDrag(Control control)
        {
            control.MouseDown += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left) return;
                if (control == this && _closeRect.Contains(e.Location)) return;
                _dragging = true;
                _dragCursor = Cursor.Position;
                _dragForm = Location;
            };
            control.MouseMove += delegate
            {
                if (!_dragging) return;
                Point now = Cursor.Position;
                Location = new Point(_dragForm.X + (now.X - _dragCursor.X),
                                     _dragForm.Y + (now.Y - _dragCursor.Y));
            };
            control.MouseUp += delegate { _dragging = false; };
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && _closeRect.Contains(e.Location))
            {
                Close();
                return;
            }
            if (e.Button == MouseButtons.Left)
            {
                _dragging = true;
                _dragCursor = Cursor.Position;
                _dragForm = Location;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _dragging = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!_dragging) return;
            Point now = Cursor.Position;
            Location = new Point(_dragForm.X + (now.X - _dragCursor.X),
                                 _dragForm.Y + (now.Y - _dragCursor.Y));
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (Pen border = new Pen(Color.FromArgb(54, 60, 74)))
                e.Graphics.DrawRectangle(border, 0, 0, Width - 1, Height - 1);

            using (SolidBrush back = new SolidBrush(Color.FromArgb(60, 66, 80)))
                e.Graphics.FillEllipse(back, _closeRect);
            using (SolidBrush mark = new SolidBrush(Color.FromArgb(214, 220, 232)))
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddEllipse(_closeRect);
                    e.Graphics.SetClip(path);
                    e.Graphics.DrawString("×", CloseFont, mark, _closeRect, sf);
                    e.Graphics.ResetClip();
                }
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            if (_payImage != null) _payImage.Dispose();
        }
    }

    internal class HintForm : Form
    {
        // 双拼页
        private const int CellW = 54;
        private const int CellH = 50;
        // 五笔页：字根多，格子更大，两页尺寸互相独立
        private const int WubiCellW = 72;
        private const int WubiCellH = 78;
        private const int Gap = 4;
        private const int Pad = 7;
        private const int Rows = 3;
        private const int Cols = 10;

        // 按真实键盘三行错位：第二行相对第一行 +0.25 键宽，第三行 +0.75 键宽
        private int RowOffset(int row)
        {
            if (row <= 0) return 0;
            double factor = row == 1 ? 0.25 : 0.75;
            return (int)(factor * (CellWidth + Gap));
        }

        // 搜狗双拼方案键位（声母 + 韵母）
        private static readonly KeyDef[][] KeyRows = new KeyDef[][]
        {
            new KeyDef[]
            {
                new KeyDef("Q", "q", "iu"), new KeyDef("W", "w", "ia", "ua"), new KeyDef("E", null, "e"),
                new KeyDef("R", "r", "uan", "er"), new KeyDef("T", "t", "ve", "ue"), new KeyDef("Y", "y", "uai", "v"),
                new KeyDef("U", "sh", "u"), new KeyDef("I", "ch", "i"), new KeyDef("O", null, "o", "uo"),
                new KeyDef("P", "p", "un")
            },
            new KeyDef[]
            {
                new KeyDef("A", null, "a"), new KeyDef("S", "s", "ong", "iong"), new KeyDef("D", "d", "iang", "uang"),
                new KeyDef("F", "f", "en"), new KeyDef("G", "g", "eng"), new KeyDef("H", "h", "ang"),
                new KeyDef("J", "j", "an"), new KeyDef("K", "k", "ao"), new KeyDef("L", "l", "ai"),
                new KeyDef(";", null, "ing")
            },
            new KeyDef[]
            {
                new KeyDef("Z", "z", "ei"), new KeyDef("X", "x", "ie"), new KeyDef("C", "c", "iao"),
                new KeyDef("V", "zh", "ui"), new KeyDef("B", "b", "ou"), new KeyDef("N", "n", "in"),
                new KeyDef("M", "m", "ian")
            }
        };

        // 86 版五笔字根（按真实键盘位置排列，第三行为 Z 万能键 + 折区/竖区末位）
        private static readonly WubiDef[][] WubiRows = new WubiDef[][]
        {
            new WubiDef[]
            {
                new WubiDef("Q", "35", "金", "金 钅 勺 鱼 犭 儿 夕 乂"),
                new WubiDef("W", "34", "人", "人 亻 八 癸 祭"),
                new WubiDef("E", "33", "月", "月 彡 乃 用 豕 衣 氏"),
                new WubiDef("R", "32", "白", "白 手 扌 斤 厂"),
                new WubiDef("T", "31", "禾", "禾 竹 ⺮ 丿 彳 攵 夂"),
                new WubiDef("Y", "41", "言", "言 讠 文 方 广 丶 亠"),
                new WubiDef("U", "42", "立", "立 辛 丬 六 门 疒 丷"),
                new WubiDef("I", "43", "水", "水 氵 兴 小 ⺌"),
                new WubiDef("O", "44", "火", "火 灬 业 米"),
                new WubiDef("P", "45", "之", "之 辶 宀 冖 礻 衤")
            },
            new WubiDef[]
            {
                new WubiDef("A", "15", "工", "工 戈 艹 卄 廿 匚 七"),
                new WubiDef("S", "14", "木", "木 丁 西 覀"),
                new WubiDef("D", "13", "大", "大 犬 三 羊 古 石 厂 丆"),
                new WubiDef("F", "12", "土", "土 士 二 干 十 寸 雨"),
                new WubiDef("G", "11", "王", "王 一 五 戋 二 丨"),
                new WubiDef("H", "21", "目", "目 上 止 卜 虍 皮 丨"),
                new WubiDef("J", "22", "日", "日 早 刂 虫 曰"),
                new WubiDef("K", "23", "口", "口 川 丨"),
                new WubiDef("L", "24", "田", "田 甲 囗 四 车 力 罒")
            },
            new WubiDef[]
            {
                new WubiDef("Z", "--", "万能", "任意字根"),
                new WubiDef("X", "55", "纟", "幺 弓 匕 纟 丝"),
                new WubiDef("C", "54", "又", "又 巴 马 厶"),
                new WubiDef("V", "53", "女", "女 刀 九 臼 彐"),
                new WubiDef("B", "52", "子", "子 孑 耳 阝 卩 了 也 凵"),
                new WubiDef("N", "51", "已", "已 巳 己 尸 心 忄 羽 乙"),
                new WubiDef("M", "25", "山", "山 由 贝 冂 几")
            }
        };

        private static readonly Font KeyFont = new Font("Consolas", 14f, FontStyle.Bold);
        private static readonly Font SoundFont = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        private static readonly Font VowelFont = new Font("Segoe UI", 8f);
        private static readonly Font WubiNameFont = new Font("Microsoft YaHei", 10f, FontStyle.Bold);
        private static readonly Font RootFont = new Font("Microsoft YaHei", 6.5f);
        private static readonly Font WubiRootFont = new Font("Microsoft YaHei", 8f);
        private static readonly StringFormat Center = CreateFormat(StringAlignment.Center, StringAlignment.Center);
        private static readonly StringFormat WrapCenter = CreateWrapFormat();
        private static readonly StringFormat TopLeft = CreateFormat(StringAlignment.Near, StringAlignment.Near);
        private static readonly StringFormat TopRight = CreateFormat(StringAlignment.Far, StringAlignment.Near);

        private enum ShowMode
        {
            Off,     // 两个模式都不勾选：不显示
            Auto,    // 候选词框出现时显示
            Always   // 始终显示
        }

        private enum KeyLayout
        {
            Shuangpin,   // 双拼声韵母提示
            Wubi         // 86 版五笔字根提示
        }

        private readonly System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer();
        private readonly ContextMenuStrip _menu;
        private readonly ToolStripMenuItem _shuangpinItem;
        private readonly ToolStripMenuItem _wubiItem;
        private readonly ToolStripMenuItem _autoItem;
        private readonly ToolStripMenuItem _alwaysItem;
        private readonly ToolStripMenuItem _autoStartItem;
        private readonly NotifyIcon _tray;
        private readonly string _iniPath;

        private IntPtr _hook = IntPtr.Zero;
        private Native.LowLevelKeyboardProc _hookProc;
        private string _hotKey;
        private DateTime _hotUntil = DateTime.MinValue;
        private IntPtr _lastFg = IntPtr.Zero;
        private bool _active;          // 敲过字母、尚未按非字母键
        private ShowMode _mode = ShowMode.Auto;
        private KeyLayout _layout = KeyLayout.Shuangpin;
        private DateTime _lastUiaLookup = DateTime.MinValue;
        private IntPtr _inputTarget = IntPtr.Zero;   // 最近一次按键发往的窗口

        // 焦点控件探测结果：自绘输入框（微信、QQ 等）拿不到插入点，只能拿控件矩形兜底
        private readonly object _focusLock = new object();
        private int _focusBusy;
        private DateTime _lastFocusProbe = DateTime.MinValue;
        private bool _focusEditable;
        private Rectangle _focusRect = Rectangle.Empty;
        private string _caretSource = "none";
        private string _lastLogKey = string.Empty;
        private bool _lastShowFlag;
        private bool _suppressed;   // 始终显示模式下点了关闭按钮，等下次输入再出现
        private bool _ready;

        public HintForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Text = "键位提示";
            TopMost = true;
            ShowInTaskbar = false;
            BackColor = Color.FromArgb(20, 22, 27);
            DoubleBuffered = true;
            Opacity = 0.92;
            ApplyLayoutSize();
            MoveToTopRight();

            _iniPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ShuangpinHint", "settings.ini");

            _shuangpinItem = new ToolStripMenuItem("双拼提示");
            _shuangpinItem.CheckOnClick = true;
            _shuangpinItem.Checked = true;
            _shuangpinItem.Click += delegate { SetLayout(KeyLayout.Shuangpin); };

            _wubiItem = new ToolStripMenuItem("五笔 86 提示");
            _wubiItem.CheckOnClick = true;
            _wubiItem.Click += delegate { SetLayout(KeyLayout.Wubi); };

            _autoItem = new ToolStripMenuItem("自动：候选词框出现时显示");
            _autoItem.CheckOnClick = true;
            _autoItem.Checked = true;   // 默认自动
            _autoItem.Click += delegate { SetMode(_autoItem.Checked ? ShowMode.Auto : ShowMode.Off); };

            _alwaysItem = new ToolStripMenuItem("始终显示");
            _alwaysItem.CheckOnClick = true;
            _alwaysItem.Click += delegate { SetMode(_alwaysItem.Checked ? ShowMode.Always : ShowMode.Off); };

            _autoStartItem = new ToolStripMenuItem("开机自启");
            _autoStartItem.CheckOnClick = true;
            _autoStartItem.Checked = IsAutoStart();
            _autoStartItem.Click += delegate { SetAutoStart(_autoStartItem.Checked); };

            ToolStripMenuItem aboutItem = new ToolStripMenuItem("关于");
            aboutItem.Click += delegate { ShowAbout(); };

            ToolStripMenuItem exitItem = new ToolStripMenuItem("退出");
            exitItem.Click += delegate { ExitApp(); };

            _menu = new ContextMenuStrip();
            _menu.Items.Add(_shuangpinItem);
            _menu.Items.Add(_wubiItem);
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(_autoItem);
            _menu.Items.Add(_alwaysItem);
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(_autoStartItem);
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(aboutItem);
            _menu.Items.Add(exitItem);
            ContextMenuStrip = _menu;

            _tray = new NotifyIcon();
            _tray.Icon = LoadAppIcon();
            _tray.Text = "键位提示（双拼 / 五笔）";
            _tray.Visible = true;
            _tray.ContextMenuStrip = _menu;
            _tray.DoubleClick += delegate
            {
                SetMode(_mode == ShowMode.Always ? ShowMode.Auto : ShowMode.Always);
            };

            _timer.Interval = 100;
            _timer.Tick += OnTick;
        }

        private static Icon LoadAppIcon()
        {
            // 优先取 ico 里 16px 的小图标，托盘显示更清晰
            try
            {
                IntPtr[] large = new IntPtr[1];
                IntPtr[] small = new IntPtr[1];
                uint n = Native.ExtractIconEx(Application.ExecutablePath, 0, large, small, 1);
                if (n > 0 && small[0] != IntPtr.Zero) return Icon.FromHandle(small[0]);
            }
            catch
            {
            }

            try
            {
                Icon icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                if (icon != null) return icon;
            }
            catch
            {
            }
            return SystemIcons.Application;
        }

        private static StringFormat CreateFormat(StringAlignment align, StringAlignment lineAlign)
        {
            StringFormat sf = new StringFormat();
            sf.Alignment = align;
            sf.LineAlignment = lineAlign;
            sf.FormatFlags = StringFormatFlags.NoWrap;
            return sf;
        }

        // 允许自动换行并居中排版（字根一行放不下时折行）
        private static StringFormat CreateWrapFormat()
        {
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Near;
            return sf;
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE：弹出不抢输入焦点
                cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW：不显示在 Alt+Tab
                return cp;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyRoundRegion();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadSettings();
            // 启动时不显示窗体，这里先创建句柄，保证托盘菜单与键盘钩子可用
            IntPtr ignore = Handle;
            InstallHook();
            _timer.Start();
            _ready = true;
            SaveSettings();   // 首次运行也落一份配置，状态一目了然
            OnTick(null, EventArgs.Empty);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _timer.Stop();
            if (_hook != IntPtr.Zero)
            {
                Native.UnhookWindowsHookEx(_hook);
                _hook = IntPtr.Zero;
            }
            _tray.Visible = false;
            _tray.Dispose();
            SaveSettings();
            base.OnFormClosing(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // 只在"始终显示"下有关闭按钮：点它临时收起，下次打字再出现
            if (e.Button == MouseButtons.Left && _mode == ShowMode.Always && CloseButtonBounds.Contains(e.Location))
            {
                SuppressUntilInput();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            using (SolidBrush bg = new SolidBrush(BackColor))
                g.FillRectangle(bg, ClientRectangle);

            if (_layout == KeyLayout.Wubi)
            {
                DrawWubiBoard(g);
            }
            else
            {
                int y = Pad;
                for (int r = 0; r < KeyRows.Length; r++)
                {
                    int x = Pad + RowOffset(r);
                    for (int c = 0; c < KeyRows[r].Length; c++)
                    {
                        DrawCell(g, new Rectangle(x, y, CellW, CellH), KeyRows[r][c]);
                        x += CellW + Gap;
                    }
                    y += CellH + Gap;
                }
            }

            // 始终显示模式下给一个关闭按钮，关掉后打字再自动回来
            if (_mode == ShowMode.Always && !_suppressed)
                DrawCloseButton(g);
        }

        private Rectangle CloseButtonBounds
        {
            get { return new Rectangle(Width - 20, 6, 14, 14); }
        }

        private static readonly Font CloseFont = new Font("Segoe UI", 7.5f, FontStyle.Bold);

        private void DrawCloseButton(Graphics g)
        {
            Rectangle r = CloseButtonBounds;
            using (SolidBrush back = new SolidBrush(Color.FromArgb(70, 76, 90)))
                g.FillEllipse(back, r);
            using (SolidBrush mark = new SolidBrush(Color.FromArgb(214, 220, 232)))
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                g.DrawString("×", CloseFont, mark, r, sf);
            }
        }

        private void DrawCell(Graphics g, Rectangle rect, KeyDef def)
        {
            bool hot = _hotKey != null && _hotKey == def.Key;

            using (GraphicsPath path = RoundRect(rect, 6))
            {
                using (SolidBrush fill = new SolidBrush(hot ? Color.FromArgb(48, 124, 240) : Color.FromArgb(36, 40, 49)))
                    g.FillPath(fill, path);
                using (Pen pen = new Pen(hot ? Color.FromArgb(120, 180, 255) : Color.FromArgb(58, 64, 78)))
                    g.DrawPath(pen, path);
            }

            // 键名：左上角
            using (SolidBrush keyBrush = new SolidBrush(hot ? Color.White : Color.FromArgb(150, 158, 172)))
                g.DrawString(def.Key, KeyFont, keyBrush,
                             new RectangleF(rect.X + 6, rect.Y + 2, 28, 20), TopLeft);

            // 声母：右上角（橙色）
            if (def.Sound != null)
            {
                using (SolidBrush soundBrush = new SolidBrush(hot ? Color.FromArgb(255, 232, 196) : Color.FromArgb(255, 168, 96)))
                    g.DrawString(def.Sound, SoundFont, soundBrush,
                                 new RectangleF(rect.X, rect.Y + 4, rect.Width - 6, 15), TopRight);
            }

            // 韵母：下方居中（蓝色）
            using (SolidBrush vowelBrush = new SolidBrush(hot ? Color.White : Color.FromArgb(112, 194, 255)))
                g.DrawString(string.Join("\n", def.Vowels), VowelFont, vowelBrush,
                             new RectangleF(rect.X, rect.Y + 22, rect.Width, rect.Height - 24), Center);
        }

        private void DrawWubiBoard(Graphics g)
        {
            int y = Pad;
            for (int r = 0; r < WubiRows.Length; r++)
            {
                int x = Pad + RowOffset(r);
                for (int c = 0; c < WubiRows[r].Length; c++)
                {
                    DrawWubiCell(g, new Rectangle(x, y, CellWidth, CellHeight), WubiRows[r][c]);
                    x += CellWidth + Gap;
                }
                y += CellHeight + Gap;
            }
        }

        private void DrawWubiCell(Graphics g, Rectangle rect, WubiDef def)
        {
            bool hot = _hotKey != null && _hotKey == def.Key;

            using (GraphicsPath path = RoundRect(rect, 6))
            {
                using (SolidBrush fill = new SolidBrush(hot ? Color.FromArgb(48, 124, 240) : Color.FromArgb(36, 40, 49)))
                    g.FillPath(fill, path);
                using (Pen pen = new Pen(hot ? Color.FromArgb(120, 180, 255) : Color.FromArgb(58, 64, 78)))
                    g.DrawPath(pen, path);
            }

            // 字母：左上角
            using (SolidBrush keyBrush = new SolidBrush(hot ? Color.White : Color.FromArgb(150, 158, 172)))
                g.DrawString(def.Key, KeyFont, keyBrush,
                             new RectangleF(rect.X + 5, rect.Y + 1, 26, 20), TopLeft);

            // 键名字：右上角（橙色）
            using (SolidBrush nameBrush = new SolidBrush(hot ? Color.FromArgb(255, 232, 196) : Color.FromArgb(255, 168, 96)))
                g.DrawString(def.Name, WubiNameFont, nameBrush,
                             new RectangleF(rect.X, rect.Y + 3, rect.Width - 6, 21), TopRight);

            // 区位号：键名字下方（留足行高，否则数字下半截会被裁掉）
            using (SolidBrush codeBrush = new SolidBrush(hot ? Color.FromArgb(220, 230, 245) : Color.FromArgb(110, 118, 132)))
                g.DrawString(def.Code, RootFont, codeBrush,
                             new RectangleF(rect.X, rect.Y + 20, rect.Width - 6, 15), TopRight);

            // 字根：下方居中（蓝色），一行放不下自动折行
            using (SolidBrush rootBrush = new SolidBrush(hot ? Color.White : Color.FromArgb(120, 196, 255)))
                g.DrawString(def.Roots, WubiRootFont, rootBrush,
                             new RectangleF(rect.X + 2, rect.Y + 31, rect.Width - 4, rect.Height - 31), WrapCenter);
        }

        private static GraphicsPath RoundRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            GraphicsPath p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        // 五笔字根多，格子略高一些
        private int CellWidth
        {
            get { return _layout == KeyLayout.Wubi ? WubiCellW : CellW; }
        }

        private int CellHeight
        {
            get { return _layout == KeyLayout.Wubi ? WubiCellH : CellH; }
        }

        private void ApplyLayoutSize()
        {
            int rowWidth = Cols * CellWidth + (Cols - 1) * Gap;
            ClientSize = new Size(Pad * 2 + rowWidth + RowOffset(1),
                                  Pad * 2 + Rows * CellHeight + (Rows - 1) * Gap);

            // 尺寸变了要重做圆角区域，否则窗口会被旧的裁剪区切掉一截
            if (IsHandleCreated)
            {
                ApplyRoundRegion();
                KeepOnScreen();
            }
        }

        // 尺寸变化后把窗口拉回屏幕内，避免右边一列被屏幕边缘截掉
        private void KeepOnScreen()
        {
            Rectangle wa = Screen.FromPoint(new Point(Location.X + Width / 2, Location.Y + Height / 2)).WorkingArea;
            int x = Location.X;
            int y = Location.Y;
            if (x + Width > wa.Right - 8) x = wa.Right - 8 - Width;
            if (x < wa.Left + 8) x = wa.Left + 8;
            if (y + Height > wa.Bottom - 8) y = wa.Bottom - 8 - Height;
            if (y < wa.Top + 8) y = wa.Top + 8;
            if (x != Location.X || y != Location.Y) Location = new Point(x, y);
        }

        private void ApplyRoundRegion()
        {
            IntPtr rgn = Native.CreateRoundRectRgn(0, 0, Width + 1, Height + 1, 14, 14);
            Native.SetWindowRgn(Handle, rgn, true);
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (_hotKey != null && DateTime.Now >= _hotUntil)
            {
                _hotKey = null;
                Invalidate();
            }

            // 换应用、点别的窗口视为离开输入
            IntPtr fg = Native.GetForegroundWindow();
            if (fg != _lastFg)
            {
                _lastFg = fg;
                if (_active) StopInput();
            }

            // 敲过字母才显示；非字母键（选词、空格、回车、Esc…）已经让它收起
            bool composing = _mode == ShowMode.Auto && _active && IsComposing();
            bool show = (_mode == ShowMode.Always && !_suppressed) || composing;

            // 显示状态变化时记一笔，多屏或某些应用不弹时便于判断卡在哪一步
            if (show != _lastShowFlag)
            {
                _lastShowFlag = show;
                LogCaret("state show=" + (show ? 1 : 0) + " active=" + (_active ? 1 : 0)
                         + " composing=" + (composing ? 1 : 0) + " fg=" + Hex(fg)
                         + " loc=" + Location.X + "," + Location.Y + " " + ScreenInfo(Location));
            }

            if (show)
            {
                FollowCaret();
                if (!Visible) Show();
            }
            else if (Visible)
            {
                Hide();
            }

            TrimMemoryIfNeeded();
        }

        // 轮询定位会攒下不少跨进程 COM 临时对象，涨到一定量就回收一次并收缩工作集
        private DateTime _lastTrim = DateTime.MinValue;

        private void TrimMemoryIfNeeded()
        {
            if ((DateTime.Now - _lastTrim).TotalSeconds < 20) return;
            _lastTrim = DateTime.Now;

            try
            {
                // 窗口收起来（说明没在打字）时更积极地缩，反正下次显示前会重新加载
                long limit = Visible ? 70L : 40L;

                using (Process self = Process.GetCurrentProcess())
                {
                    if (self.WorkingSet64 < limit * 1024 * 1024) return;
                }

                GC.Collect(2, GCCollectionMode.Forced, true);
                GC.WaitForPendingFinalizers();
                GC.Collect(2, GCCollectionMode.Forced, true);

                Native.SetProcessWorkingSetSize(Native.GetCurrentProcess(), (IntPtr)(-1), (IntPtr)(-1));
            }
            catch
            {
            }
        }

        private void ShowAbout()
        {
            using (AboutForm about = new AboutForm())
                about.ShowDialog();
        }

        private void ExitApp()
        {
            try { _tray.Visible = false; }
            catch { }
            Close();
            Application.Exit();
        }

        private void SetLayout(KeyLayout layout)
        {
            _layout = layout;
            _suppressed = false;
            _shuangpinItem.Checked = layout == KeyLayout.Shuangpin;
            _wubiItem.Checked = layout == KeyLayout.Wubi;
            ApplyLayoutSize();
            if (_ready) SaveSettings();
            Invalidate();
        }

        private void SetMode(ShowMode mode)
        {
            _mode = mode;
            _suppressed = false;
            _autoItem.Checked = mode == ShowMode.Auto;
            _alwaysItem.Checked = mode == ShowMode.Always;
            if (_ready) SaveSettings();
            OnTick(null, EventArgs.Empty);
        }

        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string RunName = "ShuangpinHint";

        private static bool IsAutoStart()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKey, false))
                {
                    if (key == null) return false;
                    string value = key.GetValue(RunName) as string;
                    if (string.IsNullOrEmpty(value)) return false;
                    return string.Equals(value.Trim('"'), Application.ExecutablePath,
                                         StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                return false;
            }
        }

        private void SetAutoStart(bool on)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKey))
                {
                    if (key == null) return;
                    if (on) key.SetValue(RunName, "\"" + Application.ExecutablePath + "\"");
                    else key.DeleteValue(RunName, false);
                }
            }
            catch
            {
                // 注册表被策略锁定时忽略，勾选状态以实际写入为准
                if (_autoStartItem != null) _autoStartItem.Checked = IsAutoStart();
            }
        }

        // 跟随打字光标：默认贴在光标上方，上方放不下就放到下方，间距为一个悬浮窗高度
        private void FollowCaret()
        {
            if (IsDisposed || !IsHandleCreated) return;

            Point caret;
            int caretHeight;
            if (!TryGetCaretPoint(out caret, out caretHeight))
            {
                LogCaret("fail src=" + _caretSource + " fg=" + Hex(Native.GetForegroundWindow())
                         + " target=" + Hex(_inputTarget) + " loc=" + Location.X + "," + Location.Y
                         + " " + ScreenInfo(Location));

                // 取不到光标时至少别留在别的屏幕上，挪到正在打字的这一块里
                MoveToForegroundScreen();
                return;
            }

            Rectangle wa = Screen.FromPoint(caret).WorkingArea;
            int gap = Height;   // 与光标的间距 = 悬浮窗高度

            int above = caret.Y - gap - Height;
            int below = caret.Y + caretHeight + gap;

            int y = above >= wa.Top + 8 ? above : below;
            int x = caret.X - Width / 2;

            if (x < wa.Left + 8) x = wa.Left + 8;
            if (x + Width > wa.Right - 8) x = wa.Right - 8 - Width;
            if (y + Height > wa.Bottom - 8) y = wa.Bottom - 8 - Height;
            if (y < wa.Top + 8) y = wa.Top + 8;

            Point target = new Point(x, y);
            if (Location != target)
            {
                Location = target;
                LogCaret("ok src=" + _caretSource + " caret=" + caret.X + "," + caret.Y + " h=" + caretHeight
                         + " -> " + target.X + "," + target.Y);
            }
        }

        private static string ScreenInfo(Point point)
        {
            Rectangle wa = Screen.FromPoint(point).WorkingArea;
            return "scr=" + wa.Left + "," + wa.Top + "," + wa.Width + "x" + wa.Height;
        }

        // 定位失败时的兜底：窗口如果在别的屏上，挪到前台窗口所在的那块屏
        private void MoveToForegroundScreen()
        {
            if (IsDisposed || !IsHandleCreated) return;

            Rectangle wa;
            IntPtr fg = Native.GetForegroundWindow();
            Native.RECT r;

            if (fg != IntPtr.Zero && Native.GetWindowRect(fg, out r) && r.Right > r.Left && r.Bottom > r.Top)
            {
                wa = Screen.FromRectangle(Rectangle.FromLTRB(r.Left, r.Top, r.Right, r.Bottom)).WorkingArea;
            }
            else
            {
                wa = Screen.PrimaryScreen.WorkingArea;
            }

            // 已经在同一块屏里就不动，避免位置来回跳
            if (wa.Contains(new Rectangle(Location, Size))) return;

            int x = wa.Left + (wa.Width - Width) / 2;
            int y = wa.Top + 80;
            if (x < wa.Left + 8) x = wa.Left + 8;
            if (x + Width > wa.Right - 8) x = wa.Right - 8 - Width;
            if (y + Height > wa.Bottom - 8) y = wa.Bottom - 8 - Height;

            Location = new Point(x, y);
            LogCaret("moved scr=" + wa.Left + "," + wa.Top + " -> " + x + "," + y);
        }

        private static string Hex(IntPtr value)
        {
            return "0x" + value.ToInt64().ToString("X");
        }

        private DateTime _logWindow = DateTime.MinValue;
        private int _logCount;

        // 记录定位来源，位置或来源变化时才写一行，便于排查；并做限流，避免打字时频繁写盘
        private void LogCaret(string message)
        {
            if (message == _lastLogKey) return;
            _lastLogKey = message;

            DateTime now = DateTime.Now;
            if ((now - _logWindow).TotalSeconds >= 60)
            {
                _logWindow = now;
                _logCount = 0;
            }
            if (_logCount >= 30) return;
            _logCount++;

            try
            {
                string path = Path.Combine(Path.GetTempPath(), "shuangpin-caret.log");
                FileInfo fi = new FileInfo(path);
                if (fi.Exists && fi.Length > 128 * 1024) File.Delete(path);
                File.AppendAllText(path,
                    now.ToString("HH:mm:ss.fff") + " " + message + Environment.NewLine,
                    Encoding.UTF8);
            }
            catch
            {
            }
        }

        // 取当前打字光标的屏幕坐标；没有标准光标时退化为输入法组字窗口位置
        private bool TryGetCaretPoint(out Point screen, out int caretHeight)
        {
            screen = Point.Empty;
            caretHeight = 18;
            _caretSource = "none";

            // 自己成为前台时（点了悬浮窗等），退回最近一次按键的目标窗口
            IntPtr fg = Native.GetForegroundWindow();
            if (fg == IntPtr.Zero || fg == Handle) fg = _inputTarget;
            if (fg == IntPtr.Zero || !Native.IsWindow(fg))
            {
                _caretSource = "no-target";
                return false;
            }

            uint pid;
            uint tid = Native.GetWindowThreadProcessId(fg, out pid);

            Native.GUITHREADINFO gti = new Native.GUITHREADINFO();
            gti.cbSize = (uint)Marshal.SizeOf(typeof(Native.GUITHREADINFO));
            if (Native.GetGUIThreadInfo(tid, ref gti) && gti.hwndCaret != IntPtr.Zero)
            {
                int h = gti.rcCaret.Bottom - gti.rcCaret.Top;
                Native.POINT pt = new Native.POINT();
                pt.X = gti.rcCaret.Left;
                pt.Y = gti.rcCaret.Top;
                if (Native.ClientToScreen(gti.hwndCaret, ref pt))
                {
                    screen = new Point(pt.X, pt.Y);
                    if (h > 0) caretHeight = h;
                    _caretSource = "gti";
                    return true;
                }
            }

            // MSAA：OBJID_CARET 能给出插入点的屏幕坐标
            Point msaa;
            int msaaHeight;
            if (TryGetCaretByMsaa(fg, out msaa, out msaaHeight))
            {
                screen = msaa;
                caretHeight = msaaHeight;
                _caretSource = "msaa";
                return true;
            }

            IntPtr himc = Native.ImmGetContext(fg);
            if (himc != IntPtr.Zero)
            {
                try
                {
                    Native.COMPOSITIONFORM cf = new Native.COMPOSITIONFORM();
                    if (Native.ImmGetCompositionWindow(himc, out cf))
                    {
                        Native.POINT pt = new Native.POINT();
                        pt.X = cf.ptCurrentPos.X;
                        pt.Y = cf.ptCurrentPos.Y;
                        if (Native.ClientToScreen(fg, ref pt))
                        {
                            screen = new Point(pt.X, pt.Y);
                            _caretSource = "ime";
                            return true;
                        }
                    }
                }
                finally
                {
                    Native.ImmReleaseContext(fg, himc);
                }
            }

            // 最后兜底：UI Automation 取当前编辑框的插入点（浏览器、Office、聊天软件多半支持）
            if ((DateTime.Now - _lastUiaLookup).TotalMilliseconds >= 500)
            {
                _lastUiaLookup = DateTime.Now;
                Point uia;
                int uiaHeight;
                if (TryGetCaretByUia(out uia, out uiaHeight))
                {
                    screen = uia;
                    caretHeight = uiaHeight;
                    _caretSource = "uia";
                    return true;
                }
            }

            // 输入法候选框贴着插入点，微信这类自绘界面只能靠它定位
            Rectangle candidateRect;
            if (TryGetImeCandidateRect(out candidateRect))
            {
                screen = new Point(candidateRect.Left + 28, candidateRect.Top - 26);
                caretHeight = 18;
                _caretSource = "candidate";
                return true;
            }

            // 再兜底：拿焦点控件的矩形左下角（部分自绘输入框没有标准插入点）
            Rectangle focusRect;
            if (TryGetFocusRect(out focusRect))
            {
                screen = new Point(focusRect.Left + 10, focusRect.Bottom - 6);
                caretHeight = 18;
                _caretSource = "focus";
                return true;
            }

            // 前面都失败才去问 UIA，平时不碰它（省内存也省时间），结果留给下一轮用
            ProbeFocusAsync();

            _caretSource = "fail";
            return false;
        }

        // 后台探一次焦点是否落在可编辑控件上，结果给取插入点失败时兜底用
        private void ProbeFocusAsync()
        {
            if ((DateTime.Now - _lastFocusProbe).TotalMilliseconds < 1200) return;
            if (Interlocked.CompareExchange(ref _focusBusy, 1, 0) != 0) return;
            _lastFocusProbe = DateTime.Now;

            ThreadPool.QueueUserWorkItem(delegate
            {
                bool editable = false;
                Rectangle rect = Rectangle.Empty;

                try
                {
                    AutomationElement element = AutomationElement.FocusedElement;
                    if (element != null)
                    {
                        // 只要矩形：微信的输入框给不出 TextPattern 的选区，但能给出控件范围
                        System.Windows.Rect r = element.Current.BoundingRectangle;
                        if (!r.IsEmpty && r.Width >= 40 && r.Height >= 12 && r.Height <= 400)
                        {
                            rect = Rectangle.FromLTRB((int)Math.Round(r.Left), (int)Math.Round(r.Top),
                                                      (int)Math.Round(r.Right), (int)Math.Round(r.Bottom));
                        }

                        ControlType type = element.Current.ControlType;
                        editable = type == ControlType.Edit || type == ControlType.Document;

                        if (!editable)
                        {
                            object pattern;
                            editable = element.TryGetCurrentPattern(ValuePattern.Pattern, out pattern)
                                       || element.TryGetCurrentPattern(TextPattern.Pattern, out pattern);
                        }
                    }
                }
                catch
                {
                    // UIA 在部分应用里会抛异常，按"取不到"处理
                }
                finally
                {
                    lock (_focusLock)
                    {
                        _focusEditable = editable;
                        _focusRect = rect;
                    }
                    Interlocked.Exchange(ref _focusBusy, 0);
                }
            });
        }

        private bool TryGetFocusRect(out Rectangle rect)
        {
            lock (_focusLock)
            {
                rect = _focusRect;
                return _focusEditable && rect.Width > 0 && rect.Height > 0;
            }
        }

        // 输入法候选框紧贴插入点，属于输入法进程的无标题小窗，用它反推光标位置
        private static readonly string[] ImeProcessHints =
        {
            "chsime", "textinputhost", "sogoucloud", "sgtool", "sogouinput",
            "qqpinyin", "baidupinyin", "imebroker"
        };

        private DateTime _lastCandidateProbe = DateTime.MinValue;
        private Rectangle _candidateRect = Rectangle.Empty;

        private bool TryGetImeCandidateRect(out Rectangle rect)
        {
            // 枚举窗口比较重，400ms 内复用上次结果
            if ((DateTime.Now - _lastCandidateProbe).TotalMilliseconds >= 400)
            {
                _lastCandidateProbe = DateTime.Now;
                _candidateRect = FindImeCandidateRect();
            }

            rect = _candidateRect;
            return rect.Width > 0;
        }

        private static Rectangle FindImeCandidateRect()
        {
            Rectangle inForeground = Rectangle.Empty;   // 和前台窗口同进程（搜狗这类建在应用进程里）
            Rectangle inIme = Rectangle.Empty;          // 独立输入法进程（微软拼音）

            try
            {
                IntPtr fg = Native.GetForegroundWindow();
                uint fgPid;
                Native.GetWindowThreadProcessId(fg, out fgPid);

                Native.EnumWindows(delegate(IntPtr h, IntPtr l)
                {
                    if (!Native.IsWindowVisible(h)) return true;
                    if (Native.GetWindowTextLength(h) > 0) return true;

                    // 候选框一定是置顶小窗，先按这个条件过滤，减少查进程名的次数
                    int exStyle = Native.GetWindowLong(h, -20);   // GWL_EXSTYLE
                    if ((exStyle & 0x00000008) == 0) return true; // WS_EX_TOPMOST

                    Native.RECT r;
                    if (!Native.GetWindowRect(h, out r)) return true;

                    int w = r.Right - r.Left;
                    int hgt = r.Bottom - r.Top;
                    if (w < 60 || w > 1000 || hgt < 18 || hgt > 240) return true;

                    // 屏幕外的残留窗口不要。注意副屏可能挂在主屏左边，
                    // 坐标是负数，只能用整个虚拟桌面的范围来判断
                    Rectangle vs = SystemInformation.VirtualScreen;
                    if (r.Right <= vs.Left || r.Bottom <= vs.Top ||
                        r.Left >= vs.Right || r.Top >= vs.Bottom) return true;

                    uint pid;
                    Native.GetWindowThreadProcessId(h, out pid);

                    // 搜狗这类输入法的组字窗其实建在目标进程里（类名 SoPY_Comp），
                    // 所以不能只看进程名，还要认类名
                    StringBuilder cls = new StringBuilder(128);
                    Native.GetClassName(h, cls, cls.Capacity);
                    string className = cls.ToString();

                    bool looksLikeCandidate =
                        className.IndexOf("SoPY", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        className.IndexOf("Sogou", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        className.IndexOf("QQPinyin", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        className.IndexOf("Baidu", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        className.IndexOf("Candidate", StringComparison.OrdinalIgnoreCase) >= 0;

                    if (looksLikeCandidate && pid == fgPid)
                    {
                        // 同一个进程可能有多个残留组字窗，取最靠下的那个（正在追随插入点）
                        Rectangle found = new Rectangle(r.Left, r.Top, w, hgt);
                        if (inForeground.IsEmpty || found.Top > inForeground.Top) inForeground = found;
                        return true;
                    }

                    if (inIme.IsEmpty && (looksLikeCandidate || IsImeProcess(pid)))
                        inIme = new Rectangle(r.Left, r.Top, w, hgt);

                    return true;
                }, IntPtr.Zero);
            }
            catch
            {
                inForeground = Rectangle.Empty;
                inIme = Rectangle.Empty;
            }

            return inForeground.IsEmpty ? inIme : inForeground;
        }

        private static readonly Dictionary<uint, string> ProcNameCache = new Dictionary<uint, string>();

        private static bool IsImeProcess(uint pid)
        {
            string name;
            if (!ProcNameCache.TryGetValue(pid, out name))
            {
                try
                {
                    using (Process process = Process.GetProcessById((int)pid))
                        name = process.ProcessName.ToLowerInvariant();
                }
                catch
                {
                    name = string.Empty;   // 进程已退出
                }

                if (ProcNameCache.Count > 64) ProcNameCache.Clear();
                ProcNameCache[pid] = name;
            }

            foreach (string hint in ImeProcessHints)
            {
                if (name.Contains(hint)) return true;
            }
            return false;
        }

        private static bool TryGetCaretByMsaa(IntPtr hwnd, out Point screen, out int caretHeight)
        {
            screen = Point.Empty;
            caretHeight = 18;

            object obj = null;
            try
            {
                Guid iid = typeof(Accessibility.IAccessible).GUID;
                if (Native.AccessibleObjectFromWindow(hwnd, Native.OBJID_CARET, ref iid, out obj) != 0 || obj == null)
                    return false;

                Accessibility.IAccessible acc = obj as Accessibility.IAccessible;
                if (acc == null) return false;

                int x, y, w, h;
                acc.accLocation(out x, out y, out w, out h, 0);
                if (w <= 0 || h <= 0) return false;

                screen = new Point(x, y);
                caretHeight = h;
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                // 每次轮询都会拿到一个跨进程 COM 对象，不主动释放会越积越多
                if (obj != null)
                {
                    try
                    {
                        if (Marshal.IsComObject(obj)) Marshal.ReleaseComObject(obj);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static bool TryGetCaretByUia(out Point screen, out int caretHeight)
        {
            screen = Point.Empty;
            caretHeight = 18;

            try
            {
                AutomationElement element = AutomationElement.FocusedElement;
                if (element == null) return false;

                object pattern;
                if (!element.TryGetCurrentPattern(TextPattern.Pattern, out pattern)) return false;

                TextPatternRange[] ranges = ((TextPattern)pattern).GetSelection();
                if (ranges == null || ranges.Length == 0) return false;

                System.Windows.Rect[] rects = ranges[0].GetBoundingRectangles();
                if (rects == null || rects.Length == 0 || rects[0].Height <= 0) return false;

                System.Windows.Rect r = rects[0];
                screen = new Point((int)r.X, (int)r.Y);
                caretHeight = (int)Math.Max(12, Math.Min(40, r.Height));
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 是否正在中文组字：有输入上下文的只看组字串，
        // 拿不到上下文的界面（微信、Electron）改看输入法候选框是否真的在显示
        // 注意：这里不做 AttachThreadInput，附加输入线程会共享输入队列，导致前台鼠标点击丢失
        private bool IsComposing()
        {
            IntPtr fg = Native.GetForegroundWindow();
            if (fg == IntPtr.Zero || fg == Handle) return Visible;

            // 焦点可能在子窗口上，顶层窗口的输入上下文并不代表实际状态
            IntPtr target = GetImeWindow(fg);
            IntPtr himc = Native.ImmGetContext(target);
            if (himc != IntPtr.Zero)
            {
                try
                {
                    // 只认组字串：英文直通时永远是 0。
                    // 不再看 conversion mode —— 新开的窗口那个值常常还报"中文"，实际是英文
                    if (Native.ImmGetCompositionString(himc, Native.GCS_COMPSTR, IntPtr.Zero, 0) > 0)
                        return true;
                }
                finally
                {
                    Native.ImmReleaseContext(target, himc);
                }
            }

            // 没有组字串：以输入法候选框是否在显示为准
            Rectangle candidate;
            return TryGetImeCandidateRect(out candidate);
        }

        // 输入焦点实际所在的窗口
        private static IntPtr GetImeWindow(IntPtr fg)
        {
            uint pid;
            uint tid = Native.GetWindowThreadProcessId(fg, out pid);

            Native.GUITHREADINFO gti = new Native.GUITHREADINFO();
            gti.cbSize = (uint)Marshal.SizeOf(typeof(Native.GUITHREADINFO));
            if (Native.GetGUIThreadInfo(tid, ref gti) && gti.hwndFocus != IntPtr.Zero)
                return gti.hwndFocus;

            return fg;
        }

        private void InstallHook()
        {
            _hookProc = HookCallback;
            _hook = Native.SetWindowsHookEx(Native.WH_KEYBOARD_LL, _hookProc,
                                           Native.GetModuleHandle(null), 0);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= 0 &&
                    (wParam == (IntPtr)Native.WM_KEYDOWN || wParam == (IntPtr)Native.WM_SYSKEYDOWN))
                {
                    Native.KBDLLHOOKSTRUCT info =
                        (Native.KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(Native.KBDLLHOOKSTRUCT));
                    int vk = (int)info.vkCode;

                    // 记住按键发往的窗口：之后即使自己短暂成为前台，也能据此取光标
                    IntPtr target = Native.GetForegroundWindow();
                    if (target != IntPtr.Zero && target != Handle) _inputTarget = target;

                    // 按住 Ctrl / Alt / Win：属于快捷键，不算打字，不显示也不高亮
                    if (IsHotkeyCombo())
                    {
                        StopInput();
                    }
                    else
                    {
                        string name = VkToName(vk);
                        if (name != null)
                        {
                            FlashKey(name);
                        }
                        else if (!IsModifierKey(vk))
                        {
                            // 非字母键（含退格、Delete、数字选词、空格、回车、Esc…）一律立即收起
                            StopInput();
                        }
                    }
                }
            }
            catch
            {
            }
            return Native.CallNextHookEx(_hook, nCode, wParam, lParam);
        }

        private void FlashKey(string name)
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                try { BeginInvoke(new Action<string>(FlashKey), name); }
                catch { }
                return;
            }
            _hotKey = name;
            _hotUntil = DateTime.Now.AddMilliseconds(420);
            _active = true;
            _suppressed = false;   // 又开始打字了，恢复显示
            Invalidate();
        }

        // 点关闭按钮：始终显示模式下临时收起，下次打字再出现
        private void SuppressUntilInput()
        {
            _suppressed = true;
            Hide();
        }

        // 非字母键：立即收起，直到下次敲字母才再出现
        private void StopInput()
        {
            _active = false;
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                try { BeginInvoke(new Action(StopInput)); }
                catch { }
                return;
            }
            // "始终显示"模式下按键不收起窗口
            if (_mode != ShowMode.Always && Visible) Hide();
        }

        // 按住 Shift / Ctrl / Alt / Win 时按键属于组合键（大写、复制、粘贴、切换输入法…），不当作打字
        private static bool IsHotkeyCombo()
        {
            return IsKeyDown(0x10) || IsKeyDown(0x11) || IsKeyDown(0x12)
                || IsKeyDown(0x5B) || IsKeyDown(0x5C);
        }

        private static bool IsKeyDown(int vk)
        {
            return (Native.GetAsyncKeyState(vk) & 0x8000) != 0;
        }

        // Shift / Ctrl / Alt / Win / CapsLock 等纯修饰键不影响显示
        private static bool IsModifierKey(int vk)
        {
            return vk == 0x10 || vk == 0x11 || vk == 0x12
                || (vk >= 0xA0 && vk <= 0xA5)
                || vk == 0x5B || vk == 0x5C
                || vk == 0x14 || vk == 0x90 || vk == 0x91;
        }

        private static string VkToName(int vk)
        {
            if (vk >= 0x41 && vk <= 0x5A) return ((char)vk).ToString();
            if (vk == 0xBA) return ";";
            return null;
        }

        // 窗口未取到光标前的临时落点，避免出现在屏幕左上角
        private void MoveToTopRight()
        {
            Rectangle wa = Screen.PrimaryScreen.WorkingArea;
            Location = new Point(wa.Right - Width - 24, wa.Top + 72);
        }

        // 只保存/读取显示模式，其余（位置、透明度、跟随）都是固定行为
        private void LoadSettings()
        {
            // 没有配置或旧配置一律按"自动"，只有明确保存过 always 才始终显示
            string mode = ReadSetting("mode", string.Empty).ToLowerInvariant();
            if (mode == "off") _mode = ShowMode.Off;
            else if (mode == "always") _mode = ShowMode.Always;
            else _mode = ShowMode.Auto;

            _autoItem.Checked = _mode == ShowMode.Auto;
            _alwaysItem.Checked = _mode == ShowMode.Always;

            _layout = ReadSetting("layout", "shuangpin") == "wubi" ? KeyLayout.Wubi : KeyLayout.Shuangpin;
            _shuangpinItem.Checked = _layout == KeyLayout.Shuangpin;
            _wubiItem.Checked = _layout == KeyLayout.Wubi;
            ApplyLayoutSize();
        }

        private void SaveSettings()
        {
            try
            {
                string dir = Path.GetDirectoryName(_iniPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("mode=" + _mode.ToString().ToLowerInvariant());
                sb.AppendLine("layout=" + (_layout == KeyLayout.Wubi ? "wubi" : "shuangpin"));
                File.WriteAllText(_iniPath, sb.ToString(), Encoding.UTF8);
            }
            catch
            {
            }
        }

        private string ReadSetting(string key, string defaultValue)
        {
            try
            {
                if (!File.Exists(_iniPath)) return defaultValue;
                foreach (string line in File.ReadAllLines(_iniPath))
                {
                    int i = line.IndexOf('=');
                    if (i > 0 && line.Substring(0, i).Trim() == key)
                        return line.Substring(i + 1).Trim();
                }
            }
            catch
            {
            }
            return defaultValue;
        }
    }

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            bool created;
            using (Mutex mutex = new Mutex(true, "ShuangpinHint_9F1C", out created))
            {
                if (!created) return;

                try { Native.SetProcessDPIAware(); }
                catch { }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                // 用 ApplicationContext 启动，避免 Run(form) 在启动瞬间自动显示窗体
                using (HintForm form = new HintForm())
                {
                    Application.Run(new ApplicationContext(form));
                }
            }
        }
    }
}
