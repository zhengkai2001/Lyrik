using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;

namespace Lyrik
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App
    {
        private static Dictionary<string, ResourceDictionary> _cultures;

        public static string Culture { get; set; }
        private const string DefaultCultureFile = @"Cultures/UIText.xaml";

        public static ResourceDictionary CultureDictionary;

        protected override void OnStartup(StartupEventArgs e)
        {
            _cultures = new Dictionary<string, ResourceDictionary>();
            foreach (var dictionary in Current.Resources.MergedDictionaries)
            {
                _cultures[dictionary.Source.OriginalString] = dictionary;
            }

            SetDefaultCulture();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SaveCulture();
        }

        private static void SetDefaultCulture()
        {
            Culture = Lyrik.Properties.Settings.Default.Culture.Trim();

            if (string.IsNullOrEmpty(Culture))
            {
                Culture = CultureInfo.CurrentCulture.ToString();
            }

            if (string.IsNullOrEmpty(Culture))
            {
                Culture = "en-US";
            }

            UpdateCulture();
        }

        private static void SaveCulture()
        {
            Lyrik.Properties.Settings.Default.Culture = Culture;
            Lyrik.Properties.Settings.Default.Save();
        }

        public static void SetCulture(string culture)
        {
            Culture = culture;
            UpdateCulture();
        }

        private static void UpdateCulture()
        {
            var requestedCultureFile = string.Format(CultureInfo.CurrentCulture, @"Cultures/UIText.{0}.xaml", Culture);
            if (!SetCultureFile(requestedCultureFile))
            {
                SetCultureFile(DefaultCultureFile);
            }
        }

        private static bool SetCultureFile(string cultureFile)
        {
            if (!_cultures.ContainsKey(cultureFile))
            {
                return false;
            }

            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(Culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Culture);

            CultureDictionary = _cultures[cultureFile];
            return true;
        }
    }
}
