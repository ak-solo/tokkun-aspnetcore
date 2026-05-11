# tokkun-aspnetcore

ASP.NET Core MVC の学習教材です。
C# と SQL の基礎（tokkun-csharp / tokkun-sql）を学び終えた方が、Web アプリケーション開発の基礎を段階的に習得することを目的としています。

---

## 概要

架空の社内システムを題材に、社員情報を管理する Web アプリケーションを作りながら
ASP.NET Core MVC の CRUD 操作を学びます。

- **フェーズ 1（ch01〜ch06）**：動作するスターターコードに機能追加・仕様変更を行う
- **フェーズ 2（ch07〜ch08）**：仕様書を読みながら新しい画面を一から実装する

---

## 対象者・前提知識

| 教材 | 習得済み想定の知識 |
|------|------------------|
| tokkun-csharp ch01〜09 | 変数・型・分岐・繰り返し・配列・メソッド・クラス・LINQ |
| tokkun-sql ch00〜11 | SELECT / WHERE / JOIN / GROUP BY / DML（INSERT/UPDATE/DELETE）/ DDL |

---

## 技術スタック

| 要素 | 採用技術 |
|------|----------|
| フレームワーク | ASP.NET Core MVC (.NET 8) |
| テンプレートエンジン | Razor (.cshtml) |
| データアクセス | Dapper |
| DB | PostgreSQL |
| 開発環境 | VS Code + Dev Container |

---

## 学習の進め方

1. **[環境構築](docs/setup.md)** に従って Dev Container を起動し、DB を準備する
2. **[第0章 はじめに](docs/chapter00.md)** を読んで Web と MVC の基本を把握する（演習なし）
3. 各章のスターターコードを `git checkout` で取得し、練習問題を解く（ch01〜06）
4. ch07・ch08 は仕様書を見ながら一から実装する

---

## 章一覧

### フェーズ 1：既存画面への機能追加（ch01〜ch06）

| 章 | テーマ | スターターコード |
|----|--------|----------------|
| [第0章 はじめに](docs/chapter00.md) | Web の仕組み・HTTP・MVC パターン（演習なし） | — |
| [第1章 一覧表示](docs/chapter01.md) | Controller → Dapper → View の基本の流れ | `git checkout ch01-start` |
| [第2章 詳細表示](docs/chapter02.md) | ルーティング・1件取得・JOIN | `git checkout ch02-start` |
| [第3章 新規登録](docs/chapter03.md) | GET/POST・INSERT・PRG パターン | `git checkout ch03-start` |
| [第4章 編集](docs/chapter04.md) | UPDATE・DataAnnotations バリデーション | `git checkout ch04-start` |
| [第5章 削除](docs/chapter05.md) | DELETE・削除確認フロー | `git checkout ch05-start` |
| [第6章 絞り込み・ソート](docs/chapter06.md) | クエリパラメータ・動的 WHERE / ORDER BY | `git checkout ch06-start` |

### フェーズ 2：新画面の作成（ch07〜ch08）

| 章 | テーマ |
|----|--------|
| [第7章 新画面作成（一覧・詳細）](docs/chapter07.md) | Controller / Model / View を新規作成・JOIN 集計 |
| [第8章 新画面作成（CRUD完成）](docs/chapter08.md) | 登録・編集・削除を追加して CRUD を完成させる |

---

## スターターコードの使い方

各章の練習問題を始める前に、対応するタグをチェックアウトしてください。

```bash
# ch01 のスターターコードに切り替える
git checkout ch01-start

# 作業が終わったら main ブランチに戻る
git checkout main
```

> **注意：** `git checkout` は未コミットの変更を上書きします。
> 自分の回答を残したい場合は `git stash` または `git add && git commit` してから切り替えてください。

---

## ディレクトリ構成

```
tokkun-aspnetcore/
├── docs/                        # 学習ドキュメント
│   ├── setup.md                 # 環境構築手順
│   ├── chapter00.md〜chapter08.md
├── src/
│   └── EmployeeApp/             # ASP.NET Core MVC アプリ
│       ├── Controllers/         # リクエスト処理（学習者が編集）
│       ├── Models/              # データモデル（学習者が編集）
│       ├── Views/               # HTML テンプレート（学習者が編集）
│       ├── Program.cs           # 起動設定（変更不要）
│       └── appsettings.json     # DB 接続設定（変更不要）
└── db/
    ├── 00_schema.sql            # テーブル定義
    └── 01_seed.sql              # 初期データ
```
