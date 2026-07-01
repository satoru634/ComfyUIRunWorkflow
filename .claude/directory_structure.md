# ディレクトリ構成

```
ComfyUIRunWorkflow/                     <- ソリューションルート
  ComfyUILibs/                          <- サブモジュール（共有ライブラリ）
    ComfyUILibs/
      Base/                             <- 汎用ジオメトリ値ラッパー（INotifyPropertyChanged 対応）
        ObservablePoint.cs              <- Point ラッパー（ToPoint / FromPoint）
        ObservableSize.cs               <- Size ラッパー（ToSize / FromSize）
      Ui/                               <- UI 選択リスト管理用の汎用ベースクラス（WPF ComboBox 等／将来の Discord ボット選択メニューでも共用想定）
        UIItemBaseModel.cs              <- アイテムリスト＋選択インデックスを管理するジェネリッククラス（Init/Add/Clear）
      Common/                           <- 汎用ユーティリティ（実装済み）
        JsonLoader.cs                   <- JSON ファイル読み書きユーティリティ（静的クラス）
        Setting.cs                      <- 設定ファイル管理ジェネリッククラス
      Exceptions/                       <- 独自例外クラス群
        ComfyUIException.cs             <- 基底例外クラス
      Models/                           <- データモデル（WorkflowConfig, WorkflowInput, 等）
        WorkflowConfig.cs               <- config.json モデル（ImageSize, LoraEntry, WorkflowSettings, Wd14TaggerConfig, WorkflowConfig）
        WorkflowInput.cs                <- 入力 JSON モデル（PromptPair, WorkflowInput）
        WorkflowResult.cs               <- 結果モデル（OutputFile, WorkflowParameters, WorkflowResult）
        ResolvedLora.cs                 <- LoRA 解決済みエントリ
      Services/                         <- ComfyUI API 通信・ワークフロー制御ロジック
        IComfyUIClient.cs               <- ComfyUIClient インターフェース（DI / テスト用）
        ComfyUIClient.cs                <- comfyui_client.py の移植
        WorkflowBuilder.cs              <- workflow_builder.py の移植
        WorkflowRunner.cs               <- WorkflowRunner クラスの移植
        Wd14TaggerRunner.cs             <- wd14_tagger_runner.py の移植
        ConfigLoader.cs                 <- load_files.py の移植
      Properties/
        AssemblyInfo.cs                 <- InternalsVisibleTo("ComfyUILibsTests") を宣言
  ComfyUILibsTests/                     <- xUnit v3 テストプロジェクト
    Base/
      ObservablePointTests.cs           <- ObservablePoint テスト
      ObservableSizeTests.cs            <- ObservableSize テスト
    Ui/
      UIItemBaseModelTests.cs           <- UIItemBaseModel テスト（17件）
    Exceptions/
      ComfyUIExceptionTests.cs          <- ComfyUIException テスト（3件）
    Services/
      ConfigLoaderTests.cs              <- ConfigLoader テスト（38件）
      ComfyUIClientTests.cs             <- ComfyUIClient テスト（9件、FakeHttpMessageHandler 使用）
      WorkflowBuilderTests.cs           <- WorkflowBuilder テスト（14件）
      WorkflowRunnerTests.cs            <- WorkflowRunner テスト（9件、FakeComfyUIClient 使用）
      Wd14TaggerRunnerTests.cs          <- Wd14TaggerRunner テスト（5件）
  ComfyUIRunWorkflow/                   <- メイン WPF プロジェクト（GUI のみ）
    Models/
      AppConfig.cs                      <- アプリ設定（ウィンドウ状態・ComfyUIUrl・ConfigPath・ResultsFolder）
      LoraSlot.cs                       <- LoRA 選択スロット（ObservableObject ラッパー）
      SizeOption.cs                     <- 画像サイズ選択コンボボックスの1項目（Key/Label レコード）
    ViewModels/Pages/
      DashboardViewModel.cs             <- ワークフロー実行 VM（ConfigLoader + WorkflowRunner 使用）
      SettingsViewModel.cs              <- 設定 VM（テーマ・URL・パス管理）
      DataViewModel.cs                  <- 実行結果一覧 VM（result_*.json 読み込み）
    ViewModels/Windows/
      MainWindowViewModel.cs            <- ナビゲーション定義・ウィンドウ状態保存
    Views/Pages/
      DashboardPage.xaml                <- ワークフロー実行 UI
      SettingsPage.xaml                 <- 設定 UI
      DataPage.xaml                     <- 実行結果一覧 UI
    Views/Windows/
      MainWindow.xaml                   <- ナビゲーションホスト
      ResultDetailWindow.xaml           <- 実行結果詳細ダイアログ
    Helpers/
      EnumToBooleanConverter.cs         <- テーマ切り替え用列挙型コンバーター
      BoolToVisibilityConverter.cs      <- bool→Visibility・逆変換・null→Visibility コンバーター
    Services/
      ApplicationHostService.cs         <- 起動時ウィンドウ表示
    Assets/
    Resources/
    templates/                          <- ワークフローテンプレート（ビルド時に出力ディレクトリへコピー）
      anima/                            <- anima ワークフロー用（template_lora_0〜4.json）
      anima_rapid/                      <- anima_rapid ワークフロー用（template_lora_0〜4.json）
      sdxl/                             <- sdxl ワークフロー用（template_lora_0〜4.json）
      template_wd14_tagger.json         <- WD14 Tagger ワークフローテンプレート
  doc/                                <- ドキュメント
    class_diagram.md                  <- Mermaid 記法によるクラス図（全体・ComfyUIRunWorkflow・ComfyUILibs の3図）
```
