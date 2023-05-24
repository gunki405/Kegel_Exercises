using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Globalization;
using System.Resources;

namespace CycleApp.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer m_CycleTimer;
        private Stopwatch m_CycleStopwatch;

        private bool m_bRest = false;
        private int m_nCycle = 0;
        private int m_nRam = 0;

        public MainWindow()
        {
            InitializeComponent();

            TBox_Time.Text = string.Format("{0}", Properties.Settings.Default.Last_ExerciseTime);
            TBox_RestTime.Text = string.Format("{0}", Properties.Settings.Default.Last_RestTime);
            TBox_Cycle.Text = string.Format("{0}", Properties.Settings.Default.Last_CycleCount);

            m_CycleTimer = new DispatcherTimer();
            m_CycleTimer.Interval = TimeSpan.FromMicroseconds(500);
            m_CycleTimer.Tick += CycleTimer_CallBack;

            m_CycleStopwatch = new Stopwatch();
        }

        private void CycleTimer_CallBack(object sender, EventArgs e)
        {
            m_CycleTimer.Stop();
            if (m_nCycle >= int.Parse(TBox_Cycle.Text))
            {
                TBlo_Done.Text = CycleApp.App.Resources.Strings.MainWin_EndMessage;
                TBlo_CycleCount.Text = string.Empty;
                TBlo_CycleTimer.Text = string.Empty;
                Grd_CycleBackground.Background = new SolidColorBrush(Colors.White);
                UISetting(false);
                return;
            }

            TBlo_CycleCount.Text = string.Format(CycleApp.App.Resources.Strings.MainWin_RemainCountString, int.Parse(TBox_Cycle.Text) - m_nCycle);
            if (!m_bRest)
            {
                if (m_CycleStopwatch.ElapsedMilliseconds > int.Parse(TBox_Time.Text) * 1000)
                {
                    m_nCycle++;
                    m_bRest = true;
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
            Btn_Start.IsEnabled = !bStart;
            Btn_Stop.IsEnabled = bStart;

            TBox_Time.IsEnabled = !bStart;
            TBox_RestTime.IsEnabled = !bStart;
            TBox_Cycle.IsEnabled = !bStart;
        }

        private void Btn_Start_Click(object sender, RoutedEventArgs e)
        {
            m_bRest = false;
            TBlo_Done.Text = string.Empty;

            if (Checking_Values() == false)
            {
                return;
            }

            Properties.Settings.Default.Last_ExerciseTime = int.Parse(TBox_Time.Text);
            Properties.Settings.Default.Last_RestTime = int.Parse(TBox_RestTime.Text);
            Properties.Settings.Default.Last_CycleCount = int.Parse(TBox_Cycle.Text);
            Properties.Settings.Default.Save();
            m_nCycle = 0;

            m_CycleTimer.Start();
            m_CycleStopwatch.Restart();

            UISetting(true);
        }

        private void Btn_Stop_Click(object sender, RoutedEventArgs e)
        {
            m_CycleTimer.Stop();
            TBlo_CycleCount.Text = string.Empty;
            TBlo_CycleTimer.Text = string.Empty;
            Grd_CycleBackground.Background = new SolidColorBrush(Colors.White);

            UISetting(false);
        }

        private void Btn_AppClose_Click(object sender, RoutedEventArgs e)
        {
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
            int minValue = 1;

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
            int minValue = 1;

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
    }
}
