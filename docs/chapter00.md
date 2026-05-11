# 第0章 はじめに

この章は**読み物のみ**です。演習問題はありません。
Web アプリケーションの仕組みと、この教材のプロジェクト構成を理解することが目的です。

---

## このチャプターで学ぶこと

- ブラウザとサーバーがどのようにやり取りするか（HTTP の基本）
- MVC パターンとは何か（Model / View / Controller の役割）
- このプロジェクトのファイル構成と、学習者が触るファイルの範囲
- tokkun-sql で書いた SQL がそのまま使えること

---

## 1. Web アプリケーションとは

### ブラウザ ↔ サーバーの通信の流れ

ブラウザでURLにアクセスすると、以下の流れで画面が表示されます。

```
あなたのPC                              サーバー
┌──────────┐                        ┌──────────────────┐
│          │  ① HTTPリクエスト      │                  │
│  ブラウザ │ ──────────────────►  │  ASP.NET Core    │
│          │                        │  アプリケーション │
│          │  ④ HTTPレスポンス      │                  │
│          │ ◄──────────────────   │                  │
└──────────┘    (HTMLが返ってくる)   └────────┬─────────┘
                                             │ ② SQLクエリ
                                             ▼
                                    ┌──────────────────┐
                                    │   PostgreSQL     │
                                    │   データベース    │
                                    └──────────────────┘
                                             │ ③ 結果
                                             ▲
```

1. ブラウザが URL（例：`http://localhost:5000/Employee`）にアクセスする
2. サーバーのアプリケーションが SQL を実行してデータを取得する
3. データベースから結果が返ってくる
4. アプリケーションがデータを HTML に埋め込んでブラウザに返す

ブラウザには最終的な HTML が届くだけで、SQL やデータベースの存在は見えません。

---

## 2. HTTP リクエスト・レスポンスの基本

### HTTP リクエストとは

ブラウザがサーバーに送る「お願い」のことです。
最も基本的なリクエストの種類は **GET** です。

**GET リクエスト** — 「このURLのページを見せてください」というお願い。
ブラウザのアドレスバーに URL を入力してエンターを押すと、自動的に GET リクエストが送られます。

```
GET /Employee HTTP/1.1
Host: localhost:5000
```

### HTTP レスポンスとは

サーバーからブラウザへの「返答」のことです。

```
HTTP/1.1 200 OK
Content-Type: text/html

<html>
  <body>
    <h1>社員一覧</h1>
    ...
  </body>
</html>
```

`200 OK` はリクエストが成功したことを示します。
存在しないURLにアクセスすると `404 Not Found` が返ります。

> **tokkun-csharp との接続**
> C# のメソッド呼び出しに似ています。`/Employee` というリクエストが来たら、対応する C# のメソッドが呼ばれ、その戻り値（HTML）がブラウザに返されます。

---

## 3. MVC パターン

### 役割の分担

ASP.NET Core MVC では、処理を 3 つの役割に分けて実装します。

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│             │     │             │     │             │
│  Controller │────►│   Model     │────►│    View     │
│             │     │             │     │             │
│ リクエストを │     │ データを    │     │ データを    │
│ 受け取る    │     │ 表す        │     │ HTMLに変換  │
│             │     │             │     │ して返す    │
└─────────────┘     └─────────────┘     └─────────────┘
      ▲                   │
      │                   ▼
  ブラウザ          PostgreSQL
  からの              (Dapper
  リクエスト           経由)
```

| 役割 | ファイルの場所 | やること |
|------|--------------|---------|
| **Controller** | `Controllers/` | リクエストを受け取り、DBを操作し、Viewにデータを渡す |
| **Model** | `Models/` | DBのテーブルに対応するデータの「型」を定義する |
| **View** | `Views/` | Controllerから受け取ったデータをHTMLに変換する |

### 社員一覧表示の流れ（具体例）

ブラウザが `http://localhost:5000/Employee` にアクセスしたときの流れを追いましょう。

```
ブラウザ
  │
  │ GET /Employee
  ▼
EmployeeController.cs の Index() メソッドが呼ばれる
  │
  │ SELECT id, name FROM employees ORDER BY id
  ▼
PostgreSQL からデータが返ってくる
  │
  │ List<Employee> をViewに渡す
  ▼
Views/Employee/Index.cshtml が HTML を生成する
  │
  ▼
ブラウザに HTML が届き、表が表示される
```

`Controllers/EmployeeController.cs` の `Index` メソッドを見てみましょう。

```csharp
// Controllers/EmployeeController.cs
public IActionResult Index()
{
    var employees = _db.Query<Employee>(
        "SELECT id, name FROM employees ORDER BY id");
    return View(employees);
}
```

- `_db.Query<Employee>(...)` — SQLを実行してデータを取得する（Dapper）
- `return View(employees)` — 取得したデータをViewに渡してHTMLを生成する

`Views/Employee/Index.cshtml` がそのデータを受け取って HTML を作ります。

```html
@* Views/Employee/Index.cshtml *@
@model IEnumerable<EmployeeApp.Models.Employee>

<table class="table">
    @foreach (var employee in Model)
    {
        <tr>
            <td>@employee.Id</td>
            <td>@employee.Name</td>
        </tr>
    }
</table>
```

