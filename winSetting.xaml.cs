using IWshRuntimeLibrary;
using MamsdsTimer.Properties;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
namespace MamsdsTimer
{
    /// <summary>
    /// winSetting.xaml 的交互逻辑
    /// </summary>
    public partial class winSetting : Window
    {
        private OpenFileDialog SelectBGPDialog;
        /// <summary>
        /// 就是从主窗体传入的主窗体的指针
        /// </summary>
        private MainWindow theWin = null;

        public winSetting(MainWindow MyMainWin)
        {
            InitializeComponent();
            theWin = MyMainWin;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {            
            Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //截止日期
            if (DeadlinePicker.Value.HasValue)
                Time.Default.Deadline = DeadlinePicker.Value.Value;
            //倒计时时间文字
            Time.Default.FontName = theWin.lblCountDown.FontFamily.ToString();
            Time.Default.FontSize = theWin.lblCountDown.FontSize;
            Time.Default.FontColor = (Color)ColorConverter.ConvertFromString(theWin.lblCountDown.Foreground.ToString());
            Time.Default.Margin = theWin.lblCountDown.Margin;
            Time.Default.HorizontalAlignment = theWin.lblCountDown.HorizontalContentAlignment;

            ValidateNewDatetimeFormatString();
            Time.Default.dtFormat = theWin.dtFormat;
            Time.Default.Save();

            Settings.Default.WindowHeight = theWin.Height;
            Settings.Default.WindowWidth = theWin.Width;
            Settings.Default.WindowStartupLocation = theWin.WindowStartupLocation;
            Settings.Default.WindowTop = theWin.Top;
            Settings.Default.WindowLeft = theWin.Left;
            if (!(theWin.Background is ImageBrush))
                Settings.Default.BGPPath = string.Empty;
            else if (SelectBGPDialog != null && System.IO.File.Exists(SelectBGPDialog.FileName))
                Settings.Default.BGPPath = SelectBGPDialog.FileName;

            Settings.Default.IsTopMost = chkIsTopmost.IsChecked.HasValue ? chkIsTopmost.IsChecked.Value : false;
            Settings.Default.IsAutoStart = chkIsAutoStart.IsChecked.HasValue ? chkIsAutoStart.IsChecked.Value : true;
            Settings.Default.IsDropShadowEffectEnabled = chkIsDropShadowEffect.IsChecked.HasValue ? chkIsDropShadowEffect.IsChecked.Value : true;
            Settings.Default.MainTitleContent = System.Windows.Markup.XamlWriter.Save(rtfMainTitle.Document);

            Settings.Default.Save();

            SetAutoStart(Settings.Default.IsAutoStart);

            Close();
        }

        private void winSetting1_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DeadlinePicker.Value = Time.Default.Deadline;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Mamsds桌面倒计时软件日期时间载入出错，错误信息：" + Environment.NewLine + ex.Message + Environment.NewLine + "请尝试重新设置日期时间格式字符串！"
                , "Mamsds桌面倒计时软件", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            foreach (System.Drawing.FontFamily font in System.Drawing.FontFamily.Families)
                cboSelectFont.Items.Add(font.Name);
            cboSelectFont.Text = theWin.lblCountDown.FontFamily.ToString();
            FontSizeUpDown.Text = ((int)theWin.lblCountDown.FontSize).ToString();
            FontColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(theWin.lblCountDown.Foreground.ToString());

            if (theWin.lblCountDown.HorizontalContentAlignment == System.Windows.HorizontalAlignment.Left) cboFontAlign.SelectedIndex = 0;
            else if (theWin.lblCountDown.HorizontalContentAlignment == System.Windows.HorizontalAlignment.Right) cboFontAlign.SelectedIndex = 2;
            else cboFontAlign.SelectedIndex = 1;

            chkWindowCenter.IsChecked = (Settings.Default.WindowStartupLocation == System.Windows.WindowStartupLocation.CenterScreen);
            WinVUpDown.IsEnabled = !chkWindowCenter.IsChecked.Value;
            WinHUpDown.IsEnabled = !chkWindowCenter.IsChecked.Value;
            WidthUpDown.Text = theWin.Width.ToString();
            HeightUpdown.Text = theWin.Height.ToString();

            chkIsAutoStart.IsChecked = Settings.Default.IsAutoStart;
            chkIsTopmost.IsChecked = Settings.Default.IsTopMost;
            chkIsDropShadowEffect.IsChecked = Settings.Default.IsDropShadowEffectEnabled;

            string Content = System.Windows.Markup.XamlWriter.Save(theWin.rtfTitle.Document);
            StringReader sr = new StringReader(Content);
            XmlReader xr = XmlReader.Create(sr);
            rtfMainTitle.Document = (FlowDocument)System.Windows.Markup.XamlReader.Load(xr);

            txtStringFormat.Text = Time.Default.dtFormat;
        }

