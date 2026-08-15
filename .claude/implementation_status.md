# 実装状況

## 現在の状態（2026-08-14 時点）

`ComfyUILibs` のフェーズ1実装・フェーズ2（例外メッセージの多言語化）が完了・master マージ済み。
`ComfyUIRunWorkflow` のフェーズ2（GUI 実装）・フェーズ3（テンプレート配置）・フェーズ4（生成画像プレビュー表示）・フェーズ5（バッチ数指定）・フェーズ6（WD14 Tagger 専用ページ）・フェーズ7（タグ付け履歴の DataPage への統合）・フェーズ9（GUI の多言語化）・フェーズ10（結果詳細ダイアログ・画像拡大表示ダイアログの ui:ContentDialog 化・Controls への再配置）が完了・master マージ済み。
フェーズ11（複数ワークフロー連続実行 Queue ページ）が `feature/queue-page` ブランチで実装完了。

**注意**: `ResultDetailWindow`/`ImagePreviewWindow`（View）は `Views/Windows/` → `Views/Controls/` へ、`ResultDetailViewModel` は `ViewModels/Windows/` → `ViewModels/Controls/` へ移動済み（フェーズ10）。フェーズ2〜9 の記述中の該当パスは変更前時点のものである。

### 存在するファイル（テンプレート由来）

**ComfyUIRunWorkflow（WPF）**
- `App.xaml` / `App.xaml.cs` — DI・ホスト設定（流用可能）
- `Services/ApplicationHostService.cs` — 起動時ウィンドウ表示（流用可能）
- `Views/Pages/DashboardPage.xaml` — カウンターデモ（→ ワークフロー実行ページに置換）
- `Views/Pages/DataPage.xaml` — 空（→ 実行結果ページに置換）
- `Views/Pages/SettingsPage.xaml` — 空（→ 設定ページとして実装）
- `Models/AppConfig.cs` — スタブ（→ 削除、ComfyUILibs に移管）
- `Helpers/EnumToBooleanConverter.cs` — テーマ切り替え用（流用可能）

**ComfyUILibs（実装済み・別リポジトリ）**
実装済みクラスの一覧は `ComfyUILibs/.claude/implementation_status.md` を参照。

---

## 実装ロードマップ

### フェーズ 1: ComfyUILibs の実装（Python版移植）

`ComfyUILibs` は別リポジトリ（サブモジュール）に分離されており、実装状況・クラス一覧・テスト件数は `ComfyUILibs/.claude/implementation_status.md` および `ComfyUILibs/README.md` を参照。完了・master マージ済み。

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

### フェーズ8: ComfyUILibs の例外メッセージ多言語化（`ComfyUILibs` リポジトリ側、実装完了）

ComfyUIException がスローするメッセージを `.resx` ベースのリソースに外部化した。詳細は `ComfyUILibs/.claude/implementation_status.md` のフェーズ2を参照。

### フェーズ9: GUI の多言語化（`feature/i18n-gui` ブランチ、実装完了）

日本語に加えて英語を選択できるよう、GUI の全画面（XAML 固定文言・ViewModel のステータス/Snackbarメッセージ・ファイル選択ダイアログ・ウィンドウタイトル・ナビゲーションメニュー・トレイメニュー）を多言語化した。

