# プロジェクト概要

[comfyui_tools](https://github.com/satoru634/comfyui_tools) の C# 移植版。
[run_workflow](https://github.com/satoru634/comfyui_tools/tree/master/run_workflow) と同等の機能を WPF-UI (Wpf.Ui) ベースの GUI で操作できるツール。

## 詳細ドキュメント

タスク開始前に、関連するドキュメントを確認すること。

- 実装状況: @.claude/implementation_status.md
- 技術スタック: @.claude/tech_stack.md
- ディレクトリ構成: @.claude/directory_structure.md

## 開発ルール
- ファイルの変更や追加を行う前に、作業ブランチを切ること。
- クラスを追加・変更したら、対応するユニットテストを追加すること（テストフレームワーク: xUnit、配置先: `<プロダクトのプロジェクト名>Tests/<同じ名前空間>/`）
- ユニットテストがパスするまで次の実装に進まないこと。
- 指示があるまでコミットしないこと。
- 実装後は、各ツール配下にあるREADME.mdを更新する。doc/README_english.mdも同様に更新する。
- クラス新規追加・変更などが合った場合は、クラス図も変更すること。クラス図: @doc/class_diagram.md
- ファイルやディレクトリ構成を変更した場合は、CLAUDE.mdおよび.claude配下に記載の該当箇所も変更する。
- プルリクマージ後は、作業ブランチをローカル・リモート共に削除し、masterブランチを最新にする。

## 責務の分離（重要）

| プロジェクト | 責務 |
|---|---|
| `ComfyUILibs`（サブモジュール、別リポジトリ） | ComfyUI API 通信・ワークフロー制御・設定管理などのビジネスロジック全般。将来の C# Discord ボットでも共用する |
| `ComfyUIRunWorkflow`（本プロジェクト） | GUI のみ。ViewModel・View・UI ヘルパーに限定。ComfyUI API の直接呼び出しは行わない |

**ComfyUIRunWorkflow は ComfyUILibs を参照し、そのサービスを DI 経由で利用する。**

`ComfyUILibs` は独自の Git リポジトリ（Discord ボット等の他プロジェクトからも参照される想定）のため、実装ルール・技術スタック・ディレクトリ構成・クラス図は `ComfyUILibs/CLAUDE.md` および `ComfyUILibs/.claude/` 配下に独立して管理している。ComfyUILibs 配下のファイルを変更する場合は、本 CLAUDE.md ではなく `ComfyUILibs/CLAUDE.md` の開発ルールに従うこと。

## コーディング規約

- 非同期メソッドは必ず `async`/`await` を使用（`Task.Result` / `.Wait()` 禁止）
- nullable 有効化済み（`#nullable enable`）
- 例外は独自例外クラスで統一（`ComfyUILibs/Exceptions/` 参照）
- Python版の `ValueError` に相当するものは `ComfyUIException` などプロジェクト固有例外にマップする
- WebSocket 受信ループではバイナリフレームを必ずスキップすること（ComfyUI がプレビュー画像をバイナリで送信するため）
- `WorkflowBuilder` でテンプレートを書き換える際は必ず DeepCopy/Clone してから行うこと

## ComfyUI API 概要

- `POST /prompt` — ワークフロー送信、`prompt_id` 取得
- `GET /history/{prompt_id}` — 実行結果・出力ファイル一覧取得
- `POST /upload/image` — 画像アップロード（WD14 Tagger 用）
- `ws://host/ws?clientId={uuid}` — 実行進捗の WebSocket 監視
  - `execution_complete` → 正常完了
  - `execution_error` → エラー
  - `executing` (node=null) → 古い ComfyUI の完了シグナル（ポーリングへフォールバック）
