# TODO — tokkun-asp-dotnet 教材作成タスク

各タスク ≈ 1コミット分の作業単位。上から順に依存関係がある。

---

## Phase 0: プロジェクト基盤

- [x] `.gitignore` 作成（.NET 標準テンプレート）
- [x] `.devcontainer/devcontainer.json` + `Dockerfile` 作成（.NET 8 + PostgreSQL クライアント）
- [x] `docs/setup.md` 作成（学習者向け環境構築手順）

---

## Phase 1: DB レイヤー

- [ ] `db/00_schema.sql` 作成（employees / departments / projects / employee_projects の CREATE TABLE）
- [ ] `db/01_seed.sql` 作成（tokkun-sql と同一のシードデータ）

---

## Phase 2: ベースアプリ構築 → `ch01-start` タグ

学習者が触らないファイルを完成させ、ch01 の練習問題が解ける最小の起点を作る。
学習者が触るのは `Controllers/` `Views/` `Models/` のみ。

- [ ] `src/EmployeeApp/` プロジェクト作成（`dotnet new mvc`・Dapper + Npgsql 追加）
- [ ] `Program.cs` / `appsettings.json` 整備（PostgreSQL 接続・DI 設定）
- [ ] `Views/Shared/_Layout.cshtml` 作成（Bootstrap 5・ナビゲーション）
- [ ] `Models/Employee.cs` 作成（全カラム対応のプロパティ）
- [ ] `Models/Department.cs` 作成
- [ ] `Controllers/EmployeeController.cs` の `Index` アクション実装（`SELECT id, name FROM employees ORDER BY id`）
- [ ] `Views/Employee/Index.cshtml` 作成（id と name だけを表示する最小の一覧）
- [ ] 動作確認（`dotnet run` で社員一覧が表示されること）
- [ ] `ch01-start` タグ付け

---

## Phase 3: ch00 ドキュメント（演習なし・読み物のみ）

- [ ] `docs/chapter00.md` 作成
  - Web アプリケーションとは（ブラウザ ↔ サーバーの通信の流れ図）
  - HTTP リクエスト・レスポンスの基本（GET の概念）
  - MVC パターンの役割（Model / View / Controller を図で説明）
  - プロジェクト構成の説明（各ファイルが何をするか・学習者が触う範囲）
  - Dapper と SQL の接続（tokkun-sql で書いた SELECT がそのまま使えること）

---

## Phase 4: ch01「一覧表示」→ `ch02-start` タグ

- [ ] `docs/chapter01.md` 作成（基礎知識 + 練習問題 4〜6 問）
  - 基礎：Controller → Dapper → View のデータの流れを図で説明
  - 問題例：一覧に「給与」カラムを追加 / ソート順を入社日の新しい順に変更 / 一覧に行数カウントを表示 / 部署 ID を追加表示
- [ ] ch01 解答コードを実装（ch02 の起点を作る）
  - 一覧に全カラム表示・入社日降順ソート・詳細ページへのリンク付き
  - `EmployeeController.Details` アクション追加（`SELECT * FROM employees WHERE id = @id`）
  - `Views/Employee/Details.cshtml` 作成（id と name だけ表示する最小の詳細画面）
- [ ] `ch02-start` タグ付け

---

## Phase 5: ch02「詳細表示」→ `ch03-start` タグ

- [ ] `docs/chapter02.md` 作成（基礎知識 + 練習問題 4〜6 問）
  - 基礎：ルーティング（`/employees/3` の仕組み）・`@Model` で View がデータを受け取る方法
  - 問題例：詳細画面に項目を追加 / 存在しない ID にアクセスしたときの処理を追加 / 部署名を JOIN で表示 / 一覧に「詳細」リンクを追加
- [ ] ch02 解答コードを実装（ch03 の起点を作る）
  - Details に全カラム + 部署名（LEFT JOIN）を表示
  - `EmployeeController.Create`（GET）アクション追加（空のフォームを返す）
  - `Views/Employee/Create.cshtml` 作成（name と salary だけのフォーム骨格）
- [ ] `ch03-start` タグ付け

---

## Phase 6: ch03「新規登録」→ `ch04-start` タグ