**ComfyUIRunWorkflow**
- [x] `Resources/Strings.resx`（既定・neutral resource、日本語）／`Strings.en.resx`（英語サテライト）／`Strings.cs`（ResourceManager ラッパー）を新規作成
- [x] `Helpers/LocalizationManager.cs` — `INotifyPropertyChanged` を実装したシングルトン。インデクサー `this[string key]` を XAML から `{Binding Source={x:Static helpers:LocalizationManager.Instance}, Path=[キー]}` の形でバインドすることで、`CurrentCulture` 変更時に全画面へ即座に反映される（再起動不要）
- [x] `Models/AppConfig.cs` に `string Language`（既定値 `"ja"`）を追加。OS ロケールに関わらず既定は日本語を維持するため、`ApplicationHostService.StartAsync` で明示的に `LocalizationManager.Instance.CurrentCulture` を設定する
- [x] `Views/Pages/SettingsPage.xaml` / `ViewModels/Pages/SettingsViewModel.cs` — テーマ選択の下に言語選択 `ComboBox` を追加（`Models/LanguageOption.cs`、ラベルは「日本語」「English」の現地語表記で固定・非翻訳）。選択変更で即座に `Config.Data.Language` を保存し `LocalizationManager` へ反映
- [x] `Views/Pages/DashboardPage.xaml` / `DataPage.xaml` / `TaggerPage.xaml`、`Views/Windows/MainWindow.xaml` / `ResultDetailWindow.xaml` / `ImagePreviewWindow.xaml` の固定文言を全てリソース参照に置換
- [x] `ViewModels/Pages/DashboardViewModel.cs` — Snackbar メッセージ・バッチ進捗テキスト（`FormatBatchProgress`）・画像サイズ向きラベル（vertical/horizontal/square/custom）を多言語化。ラベルは言語切替時に再生成されるよう `LocalizationManager` の変更通知を購読
- [x] `ViewModels/Windows/MainWindowViewModel.cs` — ナビゲーションメニュー・フッターメニュー・トレイメニューの項目名を多言語化し、言語切替時に再構築する。NavigationViewItem/MenuItem の生成には STA スレッドが必要なため、非 STA スレッドからの呼び出し（テスト等）に対するガードを追加
- [x] `Helpers/LoraDisplayConverter.cs` — `ResultDetailWindow` の LoRA 表示（`{0} ({1}, strength: {2})`）を多言語化するマルチバインディングコンバーターを新規作成（XAML の `StringFormat` では多言語対応できないため）
- [x] `ViewModels/Pages/DataViewModel.cs` / `TaggerViewModel.cs` / `SettingsViewModel.cs` — ステータスメッセージ・ファイル選択ダイアログのタイトル/フィルターを多言語化
- [x] `Helpers/ResultMessageConverter.cs` / `FileTypeToVisibilityConverter.cs` は動的データ（`result.Error`・`output.Filename` 等）を返すのみで固定文言を持たないため対象外と判断

**テスト**
- [x] `ComfyUIRunWorkflowTests/Helpers/LocalizationManagerTests.cs`（新規） — ja/en/en-US でのメッセージ解決・`CurrentCulture` 変更時の `PropertyChanged("Item[]")` 通知・未知キー時の挙動を検証
- [x] `Models/AppConfigTests.cs` — `Language` の既定値・変更通知テストを追加
- [x] `ViewModels/Pages/SettingsViewModelTests.cs` — `LanguageList`・`SelectedLanguage` 変更時の `Config`/`LocalizationManager` 反映・`OnNavigatedToAsync` での読み込みを追加
- [x] `ViewModels/Pages/DashboardViewModelTests.cs` / `ViewModels/Windows/MainWindowViewModelTests.cs` — 日本語ハードコード比較・固定英語文字列比較になっていた箇所を `LocalizationManager` 参照による culture 非依存の比較に修正

合計テスト数: ComfyUILibsTests 162件 / ComfyUIRunWorkflowTests 173件（全パス）

### フェーズ10: 結果詳細ダイアログ・画像拡大表示ダイアログの ui:ContentDialog 化・Controls への再配置（`feature/result-detail-content-dialog` ブランチ、実装中）

`ResultDetailWindow`・`ImagePreviewWindow` を別ウィンドウ表示（`ui:FluentWindow`）から、メイン画面上にオーバーレイ表示する `ui:ContentDialog` に変更した。あわせて、ウィンドウではなくなったため配置先を `Views/Windows/` → `Views/Controls/`、`ViewModels/Windows/` → `ViewModels/Controls/` に変更した。

