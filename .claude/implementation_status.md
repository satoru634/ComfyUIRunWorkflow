# 実装状況

## 現在の状態（2026-07-04 時点）

`ComfyUILibs` のフェーズ1実装が完了・master マージ済み。
`ComfyUIRunWorkflow` のフェーズ2（GUI 実装）・フェーズ3（テンプレート配置）・フェーズ4（生成画像プレビュー表示）・フェーズ5（バッチ数指定）・フェーズ6（WD14 Tagger 専用ページ）が完了・master マージ済み。
フェーズ7（タグ付け履歴の DataPage への統合）が `feature/tagger-history` ブランチで実装中。

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
- `ComfyUILibs.csproj` — `net8.0-windows7.0` / WPF / `CommunityToolkit.Mvvm 8.4.2` 設定済み

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
- [x] `Services/ConfigLoader.cs` — workflow_config.json ロード・バリデーション（load_files.py 移植）
- [x] `Services/ComfyUIClient.cs` — REST API + WebSocket クライアント（comfyui_client.py 移植）
- [x] `Services/WorkflowBuilder.cs` — テンプレート選択・書き換え（workflow_builder.py 移植）
- [x] `Services/WorkflowRunner.cs` — 実行ファサード（WorkflowRunner 移植）
- [x] `Services/Wd14TaggerRunner.cs` — WD14 Tagger（wd14_tagger_runner.py 移植）
- [x] `Services/PreviewImageCacheService.cs` — 生成画像プレビューのローカルキャッシュ管理（フェーズ4）

**テスト（ComfyUILibsTests）**
- [x] `Exceptions/ComfyUIExceptionTests.cs` — ComfyUIException テスト
- [x] `Services/ConfigLoaderTests.cs` — ConfigLoader テスト（38件）
- [x] `Services/ComfyUIClientTests.cs` — ComfyUIClient テスト（13件、FakeHttpMessageHandler 使用、GetImageAsync 含む）
- [x] `Services/WorkflowBuilderTests.cs` — WorkflowBuilder テスト（14件）
- [x] `Services/WorkflowRunnerTests.cs` — WorkflowRunner テスト（9件、FakeComfyUIClient 使用）
- [x] `Services/Wd14TaggerRunnerTests.cs` — Wd14TaggerRunner テスト（5件）
- [x] `Services/PreviewImageCacheServiceTests.cs` — PreviewImageCacheService テスト（12件）

合計テスト数: 151件（全パス）

### フェーズ 2: ComfyUIRunWorkflow の GUI 実装（完了）

**ViewModel**
- [x] `ViewModels/Pages/DashboardViewModel.cs` — ワークフロー実行 VM（ConfigLoader + WorkflowRunner 使用）
- [x] `ViewModels/Pages/SettingsViewModel.cs` — 設定 VM（ComfyUI URL・config パス・結果フォルダ）
- [x] `ViewModels/Pages/DataViewModel.cs` — 実行結果一覧 VM（result_*.json 読み込み）

**View**
- [x] `Views/Pages/DashboardPage.xaml` — ワークフロー実行 UI
- [x] `Views/Pages/SettingsPage.xaml` — 設定 UI
- [x] `Views/Pages/DataPage.xaml` — 実行結果 UI
- [x] `Views/Windows/ResultDetailWindow.xaml` — 結果詳細ダイアログ

**Model**
- [x] `Models/AppConfig.cs` — ComfyUIUrl・ConfigPath・ResultsFolder フィールド追加
- [x] `Models/LoraSlot.cs` — LoRA 選択スロット Observable ラッパー

**Helpers**
- [x] `Helpers/BoolToVisibilityConverter.cs` — bool→Visibility 変換
- [x] `App.xaml` — BoolToVisibilityConverter・NullToVisibilityConverter をリソース登録

**テスト（ComfyUIRunWorkflowTests）**
- [x] `Models/AppConfigTests.cs` — 新フィールド（ComfyUIUrl・ConfigPath・ResultsFolder）テスト追加
- [x] `ViewModels/Pages/DashboardViewModelTests.cs` — ワークフロー実行 VM テスト（29件）
- [x] `ViewModels/Pages/DataViewModelTests.cs` — 結果一覧 VM テスト（12件）

合計テスト数: 89件（全パス）

### フェーズ 3: テンプレートファイルの配置（完了）

- [x] `templates/` ディレクトリをリポジトリに追加
  - Python版の `run_workflow/templates/` をコピー（anima / anima_rapid / sdxl 各5ファイル + template_wd14_tagger.json）
  - csproj に `<Content Include="templates\**\*"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></Content>` を追加

### フェーズ 4: 生成画像プレビュー表示（`feature/preview-image` ブランチ、実装完了）

**ComfyUILibs**
- [x] `Services/IComfyUIClient.cs` / `ComfyUIClient.cs` — `GetImageAsync`（GET /view）を追加
- [x] `Services/IPreviewImageCacheService.cs` / `PreviewImageCacheService.cs` — 画像判定・ローカルキャッシュ管理を新規実装

