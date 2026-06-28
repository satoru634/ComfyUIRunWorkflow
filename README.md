# ComfyUIRunWorkflow

ComfyUI のワークフローを GUI から実行するツール。[comfyui_tools](https://github.com/satoru634/comfyui_tools) の C# WPF 移植版。

## 機能

- ワークフロー実行（プロンプト・LoRA・画像サイズを GUI で指定）
- 実行結果の一覧表示と詳細確認
- テーマ切り替え・接続設定の永続化

## 使い方

### 初回設定

1. アプリを起動し、**設定**ページを開く
2. **ComfyUI URL** を入力（デフォルト: `http://127.0.0.1:8188`）
3. **config.json パス** で `config.json` ファイルを選択
4. **結果出力フォルダ** で結果 JSON の保存先を選択

### ワークフロー実行

1. **Home** ページを開く
2. ワークフローを選択
3. ポジティブ・ネガティブプロンプトを入力
4. 画像サイズ（プリセット or カスタム）を選択
5. LoRA を追加（任意）
6. **実行** ボタンをクリック

### 結果確認

- **Data** ページで実行履歴を一覧表示
- 各行をクリックすると詳細ダイアログが開く

## 技術スタック

| 項目 | 内容 |
|---|---|
| ランタイム | .NET 8 / WPF |
| UI フレームワーク | Wpf.Ui v4.3.0 |
| MVVM | CommunityToolkit.Mvvm v8.4.2 |
| DI | Microsoft.Extensions.Hosting |
| 共有ライブラリ | ComfyUILibs（サブモジュール） |

## プロジェクト構成

```
ComfyUIRunWorkflow/   ← ソリューションルート
  ComfyUILibs/        ← 共有ライブラリ（サブモジュール）
  ComfyUILibsTests/   ← ComfyUILibs テスト（120件）
  ComfyUIRunWorkflow/ ← WPF GUI プロジェクト
  ComfyUIRunWorkflowTests/ ← GUI テスト（89件）
```
