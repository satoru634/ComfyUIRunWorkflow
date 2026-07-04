# ComfyUILibs
別リポジトリ（サブモジュール）のため、技術スタックは `ComfyUILibs/.claude/tech_stack.md` を参照。

# ComfyUIRunWorkflow
- .NET 8 / WPF
- [Wpf.Ui](https://github.com/lepoco/wpfui) v4.2.0 — UI フレームワーク
- CommunityToolkit.Mvvm v8.4.0 — MVVM（`[ObservableProperty]`/`[RelayCommand]`）
- Microsoft.Extensions.Hosting — DI・ライフサイクル管理
- `.resx` + `System.Resources.ResourceManager` — GUI の多言語化（`Helpers/LocalizationManager.cs`、XAML インデクサーバインディングで即時反映）
