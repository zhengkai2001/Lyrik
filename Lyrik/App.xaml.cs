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
        private static Dictionary<string, ResourceDictionary> _cultureDictionaries;
        private static ResourceDictionary _cultureDictionary;

        private const string DefaultCulture = @"en-US";
        private const string DefaultCultureFile = @"Cultures/UIText.xaml";

        protected override void OnStartup(StartupEventArgs e)
        {
            _cultureDictionaries = new Dictionary<string, ResourceDictionary>();
            foreach (var dictionary in Current.Resources.MergedDictionaries)
            {
                _cultureDictionaries[dictionary.Source.OriginalString] = dictionary;
            }

            InitilizeCulture();
        }

        private static void InitilizeCulture()
        {
            // 1. try the culture in the setting file
            var culture = Lyrik.Properties.Settings.Default.Culture.Trim();

            // 2. if failed, try the culture of the user's machine
            if (string.IsNullOrEmpty(culture))
            {
                culture = CultureInfo.CurrentCulture.ToString();
            }

            // 3. if failed, try default culture
            if (string.IsNullOrEmpty(culture))
            {
                culture = DefaultCulture;
            }

            SetCulture(culture);
        }

        private static void SaveCultureSettings(string culture, string cultureFile)
        {
            Lyrik.Properties.Settings.Default.Culture = culture;
            Lyrik.Properties.Settings.Default.CultureFile = cultureFile;
            Lyrik.Properties.Settings.Default.Save();
        }

        public static void SetCulture(string culture)
        {
            var cultureFile = string.Format(CultureInfo.CurrentCulture, @"Cultures/UIText.{0}.xaml", culture);
            if (!_cultureDictionaries.ContainsKey(cultureFile))
            {
                culture = DefaultCulture;
                cultureFile = DefaultCultureFile;
            }

            SaveCultureSettings(culture, cultureFile);

            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);

            _cultureDictionary = _cultureDictionaries[cultureFile];
        }

        public static string GetLocalizedString(string key)
        {
            return _cultureDictionary[key].ToString();
        }
    }
}