**ComfyUIRunWorkflow**
- [x] `App.xaml.cs` — `IContentDialogService` を DI 登録
- [x] `Views/Windows/MainWindow.xaml` — ダイアログホストを `ContentPresenter` から `ui:ContentDialogHost` に変更（`RootContentDialog`）
- [x] `Views/Windows/MainWindow.xaml.cs` — `IContentDialogService.SetDialogHost(ContentDialogHost)` でホストを設定
- [x] `Views/Controls/ResultDetailWindow.xaml` / `.xaml.cs`（旧 `Views/Windows/`） — `ui:FluentWindow` → `ui:ContentDialog` に変更。`DataContext` を ViewModel に設定するよう修正（`ContentDialog` 化時に `DataContext = this` になっていてバインディングが機能しない不具合を修正）。`MaxWidth="480"` を設定
- [x] `Views/Controls/ImagePreviewWindow.xaml` / `.xaml.cs`（旧 `Views/Windows/`） — `ui:FluentWindow` → `ui:ContentDialog` に変更。独自の `ui:TitleBar` を廃止し `ContentDialog.Title` に統一。コンストラクタが `ContentDialogHost?` を受け取り `ShowAsync()` で表示する方式に変更
- [x] `ViewModels/Controls/ResultDetailViewModel.cs`（旧 `ViewModels/Windows/`） — `IContentDialogService` をコンストラクタで受け取るよう変更。`OpenEnlargedCommand` を `ImagePreviewWindow.Show()`（別ウィンドウ表示）から `await ImagePreviewWindow.ShowAsync()`（`ResultDetailWindow` と同じ `ContentDialogHost` 上に表示）に変更
- [x] `ViewModels/Pages/DataViewModel.cs` — `IContentDialogService` を DI 経由で受け取り、`ResultDetailWindow.ShowAsync()` でダイアログ表示する方式に変更。`GetDialogHostEx()` を使用（非推奨 API の `GetDialogHost()` は不使用）。`ResultDetailViewModel` 生成時に `IContentDialogService` を渡すよう変更

**テスト**
- [x] `ComfyUIRunWorkflowTests/Fakes/FakeContentDialogService.cs`（新規） — `IContentDialogService` のテスト用スタブ
- [x] `ComfyUIRunWorkflowTests/ViewModels/Pages/DataViewModelTests.cs` — `DataViewModel` コンストラクタへの `IContentDialogService` 引数追加に追従
- [x] `ComfyUIRunWorkflowTests/ViewModels/Controls/ResultDetailViewModelTests.cs`（旧 `ViewModels/Windows/`） — `ResultDetailViewModel` コンストラクタへの `IContentDialogService` 引数追加に追従
- [x] `ComfyUIRunWorkflowTests/ViewModels/Controls/ResultDetailViewModelTests.cs`（旧 `ViewModels/Windows/`） — 名前空間のみ変更

合計テスト数: ComfyUILibsTests 162件 / ComfyUIRunWorkflowTests 173件（全パス）

### フェーズ11: 複数ワークフロー連続実行 Queue ページ（`feature/queue-page` ブランチ、実装完了）

これまで DashboardPage（Home）では1つのワークフロー・1組の設定しか実行できなかった。複数のワークフロー種別・設定（プロンプト・LoRA・画像サイズ・バッチ数）を「ジョブ」としてリストに登録し、順番に自動実行できる新規ページ「Queue」を追加した。

**仕様（ユーザーとの合意事項）**
- ジョブキュー方式: ワークフロー種別ごとに異なる設定を持つジョブを複数登録し、順番に1件ずつ実行
- エラー時: 該当ジョブは失敗として記録し、次のジョブへ継続（キュー全体は止めない）
- 中断: 協調的キャンセル（現在実行中のジョブは最後まで完了させ、以降のジョブへの着手を止める。ComfyUILibs 側への変更は不要な範囲に収めた）
- 結果保存: ジョブごとに個別の `result_*.json`（DataPage の「生成結果」タブにそのまま統合される）
- 画面構成: リスト（ジョブ一覧・ステータス表示）＋個別編集パネル（選択中ジョブの詳細設定、DashboardPage と同様の入力項目）
- ジョブリストの永続化: ジョブ定義（ワークフロー・プロンプト・LoRA・画像サイズ・バッチ数）は `AppConfig`（`ComfyUIRunWorkflow_setting.json`）とは別に、アプリのカレントディレクトリ直下の `queue_jobs.json` に保存し再起動後も保持。ファイルが存在しない場合は空リスト（初期状態）として扱う。実行ステータス・実行結果はセッション限りで、再起動後は全ジョブ「未実行」から開始（結果自体は `result_*.json` に残る）
- 既に「成功」のジョブは「すべて実行」の再実行時にスキップされる（失敗ジョブだけの再実行が可能）

