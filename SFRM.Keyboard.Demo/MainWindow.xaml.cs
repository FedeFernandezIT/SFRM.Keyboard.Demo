using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
        private string keyStored;

        public MainWindow()
        {
            InitializeComponent();
            hook = new LowLevelKeyboardProc(MyCallbackFunction);
            
            // setup a keyboard hook                                    
            IntPtr hModule = GetModuleHandle(null);
            hHook = SetWindowsHookEx(WH_KEYBOARD_LL, hook, hModule, 0);            
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

        private void Window_Closed(object sender, EventArgs e)
        {
            UnhookWindowsHookEx(hHook);
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
        #endregion        
    }
}