        private void winSetting1_Closed(object sender, EventArgs e)
        {
            theWin.InitUserSettings();
        }
        
        private void FontsizeSwitcher_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            theWin.lblCountDown.FontSize = Convert.ToDouble(FontSizeUpDown.Text);
        }

        private void cboSelectFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboSelectFont.SelectedItem != null)
                theWin.lblCountDown.FontFamily = new FontFamily(cboSelectFont.SelectedItem.ToString());
        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            theWin.lblCountDown.Foreground = new SolidColorBrush(FontColorPicker.SelectedColor);
        }

        private void CountDownLocationUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (CountDownLocationUpDown.Value.HasValue && e.OldValue != null & e.NewValue != null)
            {
                if (Convert.ToInt32(e.OldValue) - Convert.ToInt32(e.NewValue) > 0)
                     theWin.lblCountDown.Margin = new Thickness(theWin.lblCountDown.Margin.Left, theWin.lblCountDown.Margin.Top + 1, theWin.lblCountDown.Margin.Right, theWin.lblCountDown.Margin.Bottom);
                else if (Convert.ToInt32(e.OldValue) - Convert.ToInt32(e.NewValue) < 0)
                    theWin.lblCountDown.Margin = new Thickness(theWin.lblCountDown.Margin.Left, theWin.lblCountDown.Margin.Top - 1, theWin.lblCountDown.Margin.Right, theWin.lblCountDown.Margin.Bottom);
            }
        }

        private void cboFontAlign_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            if (cboFontAlign.SelectedItem == null) return;
            if (cboFontAlign.SelectedIndex == 0)
                theWin.lblCountDown.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
            else if (cboFontAlign.SelectedIndex == 2)
                theWin.lblCountDown.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
            else
                theWin.lblCountDown.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
        }

        private void WidthUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (WidthUpDown.Value.HasValue)
                theWin.Width = WidthUpDown.Value.Value;
        }

        private void HeightUpdown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (HeightUpdown.Value.HasValue)
                theWin.Height = HeightUpdown.Value.Value;
        }

        private void WinVUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (WinVUpDown.Value.HasValue && e.OldValue != null & e.NewValue != null)
            {
                if (Convert.ToInt32(e.OldValue) - Convert.ToInt32(e.NewValue) > 0)
                    theWin.Top = theWin.Top + 1;
                else if (Convert.ToInt32(e.OldValue) - Convert.ToInt32(e.NewValue) < 0)
                    theWin.Top = theWin.Top - 1;
            }
        }

        private void WinHUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (WinHUpDown.Value.HasValue && e.OldValue != null & e.NewValue != null)
            {
                if (Convert.ToInt32(e.OldValue) - Convert.ToInt32(e.NewValue) > 0)
                    theWin.Left = theWin.Left + 1;
                else if (Convert.ToInt32(e.OldValue) - Convert.ToInt32(e.NewValue) < 0)
                    theWin.Left = theWin.Left - 1;
            }
        }

        private void btnSelectBGP_Click(object sender, RoutedEventArgs e)
        {            
            SelectBGPDialog = new OpenFileDialog();
            SelectBGPDialog.InitialDirectory = Settings.Default.BGPPath.Length > 0 ? Settings.Default.BGPPath : "D:\\";
            SelectBGPDialog.Filter = "图片文件 (*.jpg, *.bmp, *.png, *.gif)|*.jpg; *.bmp; *.png; *.gif";
            SelectBGPDialog.Title = "选择你喜欢的背景图吧！";
            SelectBGPDialog.ValidateNames = true;
            SelectBGPDialog.Multiselect = false;
            SelectBGPDialog.RestoreDirectory = true;
            SelectBGPDialog.CheckFileExists = true;

            if (SelectBGPDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImageBrush b3 = new ImageBrush();
                b3.ImageSource = new BitmapImage(new Uri(SelectBGPDialog.FileName, UriKind.RelativeOrAbsolute));
                theWin.Background = b3;
            }
        }

        private void btnNoBGP_Click(object sender, RoutedEventArgs e)
        {
            theWin.Background = null;
        //  theWin.AllowsTransparency = true; 这句话不行，上面那句才行。
            if (SelectBGPDialog != null)
            {
                SelectBGPDialog.Dispose();
                SelectBGPDialog = null;
            }
        }

        private void SetAutoStart(bool IsAutoStart)
        {
            if (IsAutoStart)
            {
                //之所以要先删除，是因为逗比360会提示修改快捷方式，却不提示建立、删除快捷方式，为了适应逗比数字卫士，只能先删除，再建立一个新的
                if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\MamsdsTimer.lnk"))
                    System.IO.File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\MamsdsTimer.lnk");

                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\MamsdsTimer.lnk");
                shortcut.TargetPath = System.Windows.Forms.Application.ExecutablePath;
                shortcut.WorkingDirectory = System.Windows.Forms.Application.StartupPath;
                shortcut.Description = "Mamsds桌面倒计时软件 开机启动项";
                shortcut.Save();
            }
            else
            {
                System.IO.File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\MamsdsTimer.lnk");
            }
        }

        private void chkIsTopMost_Click(object sender, RoutedEventArgs e)
        {
            if (chkIsTopmost.IsChecked.HasValue)
                theWin.Topmost = chkIsTopmost.IsChecked.Value;
        }

        private void rtfMainTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (theWin != null && rtfMainTitle != null && rtfMainTitle.Document != null)
            {
                string Content = System.Windows.Markup.XamlWriter.Save(rtfMainTitle.Document);
                StringReader sr = new StringReader(Content);
                XmlReader xr = XmlReader.Create(sr);
                theWin.rtfTitle.Document = (FlowDocument)System.Windows.Markup.XamlReader.Load(xr);
            }
        }

        private void btnViewHelper_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"http://msdn.microsoft.com/zh-cn/library/ee372287.aspx");
        }

        private void chkWindowCenter_Click(object sender, RoutedEventArgs e)
        {
            if (!chkWindowCenter.IsChecked.HasValue) return;

            WinVUpDown.IsEnabled = !chkWindowCenter.IsChecked.Value;
            WinHUpDown.IsEnabled = !chkWindowCenter.IsChecked.Value;

            if (chkWindowCenter.IsChecked.Value)
            {
                theWin.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                theWin.Left = SystemParameters.WorkArea.Width / 2 - theWin.Width / 2;
                theWin.Top = SystemParameters.WorkArea.Height / 2 - theWin.Height / 2;
            }
            else
            {
                theWin.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
                theWin.Top = Settings.Default.WindowTop;
                theWin.Left = Settings.Default.WindowLeft;
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("您真的要重置Mamsds桌面倒计时软件的设置吗？！", "Mamsds桌面倒计时软件", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {
                Time.Default.Reset();
                Settings.Default.Reset();
            }
            Close();
        }

        private void chkIsDropShadowEffect_Click(object sender, RoutedEventArgs e)
        {
            if (!chkIsDropShadowEffect.IsChecked.HasValue) return;
            theWin.EnableDropShadowEffect(chkIsDropShadowEffect.IsChecked.Value);
        }

        private void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            ValidateNewDatetimeFormatString();
        }

        private void ValidateNewDatetimeFormatString()
        {
            TimeSpan ts1 = new TimeSpan(DateTime.Now.Ticks);
            TimeSpan ts2 = new TimeSpan(DateTime.Now.Ticks);
            try
            {
                ts2.Subtract(ts1).ToString(txtStringFormat.Text);
                theWin.dtFormat = txtStringFormat.Text;
            }
            catch
            {
                txtStringFormat.Text = Time.Default.dtFormat;
                theWin.dtFormat = Time.Default.dtFormat;
            }
        }
    }
}