**共通ロジックの切り出し（DashboardViewModel との重複排除）**
- [x] `Helpers/BatchProgressFormatter.cs`（新規） — バッチ進捗テキスト組み立てロジックを共通化。`DashboardViewModel.FormatBatchProgress` はこれに委譲
- [x] `Services/WorkflowSizeOptionBuilder.cs`（新規） — 「ワークフロー設定 → 画像サイズ選択肢（vertical/horizontal/square/custom）＋プリセットサイズ辞書」の組み立てロジックを共通化。`DashboardViewModel.OnSelectedWorkflowChanged` と `QueueJobViewModel` の両方から使用
- [x] `Services/WorkflowExecutionService.cs`（新規） — 「指定回数分バッチ実行して1件の `WorkflowResult` にまとめる」処理（`RunBatchAsync`）と、結果を `result_*.json` として保存する処理（`SaveResultAsync`）を共通化。`DashboardViewModel.RunWorkflowAsync` と `QueueViewModel.RunAllAsync` の両方から使用
- [x] `Models/WorkflowBatchOutcome.cs`（新規） — `RunBatchAsync` の戻り値。保存用の `WorkflowResult` に加え、発生した例外（`ComfyUIException` かどうか）を保持し、呼び出し側が通知メッセージを出し分けられるようにした
- [x] `ViewModels/Pages/DashboardViewModel.cs` — 上記を使うようリファクタ（外部から見た挙動・既存テストは変更なし、全41件パス確認済み）

**新規実装**
- [x] `Models/QueueJobStatus.cs` — ジョブの実行状態列挙体（Pending/Running/Success/Error/Cancelled）
- [x] `Models/QueueJobData.cs` — ジョブ1件分の永続化用データ（ワークフロー名・プロンプト・LoRA・画像サイズ・バッチ数。実行状態は含まない）
- [x] `Models/QueueJobListData.cs`（新規） — `QueueJobData` のリストを保持する永続化ルートクラス。`Setting<QueueJobListData>` 経由でアプリのカレントディレクトリ直下の `queue_jobs.json` に保存する（`AppConfig`／`ComfyUIRunWorkflow_setting.json` とは独立したファイル）
- [x] `ViewModels/Pages/QueueJobViewModel.cs`（新規） — 1ジョブの編集状態・実行状態を保持。`DashboardViewModel` の「ワークフロー選択→LoRA/画像サイズ一覧更新」ロジックを踏襲（`WorkflowSizeOptionBuilder` 経由）。`ToData()`/`FromData()` で永続化データと相互変換
- [x] `ViewModels/Pages/QueueViewModel.cs`（新規） — ジョブ一覧・追加/削除・すべて実行（協調的キャンセル対応）・実行結果詳細ダイアログ表示（`DataViewModel.OpenDetail` と同じ `ResultDetailWindow`/`IContentDialogService` パターンを再利用）・`Setting<QueueJobListData>`（`queue_jobs.json`）へのジョブ定義永続化を担当
- [x] `Views/Pages/QueuePage.xaml` / `.xaml.cs`（新規） — リスト＋個別編集パネルの2カラム構成。トップにジョブ追加・すべて実行/中断ボタン・全体進捗テキスト
- [x] `Helpers/QueueJobStatusToBrushConverter.cs`（新規） — ジョブ一覧のステータス文字色を状態ごとに変える IValueConverter
- [x] `App.xaml.cs` — `QueuePage`/`QueueViewModel` を DI 登録。`Setting<QueueJobListData>` を `Path.GetFullPath("queue_jobs.json")` を指す新規シングルトンとして登録（`Setting<AppConfig>` と同じパターン。ファイル未存在時は空の `QueueJobListData` で自動作成される）
- [x] `App.xaml` — `QueueJobStatusToBrushConverter` をリソース登録
- [x] `ViewModels/Windows/MainWindowViewModel.cs` — ナビゲーションメニューに「Queue」項目を Dashboard と Results の間に追加
- [x] `Resources/Strings.resx` / `Strings.en.resx` — `Queue_*` キーを追加（タイトル・ボタン・プレースホルダー・ステータスラベル・進捗フォーマット）

