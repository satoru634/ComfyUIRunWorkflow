# ディレクトリ構成

```
ComfyUIRunWorkflow/                     <- ソリューションルート
  ComfyUILibs/                          <- サブモジュール（共有ライブラリ）
    ComfyUILibs/
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
    Exceptions/
      ComfyUIExceptionTests.cs          <- ComfyUIException テスト（3件）
    Services/
      ConfigLoaderTests.cs              <- ConfigLoader テスト（38件）
      ComfyUIClientTests.cs             <- ComfyUIClient テスト（9件、FakeHttpMessageHandler 使用）
      WorkflowBuilderTests.cs           <- WorkflowBuilder テスト（14件）
      WorkflowRunnerTests.cs            <- WorkflowRunner テスト（9件、FakeComfyUIClient 使用）
      Wd14TaggerRunnerTests.cs          <- Wd14TaggerRunner テスト（5件）
  ComfyUIRunWorkflow/                   <- メイン WPF プロジェクト（GUI のみ）
    ViewModels/Pages/                   <- 各ページの ViewModel
    ViewModels/Windows/
    Views/Pages/                        <- XAML ページ
    Views/Windows/
    Helpers/                            <- UI ヘルパー・コンバーター
    Services/                           <- GUI 固有サービス（ApplicationHostService 等）
    Assets/
    Resources/
```
