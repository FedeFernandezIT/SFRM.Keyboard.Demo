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

        public MainWindow()
        {
            InitializeComponent();            
            hook = new LowLevelKeyboardProc(MyCallbackFunction);                                
            hModule = GetModuleHandle(null);
            hHook = SetWindowsHookEx(WH_KEYBOARD_LL, hook, hModule, 0);

            winHook = new WinEventDelegate(WinEventProc);
            wHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, winHook, 0, 0, WINEVENT_OUTOFCONTEXT);            
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

        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            var window = GetActiveWindowTitle();
            if (window == "Keyboard Disable Demo")
            {
                Debug.WriteLine(window);
                Thread.Sleep(150);
                UnhookWindowsHookEx(hHook);
                hHook = SetWindowsHookEx(WH_KEYBOARD_LL, hook, hModule, 0);
            }            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UnhookWindowsHookEx(hHook);
            UnhookWinEvent(wHook);
        }

        #region DLLs Imported
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
    }
}
