using MamsdsTimer.Properties;
using MamsdsStudio.SystemHelper;
using System;
using System.Diagnostics;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Threading;
using System.Windows;

namespace MamsdsTimer
{
    public static class Util
    {
        /// <summary>
        /// Check if this instance is the first instance of the program
        /// </summary>
        /// <returns>Result</returns>
        public static bool IsSingleInstance()
        {
            int Count = 0;
            try
            {                
                Process[] processes = Process.GetProcesses();
                foreach (Process p in processes)
                {
                    if (p.ProcessName.ToLower() == "mamsdstimer")
                        Count++;
                    if (Count >= 2)                        
                        return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("程序初始化失败：" + ex.Message, "Mamsds桌面倒计时软件", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }
    }
}
