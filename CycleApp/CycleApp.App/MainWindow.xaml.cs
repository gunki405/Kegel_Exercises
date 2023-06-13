using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Globalization;
using System.Resources;
using System.Windows.Markup;
using System.Reflection;
using System.Collections;
using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Input;

namespace CycleApp.App
{
    public class UpdateChecker
    {

        private const string GitHubApiUrl = "https://api.github.com/repos/{owner}/{repo}/releases/latest";
        private const string Owner = "gunki405";
        private const string Repo = "Kegel_Exercises";
        private readonly string currentVersion;

        private string strNewVersionDounloadUrl = string.Empty;

        public UpdateChecker(string currentVersion)
        {
            this.currentVersion = currentVersion;
        }

        public bool CheckForUpdate(ref string UpdateCheckResult)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)");

                string apiUrl = GitHubApiUrl.Replace("{owner}", Owner).Replace("{repo}", Repo);
                Task<HttpResponseMessage> GetAsyncResult = client.GetAsync(apiUrl);
                HttpResponseMessage response = GetAsyncResult.Result;

                if (response.IsSuccessStatusCode)
                {
                    Task<string> ReadAsStringAsyncResult = response.Content.ReadAsStringAsync();
                    string jsonResponse = ReadAsStringAsyncResult.Result;
                    dynamic release = JsonConvert.DeserializeObject(jsonResponse);

                    string latestVersion = release.tag_name;
                    if (IsUpdateAvailable(latestVersion))
                    {
                        strNewVersionDounloadUrl = release.assets[0].browser_download_url;
                        UpdateCheckResult = latestVersion;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        private bool IsUpdateAvailable(string latestVersion)
        {
            Version current = new Version(currentVersion);
            Version latest = new Version(latestVersion);

            return latest > current;
        }

        public bool DownloadAndInstallUpdate()
        {
            using (HttpClient client = new HttpClient())
            {
                string fileName = Path.GetFileName(strNewVersionDounloadUrl);
                string tempFilePath = Path.Combine(Path.GetTempPath(), fileName);


                Task<HttpResponseMessage> GetAsyncResult = client.GetAsync(strNewVersionDounloadUrl);
                HttpResponseMessage response = GetAsyncResult.Result;

                Task<Stream> ReadAsStreamResult = response.Content.ReadAsStreamAsync();
                using (Stream contentStream = ReadAsStreamResult.Result)
                {
                    using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        contentStream.CopyTo(fileStream);
                    }
                }
                Process p = new Process();
                p.StartInfo.FileName = "msiexec";
                p.StartInfo.Arguments = "/i " + tempFilePath;
                p.Start();
                return true;
            }
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer m_CycleTimer;
        private Stopwatch m_CycleStopwatch;

        private bool m_bExercising = false;
        private bool m_bReadySet = false;
        private bool m_bRest = false;
        private bool m_bMoreCount = false;
        private int m_nCycle = 0;

        int m_nCurAllExerciseTime = 0;
        int m_nCurAllExerciseCount = 0;

        public MainWindow()
        {
            LanguageSetting();
            InitializeComponent();

            if (Properties.Settings.Default.DisableUpdate == false)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Version version = assembly.GetName().Version;

                string currentVersion = version.ToString();
                string strUpdateText = string.Empty;

                UpdateChecker updateChecker = new UpdateChecker(currentVersion);
                bool updateAvailable = updateChecker.CheckForUpdate(ref strUpdateText);

                if (updateAvailable)
                {
                    MessageBoxResult result = MessageBox.Show(string.Format(CycleApp.App.Resources.Strings.Update_UpdateMessage, strUpdateText), CycleApp.App.Resources.Strings.Update_UpdateTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        if(!updateChecker.DownloadAndInstallUpdate())
                        {
                            MessageBox.Show("Update Failed", "Warning");
                        }
                        else
                        {
                            this.Close();
                        }
                    }
                    else
                    {
                        result = MessageBox.Show(CycleApp.App.Resources.Strings.Update_DisableUpdateMessage, CycleApp.App.Resources.Strings.Update_DisableUpdateTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            Properties.Settings.Default.DisableUpdate = true;
                            Properties.Settings.Default.Save();
                        }
                    }
                }
            }

            TBox_Time.Text = string.Format("{0}", Properties.Settings.Default.Last_ExerciseTime);
            TBox_RestTime.Text = string.Format("{0}", Properties.Settings.Default.Last_RestTime);
            TBox_Cycle.Text = string.Format("{0}", Properties.Settings.Default.Last_CycleCount);
            ChkBox_AlwaysVisibleMode.IsChecked = Properties.Settings.Default.Last_AlwaysVisibleMode;

            if (ChkBox_AlwaysVisibleMode.IsChecked == true)
            {
                this.Topmost = true;
            }

            m_CycleTimer = new DispatcherTimer();
            m_CycleTimer.Interval = TimeSpan.FromMicroseconds(250);
            m_CycleTimer.Tick += CycleTimer_CallBack;

            m_CycleStopwatch = new Stopwatch();

            PrintExerciseLog();
        }

        private void LanguageSetting()
        {
            string strLanguageCode = Properties.Settings.Default.LanguageSetting;
            if (string.IsNullOrEmpty(strLanguageCode))
            {
                strLanguageCode = "en-US";
            }

            CultureInfo cultureInfo = new CultureInfo(strLanguageCode);

            System.Threading.Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }

        private void CycleTimer_CallBack(object sender, EventArgs e)
        {
            if (ChkBox_AlwaysVisibleMode.IsChecked == true)
            {
                this.Topmost = true;
            }
            else
            {
                this.Topmost = false;
            }

            m_CycleTimer.Stop();
            if (m_nCycle >= int.Parse(TBox_Cycle.Text))
            {
                if (m_bMoreCount == false)
                {
                    Random nMorCount = new Random();
                    int nMoreCount = nMorCount.Next(int.Parse(TBox_Cycle.Text) / 5);

                    if (nMoreCount > 0)
                    {
                        TBlo_CycleCount.Text = string.Empty;
                        TBlo_CycleTimer.Text = string.Empty;

                        m_bRest = false;
                        m_bMoreCount = true;
                        m_bReadySet = true;
                        m_nCycle -= nMoreCount;

                        m_CycleStopwatch.Restart();
                        m_CycleTimer.Start();
                        return;
                    }
                }

                m_bReadySet = false;
                TBlo_Done.Text = CycleApp.App.Resources.Strings.MainWin_EndMessage;
                TBlo_MoreCount.Text = string.Empty;
                TBlo_CycleCount.Text = string.Empty;
                TBlo_CycleTimer.Text = string.Empty;
                Grd_CycleBackground.Background = new SolidColorBrush(Colors.White);
                UISetting(false);
                return;
            }

            if(m_bMoreCount)
            {
                TBlo_MoreCount.Text = string.Format(CycleApp.App.Resources.Strings.MainWin_MoreCount, int.Parse(TBox_Cycle.Text) - m_nCycle);
            }
            else
            {
                TBlo_MoreCount.Text = string.Empty;
            }

            if (m_bReadySet)
            {
                Color color;
                switch ((int)(m_CycleStopwatch.ElapsedMilliseconds / 1000))
                {
                    case 0:
                        color = Color.FromArgb(10, 255, 0, 0);
                        Grd_CycleBackground.Background = new SolidColorBrush(color);
                        TBlo_Done.Text = "3";
                        break;
                    case 1:
                        color = Color.FromArgb(10, 215, 215, 0);
                        Grd_CycleBackground.Background = new SolidColorBrush(color);
                        TBlo_Done.Text = "2";
                        break;
                    case 2:
                        color = Color.FromArgb(10, 0, 255, 0);
                        Grd_CycleBackground.Background = new SolidColorBrush(color);
                        TBlo_Done.Text = "1";
                        break;
                    case 3:
                        TBlo_Done.Text = string.Empty;
                        m_CycleStopwatch.Restart();
                        m_bReadySet = false;
                        break;
                }
                m_CycleTimer.Start();
                return;
            }

            TBlo_CycleCount.Text = string.Format(CycleApp.App.Resources.Strings.MainWin_RemainCountString, int.Parse(TBox_Cycle.Text) - m_nCycle);
            if (!m_bRest)
            {
                if (m_CycleStopwatch.ElapsedMilliseconds > int.Parse(TBox_Time.Text) * 1000)
                {
                    m_nCycle++;
                    m_bRest = true;

                    m_nCurAllExerciseTime += int.Parse(TBox_Time.Text);
                    m_nCurAllExerciseCount++;

                    WriteExerciseLog();

                    m_CycleStopwatch.Restart();
                }
                else
                {
                    TBlo_CycleTimer.Text = string.Format(CycleApp.App.Resources.Strings.MainWin_RemainExerciseTimeString, int.Parse(TBox_Time.Text) - (m_CycleStopwatch.ElapsedMilliseconds / 1000));
                    if (m_CycleStopwatch.ElapsedMilliseconds > 1000)
                    {
                        Color color;
                        if ((int.Parse(TBox_Time.Text) - (m_CycleStopwatch.ElapsedMilliseconds / 1000)) <= 0)
                        {
                            color = Color.FromArgb(255, 0, 255, 0);
                        }
                        else
                        {
                            color = Color.FromArgb((byte)(255 / (int.Parse(TBox_Time.Text) - (m_CycleStopwatch.ElapsedMilliseconds / 1000))), 0, 255, 0);
                        }
                        Grd_CycleBackground.Background = new SolidColorBrush(color);
                    }
                    else
                    {
                        Grd_CycleBackground.Background = new SolidColorBrush(Colors.White);
                    }
                }
            }
            else
            {
                if (m_CycleStopwatch.ElapsedMilliseconds > int.Parse(TBox_RestTime.Text) * 1000)
                {
                    m_bRest = false;
                    m_CycleStopwatch.Restart();
                }
                else
                {
                    TBlo_CycleTimer.Text = string.Format(CycleApp.App.Resources.Strings.MainWin_RemainRestTimeString, int.Parse(TBox_RestTime.Text) - (m_CycleStopwatch.ElapsedMilliseconds / 1000));
                    if (m_CycleStopwatch.ElapsedMilliseconds > 1000)
                    {
                        Color color;
                        if ((int.Parse(TBox_Time.Text) - (m_CycleStopwatch.ElapsedMilliseconds / 1000)) <= 0)
                        {
                            color = Color.FromArgb(255, 215, 215, 0);
                        }
                        else
                        {
                            color = Color.FromArgb((byte)(255 / (int.Parse(TBox_Time.Text) - (m_CycleStopwatch.ElapsedMilliseconds / 1000))), 215, 215, 0);
                        }
                        Grd_CycleBackground.Background = new SolidColorBrush(color);
                    }
                    else
                    {
                        Grd_CycleBackground.Background = new SolidColorBrush(Colors.White);
                    }
                }
            }

            m_CycleTimer.Start();
        }

        private bool Checking_Values()
        {
            int nTime = 0;

            if (int.TryParse(TBox_Time.Text, out nTime) == false)
            {
                MessageBox.Show("집중 시간에 잘못된 값을 입력하셨습니다");
                TBox_Time.Focus();
                return false;
            }
            else
            {
                if (nTime > 100 || nTime <= 1)
                {
                    MessageBox.Show("집중 시간에 1이상 100미만 값을 입력해주세요");
                    TBox_Time.Focus();
                    return false;
                }
            }

            if (int.TryParse(TBox_RestTime.Text, out nTime) == false)
            {
                MessageBox.Show("쉬는 시간에 잘못된 값을 입력하셨습니다");
                TBox_RestTime.Focus();
                return false;
            }
            else
            {
                if (nTime > 100 || nTime <= 1)
                {
                    MessageBox.Show("쉬는 시간에 1이상 100미만 값을 입력해주세요");
                    TBox_RestTime.Focus();
                    return false;
                }
            }

            if (int.TryParse(TBox_Cycle.Text, out nTime) == false)
            {
                MessageBox.Show("횟수에 잘못된 값을 입력하셨습니다");
                TBox_Cycle.Focus();
                return false;
            }
            else
            {
                if (nTime > 100 || nTime <= 1)
                {
                    MessageBox.Show("횟수에 1이상 100미만 값을 입력해주세요");
                    TBox_Cycle.Focus();
                    return false;
                }
            }

            return true;
        }

        private void UISetting(bool bStart)
        {
            TBlo_CycleCount.Text = string.Empty;
            TBlo_CycleTimer.Text = string.Empty;
            TBlo_MoreCount.Text = string.Empty;

            Btn_Start.IsEnabled = !bStart;
            Btn_LangSetting.IsEnabled = !bStart;
            Btn_Stop.IsEnabled = bStart;

            TBox_Time.IsEnabled = !bStart;
            TBox_RestTime.IsEnabled = !bStart;
            TBox_Cycle.IsEnabled = !bStart;
        }

        private void Btn_Start_Click(object sender, RoutedEventArgs e)
        {
            m_bRest = false;
            if (Checking_Values() == false)
            {
                return;
            }

            Properties.Settings.Default.Last_ExerciseTime = int.Parse(TBox_Time.Text);
            Properties.Settings.Default.Last_RestTime = int.Parse(TBox_RestTime.Text);
            Properties.Settings.Default.Last_CycleCount = int.Parse(TBox_Cycle.Text);
            Properties.Settings.Default.Last_AlwaysVisibleMode = (bool)ChkBox_AlwaysVisibleMode.IsChecked;
            Properties.Settings.Default.Save();
            m_nCycle = 0;

            m_bReadySet = true;
            m_bMoreCount = false;
            m_CycleTimer.Start();
            m_CycleStopwatch.Restart();

            TBlo_Done.Text = string.Empty;
            UISetting(true);

            m_bExercising = true;
        }

        private void Btn_Stop_Click(object sender, RoutedEventArgs e)
        {
            m_CycleTimer.Stop();
            m_bReadySet = false;
            m_bMoreCount = false;
            Grd_CycleBackground.Background = new SolidColorBrush(Colors.White);

            TBlo_Done.Text = string.Empty;
            UISetting(false);

            WriteExerciseLog();

            m_bExercising = false;
        }

        private void Btn_LangSetting_Click(object sender, RoutedEventArgs e)
        {
            Grd_GrayBlock.Visibility = Visibility.Visible;

            string strBufferLanguage = Properties.Settings.Default.LanguageSetting;

            Win_LanguageSetting win_Language = new Win_LanguageSetting();
            win_Language.Owner = this;
            win_Language.ShowDialog();

            if(Properties.Settings.Default.LanguageSetting != strBufferLanguage)
            {
                LanguageSetting();

                MessageBoxResult result = MessageBox.Show(this, CycleApp.App.Resources.Strings.LanguageSetting_Restart);
                Application.Current.Shutdown();
                return;
            }
            Grd_GrayBlock.Visibility = Visibility.Hidden;
        }

        private void Btn_AppClose_Click(object sender, RoutedEventArgs e)
        {
            WriteExerciseLog();

            this.Close();
        }

        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);