- [ ] `docs/chapter03.md` 作成（基礎知識 + 練習問題 4〜6 問）
  - 基礎：GET / POST の違い・`<form>` タグ・PRG パターン（Post-Redirect-Get）
  - Dapper での INSERT・`RETURNING id` で採番した ID を取得する方法
  - 問題例：フォームに「入社日」項目を追加 / INSERT 文を実装する / 登録後に詳細画面へリダイレクト
- [ ] ch03 解答コードを実装（ch04 の起点を作る）
  - `EmployeeController.Create`（POST）実装（INSERT + リダイレクト）
  - `EmployeeController.Edit`（GET）アクション追加（既存データをフォームに表示）
  - `Views/Employee/Edit.cshtml` 作成（Create と同構造の編集フォーム骨格）
- [ ] `ch04-start` タグ付け

---

## Phase 7: ch04「編集」→ `ch05-start` タグ

- [ ] `docs/chapter04.md` 作成（基礎知識 + 練習問題 4〜6 問）
  - 基礎：DataAnnotations によるバリデーション・`ModelState.IsValid`・`asp-validation-for` でエラー表示
  - Dapper での UPDATE
  - 問題例：`[Required]` を追加して必須チェック / エラーメッセージを日本語化 / `[Range]` で給与の範囲を制限 / UPDATE 文を実装
- [ ] ch04 解答コードを実装（ch05 の起点を作る）
  - `EmployeeController.Edit`（POST）実装（バリデーション + UPDATE + リダイレクト）
  - `EmployeeController.Delete`（GET）アクション追加（削除確認画面を表示）
  - `Views/Employee/Delete.cshtml` 作成（削除対象の情報と確認ボタンの骨格）
- [ ] `ch05-start` タグ付け

---

## Phase 8: ch05「削除」→ `ch06-start` タグ

- [ ] `docs/chapter05.md` 作成（基礎知識 + 練習問題 4〜6 問）
  - 基礎：DELETE の実装・削除確認を挟む理由（GET で削除しない理由）
  - 問題例：削除確認メッセージに社員名を表示 / DELETE 文を実装 / 削除後のリダイレクト先を変更 / 一覧に「削除」ボタンを追加
- [ ] ch05 解答コードを実装（ch06 の起点を作る）
  - `EmployeeController.Delete`（POST）実装（DELETE + リダイレクト）
  - `EmployeeController.Index` に `keyword` クエリパラメータ追加（空の検索フォーム表示のみ・絞り込みはまだ動かない）
- [ ] `ch06-start` タグ付け

---

## Phase 9: ch06「絞り込み・ソート」→ `ch07-start` タグ

- [ ] `docs/chapter06.md` 作成（基礎知識 + 練習問題 4〜6 問）
  - 基礎：クエリパラメータ（`?keyword=田中`）の受け取り方・動的 WHERE / ORDER BY の書き方
  - 問題例：名前で部分一致検索 / 部署 ID でフィルタ / 給与の高い順にソート / ソートの昇降順を切り替えるリンク追加
- [ ] ch06 解答コードを実装（フェーズ 2 の起点を作る）
  - 検索フォーム + 動的 WHERE + ソート機能を完全実装
- [ ] `ch07-start` タグ付け

---

## Phase 10: ch07「新画面作成（一覧＋詳細）」

フェーズ 2：スターターコードなし。仕様書だけを渡して一から自力実装。

- [ ] `docs/chapter07.md` 作成（仕様書形式）
  - 作るべき画面の要件（プロジェクト一覧と詳細。JOIN でステータスや参加人数を表示）
  - URL 設計・表示項目・ソート条件を箇条書きで明示
  - ヒントは Controller / View / Model の新規作成手順へのポインタのみ

---

## Phase 11: ch08「新画面作成（CRUD 完成）」

- [ ] `docs/chapter08.md` 作成（仕様書形式）
  - ch07 の画面に登録・編集・削除を追加する要件
  - バリデーション条件（予算は 1 以上 / 終了日は開始日より後、または空欄可）・リダイレクト先も明示
  - ヒントは最小限（詰まったときに読む程度）

---

## Phase 12: 仕上げ

- [ ] 全ドキュメントの通し読み（表記ゆれ・リンク切れ・章間の矛盾を修正）
- [ ] 各 `chXX-start` タグで `dotnet run` が通ることを確認
- [ ] `README.md` 作成（教材の概要・対象者・前提知識・各章へのリンク・学習の進め方）