**ComfyUIRunWorkflow**
- [x] `Models/OutputFilePreview.cs` — 出力ファイル1件分のプレビュー状態（Thumbnail・IsLoading・HasError）
- [x] `Models/WorkflowResultPreview.cs` — DataPage 一覧行ラッパー（WorkflowResult + サムネイル1枚）
- [x] `Services/PreviewImageLoader.cs` — BitmapImage 読み込み（サムネイル/原寸）
- [x] `ViewModels/Windows/ResultDetailViewModel.cs` — 詳細ダイアログの出力ファイル一覧・拡大表示コマンド
- [x] `Views/Windows/ImagePreviewWindow.xaml` — 画像拡大表示ウィンドウ
- [x] `ViewModels/Pages/DataViewModel.cs` / `Views/Pages/DataPage.xaml` — 一覧カードへのサムネイル追加
- [x] `ViewModels/Pages/DashboardViewModel.cs` / `Views/Pages/DashboardPage.xaml` — 実行直後のプレビュー表示
- [x] `Views/Windows/ResultDetailWindow.xaml` — 出力ファイル欄をサムネイル一覧＋拡大表示に変更

キャッシュ先: `{ResultsFolder}/preview_cache/`（`GET /view` で取得した画像をファイルとして保存し、以降は再取得しない）

**テスト**
- [x] `ComfyUILibsTests/Services/PreviewImageCacheServiceTests.cs`（12件）
- [x] `ComfyUIRunWorkflowTests/Models/OutputFilePreviewTests.cs`
- [x] `ComfyUIRunWorkflowTests/Services/PreviewImageLoaderTests.cs`
- [x] `ComfyUIRunWorkflowTests/ViewModels/Windows/ResultDetailViewModelTests.cs`
- [x] `ComfyUIRunWorkflowTests/ViewModels/Pages/DataViewModelTests.cs` — `Results` の型変更（`WorkflowResultPreview`）に追従

合計テスト数: ComfyUILibsTests 151件 / ComfyUIRunWorkflowTests 121件（全パス）

### フェーズ 5: バッチ数指定（`feature/batch-count` ブランチ、実装完了）

ComfyUI Web 画面の「バッチ数」と同様、指定回数だけワークフロー実行をキューへ繰り返し送信する機能。
`EmptyLatentImage.batch_size` は変更せず、`WorkflowRunner.ExecuteAsync` を順番に複数回呼び出す方式で実装（各回シードは既存仕様通り自動採番）。

**ComfyUIRunWorkflow**
- [x] `ViewModels/Pages/DashboardViewModel.cs`
  - `BatchCount`（int, 既定 1, 1〜10 を想定）・`BatchProgressText`（例: "2/5件目を実行中"）プロパティを追加
  - `RunWorkflowAsync` を `BatchCount` 回のループに変更。各回の出力・プレビューサムネイルを累積し、result_*.json は1件にまとめて保存
  - 途中で `ComfyUIException` が発生した場合はその時点で中断し、成功済み分の出力を含めたエラー結果を保存
  - 進捗テキスト生成ロジックを `FormatBatchProgress(int, int)`（internal static）として切り出し、単体テスト可能にした
- [x] `Views/Pages/DashboardPage.xaml` — 実行ボタン左に「バッチ数」`ui:NumberBox`（Minimum=1, Maximum=10）を配置、ProgressBar 下に進捗テキストを表示
- [x] 実行中の二重実行防止のため `CanRun()` に `!IsRunning` を追加（`IsRunning` に `NotifyCanExecuteChangedFor` を付与）

**実装後に発覚した不具合の修正**
- [x] `PreviewThumbnails` へのバッチ毎の `Add` が `HasPreviewThumbnails`（右パネルの表示切り替え）の再通知に繋がらず、生成結果が表示されない不具合を修正（`Add` 後に `OnPropertyChanged(nameof(HasPreviewThumbnails))` を明示的に呼び出す）
- [x] `WorkflowRunner.ExecuteAsync`：`MonitorAsync` の完了検知直後は ComfyUI 側の history 反映がわずかに遅延し `GetOutputsAsync` が空リストを返すことがあり、バッチ実行時に出力件数が欠落する不具合を修正（300ms 間隔で最大3回リトライ）

**テスト**
- [x] `ComfyUIRunWorkflowTests/ViewModels/Pages/DashboardViewModelTests.cs` — `BatchCount`/`BatchProgressText` 既定値、`FormatBatchProgress` のテストを追加
- [x] `ComfyUILibsTests/Services/WorkflowRunnerTests.cs` — outputs 空リトライの成功/リトライ上限到達のテストを追加

合計テスト数: ComfyUILibsTests 153件 / ComfyUIRunWorkflowTests 126件（全パス）

