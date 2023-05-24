using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CycleApp.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer m_CycleTimer;
        private Stopwatch m_CycleStopwatch;

        private bool m_bActivated = false;
        private bool m_bRest = false;
        private int m_nCycle = 0;
        private int m_nRam = 0;

        public MainWindow()
        {
            InitializeComponent();
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
                TBlo_CycleCount.Text = string.Empty;
                TBlo_CycleTimer.Text = string.Empty;
                Grd_CycleBackground.Background = new SolidColorBrush(Colors.White);
                Img_Activate.Opacity = 0;
                UISetting(false);

                MessageBox.Show("수고하셨습니다!!");
                return;
            }
            if (m_bActivated)
            {
                this.Topmost = true;
            }

            TBlo_CycleCount.Text = string.Format("{0}회\r\n남음", int.Parse(TBox_Cycle.Text) - m_nCycle);
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
                    TBlo_CycleTimer.Text = string.Format("집중 시간\r\n{0}초\r\n남음", int.Parse(TBox_Time.Text) - (m_CycleStopwatch.ElapsedMilliseconds / 1000));
                    if (m_CycleStopwatch.ElapsedMilliseconds > 1000)
                    {
                        if (m_bActivated)
                        {
                            if ((int.Parse(TBox_Time.Text) - (m_CycleStopwatch.ElapsedMilliseconds / 1000)) <= 0)
                            {
                                Img_Activate.Opacity = 1;
                            }
                            else
                            {
                                double nChecking = int.Parse(TBox_Time.Text) - (m_CycleStopwatch.ElapsedMilliseconds / 1000);
                                Img_Activate.Opacity = 1.0 / nChecking;
                            }
                        }
                        else
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
                    }
                    else
                    {
                        if (m_bActivated)
                        {
                            Img_Activate.Opacity = 0;
                        }
                        Grd_CycleBackground.Background = new SolidColorBrush(Colors.White);
                    }
                }
            }
            else
            {
                Img_Activate.Opacity = 0;
                if (m_CycleStopwatch.ElapsedMilliseconds > int.Parse(TBox_RestTime.Text) * 1000)
                {
                    m_bRest = false;
                    m_CycleStopwatch.Restart();
                }
                else
                {
                    TBlo_CycleTimer.Text = string.Format("쉬는 시간\r\n{0}초\r\n남음", int.Parse(TBox_RestTime.Text) - (m_CycleStopwatch.ElapsedMilliseconds / 1000));
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
            m_bActivated = false;
            m_bRest = false;

            if (Checking_Values() == false)
            {
                return;
            }
            m_nCycle = 0;

            m_CycleTimer.Start();
            m_CycleStopwatch.Restart();

            UISetting(true);
        }

        private void Btn_Stop_Click(object sender, RoutedEventArgs e)
        {
            if (m_bActivated)
            {
                return;
            }
            m_CycleTimer.Stop();
            TBlo_CycleCount.Text = string.Empty;
            TBlo_CycleTimer.Text = string.Empty;
            Grd_CycleBackground.Background = new SolidColorBrush(Colors.White);
            Img_Activate.Opacity = 0;
            m_bActivated = false;

            UISetting(false);
        }

        private void Btn_AppClose_Click(object sender, RoutedEventArgs e)
        {
            if (m_bActivated)
            {
                return;
            }
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

            int maxValue = 100;
            int minValue = 1;

            if (!string.IsNullOrEmpty(newText) && int.Parse(newText) > maxValue)
            {
                e.Handled = true;
                textBox.Text = maxValue.ToString();
                textBox.CaretIndex = textBox.Text.Length;
                return;
            }

            if (!string.IsNullOrEmpty(newText) && int.Parse(newText) < minValue)
            {
                e.Handled = true;
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (m_bActivated)
            {
                e.Cancel = true;
            }
        }
    }
}
