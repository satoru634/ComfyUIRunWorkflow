# ディレクトリ構成

```
ComfyUIRunWorkflow/                     <- ソリューションルート
  ComfyUILibs/                          <- サブモジュール（共有ライブラリ、別リポジトリ）
                                            詳細は ComfyUILibs/.claude/directory_structure.md を参照
  ComfyUILibsTests/                     <- xUnit v3 テストプロジェクト（ComfyUILibs リポジトリに含まれる）
  ComfyUIRunWorkflow/                   <- メイン WPF プロジェクト（GUI のみ）
    Models/
      AppConfig.cs                      <- アプリ設定（ウィンドウ状態・ComfyUIUrl・ConfigPath・ResultsFolder・Language）
      LoraSlot.cs                       <- LoRA 選択スロット（ObservableObject ラッパー）
      SizeOption.cs                     <- 画像サイズ選択コンボボックスの1項目（Key/Label レコード）
      LanguageOption.cs                 <- 言語選択コンボボックスの1項目（Key/Label レコード、ラベルは現地語表記固定）
      OutputFilePreview.cs              <- 出力ファイル1件分のプレビュー状態（Thumbnail・IsLoading・HasError・CachedFilePath）
      WorkflowResultPreview.cs          <- DataPage 一覧行のラッパー（WorkflowResult + サムネイル1枚分の Preview）
    ViewModels/Pages/
      DashboardViewModel.cs             <- ワークフロー実行 VM（ConfigLoader + WorkflowRunner 使用、実行直後のプレビュー表示を含む）
      SettingsViewModel.cs              <- 設定 VM（テーマ・URL・パス管理）
      DataViewModel.cs                  <- 実行結果一覧 VM（result_*.json / tag_result_*.json 読み込み、サムネイル非同期取得、生成結果⇔タグ付け履歴のタブ切り替え）
      TaggerViewModel.cs                <- WD14 Tagger VM（画像選択・タグ付け実行・tag_result_*.json 保存）
    ViewModels/Windows/
      MainWindowViewModel.cs            <- ナビゲーション定義・ウィンドウ状態保存
      ResultDetailViewModel.cs          <- 実行結果詳細ダイアログ VM（出力ファイルごとのサムネイル取得・拡大表示コマンド）
    Views/Pages/
      DashboardPage.xaml                <- ワークフロー実行 UI（生成結果プレビューを含む）
      SettingsPage.xaml                 <- 設定 UI
      DataPage.xaml                     <- 実行結果一覧 UI（サムネイル付き、生成結果⇔タグ付け履歴のタブ切り替え）
      TaggerPage.xaml                   <- WD14 Tagger UI（画像選択・ドラッグ&ドロップ・タグ結果表示/コピー）
    Views/Windows/
      MainWindow.xaml                   <- ナビゲーションホスト
      ResultDetailWindow.xaml           <- 実行結果詳細ダイアログ（出力ファイルのサムネイル一覧）
      ImagePreviewWindow.xaml           <- 生成画像の拡大表示ウィンドウ
    Helpers/
      EnumToBooleanConverter.cs         <- テーマ切り替え用列挙型コンバーター
      BoolToVisibilityConverter.cs      <- bool→Visibility・逆変換・null→Visibility・null→Visibility逆変換 コンバーター
      LocalizationManager.cs            <- 多言語化用シングルトン（Strings.resx を CurrentCulture に応じて解決、XAML インデクサーバインディングで即時反映）
      LoraDisplayConverter.cs           <- LoRA 表示文字列の多言語対応マルチバインディングコンバーター（ResultDetailWindow 用）
    Services/
      ApplicationHostService.cs         <- 起動時ウィンドウ表示・保存済み Language からのカルチャ適用
      PreviewImageLoader.cs             <- サムネイル/原寸画像の BitmapImage 読み込み（PreviewImageCacheService に委譲）
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