- `@model` — Controllerから受け取るデータの型を宣言する
- `Model` — Controllerが `View(employees)` で渡したデータそのもの
- `@employee.Name` — C# の変数の値を HTML に埋め込む（Razor 構文）

> **tokkun-csharp との接続**
> `@foreach` や `@employee.Name` は、tokkun-csharp で学んだ C# の構文がそのまま使えます。`.cshtml` ファイルは HTML の中に C# を書ける「Razor」という書き方です。

---

## 4. プロジェクト構成

### ファイルの役割一覧

```
src/EmployeeApp/
├── Program.cs                    ← アプリの起動設定（触らなくてOK）
├── appsettings.json              ← DB接続文字列などの設定（触らなくてOK）
│
├── Controllers/                  ★ 学習者が触るファイル
│   └── EmployeeController.cs
│
├── Models/                       ★ 学習者が触るファイル
│   ├── Employee.cs
│   └── Department.cs
│
└── Views/                        ★ 学習者が触るファイル
    ├── Employee/
    │   └── Index.cshtml
    └── Shared/
        └── _Layout.cshtml        ← 全ページ共通のレイアウト（触らなくてOK）
```

### 各ファイルの詳細

**`Program.cs`** — アプリケーションの起動設定。DI（依存性注入）の登録や URL ルーティングの設定が書かれています。この教材では変更しません。

**`appsettings.json`** — データベース接続文字列などの設定値を管理するファイルです。この教材では変更しません。

**`Controllers/EmployeeController.cs`** — ブラウザからのリクエストを受け取り、DB に問い合わせ、View にデータを渡します。**ch01 から編集します。**

**`Models/Employee.cs`** — `employees` テーブルの 1 行分のデータを表す C# のクラスです。DB のカラムと C# のプロパティが対応します。

```csharp
// Models/Employee.cs
public class Employee
{
    public int Id { get; set; }       // id カラム
    public string Name { get; set; }  // name カラム
    public int? DeptId { get; set; }  // dept_id カラム
    public decimal? Salary { get; set; }   // salary カラム
    public DateOnly? HireDate { get; set; } // hire_date カラム
    public int? ManagerId { get; set; }    // manager_id カラム
}
```

**`Views/Employee/Index.cshtml`** — 社員一覧の HTML テンプレートです。**ch01 から編集します。**

**`Views/Shared/_Layout.cshtml`** — ナビゲーションバーやフッターなど、全ページ共通の枠組みです。この教材では変更しません。

### URL とファイルの対応

ASP.NET Core MVC では、URL と Controller / Action メソッドが自動的に対応付けられます。

| URL | Controller | アクションメソッド |
|-----|-----------|-----------------|
| `/Employee` | EmployeeController | Index() |
| `/Employee/Details/3` | EmployeeController | Details(3) |
| `/Employee/Create` | EmployeeController | Create() |

このルールを**ルーティング**と呼びます。`Program.cs` に設定が書かれていますが、この規則を知っているだけで十分です。

---

## 5. Dapper と SQL の接続

### Dapper とは

**Dapper** は、C# から SQL を実行するための薄いライブラリです。
tokkun-sql で学んだ SQL をそのまま書けることが最大の特長です。

```csharp
// tokkun-sql で書いたSQLをそのまま使える
var employees = _db.Query<Employee>(
    "SELECT id, name FROM employees ORDER BY id");
```

`_db.Query<Employee>(sql)` は「このSQLを実行して、結果を `Employee` 型のリストにして返して」という意味です。

### SQL → C# オブジェクトへの変換

Dapper は SQL の結果を自動的に C# オブジェクトに変換します。

```
DBの結果                          C# のオブジェクト
┌────┬──────────┐               ┌─────────────────┐
│ id │   name   │               │ Employee        │
├────┼──────────┤               │  Id   = 1       │
│  1 │ 田中 太郎 │  ──────────► │  Name = "田中..." │
└────┴──────────┘               └─────────────────┘
```

カラム名（`id`, `name`）と C# のプロパティ名（`Id`, `Name`）が自動的に対応付けられます。大文字・小文字は区別されません。

> **tokkun-sql との接続**
> tokkun-sql で書いた `SELECT` / `WHERE` / `JOIN` / `ORDER BY` などがそのまま使えます。SQL の書き方を新しく覚える必要はありません。

### パラメータの渡し方

SQL に変数を渡すときは `@パラメータ名` を使います。

```csharp
// id = 3 の社員を1件取得する
var employee = _db.QueryFirstOrDefault<Employee>(
    "SELECT * FROM employees WHERE id = @id",
    new { id = 3 });
```

この `@id` は、tokkun-sql の `$1` に相当します。
Dapper では匿名オブジェクト `new { id = 3 }` でパラメータを渡します。

> **セキュリティの重要ポイント**
> パラメータは必ず `@id` のような形で渡してください。文字列を直接 SQL に埋め込むと **SQL インジェクション**という脆弱性が生まれます。

---

## 次のステップ

この章の内容を全部覚える必要はありません。
実際にコードを読み書きしながら、少しずつ理解を深めていきましょう。

[chapter01.md](chapter01.md) で、社員一覧画面への機能追加を通して Controller → Dapper → View の流れを体験します。
