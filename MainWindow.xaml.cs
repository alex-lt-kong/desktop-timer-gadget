using MamsdsTimer.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MamsdsTimer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private DateTime timerDeadline;
        private NotifyIcon notifyIcon = null;
        private DispatcherTimer tmrTimer;
        /// <summary>
        /// The format string of the datetime displayed on main window.
        /// </summary>
        public string dtFormat;

        public MainWindow()
        {
            if (!Util.IsSingleInstance()) 
            {
                System.Windows.MessageBox.Show("请不要同时运行两个实例！这会导致配置出错！", "Mamsds桌面倒计时软件", MessageBoxButton.OK, MessageBoxImage.Information);
                System.Windows.Application.Current.Shutdown();
                return;
            }
            InitializeComponent();
            try
            {
                InitUserSettings();
                InitTimer();
                InitTray();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("程序初始化失败：" + Environment.NewLine + ex.Message, "Mamsds桌面倒计时软件"
                    , MessageBoxButton.OK, MessageBoxImage.Error);
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void InitTray()
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.Text = "Mamsds桌面倒计时软件";
            notifyIcon.Icon = Properties.Resources.clock;
            notifyIcon.Visible = true;

            notifyIcon.MouseClick +=notifyIcon_MouseClick;

            //开始停止菜单项
            MenuItem menuControl = new MenuItem("开始/暂停");
            menuControl.Click += menuControl_Click;

            //设置菜单项
            MenuItem menuSetting = new MenuItem("设置");
            menuSetting.Click += menuSetting_Click;

            //帮助菜单项
            MenuItem menuAbout = new MenuItem("关于...");
            menuAbout.Click += menuAbout_Click;
            MenuItem menuContact = new MenuItem("联系作者");
            menuContact.Click += menuContact_Click;
            MenuItem menuWebsite = new MenuItem("作者网站");
            menuWebsite.Click += menuWebsite_Click;
            MenuItem menuCheckUpdate = new MenuItem("检查更新");
            menuCheckUpdate.Click += menuCheckUpdate_Click;
            MenuItem menuHelp = new MenuItem("帮助...", new MenuItem[] { menuAbout, menuContact, menuWebsite, menuCheckUpdate });

            //退出菜单项
            MenuItem exit = new MenuItem("退出");
            exit.Click += exit_Click;

            //关联托盘控件
            MenuItem[] childen = new MenuItem[] { menuControl, menuSetting, menuHelp, exit };
            notifyIcon.ContextMenu = new ContextMenu(childen);            
        }

        #region 托盘菜单点击事件   
        void menuContact_Click(object sender, EventArgs e)
        {
            System.Windows.MessageBox.Show("联系mamsds，可以通过：" + Environment.NewLine + "邮箱：mamsds@outlook.com"
                + Environment.NewLine + "留言：www.mamsds.net" + Environment.NewLine
                + "注意：觉得软件流氓？！乱弹弹窗？这说明你可能下载了非官方软件！" 
                + "遇到上述问题，请先到官网：www.mamsds.net下载官方版本软件试试！", "Mamsds桌面倒计时软件", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        void menuCheckUpdate_Click(object sender, EventArgs e)
        {
            Process.Start(@"http://www.mamsds.net/zh/mamsds_timer_releasing_page/");
        }

        void menuWebsite_Click(object sender, EventArgs e)
        {
            Process.Start(@"http://www.mamsds.net");
        }

        void menuAbout_Click(object sender, EventArgs e)
        {
            (new MyAboutBox()).ShowDialog(); 
        }

        void menuControl_Click(object sender, EventArgs e)
        {
            tmrTimer.IsEnabled = !tmrTimer.IsEnabled;
        }

        void menuSetting_Click(object sender, EventArgs e)
        {
            (new winSetting(this)).Show();
        }

        void exit_Click(object sender, EventArgs e)
        {
             if (System.Windows.MessageBox.Show("确定要退出Mamsds桌面倒计时软件吗?", "Mamsds桌面倒计时软件",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Question,
                                                MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                notifyIcon.Dispose();
                System.Windows.Application.Current.Shutdown();
            }
        }

        void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
 	        if (e.Button == MouseButtons.Left)
                if (Visibility == Visibility.Visible)
                    Visibility = Visibility.Hidden;
                else
                {
                    Visibility = Visibility.Visible;
                    Activate();
                }
        }
        #endregion

        /// <summary>
        /// 初始化用户自定义设置
        /// </summary>
        public void InitUserSettings()
        {
            lblCountDown.FontFamily = new FontFamily(Time.Default.FontName);
            lblCountDown.FontSize = Time.Default.FontSize > 0 ? Time.Default.FontSize : 27;
            lblCountDown.Foreground = new SolidColorBrush(Time.Default.FontColor);
            lblCountDown.Margin = Time.Default.Margin;
            lblCountDown.HorizontalContentAlignment = Time.Default.HorizontalAlignment;
            dtFormat = Time.Default.dtFormat;

            timerDeadline = Time.Default.Deadline;
            if (File.Exists(Settings.Default.BGPPath))
            {
                ImageBrush MyBrush = new ImageBrush();
                MyBrush.ImageSource = new BitmapImage(new Uri(Settings.Default.BGPPath, UriKind.RelativeOrAbsolute));
                Background = MyBrush;
            }
            else
            {
                Background = null;
            }
            Width = Settings.Default.WindowWidth;
            Height = Settings.Default.WindowHeight;
            Topmost = Settings.Default.IsTopMost;
            WindowStartupLocation = Settings.Default.WindowStartupLocation;
            if (WindowStartupLocation == WindowStartupLocation.CenterScreen)
            {
                Left = SystemParameters.WorkArea.Width / 2 - Width / 2;
                Top = SystemParameters.WorkArea.Height / 2 - Height / 2;
            }
            else
            {
                Left = Settings.Default.WindowLeft;
                Top = Settings.Default.WindowTop;
            }

            StringReader sr = new StringReader(Settings.Default.MainTitleContent);
            System.Xml.XmlReader xr = System.Xml.XmlReader.Create(sr);
            rtfTitle.Document = (FlowDocument)System.Windows.Markup.XamlReader.Load(xr);

            EnableDropShadowEffect(Settings.Default.IsDropShadowEffectEnabled);
        }

        public void EnableDropShadowEffect(bool IsEnabled)
        {
            if (IsEnabled)
            {
                DropShadowEffect MyEffect = new DropShadowEffect();
                MyEffect.ShadowDepth = 0;
                MyEffect.Color = Color.FromRgb(255, 255, 255);
                MyEffect.Opacity = 1;
                MyEffect.BlurRadius = 7;

                lblCountDown.Effect = MyEffect;
                rtfTitle.Effect = MyEffect;
            }
            else
            {
                lblCountDown.Effect = null;
                rtfTitle.Effect = null;
            }
        }

        private void InitTimer()
        {
            tmrTimer = new DispatcherTimer();
            tmrTimer.Interval = TimeSpan.FromSeconds(1);
            tmrTimer.Tick += tmrTimer_Tick;
            tmrTimer.Start();
            tmrTimer_Tick(new object(), new EventArgs());
        }

        void tmrTimer_Tick(object sender, EventArgs e)
        {            
            TimeSpan ts1 = new TimeSpan(DateTime.Now.Ticks);
            TimeSpan ts2 = new TimeSpan(timerDeadline.Ticks);
            lblCountDown.Content = ts2.Subtract(ts1).ToString(dtFormat);
            //Article for reference: http://msdn.microsoft.com/zh-cn/library/ee372287(v=vs.100).aspx
        }       
        
    }
}