            if (!int.TryParse(newText, out _))
            {
                e.Handled = true;
                return;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            int maxValue = 100;
            int minValue = 2;

            if (int.Parse(textBox.Text) > maxValue)
            {
                textBox.Text = maxValue.ToString();
                textBox.CaretIndex = textBox.Text.Length;
                return;
            }

            if (int.Parse(textBox.Text) < minValue)
            {
                textBox.Text = minValue.ToString();
                textBox.CaretIndex = textBox.Text.Length;
                return;
            }
        }

        private void TextBox_Cycle_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            int maxValue = 50;
            int minValue = 2;

            if (int.Parse(textBox.Text) > maxValue)
            {
                textBox.Text = maxValue.ToString();
                textBox.CaretIndex = textBox.Text.Length;
                return;
            }

            if (int.Parse(textBox.Text) < minValue)
            {
                textBox.Text = minValue.ToString();
                textBox.CaretIndex = textBox.Text.Length;
                return;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = (Window)sender;
            window.Left = SystemParameters.WorkArea.Right - window.Width;
            window.Top = SystemParameters.WorkArea.Bottom - window.Height;
        }

        private void ChkBox_AlwaysVisibleMode_Click(object sender, RoutedEventArgs e)
        {
            if (ChkBox_AlwaysVisibleMode.IsChecked == true)
            {
                this.Topmost = true;
            }
            else
            {
                this.Topmost = false;
            }
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if(m_bExercising == false)
                {
                    Btn_Start.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
                else
                {
                    Btn_Stop.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            }
            else if (e.Key == Key.Escape)
            {
                Btn_AppClose.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void WriteExerciseLog()
        {
            DateTime CurrentDateTime = DateTime.Now;

            int nExerciseAllTime = Properties.Settings.Default.Date_AllExerciseTime;
            int nExerciseAllCount = Properties.Settings.Default.Date_AllExerciseCount;

            if (Properties.Settings.Default.Last_ExerciseDate.Date != CurrentDateTime.Date)
            {
                nExerciseAllTime = 0;
                nExerciseAllCount = 0;
            }

            nExerciseAllTime += m_nCurAllExerciseTime;
            nExerciseAllCount += m_nCurAllExerciseCount;

            Properties.Settings.Default.Date_AllExerciseTime = nExerciseAllTime;
            Properties.Settings.Default.Date_AllExerciseCount = nExerciseAllCount;
            Properties.Settings.Default.Last_ExerciseDate = DateTime.Now;
            Properties.Settings.Default.Save();

            m_nCurAllExerciseCount = 0;
            m_nCurAllExerciseTime = 0;

            PrintExerciseLog();
        }

        private void PrintExerciseLog()
        {
            TBlo_LastExcerciseDate.Text = Properties.Settings.Default.Last_ExerciseDate.ToString("yyyy/MM/dd");
            TBlo_CurDateExerciseTime.Text = string.Format("{0}", Properties.Settings.Default.Date_AllExerciseTime);
            TBlo_CurDateExerciseCount.Text = string.Format("{0}", Properties.Settings.Default.Date_AllExerciseCount);
        }
    }
}
