# 実装状況

## 現在の状態（2026-06-28 時点）

`ComfyUILibs` の `Common` および `Base` 名前空間が実装済み・master マージ済み。
`ComfyUIRunWorkflow` は WPF-UI テンプレートのスキャフォールドのみで ComfyUI 関連の実装はゼロ。

### 存在するファイル（テンプレート由来）

**ComfyUIRunWorkflow（WPF）**
- `App.xaml` / `App.xaml.cs` — DI・ホスト設定（流用可能）
- `Services/ApplicationHostService.cs` — 起動時ウィンドウ表示（流用可能）
- `Views/Pages/DashboardPage.xaml` — カウンターデモ（→ ワークフロー実行ページに置換）
- `Views/Pages/DataPage.xaml` — 空（→ 実行結果ページに置換）
- `Views/Pages/SettingsPage.xaml` — 空（→ 設定ページとして実装）
- `Models/AppConfig.cs` — スタブ（→ 削除、ComfyUILibs に移管）
- `Helpers/EnumToBooleanConverter.cs` — テーマ切り替え用（流用可能）

**ComfyUILibs（実装済み）**
- `Common/JsonLoader.cs` — JSON ファイル読み書き静的ユーティリティ（master マージ済み）
- `Common/Setting.cs` — 設定ファイル永続化ジェネリッククラス（master マージ済み）
- `Base/ObservablePoint.cs` — `INotifyPropertyChanged` 対応の Point ラッパー（master マージ済み）
- `Base/ObservableSize.cs` — `INotifyPropertyChanged` 対応の Size ラッパー（master マージ済み）
- `ComfyUILibs.csproj` — `net8.0-windows10.0.17763.0` / WPF / `CommunityToolkit.Mvvm 8.4.2` 設定済み

---

## 実装ロードマップ

### フェーズ 1: ComfyUILibs の実装（Python版移植）

**Common（実装済み・master マージ済み）**
- [x] `Common/JsonLoader.cs` — JSON ファイル読み書き静的ユーティリティ
- [x] `Common/Setting.cs` — 設定ファイル永続化ジェネリッククラス

**Base（実装済み・master マージ済み）**
- [x] `Base/ObservablePoint.cs` — `INotifyPropertyChanged` 対応の Point ラッパー（`ToPoint` / `FromPoint`）
- [x] `Base/ObservableSize.cs` — `INotifyPropertyChanged` 対応の Size ラッパー（`ToSize` / `FromSize`）

**Exceptions**
- [x] `Exceptions/ComfyUIException.cs` — 基底例外クラス

**Models**
- [x] `Models/WorkflowConfig.cs` — 設定 JSON モデル（WorkflowConfig, LoraEntry, ImageSize, WorkflowSettings, Wd14TaggerConfig）
- [x] `Models/WorkflowInput.cs` — 入力 JSON モデル（WorkflowInput, PromptPair）
- [x] `Models/WorkflowResult.cs` — 結果モデル（WorkflowResult, OutputFile, WorkflowParameters）
- [x] `Models/ResolvedLora.cs` — LoRA 解決済みエントリ

**Services**
- [x] `Services/IComfyUIClient.cs` — ComfyUIClient インターフェース（テスト用 DI 対応）
- [x] `Services/ConfigLoader.cs` — config.json ロード・バリデーション（load_files.py 移植）
- [x] `Services/ComfyUIClient.cs` — REST API + WebSocket クライアント（comfyui_client.py 移植）
- [x] `Services/WorkflowBuilder.cs` — テンプレート選択・書き換え（workflow_builder.py 移植）
- [x] `Services/WorkflowRunner.cs` — 実行ファサード（WorkflowRunner 移植）
- [x] `Services/Wd14TaggerRunner.cs` — WD14 Tagger（wd14_tagger_runner.py 移植）

**テスト（ComfyUILibsTests）**
- [x] `Exceptions/ComfyUIExceptionTests.cs` — ComfyUIException テスト
- [x] `Services/ConfigLoaderTests.cs` — ConfigLoader テスト（38件）
- [x] `Services/ComfyUIClientTests.cs` — ComfyUIClient テスト（9件、FakeHttpMessageHandler 使用）
- [x] `Services/WorkflowBuilderTests.cs` — WorkflowBuilder テスト（14件）
- [x] `Services/WorkflowRunnerTests.cs` — WorkflowRunner テスト（9件、FakeComfyUIClient 使用）
- [x] `Services/Wd14TaggerRunnerTests.cs` — Wd14TaggerRunner テスト（5件）

合計テスト数: 120件（全パス）

### フェーズ 2: ComfyUIRunWorkflow の GUI 実装

**ViewModel**
- [ ] `ViewModels/Pages/DashboardViewModel.cs` — ワークフロー実行 VM（WorkflowRunner を DI で利用）
- [ ] `ViewModels/Pages/SettingsViewModel.cs` — 設定 VM（ComfyUI URL・config パス等）
- [ ] `ViewModels/Pages/DataViewModel.cs` — 実行結果一覧 VM

**View**
- [ ] `Views/Pages/DashboardPage.xaml` — ワークフロー実行 UI
- [ ] `Views/Pages/SettingsPage.xaml` — 設定 UI
- [ ] `Views/Pages/DataPage.xaml` — 実行結果 UI

**DI 登録**
- [ ] `App.xaml.cs` に ComfyUILibs のサービス登録を追加
- [ ] WPF プロジェクトの csproj に ComfyUILibs のプロジェクト参照を追加

### フェーズ 3: テンプレートファイルの配置

- [ ] `templates/` ディレクトリをリポジトリに追加
  - Python版の `run_workflow/templates/` を参照・コピー
  - csproj でビルド時に出力ディレクトリへコピーする設定（`CopyToOutputDirectory`）

### 将来的な拡張

- C# 版 Discord ボット（ComfyUILibs を共用）
- WD14 Tagger 専用ページ
- 実行履歴の永続化（SQLite 等）
- 生成画像のプレビュー表示
