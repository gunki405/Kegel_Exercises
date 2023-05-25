using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CycleApp.App
{
    /// <summary>
    /// Win_LanguageSetting.xaml の相互作用ロジック
    /// </summary>
    public partial class Win_LanguageSetting : Window
    {
        private string m_language;
        public Win_LanguageSetting()
        {
            InitializeComponent();
            this.Topmost = true;

            StackPnl_Lang.Children.Clear();

            string resourcesDirectory = $"Resources";
            string[] resourceFiles = Directory.GetFiles(resourcesDirectory, "*.resx");

            foreach (string file in resourceFiles)
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(file);

                RadioButton LangSetting = new RadioButton();
                LangSetting.GroupName = "Lang";

                string strLanguageName = fileName.Replace("Strings", string.Empty);
                strLanguageName = strLanguageName.Replace(".", string.Empty);
                if (strLanguageName == string.Empty)
                {
                    strLanguageName = "en-US";
                }

                if(Properties.Settings.Default.LanguageSetting == strLanguageName)
                {
                    LangSetting.IsChecked = true;
                }
                else
                {
                    LangSetting.IsChecked = false;
                }

                CultureInfo cultureInfo = new CultureInfo(strLanguageName);
                LangSetting.Content = cultureInfo.NativeName;
                LangSetting.Name = "RBtn_" + cultureInfo.ThreeLetterWindowsLanguageName;
                LangSetting.Margin = new Thickness(5);
                LangSetting.Tag = strLanguageName;
                LangSetting.Checked += RadioButton_Checked;

                StackPnl_Lang.Children.Add(LangSetting);
            }
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton selectedRadioButton = (RadioButton)sender;
            m_language = (string)selectedRadioButton.Tag;

        }


        private void Btn_Setting_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.LanguageSetting = m_language;
            Properties.Settings.Default.Save();

            this.Close();
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
