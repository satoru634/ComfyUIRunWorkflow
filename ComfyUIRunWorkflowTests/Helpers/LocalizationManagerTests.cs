using System.ComponentModel;
using System.Globalization;
using ComfyUIRunWorkflow.Helpers;

namespace ComfyUIRunWorkflowTests.Helpers
{
    public class LocalizationManagerTests
    {
        /// <summary>
        /// LocalizationManager.Instance はプロセス全体で共有されるシングルトンのため、
        /// テスト間で状態が漏れないよう元のカルチャを保存・復元するヘルパー。
        /// </summary>
        private static void WithCulture(string cultureName, Action action)
        {
            var original = LocalizationManager.Instance.CurrentCulture;
            try
            {
                LocalizationManager.Instance.CurrentCulture = new CultureInfo(cultureName);
                action();
            }
            finally
            {
                LocalizationManager.Instance.CurrentCulture = original;
            }
        }

        [Fact]
        public void Indexer_JapaneseCulture_ReturnsJapaneseText() =>
            WithCulture("ja", () =>
                Assert.Equal("設定", LocalizationManager.Instance["Settings_Title"]));

        [Fact]
        public void Indexer_EnglishCulture_ReturnsEnglishText() =>
            WithCulture("en", () =>
                Assert.Equal("Settings", LocalizationManager.Instance["Settings_Title"]));

        [Fact]
        public void Indexer_EnglishUsCulture_FallsBackToEnglishSatellite() =>
            WithCulture("en-US", () =>
                Assert.Equal("Settings", LocalizationManager.Instance["Settings_Title"]));

        [Fact]
        public void Indexer_UnknownKey_ReturnsKeyItself()
        {
            Assert.Equal("NonExistentKey", LocalizationManager.Instance["NonExistentKey"]);
        }

        [Fact]
        public void CurrentCulture_Set_UpdatesCurrentUICulture()
        {
            var original = LocalizationManager.Instance.CurrentCulture;
            try
            {
                LocalizationManager.Instance.CurrentCulture = new CultureInfo("en");

                Assert.Equal("en", CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
            }
            finally
            {
                LocalizationManager.Instance.CurrentCulture = original;
            }
        }

        [Fact]
        public void CurrentCulture_SetDifferentValue_RaisesPropertyChangedForIndexer()
        {
            var original = LocalizationManager.Instance.CurrentCulture;
            try
            {
                LocalizationManager.Instance.CurrentCulture = new CultureInfo("ja");
                var changed = new List<string?>();
                ((INotifyPropertyChanged)LocalizationManager.Instance).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

                LocalizationManager.Instance.CurrentCulture = new CultureInfo("en");

                Assert.Contains("Item[]", changed);
            }
            finally
            {
                LocalizationManager.Instance.CurrentCulture = original;
            }
        }

        [Fact]
        public void CurrentCulture_SetSameValue_DoesNotRaisePropertyChanged()
        {
            var original = LocalizationManager.Instance.CurrentCulture;
            try
            {
                LocalizationManager.Instance.CurrentCulture = new CultureInfo("ja");
                var changed = new List<string?>();
                ((INotifyPropertyChanged)LocalizationManager.Instance).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

                LocalizationManager.Instance.CurrentCulture = new CultureInfo("ja");

                Assert.Empty(changed);
            }
            finally
            {
                LocalizationManager.Instance.CurrentCulture = original;
            }
        }
    }
}