**テスト**
- [x] `ComfyUIRunWorkflowTests/Helpers/BatchProgressFormatterTests.cs`（新規）
- [x] `ComfyUIRunWorkflowTests/Services/WorkflowSizeOptionBuilderTests.cs`（新規）
- [x] `ComfyUIRunWorkflowTests/Services/WorkflowExecutionServiceTests.cs`（新規） — `SaveResultAsync` の保存/未設定時no-op/ディレクトリ自動作成を検証（`RunBatchAsync` は実際に ComfyUI サーバーへの通信を伴うため、既存の `DashboardViewModel.RunWorkflowCommand` 実行系と同様に単体テスト対象外とした）
- [x] `ComfyUIRunWorkflowTests/ViewModels/Pages/QueueJobViewModelTests.cs`（新規） — ワークフロー切替に伴う LoRA/画像サイズ一覧更新・ToData/FromData 相互変換・ResolveImageSize・StatusLabel 等を検証
- [x] `ComfyUIRunWorkflowTests/ViewModels/Pages/QueueViewModelTests.cs`（新規） — config 読み込み・ジョブ追加削除・`queue_jobs.json`（`Setting<QueueJobListData>`）からの復元（重複復元防止・ファイル未存在時は空リストになることを含む）・RunAll/CancelQueue の CanExecute を検証
- [x] `ComfyUIRunWorkflowTests/Helpers/CultureCollection.cs`（新規） — `LocalizationManager.Instance.CurrentCulture` を書き換えるテストクラス群（新規3件＋既存の `LocalizationManagerTests`/`DashboardViewModelTests`/`SettingsViewModelTests`）が並列実行で相互に干渉しないよう、xUnit の `[Collection("Culture")]` でシリアライズするコレクション定義を追加（テスト追加によって顕在化した既存の flaky 要因を解消）

**不具合修正: QueueJobViewModel の画像サイズ選択がページ再訪問のたびにリセットされる問題**

`QueueViewModel.TryLoadConfig()` は Queue ページへ遷移するたびに、既存の全ジョブへ `QueueJobViewModel.ApplyWorkflowConfig()` を呼び出す。内部の `RefreshForWorkflow()` が、ワークフロー名が変わっていない場合でも画像サイズ選択（プリセット/カスタムの別・向き）を無条件に既定値へリセットしていたため、`queue_jobs.json` から復元した直後や、Queue ページを再訪問するたびにユーザーが選択した「カスタムサイズ」が外れてプリセットに戻ってしまい、実行時に `CustomWidth`/`CustomHeight` が使われず初期値のまま記録されているように見える不具合があった。

- [x] `ViewModels/Pages/QueueJobViewModel.cs` — `RefreshForWorkflow` に `resetSizeSelection` 引数を追加。ワークフロー名が実際に切り替わった場合（`OnWorkflowNameChanged`）のみ画像サイズ選択をプリセット既定値にリセットし、`ApplyWorkflowConfig`（config 再読み込み・ページ再訪問時）ではユーザーの選択（`IsCustomSize`/`ImageSizeOrientation`/`CustomWidth`/`CustomHeight`）を保持するよう修正
- [x] `ComfyUIRunWorkflowTests/ViewModels/Pages/QueueJobViewModelTests.cs` — `ApplyWorkflowConfig` の再適用でカスタムサイズ・プリセット向きが保持されること、`FromData` で復元したカスタムサイズが `ApplyWorkflowConfig` 後も保持されること、ワークフロー実際の切り替え時はリセットされることを検証するテストを追加
- [x] `ComfyUIRunWorkflowTests/ViewModels/Pages/QueueViewModelTests.cs` — `OnNavigatedToAsync` を再度呼び出しても既存ジョブのカスタムサイズが保持されることを検証するテストを追加

合計テスト数: ComfyUILibsTests 162件 / ComfyUIRunWorkflowTests 240件（全パス）

### フェーズ12: DashboardPage/QueuePage へのファイル名プレフィックス指定欄追加（`feature/filename-prefix-textbox` ブランチ、`ComfyUILibs` は `feature/filename-prefix` ブランチ、実装完了）

生成画像の出力ファイル名プレフィックス（ComfyUI の `SaveImage` ノードの `filename_prefix`）を、DashboardPage（Home）と QueuePage の両方から GUI で上書きできるようにした。未入力（空文字・空白のみ）の場合はワークフローテンプレートに記述された値をそのまま使用する。

