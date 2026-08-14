using System.Windows.Controls;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace ComfyUIRunWorkflowTests.Fakes
{
    /// <summary>
    /// テスト用の IContentDialogService スタブ。
    /// </summary>
    internal class FakeContentDialogService : IContentDialogService
    {
        public void SetContentPresenter(ContentPresenter contentPresenter) { }

        public ContentPresenter GetContentPresenter() => throw new NotImplementedException();

        public void SetDialogHost(ContentPresenter contentPresenter) { }

        public void SetDialogHost(ContentDialogHost contentDialogHost) { }

        public ContentPresenter GetDialogHost() => throw new NotImplementedException();

        public ContentDialogHost GetDialogHostEx() => throw new NotImplementedException();

        public Task<ContentDialogResult> ShowAsync(ContentDialog dialog, CancellationToken cancellationToken = default)
            => Task.FromResult(ContentDialogResult.None);
    }
}
