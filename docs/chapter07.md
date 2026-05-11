# 第7章 新画面作成（一覧・詳細）

## このチャプターで学ぶこと

- Controller・View・Model を**一から新規作成**する手順
- `LEFT JOIN` + `GROUP BY` + `COUNT` で集計値（参加人数）を一覧に表示する
- SQL の `CASE WHEN` または C# プロパティでステータスを判定する
- 詳細画面で 2 種類の情報（プロジェクト基本情報 + 参加メンバー一覧）を View に渡す

---

## フェーズ 2 へようこそ

ch01〜ch06 では「動作するスターターコード」を受け取り、そこに機能を追加・変更してきました。

ch07 からは **スターターコードがありません**。
仕様書を読みながら Controller・View・Model をすべて自力で新規作成してください。
詰まったときは ch01〜ch06 で書いたコードを参考にしてください。

---

## 基礎知識

### 新しい画面を作るために必要なもの

ASP.NET Core MVC で新しい画面を追加するには、最低 3 種類のファイルが必要です。

```
src/EmployeeApp/
├── Controllers/
│   └── ProjectController.cs        ← アクションを定義する
├── Models/
│   └── Project.cs                  ← クエリ結果を受け取る型
└── Views/
    └── Project/
        ├── Index.cshtml            ← 一覧画面
        └── Details.cshtml          ← 詳細画面
```

**ルーティングの自動設定：**

`ProjectController` というクラスを作ると、`Program.cs` の設定により URL が自動的に割り当てられます。

```
GET /Project          → ProjectController.Index アクション
GET /Project/Details/5 → ProjectController.Details アクション（id = 5）
```

`EmployeeController` → `/Employee` と同じ仕組みです。

### 参考にできる既存ファイル

| 作成するファイル | 参考にできる既存ファイル |
|---|---|
| `Controllers/ProjectController.cs` | `Controllers/EmployeeController.cs` |
| `Models/Project.cs` | `Models/Employee.cs` |
| `Views/Project/Index.cshtml` | `Views/Employee/Index.cshtml` |
| `Views/Project/Details.cshtml` | `Views/Employee/Details.cshtml` |

---

## 練習問題

> **フェーズ 2 の問題形式：** 以降は「仕様書」形式です。
> 実装すべき内容を箇条書きで示します。どのように実装するかは自分で考えてください。

---

### 問題 7-1：プロジェクト一覧画面を作成する

**URL：** `GET /Project`

**表示する項目：**

| 列 | 取得元・表示ルール |
|---|---|
| ID | `projects.id` |
| プロジェクト名 | `projects.name` |
| 開始日 | `projects.start_date` |
| 終了日 | `projects.end_date`（`NULL` の場合は「未定」と表示） |
| 予算 | `projects.budget` |
| ステータス | 終了日が `NULL` または今日以降 → 「進行中」、終了日が過去 → 「完了」 |
| 参加人数 | `employee_projects` を `LEFT JOIN` して `COUNT` で取得 |

**ソート：** 開始日の新しい順（`start_date DESC`）で固定

**その他の要件：**
- 各行に「詳細」リンクを設置すること（リンク先：`/Project/Details/{id}`）
- `Views/Shared/_Layout.cshtml` のナビゲーションバーに「プロジェクト」リンクを追加すること

**使用する SQL（概要）：**

```sql
SELECT
    p.id,
    p.name,
    p.start_date,
    p.end_date,
    p.budget,
    COUNT(ep.employee_id) AS member_count
FROM projects p
LEFT JOIN employee_projects ep ON p.id = ep.project_id
GROUP BY p.id, p.name, p.start_date, p.end_date, p.budget
ORDER BY p.start_date DESC
```

ステータスの判定は次の 2 通りどちらでも構いません。

方法 A — SQL の `CASE WHEN` で文字列として取得する（`SELECT` 句に追加）：

```sql
CASE WHEN p.end_date IS NULL OR p.end_date >= CURRENT_DATE
     THEN '進行中' ELSE '完了'
END AS status
```

方法 B — C# のプロパティとして `Project.cs` に定義する：