**ComfyUILibs**
- [x] `Services/WorkflowBuilder.cs` — `Apply` に `string? filenamePrefix = null` 引数を追加。空白以外が指定された場合のみ、ワークフロー内の `class_type` が `SaveImage` の全ノードの `inputs.filename_prefix` を上書きする（`ApplyFilenamePrefix` を新設）
- [x] `Services/WorkflowRunner.cs` — `ExecuteAsync` に `string? filenamePrefix = null` 引数を追加し `WorkflowBuilder.Apply` へ橋渡し
- [x] `ComfyUILibsTests/Services/WorkflowBuilderTests.cs` / `WorkflowRunnerTests.cs` にテストを追加。全件パス確認済み（合計187件）

**ComfyUIRunWorkflow**
- [x] `Services/WorkflowExecutionService.cs` — `RunBatchAsync` に `string? filenamePrefix = null` 引数を追加し `WorkflowRunner.ExecuteAsync` へ橋渡し
- [x] `ViewModels/Pages/DashboardViewModel.cs` — `FilenamePrefix`（string, 既定 ""）プロパティを追加し、`RunWorkflowAsync` で `RunBatchAsync` に渡す
- [x] `Models/QueueJobData.cs` — `FilenamePrefix`（string, 既定 ""）を追加（`queue_jobs.json` に永続化）
- [x] `ViewModels/Pages/QueueJobViewModel.cs` — `FilenamePrefix` プロパティを追加し、`ToData()`/`FromData()` で相互変換
- [x] `ViewModels/Pages/QueueViewModel.cs` — `RunAllAsync` で `job.FilenamePrefix` を `RunBatchAsync` に渡す
- [x] `Views/Pages/DashboardPage.xaml` — LoRA セクションと実行ボタンの間に「ファイル名プレフィックス」の `ui:TextBox`（プレースホルダーで未入力時の挙動を明示）を追加
- [x] `Views/Pages/QueuePage.xaml` — 個別編集パネルの LoRA セクションとバッチ数セクションの間に同様のテキストボックスを追加（`Grid.RowDefinitions` を5行→6行に変更）
- [x] `Resources/Strings.resx`/`Strings.en.resx` — `Common_FilenamePrefixLabel`/`Common_FilenamePrefixPlaceholder` を追加
- [x] `ComfyUIRunWorkflowTests/ViewModels/Pages/DashboardViewModelTests.cs`/`QueueJobViewModelTests.cs` にテストを追加。全件パス確認済み（合計241件）
- [x] `README.md`/`doc/README_english.md`/`doc/usage.md`/`doc/usage_english.md`/`doc/class_diagram.md` を更新

**不具合修正: QueuePageで画像サイズComboBoxを選択後、他ページを経由して戻ると選択が空になる問題**

QueuePageでジョブを選択した状態で他ページ（例: DashboardPage）へ遷移し、QueuePageへ戻ると、選択中ジョブの画像サイズ ComboBox の選択が失われて空表示になる不具合があった。

- **原因**: `QueueJobViewModel.RefreshForWorkflow()` が呼び出す `SizeLabelList.Init()`（`ComfyUILibs.Ui.UIItemBaseModel<T>.Init`）は内部で `ItemList.Clear()` を行ってから項目を再構築する。QueuePage.xaml の ComboBox は `ItemsSource="{Binding SizeLabelList.ItemList}"` と `SelectedValue="{Binding SelectedSizeOption, Mode=TwoWay}"` を組み合わせてバインドしているため、`Clear()` の瞬間に ComboBox の選択がいったん未選択（null）になり、TwoWay バインディング経由でその null が `SelectedSizeOption`（内部的には `ImageSizeOrientation`/`IsCustomSize`）へそのまま書き戻されてしまっていた。`QueueViewModel.OnNavigatedToAsync()` は毎回 `TryLoadConfig()` で全ジョブに対し `ApplyWorkflowConfig()`（`resetSizeSelection: false`）を呼び出すため、QueuePage を再訪問するたびにこの巻き添え上書きが発生していた。フェーズ11で追加した既存の単体テストは ViewModel のプロパティを直接操作するのみで実際の WPF ComboBox コントロールを介さないため、この不具合を検出できていなかった。
- [x] `ViewModels/Pages/QueueJobViewModel.cs` — `RefreshForWorkflow()` の冒頭で `IsCustomSize`/`ImageSizeOrientation` の現在値を退避し、`resetSizeSelection: false` の場合は `SizeLabelList.Init()` 呼び出し後に明示的に復元することで、ComboBox バインディングによる巻き添え上書きを打ち消すよう修正
- [x] `ComfyUIRunWorkflowTests/ViewModels/Pages/QueueJobViewModelTests.cs` — 実際の `System.Windows.Controls.ComboBox` を STA スレッド上で構築し、QueuePage.xaml と同じバインディング（`ItemsSource`/`SelectedValue` TwoWay/`SelectedValuePath`）を張った状態で `ApplyWorkflowConfig` を再実行しても選択が保持されることを検証するテストを2件追加（プリセット向き選択・カスタムサイズ選択の両方）。修正前にこのテストが実際に失敗する（`SelectedSizeOption` が null になる）ことを確認したうえで修正を適用した
- [x] `README.md`/`doc/README_english.md`（変更なし、内部実装のみの修正のため）