### フェーズ 6: WD14 Tagger 専用ページ（`feature/tagger-page` ブランチ、実装完了）

画像 1 枚を選択して WD14 Tagger ワークフローを実行し、タグ文字列を取得・コピーできる専用ページ。
モデル名・しきい値（general/character threshold）は `workflow_config.json` の固定値を使用し、ページ上での変更は不可。

**ComfyUILibs**
- [x] `Models/TagResult.cs` — タグ付け結果モデル（Status, Timestamp, InputFilename, Tags, Error）。`WorkflowResult` とはスキーマが異なるため別モデルとして新設

**ComfyUIRunWorkflow**
- [x] `ViewModels/Pages/TaggerViewModel.cs`
  - `OnNavigatedToAsync` で `Wd14TaggerRunner` の初期化を試行し、`wd14_tagger` セクション欠如等は `ISnackbarService` でエラー表示（`DashboardViewModel.TryLoadConfig` と同じパターン）
  - `BrowseImageCommand`（`OpenFileDialog`）・`SetSelectedImage(path)`（ドラッグ&ドロップ用に公開）で画像を選択、`PreviewImageLoader.LoadFullSize` でプレビュー表示
  - `TagImageCommand` で `Wd14TaggerRunner.TagAsync` を実行し、結果を `ResultTags` に反映。成功・失敗いずれも `TagResult` を `tag_result_{timestamp}.json` として `ResultsFolder` に保存（`result_*.json` とは別ファイル名で管理し、DataPage の一覧には統合しない）
  - `CopyTagsCommand` で結果をクリップボードにコピー
- [x] `Views/Pages/TaggerPage.xaml` / `TaggerPage.xaml.cs` — 画像選択・ドラッグ&ドロップ領域・プレビュー・タグ結果表示（`ui:TextBox`）・コピーボタン
- [x] `Helpers/BoolToVisibilityConverter.cs` — `NullToVisibilityInverseConverter` を追加（未選択時のプレースホルダー表示用）、`App.xaml` にリソース登録
- [x] `ViewModels/Windows/MainWindowViewModel.cs` — ナビゲーションメニューに「Tagger」項目を追加（Run workflow と Results の間）
- [x] `App.xaml.cs` — `TaggerPage` / `TaggerViewModel` を DI 登録

**テスト**
- [x] `ComfyUILibsTests/Models/TagResultTests.cs`（3件）
- [x] `ComfyUIRunWorkflowTests/ViewModels/Pages/TaggerViewModelTests.cs` — config読み込み成功/失敗、`SetSelectedImage` の画像判定、`TagImageCommand`/`CopyTagsCommand` の CanExecute 条件

合計テスト数: ComfyUILibsTests 156件 / ComfyUIRunWorkflowTests 147件（全パス）

### フェーズ 7: タグ付け履歴の DataPage への統合（`feature/tagger-history` ブランチ、実装完了）

DataPage に「生成結果」「タグ付け履歴」のタブ切り替えを追加し、`tag_result_*.json` を一覧表示する機能。
`WorkflowResult` と `TagResult` はスキーマが異なるため、既存の一覧・カード・詳細ダイアログには統合せず、タブで表示を切り替える構成にした。
タグ付け履歴カードにはサムネイル表示・詳細ダイアログを設けず、ファイル名・タイムスタンプ・タグ全文・コピーボタンのみでカード上で完結させている。

**ComfyUIRunWorkflow**
- [x] `ViewModels/Pages/DataViewModel.cs`
  - `TagResults`（`ObservableCollection<TagResult>`）・`TagStatusMessage`・`IsTagHistorySelected` プロパティを追加
  - `LoadResultsAsync` を `LoadWorkflowResultsAsync`（既存の result_*.json 読み込み）と `LoadTagHistoryAsync`（新規、tag_result_*.json 読み込み）に分割し、両方を呼び出す形に変更
  - `ShowResultsTabCommand` / `ShowTagHistoryTabCommand` でタブ切り替え、`RefreshCommand` は両タブ分を再読み込み
  - `CopyTagsCommand(TagResult)` でタグ付け履歴のタグをクリップボードにコピー
- [x] `Views/Pages/DataPage.xaml` — ヘッダー下にタブ切り替えボタン（`DataTrigger` で選択中タブを `Primary` Appearance にハイライト）、タグ付け履歴用のカード `DataTemplate`（`ComfyUILibs.Models.TagResult` 型）を追加

**テスト**
- [x] `ComfyUIRunWorkflowTests/ViewModels/Pages/DataViewModelTests.cs` — タグ付け履歴の読み込み・タブ切り替え・`CopyTagsCommand` のテストを追加

合計テスト数: ComfyUILibsTests 156件 / ComfyUIRunWorkflowTests 157件（全パス）

### 将来的な拡張

- C# 版 Discord ボット（ComfyUILibs を共用）
- 実行履歴の永続化（SQLite 等）
