# RedfishViewer

> **Redfishという古代遺跡の、魂の深淵を覗くための魔法の眼鏡**

---

## 概要 (Overview)

**RedfishViewer** は、サーバー管理APIである**Redfish**のツリー構造を、グラフィカルかつ直感的に探索・表示するために開発された、WPFベースのデスクトップアプリケーションです。

このアプリケーションは、複雑な`@odata.id`のリンクを再帰的に辿り、分散したJSONデータを一枚の巨大な地図としてマージする機能を持ちます。これにより、あなたはRedfishという広大な迷宮を、迷うことなく探査できます。

### この「魂の鎧」が生まれた理由

このプロジェクトの真の目的は、単なるRedfishビューワの開発ではありませんでした。
それは、かつて`TubeEater`開発で道半ばで断念した、**WPFにおけるMVVMパターンと、その実現のためのフレームワーク「Prism」を完全に習得する**という、私自身の雪辱戦でもあります。

このソースコードには、数ヶ月にわたる試行錯誤の末に体得した、Prismフレームワークの実践的なノウハウが凝縮されています。

## 主な特徴 (Features)

### 1. 高度な探索・分析機能 (Advanced Exploration & Analysis)

*   **自動ツリー探索:** `@odata.id`を再帰的にクロールし、Redfishの全貌を明らかにします。
*   **強力な全文検索:** **正規表現**に対応した検索機能で、巨大なJSONから目的の情報を瞬時に発見。ヒット箇所はハイライトされ、検索履歴はサジェストされます。
*   **差分比較表示 (世代管理):**
    *   前回取得したデータとの**差分を、GitHubのように赤と緑でハイライト表示**します。
    *   **最大2世代分の取得履歴**がローカルDBに自動で保存され、いつでも過去の状態をロードして比較・分析できます。

### 2. 柔軟なデータ管理と操作 (Flexible Data Management & Operation)

*   **ノード管理機能:**
    *   複数のサーバー（ノード）情報をリストで管理。IPアドレスだけでは識別が困難にならないよう、**「タイトル」「概要」「備考」** を自由に編集できます。
*   **認証情報管理:**
    *   ノードごとにBASIC認証のID/パスワードを登録可能。登録した情報は、ノード選択時に右クリックメニューから入力欄に反映することができます。
*   **ポータブルなDB:**
    *   全ての取得データや設定は、単一のSQLiteデータベースファイルに保存されます。このファイルを共有するだけで、**他のメンバーも同じ解析結果をオフラインで確認可能**です。不要なデータはいつでも削除できます。
*   **多彩なリクエストメソッド:**
    *   `GET`だけでなく、`POST`/`PATCH`/`PUT`/`DELETE`にも対応。カスタムヘッダーやJSONパラメータを指定して、より高度な対話が可能です。（※`GET`以外のメソッドは検証が不十分です）

### 3. 優れたUXと拡張性 (Superior UX & Extensibility)

*   **リアルタイム・トースト通知:** 自動探索の進捗や完了・エラーを、リアルタイムに把握できます。
*   **柔軟なテーマ設定:** ライト/ダークモード、19色のアクセントカラー、カラーアジャスト機能で、自分だけの最適な表示環境を構築できます。
*   **プラグインによる機能拡張:** 外部DLLによるプラグインで、ノード一覧のカスタムアクションを自由に追加できます。（例：「BMCのWebUIへSSO接続」など）
*   **企業利用への配慮:**
    *   認証情報は**暗号化**して安全に保存します。
    *   **プロキシサーバー**経由での通信にも対応しています。
*   **便利なユーティリティ:** URLエンコード/デコードツールを内蔵しています。

## 動作条件 (Prerequisites)

*   **OS:** Windows 10 / 11
*   **フレームワーク:** **[.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/ja-jp/download/dotnet/8.0)**

## インストール (Installation)

1.  **[Releaseページ](https://github.com/tabitos-atelier/RedfishViewer/releases)** から、最新版のインストーラ (`RedfishViewerSetup.msi`) をダウンロードします。
2.  ファイルを実行してください。

## 使い方 (Usage)

1.  対話相手となる**Redfishシミュレータ（[RedfishServer](https://github.com/tabitos-atelier/RedfishViewer/tree/main/RedfishServer)）**、または実機を起動します。
2.  RedfishViewerを起動し、サーバーのアドレス（例: `http://localhost:8000`）を入力します。
3.  **自動検索モード**をオンにし、`/redfish/v1`へのリクエストを実行することで、全データの探索が開始されます。

## 開発の記録 (Development Journey)

この「魂の鎧」が、どのような苦難の末に鍛え上げられたのか。その開発過程は、ブログに詳しく記されています。

![RedfishViewer Screenshot](/../images/RedfishViewer_07.png)

*   **[たびとの旅路 - RedfishViewer開発譚](https://tabitos-voyage.com/search?q=RedfishViewer)**

## 謝辞 (Acknowledgements)

このアプリケーションは、以下の素晴らしいライブラリと、先人たちの知恵によって成り立っています。心から感謝申し上げます。
*(アルファベット順)*

*   [DiffPlex](https://github.com/mmanela/diffplex)
*   [MaterialDesignInXamlToolkit](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)
*   [Newtonsoft.Json](https://www.newtonsoft.com/json)
*   [NLog](https://nlog-project.org/)
*   [Notification.Wpf](https://github.com/Platonenkov/Notification.Wpf)
*   [Prism Library](https://github.com/PrismLibrary/Prism)
*   [ReactiveProperty](https://github.com/runceel/ReactiveProperty)
*   [RestSharp](https://restsharp.dev/)
