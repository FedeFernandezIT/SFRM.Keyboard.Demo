using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = System.Windows.MessageBox;
using Timer = System.Timers.Timer;

namespace SFRM.Keyboard.Demo
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LowLevelKeyboardProc hook = null;
        private bool firstStroke = false;
        private IntPtr hHook;
        private IntPtr hModule;
        private string keyStored;
        private Timer farmaticListener;
        private bool farmaticActived;
        private WinEventDelegate winHook = null;
        private IntPtr wHook;
        private string lastWindowTitle;
        private string currentWindowTitle;
        private bool wFarmaticActived = false;
        private IntPtr wActive;

        public MainWindow()
        {
            InitializeComponent();            
            hook = new LowLevelKeyboardProc(MyCallbackFunction);
            hModule = GetModuleHandle(null);

            #region Process Farmatic
            //var pFarmatic = Process.GetProcessesByName("Pnucleo");
            //if (pFarmatic.Length > 0)
            //{
            //    var hexProcess = pFarmatic[0].Id.ToString("x8");
            //    var hexWindow = pFarmatic[0].MainWindowHandle.ToInt32().ToString("x8");
            //    foreach (ProcessThread thread in pFarmatic[0].Threads)
            //    {
            //        var hexThread = thread.Id.ToString("x8");
            //        var sameHWnd = GetWindowHandlesForThread(thread.Id);


            //        //const uint wparam = 0 << 29 | 0;
            //        //PostMessage(sameHWnd[83], WM_KEYDOWN, (IntPtr)Keys.N, (IntPtr)wparam);

            //    }
            //    //var whFarmatic = pFarmatic[0].Handle;

            //    //hModule = whFarmatic;
            //} else hModule = LoadLibrary("User32");
            #endregion

            hHook = SetWindowsHookEx(WH_KEYBOARD_LL, hook, hModule, 0);
            //hHook = SetWindowsHookEx(WH_KEYBOARD_LL, hook, hModule, 0x002a05f2);
            if ((int)hHook <= 0)
            {
                MessageBox.Show("No se pudo establecer Hook");
            }

            //winHook = new WinEventDelegate(WinEventProc);
            //wHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, winHook, 0, 0, WINEVENT_OUTOFCONTEXT);            
        }

        private static List<IntPtr> _results = new List<IntPtr>();

        private static IntPtr[] GetWindowHandlesForThread(int threadHandle)
        {
            _results.Clear();
            EnumWindows(WindowEnum, threadHandle);
            return _results.ToArray();
        }

        private static int WindowEnum(IntPtr hWnd, int lParam)
        {
            int processID = 0;
            int threadID = GetWindowThreadProcessId(hWnd, out processID);
            if (threadID == lParam)
            {
                _results.Add(hWnd);
                EnumChildWindows(hWnd, WindowEnum, threadID);
            }
            return 1;
        }

        private IntPtr MyCallbackFunction(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code >= 0 && (IntPtr)WM_KEYDOWN == wParam) 
            {
                if (!firstStroke)
                {
                    firstStroke = !firstStroke;
                    TextKeys.Text = string.Empty;
                }                
                int vkCode = Marshal.ReadInt32(lParam);
                string vkString = ((Keys)vkCode).ToString();
                keyStored += vkString;
                TextKeys.Text = keyStored;                
            }
            return CallNextHookEx(hHook, code, wParam, lParam);
        }

        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            IntPtr handle = IntPtr.Zero;
            StringBuilder Buff = new StringBuilder(nChars);
            handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        private delegate int EnumWindowsProc(IntPtr hwnd, int lParam);

        [DllImport("user32.Dll")]
        private static extern int EnumWindows(EnumWindowsProc x, int y);
        [DllImport("user32")]
        private static extern bool EnumChildWindows(IntPtr window, EnumWindowsProc callback, int lParam);
        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);


        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            //var window = GetActiveWindowTitle();
            //var window = GetForegroundWindow();
            //if (window != null)
            //{
            //    Debug.WriteLine(window);
            //    TextKeys.Text = window;
            //    if (window == "Conexión con Base de Datos")
            //    {
            //        var pFarmatic = Process.GetProcessesByName();
            //    }
            //    //Thread.Sleep(500);
            //    //UnhookWindowsHookEx(hHook);
            //    //hHook = SetWindowsHookEx(WH_KEYBOARD_LL, hook, hModule, 0);
            //}                                    
            wActive = GetForegroundWindow();
            var pFarmatic = Process.GetProcessesByName("Pnucleo");
            if (pFarmatic.Length > 0)
            {
                var whFarmatic = pFarmatic[0].MainWindowHandle;
                if (whFarmatic != wActive)
                {
                    //SetForegroundWindow(whFarmatic);
                    Thread.Sleep(100);

                    var ths = pFarmatic[0].Threads;

                    const uint wparam = 0 << 29 | 0;
                    var r = PostMessage(whFarmatic, WM_KEYDOWN, (IntPtr)Keys.Tab, (IntPtr)wparam);
                }                
            }            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UnhookWindowsHookEx(hHook);
            UnhookWinEvent(wHook);
        }

        #region DLLs Imported
        
        [DllImport("user32.dll")]        
        static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin,
            uint wMsgFilterMax);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] uint Msg, IntPtr wParam, IntPtr lParam);        

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(int hookType, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);        

        delegate IntPtr LowLevelKeyboardProc(int code, IntPtr wParam, IntPtr lParam);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        [DllImport("user32.dll")]
        static extern bool UnhookWinEvent(IntPtr hWinEventHook);
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region GetMessage

            //MSG msg;
            //int ret;
            //while((ret = GetMessage(out msg, IntPtr.Zero, 0, 0)) != 0)
            //{
            //    if (ret == -1)
            //    {
            //        TextKeys.Text = "ERROR";
            //    }
            //    else
            //    {
            //        Debug.WriteLine("\n\r" + msg.hwnd + " " + msg.lParam + " " + msg.lParam);
            //    }
            //}

            #endregion
        }
    }
}