```csharp
public string Status =>
    EndDate == null || EndDate >= DateOnly.FromDateTime(DateTime.Today)
        ? "進行中" : "完了";
```

> **tokkun-sql との接続**
> `CASE WHEN ... THEN ... ELSE ... END` は tokkun-sql ch08 で学んだ構文です。
> `COUNT` と `GROUP BY` の組み合わせは ch06 で扱いました。

**確認ポイント：**
- `/Project` にアクセスするとプロジェクト一覧が表示されること
- 「基幹システム刷新」の参加人数が `4` であること
- 「データ分析基盤構築」の参加人数が `4` であること
- 「新製品マーケ施策」のステータスが「進行中」であること（`end_date` が `NULL`）
- 「基幹システム刷新」のステータスが「完了」であること（`end_date` = `2023-12-31`）
- ナビゲーションバーの「プロジェクト」をクリックすると `/Project` に遷移すること

---

### 問題 7-2：プロジェクト詳細画面を作成する

**URL：** `GET /Project/Details/{id}`

**表示する項目（プロジェクト基本情報）：**

| 項目 | 取得元・表示ルール |
|---|---|
| ID | `projects.id` |
| プロジェクト名 | `projects.name` |
| 開始日 | `projects.start_date` |
| 終了日 | `projects.end_date`（`NULL` の場合は「未定」） |
| 予算 | `projects.budget` |
| ステータス | 問題 7-1 と同じ判定ルール |

**表示する項目（参加メンバー一覧）：**

| 項目 | 取得元 |
|---|---|
| 社員名 | `employees.name` |
| 役割 | `employee_projects.role` |

参加メンバーは社員名の昇順（`e.name ASC`）で表示すること。

**その他の要件：**
- 存在しない ID へのアクセスには `NotFound()` を返すこと
- ページ下部に「一覧に戻る」リンクを設置すること（リンク先：`/Project`）

**使用する SQL（概要）：**

プロジェクト基本情報（1 件取得）：

```sql
SELECT id, name, start_date, end_date, budget
FROM projects
WHERE id = @id
```

参加メンバー一覧：

```sql
SELECT e.name, ep.role
FROM employee_projects ep
JOIN employees e ON ep.employee_id = e.id
WHERE ep.project_id = @id
ORDER BY e.name
```

**確認ポイント：**
- `/Project/Details/1` にアクセスすると「基幹システム刷新」の情報が表示されること
- 参加メンバーに「鈴木 花子（リーダー）」が含まれること
- 参加メンバーが社員名の昇順で表示されること
- `/Project/Details/999` にアクセスすると 404 になること
- 「一覧に戻る」をクリックすると `/Project` に遷移すること

---

## ヒント

### 2 種類のデータを 1 つの View に渡す方法

詳細画面では「プロジェクト基本情報（1 件）」と「参加メンバー一覧（複数件）」の 2 種類を View に渡す必要があります。
`EmployeeController.Details` は `Employee` 1 件しか渡していませんでしたが、今回は情報が 2 種類あります。

**方法 A — `ViewData` を使う（シンプル）：**

```csharp
var project = _db.QueryFirstOrDefault<Project>(sql, new { id });
var members = _db.Query<ProjectMember>(memberSql, new { id });
ViewData["Members"] = members;
return View(project);
```

View 側では `ViewData["Members"] as IEnumerable<ProjectMember>` でキャストして使います。

**方法 B — ViewModel クラスを作る（推奨）：**

`Models/ProjectDetailsViewModel.cs` というファイルを新規作成します。

```csharp
public class ProjectDetailsViewModel
{
    public Project Project { get; set; } = null!;
    public IEnumerable<ProjectMember> Members { get; set; } = [];
}
```

コントローラーで両方詰めて渡し、View の先頭で `@model ProjectDetailsViewModel` と宣言します。

### 参加メンバーの Model

参加メンバー取得クエリの結果を受け取るために `Models/ProjectMember.cs` を作成してください。
必要なプロパティは `Name`（社員名）と `Role`（役割）の 2 つです。

### ビルドエラーの確認

ファイルを新規作成したらターミナルで次のコマンドを実行するとコンパイルエラーを確認できます。

```
dotnet build
```
