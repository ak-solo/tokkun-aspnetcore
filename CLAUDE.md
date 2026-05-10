# tokkun-asp-dotnet — 教材作成ガイド

## プロジェクト概要

プログラミング初学者向け ASP.NET Core MVC の学習教材。
C# と SQL の基礎（tokkun-csharp / tokkun-sql）を学び終えた人が、
Web アプリケーション開発の基礎を段階的に習得することを目的とする。

---

## 受講対象・前提知識

| 教材 | 習得済み想定の知識 |
|------|------------------|
| tokkun-csharp ch01–09 | 変数・型・分岐・繰り返し・配列・メソッド・クラス・LINQ |
| tokkun-sql ch00–11 | SELECT / WHERE / JOIN / GROUP BY / DML（INSERT/UPDATE/DELETE）/ DDL |

---

## 学習目標

1. HTTP リクエスト／レスポンスの基本的な流れを理解する
2. MVC パターン（Model / View / Controller）の役割を理解する
3. 既存の画面に対して機能追加・仕様変更ができる
4. データの一覧・詳細・登録・更新・削除（CRUD）を自分で実装できる
5. 新しい画面を一から設計・実装できる

---

## 技術スタック

| 要素 | 採用技術 | 理由 |
|------|----------|------|
| フレームワーク | ASP.NET Core MVC (.NET 8) | MVC パターンを明示的に学べる |
| テンプレートエンジン | Razor (.cshtml) | C# を活かせる |
| データアクセス | Dapper | tokkun-sql で学んだ SQL をそのまま書ける |
| DB | PostgreSQL | tokkun-sql と同一環境 |
| 開発環境 | VS Code + Dev Container | tokkun-csharp / tokkun-sql と統一 |

---

## 題材・データモデル

tokkun-sql と同じ「架空の社内システム」を題材にする。
学習者がすでに知っているテーブル構造を使うことで、DB の学習コストを省きアプリ層に集中できる。

```
employees    (id, name, dept_id, salary, hire_date, manager_id)
departments  (id, name, location)
projects     (id, name, start_date, end_date, budget)
employee_projects (employee_id, project_id, role)
```

---

## 章構成

### フェーズ 1：既存画面への機能追加・仕様変更（ch01〜ch06）

学習者はあらかじめ用意されたスターターコードを受け取る。
動く状態の画面に対して、仕様変更や機能追加を行う練習問題を解く。

| 章 | テーマ | 主な学習内容 |
|----|--------|-------------|
| ch00 | はじめに | Web の仕組み・HTTP・MVC パターン・プロジェクト構成の説明（演習なし） |
| ch01 | 一覧表示 | Controller → Dapper → View の基本の流れ。カラム追加・ソート変更など |
| ch02 | 詳細表示 | ルーティング（`/employees/3`）・1件取得・View への値渡し |
| ch03 | 新規登録 | フォーム（GET/POST）・`INSERT` 実行・リダイレクト |
| ch04 | 編集 | 編集フォーム・`UPDATE` 実行・バリデーション入門 |
| ch05 | 削除 | 削除確認・`DELETE` 実行・誤削除防止パターン |
| ch06 | 絞り込み・ソート | 検索フォーム・クエリパラメータ・動的な WHERE / ORDER BY |

### フェーズ 2：新しい画面の作成（ch07〜ch08）

スターターコードなし。仕様書だけを渡し、自力で画面を一から実装する。

| 章 | テーマ | 主な学習内容 |
|----|--------|-------------|
| ch07 | 新画面作成（一覧＋詳細） | Controller / Model / View を新規作成・JOIN を使った一覧 |
| ch08 | 新画面作成（CRUD 完成） | ch07 に登録・編集・削除を追加して完全な CRUD を実装 |

---

## ディレクトリ構成（想定）

```
tokkun-asp-dotnet/
├── CLAUDE.md
├── .claude/rules/          # Claude Code ルールファイル
│   ├── chapter-docs.md     # ドキュメント執筆・練習問題設計ルール
│   ├── coding-conventions.md  # コーディング規約・スターターコード方針
│   └── git-commit.md       # Git コミット方針
├── docs/
│   ├── setup.md
│   ├── chapter00.md
│   ├── chapter01.md
│   └── ...
├── src/
│   └── EmployeeApp/          # ASP.NET Core MVC プロジェクト
│       ├── Controllers/
│       ├── Models/
│       ├── Views/
│       └── EmployeeApp.csproj
└── db/
    ├── 00_schema.sql
    └── 01_seed.sql
```