合計テスト数: ComfyUILibsTests 187件 / ComfyUIRunWorkflowTests 243件（全パス）

### フェーズ13: DashboardPageへの設定インポート・エクスポート機能追加（`feature/dashboard-import-export` ブランチ、実装完了）

DashboardPage（Home）で入力中のワークフロー実行設定（ワークフロー・ポジティブ/ネガティブプロンプト・画像サイズ・LoRA・バッチ数・ファイル名プレフィックス）を JSON ファイルとしてインポート・エクスポートできる機能を追加した。

**設計判断**
- JSON のスキーマは既存の `Models/QueueJobData.cs`（QueuePage の1ジョブ分の永続化用データ）とそのまま共通化した。対象7項目のフィールド構成が完全に一致しており、新規モデルクラスを追加せずに済むため。QueuePage 自体には個別ジョブのインポート機能はないが、JSON ファイルとしては相互に読み替え可能なスキーマになっている
- ボタン配置はページ上部タイトル行の右側（ユーザーとの合意事項）

**ComfyUIRunWorkflow**
- [x] `ViewModels/Pages/DashboardViewModel.cs`
  - `BuildExportData()`（internal） — 現在のフォーム入力内容を `QueueJobData` に変換
  - `ApplyImportedData(QueueJobData)`（internal） — インポートしたデータをフォームへ反映。ワークフロー名は `WorkflowNames` に存在する場合のみ反映し、戻り値でその成否を返す（存在しない場合はワークフロー選択以外の項目のみ反映し、呼び出し側が警告を表示する）。LoraSlots・画像サイズ（IsCustomSize/ImageSizeOrientation/CustomWidth/CustomHeight）は `SelectedWorkflow` 反映後（`OnSelectedWorkflowChanged` による `SizeLabelList.Init()` 完了後）に設定することで、フェーズ12で対処した ComboBox バインディングの巻き添え上書きと同種の不具合を回避
  - `ExportSettingsCommand`/`ImportSettingsCommand`（`CanExecute`: `!IsRunning`） — `Microsoft.Win32.SaveFileDialog`/`OpenFileDialog`（JSON フィルター）＋ `JsonLoader.WriteJson`/`ReadJson<QueueJobData>` で実際のファイル入出力を行い、結果をスナックバーで通知
- [x] `Views/Pages/DashboardPage.xaml` — タイトル行を `Grid` 化し、右側に「インポート」「エクスポート」`ui:Button`（Icon: `ArrowImport24`/`ArrowExport24`）を配置
- [x] `Resources/Strings.resx`/`Strings.en.resx` — `Common_JsonFileDialogFilter`、`Dashboard_Export*`/`Dashboard_Import*` キーを追加
- [x] `ComfyUIRunWorkflowTests/ViewModels/Pages/DashboardViewModelTests.cs` に `BuildExportData`/`ApplyImportedData`（ワークフロー一致/不一致・カスタムサイズ・LoRA 差し替え）・`ExportSettingsCommand`/`ImportSettingsCommand` の `CanExecute`（`IsRunning` 中は不可）のテストを追加。全件パス確認済み（合計252件）
- [x] `README.md`/`doc/README_english.md`/`doc/usage.md`/`doc/usage_english.md`/`doc/class_diagram.md` を更新

合計テスト数: ComfyUILibsTests 187件 / ComfyUIRunWorkflowTests 252件（全パス）

### 将来的な拡張

- C# 版 Discord ボット（ComfyUILibs を共用）
- 実行履歴の永続化（SQLite 等）
