//来源：http://www.dotblogs.com.tw/ouch1978/archive/2011/07/29/wpf-globalization-resourcedictionary.aspx
//根据VS的代码分析，修改了一些地方

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;

namespace Lyrik.Gui
{
    class CulturesHelper
    {
        private static bool _isFoundInstalledCultures = false;
        private static string _resourcePrefix = "UIText";
        private static string _culturesFolder = "Cultures";
        private static List<CultureInfo> _supportedCultures = new List<CultureInfo>();
        public static List<CultureInfo> SupportedCultures
        {
            get
            {
                return _supportedCultures;
            }
        }
        public CulturesHelper()
        {
            if (!_isFoundInstalledCultures)
            {
                CultureInfo cultureInfo = new CultureInfo("");

                List<string> files = Directory.GetFiles(string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", new string[] { System.Windows.Forms.Application.StartupPath, _culturesFolder }))
                    .Where(s => s.Contains(_resourcePrefix) && s.EndsWith("XAML", StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (string file in files)
                {
                    try
                    {
                        string cultureName = file.Substring(file.IndexOf(".", StringComparison.OrdinalIgnoreCase) + 1).Replace(".xaml", "");
                        cultureInfo = CultureInfo.GetCultureInfo(cultureName);
                        if (cultureInfo != null)
                        {
                            _supportedCultures.Add(cultureInfo);
                        }
                    }
                    catch (ArgumentException)
                    {
                    }
                }
                if (_supportedCultures.Count > 0 && Properties.Settings.Default.DefaultCulture != null)
                {
                    ChangeCulture(Properties.Settings.Default.DefaultCulture);
                }
                _isFoundInstalledCultures = true;
            }
        }
        public static void ChangeCulture(CultureInfo culture)
        {
            if (_supportedCultures.Contains(culture))
            {
                string LoadedFileName = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}\\{2}.{3}.xaml", new string[] { System.Windows.Forms.Application.StartupPath, _culturesFolder
                    , _resourcePrefix, culture.Name });
                FileStream fileStream = new FileStream(@LoadedFileName, FileMode.Open);

                try
                {
                    ResourceDictionary resourceDictionary = XamlReader.Load(fileStream) as ResourceDictionary;
                    Application.Current.MainWindow.Resources.MergedDictionaries.Add(resourceDictionary);
                    Properties.Settings.Default.DefaultCulture = culture;
                    Properties.Settings.Default.Save();
                }
                finally
                {
                    if (fileStream != null)
                        fileStream.Dispose();
                }
            }
        }
    }
}
