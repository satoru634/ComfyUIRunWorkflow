# ディレクトリ構成

```
ComfyUIRunWorkflow/                     <- ソリューションルート
  ComfyUILibs/                          <- サブモジュール（共有ライブラリ、別リポジトリ）
                                            詳細は ComfyUILibs/.claude/directory_structure.md を参照
  ComfyUILibsTests/                     <- xUnit v3 テストプロジェクト（ComfyUILibs リポジトリに含まれる）
  ComfyUIRunWorkflow/                   <- メイン WPF プロジェクト（GUI のみ）
    Models/
      AppConfig.cs                      <- アプリ設定（ウィンドウ状態・ComfyUIUrl・ConfigPath・ResultsFolder・Language・QueueJobs）
      LoraSlot.cs                       <- LoRA 選択スロット（ObservableObject ラッパー）
      SizeOption.cs                     <- 画像サイズ選択コンボボックスの1項目（Key/Label レコード）
      LanguageOption.cs                 <- 言語選択コンボボックスの1項目（Key/Label レコード、ラベルは現地語表記固定）
      OutputFilePreview.cs              <- 出力ファイル1件分のプレビュー状態（Thumbnail・IsLoading・HasError・CachedFilePath）
      WorkflowResultPreview.cs          <- DataPage 一覧行のラッパー（WorkflowResult + サムネイル1枚分の Preview）
      WorkflowBatchOutcome.cs           <- WorkflowExecutionService.RunBatchAsync の戻り値（WorkflowResult + 発生した例外）
      QueueJobStatus.cs                 <- QueuePage の1ジョブの実行状態列挙体（Pending/Running/Success/Error/Cancelled）
      QueueJobData.cs                   <- QueuePage の1ジョブ分の永続化用データ（ワークフロー・プロンプト・LoRA・画像サイズ・バッチ数。実行状態は含まない）
    ViewModels/Pages/
      DashboardViewModel.cs             <- ワークフロー実行 VM（ConfigLoader + WorkflowExecutionService 使用、実行直後のプレビュー表示を含む）
      SettingsViewModel.cs              <- 設定 VM（テーマ・URL・パス管理）
      DataViewModel.cs                  <- 実行結果一覧 VM（result_*.json / tag_result_*.json 読み込み、サムネイル非同期取得、生成結果⇔タグ付け履歴のタブ切り替え）
      TaggerViewModel.cs                <- WD14 Tagger VM（画像選択・タグ付け実行・tag_result_*.json 保存）
      QueueJobViewModel.cs              <- QueuePage の1ジョブ分の編集状態・実行状態 VM（ワークフロー選択に連動した LoRA/画像サイズ一覧更新、ToData/FromData で永続化データと相互変換）
      QueueViewModel.cs                 <- QueuePage 全体の VM（ジョブ一覧・追加削除・すべて実行（協調的キャンセル対応）・実行結果詳細ダイアログ表示・ジョブ定義の永続化）
    ViewModels/Windows/
      MainWindowViewModel.cs            <- ナビゲーション定義・ウィンドウ状態保存
    ViewModels/Controls/
      ResultDetailViewModel.cs          <- 実行結果詳細ダイアログ VM（出力ファイルごとのサムネイル取得・拡大表示コマンド）
    Views/Pages/
      DashboardPage.xaml                <- ワークフロー実行 UI（生成結果プレビューを含む）
      QueuePage.xaml                    <- 複数ワークフロー連続実行 UI（ジョブ一覧＋個別編集パネルの2カラム構成）
      SettingsPage.xaml                 <- 設定 UI
      DataPage.xaml                     <- 実行結果一覧 UI（サムネイル付き、生成結果⇔タグ付け履歴のタブ切り替え）
      TaggerPage.xaml                   <- WD14 Tagger UI（画像選択・ドラッグ&ドロップ・タグ結果表示/コピー）
    Views/Windows/
      MainWindow.xaml                   <- ナビゲーションホスト
    Views/Controls/
      ResultDetailWindow.xaml           <- 実行結果詳細ダイアログ（ui:ContentDialog、MainWindow の ContentDialogHost 上に表示、出力ファイルのサムネイル一覧）
      ImagePreviewWindow.xaml           <- 生成画像の拡大表示ダイアログ（ui:ContentDialog、ResultDetailWindow と同じ ContentDialogHost 上に表示）
    Helpers/
      EnumToBooleanConverter.cs         <- テーマ切り替え用列挙型コンバーター
      BoolToVisibilityConverter.cs      <- bool→Visibility・逆変換・null→Visibility・null→Visibility逆変換 コンバーター
      LocalizationManager.cs            <- 多言語化用シングルトン（Strings.resx を CurrentCulture に応じて解決、XAML インデクサーバインディングで即時反映）
      LoraDisplayConverter.cs           <- LoRA 表示文字列の多言語対応マルチバインディングコンバーター（ResultDetailWindow 用）
      BatchProgressFormatter.cs         <- バッチ進捗テキスト組み立ての共通ヘルパー（DashboardViewModel・QueueViewModel で共用）
      QueueJobStatusToBrushConverter.cs <- QueueJobStatus→前景色ブラシ コンバーター（QueuePage のジョブ一覧用）
    Services/
      ApplicationHostService.cs         <- 起動時ウィンドウ表示・保存済み Language からのカルチャ適用
      PreviewImageLoader.cs             <- サムネイル/原寸画像の BitmapImage 読み込み（PreviewImageCacheService に委譲）
      WorkflowSizeOptionBuilder.cs      <- ワークフロー設定から画像サイズ選択肢を組み立てる共通ロジック（DashboardViewModel・QueueJobViewModel で共用）
      WorkflowExecutionService.cs       <- バッチ実行して WorkflowResult にまとめる処理・result_*.json 保存処理の共通サービス（DashboardViewModel・QueueViewModel で共用）
    Properties/
      AssemblyInfo.cs                   <- InternalsVisibleTo("ComfyUIRunWorkflowTests") を宣言
    Assets/
    Resources/
      Strings.resx                      <- GUI 表示文言（既定・日本語）
      Strings.en.resx                   <- GUI 表示文言（英語サテライト）
      Strings.cs                        <- CurrentUICulture に応じて文言を解決する ResourceManager ラッパー
    templates/                          <- ワークフローテンプレート（ビルド時に出力ディレクトリへコピー）
      anima/                            <- anima ワークフロー用（template_lora_0〜4.json）
      anima_rapid/                      <- anima_rapid ワークフロー用（template_lora_0〜4.json）
      sdxl/                             <- sdxl ワークフロー用（template_lora_0〜4.json）
      template_wd14_tagger.json         <- WD14 Tagger ワークフローテンプレート
  doc/                                <- ドキュメント
    class_diagram.md                  <- Mermaid 記法によるクラス図（全体・ComfyUIRunWorkflow・ComfyUILibs の3図）
    usage.md                          <- 各ページの詳細な使い方（日本語）
    usage_english.md                  <- 各ページの詳細な使い方（英語）
    README_english.md                 <- README.md の英語版（クイックスタート）
    images/                           <- README・usage 用スクリーンショット（プレースホルダー）
```
