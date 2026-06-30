using Wpf.Ui;
using Wpf.Ui.Controls;

namespace ComfyUIRunWorkflowTests.Fakes
{
    /// <summary>
    /// テスト用の ISnackbarService スタブ。Show の呼び出し履歴を記録する。
    /// </summary>
    internal class FakeSnackbarService : ISnackbarService
    {
        public List<(string Title, string Message, ControlAppearance Appearance)> Calls { get; } = new();

        public TimeSpan DefaultTimeOut { get; set; } = TimeSpan.FromSeconds(3);

        public void SetSnackbarPresenter(SnackbarPresenter contentPresenter) { }

        public SnackbarPresenter GetSnackbarPresenter() => throw new NotImplementedException();

        public void Show(string title, string message, ControlAppearance appearance, IconElement? icon, TimeSpan timeout)
            => Calls.Add((title, message, appearance));
    }
}
