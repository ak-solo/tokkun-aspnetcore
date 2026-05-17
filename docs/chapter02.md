# 第2章 詳細表示

## このチャプターで学ぶこと

- URL の中の数字（`/Employee/Details/3` の `3`）がコントローラーにどう届くか
- `QueryFirstOrDefault<T>()` で 1 件だけ取得する方法
- `@Model` で View がコントローラーから 1 件のデータを受け取る方法
- 存在しない ID にアクセスしたときの処理
- JOIN でテーブルを結合して部署名を表示する方法

---

## 基礎知識

### ルーティング：URL がコントローラーに届くまで

ブラウザで `/Employee/Details/3` にアクセスすると、何が起きているのでしょうか。

```
ブラウザが /Employee/Details/3 にアクセス
          │
          │  ASP.NET Core のルーティング
          ▼
  {controller} = Employee  →  EmployeeController
  {action}     = Details   →  Details() メソッド
  {id}         = 3         →  int id パラメータに 3 が入る
          │
          ▼
  EmployeeController.Details(int id = 3) が呼ばれる
```

ルーティングのパターンは `Program.cs` に定義されています。

```csharp
// Program.cs
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Employee}/{action=Index}/{id?}");
```

`{id?}` の末尾の `?` は「あってもなくてもよい（省略可能）」という意味です。

一覧ページの `Index` アクションには `id` が不要なので、`/Employee` だけでアクセスできます。
詳細ページの `Details` アクションには `id` が必要なので、`/Employee/Details/3` のように指定します。

### アクションメソッドで 1 件取得する

`Controllers/EmployeeController.cs` の `Details` メソッドを見てみましょう。

```csharp
// Controllers/EmployeeController.cs
public IActionResult Details(int id)
{
    var employee = _db.QueryFirstOrDefault<Employee>(
        "SELECT * FROM employees WHERE id = @id",
        new { id });
    if (employee == null) return NotFound();
    return View(employee);
}
```

**`QueryFirstOrDefault<Employee>(sql, parameters)`** — SQL を実行して**最初の 1 件**を `Employee` オブジェクトに変換して返します。
- 1 件も見つからなかった場合は `null` を返します
- `new { id }` は SQL の `@id` に変数 `id` の値を渡すパラメータです

> **tokkun-sql との接続**
> `WHERE id = @id` の SQL はそのまま tokkun-sql で書いたものと同じです。

**`if (employee == null) return NotFound()`** — 社員が見つからなかった場合に HTTP **404** を返します。

> **tokkun-csharp との接続**
> `if (employee == null)` は tokkun-csharp で学んだ null チェックと同じです。

**`return View(employee)`** — `Employee` 型の 1 件のデータを View に渡します。

一覧（ch01）との違いをまとめると:

| | ch01（一覧） | ch02（詳細） |
|---|---|---|
| 取得メソッド | `Query<Employee>()` | `QueryFirstOrDefault<Employee>()` |
| 返すデータの型 | `IEnumerable<Employee>` | `Employee`（1 件・`null` の可能性あり） |
| View での宣言 | `@model IEnumerable<Employee>` | `@model Employee` |

### View でデータを表示する（`@Model`）

`Views/Employee/Details.cshtml` を見てみましょう。

```html
@* Views/Employee/Details.cshtml *@
@model EmployeeApp.Models.Employee

<h1>社員詳細</h1>

<dl class="row">
    <dt class="col-sm-2">ID</dt>
    <dd class="col-sm-10">@Model.Id</dd>

    <dt class="col-sm-2">氏名</dt>
    <dd class="col-sm-10">@Model.Name</dd>
</dl>
```

**`@model Employee`** — Controller から受け取るデータの型を宣言します。
ch01 の一覧では `IEnumerable<Employee>`（リスト）でしたが、詳細では **`Employee`（1 件）** です。

**`@Model.Id`**、**`@Model.Name`** — Controller が `View(employee)` で渡した `employee` オブジェクトのプロパティを表示します。

```
Controller                        View
──────────────────────────        ─────────────────────────────
return View(employee);    ──►     @model Employee の Model
                                  @Model.Id     → employee.Id
                                  @Model.Name   → employee.Name
```

### リンクの仕組み（一覧 → 詳細）

一覧画面の `Index.cshtml` には次のようなリンクが書かれています。

```html
<a asp-action="Details" asp-route-id="@employee.Id">詳細</a>
```

**`asp-action="Details"`** — リンク先のアクション名を指定します。
**`asp-route-id="@employee.Id"`** — URL の `{id}` 部分に社員の ID を埋め込みます。

これにより、社員 ID が 3 の行には `/Employee/Details/3` というリンクが自動的に生成されます。

---

## スターターコードの確認

`dotnet run` でアプリを起動すると、社員一覧が表示されます。
現時点では一覧に「詳細」リンクはなく、`/Employee/Details/1` のように URL を直接入力すると詳細画面が表示されます。

```
社員詳細
────────────────────
ID    1
氏名   田中 太郎
────────────────────
一覧に戻る
```

詳細画面は **ID** と **氏名** の 2 項目のみ表示されています。

この章の練習問題では、まず一覧から詳細画面へのリンクを追加し、その後詳細画面に項目を追加したり、部署名を JOIN で表示したりします。
編集するファイルは以下の 2 〜 3 つです。

- `Views/Employee/Index.cshtml` — 詳細リンクを追加する
- `Views/Employee/Details.cshtml` — 表示する項目を変更する
- `Controllers/EmployeeController.cs` — SQL を変更する（JOIN するときのみ）
- `Models/Employee.cs` — プロパティを追加する（JOIN するときのみ）

---

## 練習問題

### 問題 2-1: 一覧に「詳細」リンクを追加する

各行の末尾に、詳細画面へのリンクを追加してください。

**編集するファイル：**
- `Views/Employee/Index.cshtml`

**ヒント：**

`<td>` の中で `asp-action` と `asp-route-id` を使うと、社員ごとの詳細 URL を自動で生成できます。

```html
<td>
    <a asp-action="Details" asp-route-id="@employee.Id">詳細</a>
</td>
```

`asp-action="Details"` はリンク先のアクション名、`asp-route-id="@employee.Id"` は URL の `{id}` 部分に社員 ID を埋め込む指定です。社員 ID が 3 の行には `/Employee/Details/3` というリンクが生成されます。

**確認ポイント：**
- 各行に「詳細」リンクが表示されること
- リンクをクリックすると、その社員の詳細画面が表示されること

---

### 問題 2-2: 詳細画面に「給与」を追加する

詳細画面に **給与（salary）** を追加してください。

**編集するファイル：**
- `Views/Employee/Details.cshtml` — 「給与」の表示を追加する

**ヒント：**

SQL は既に `SELECT *` で全カラムを取得しているため、Controller の変更は不要です。
`<dt>` に「給与」、`<dd>` に `@Model.Salary` を追加します。

```html
<dt class="col-sm-2">給与</dt>
<dd class="col-sm-10">@Model.Salary</dd>
```

**確認ポイント：**
- 詳細画面に「給与」が表示されること
- 給与が設定されていない社員は空白になること

---

### 問題 2-3: 詳細画面に残りの項目をすべて追加する

詳細画面に **入社日（hire_date）**・**部署 ID（dept_id）**・**上司 ID（manager_id）** も追加して、全カラムを表示してください。

**編集するファイル：**
- `Views/Employee/Details.cshtml`

**ヒント：**
- `@Model.HireDate`、`@Model.DeptId`、`@Model.ManagerId` を使います
- 問題 2-2 と同じパターンで `<dt>` / `<dd>` を追加するだけです

**確認ポイント：**
- すべての項目が表示されること
- `null` の項目（部署なし・上司なし）は空白になること

---

### 問題 2-4: 存在しない ID にアクセスしたときの挙動を確認する（発展）

ブラウザで `/Employee/Details/99999` にアクセスして、何が起きるか確認してください。
その後、`Details` アクションの `if (employee == null)` の行を読んで、何が起きているか説明できるようにしましょう。

**確認すること：**
1. `/Employee/Details/99999` にアクセスしたとき、画面に何が表示されるか
2. Controller のコードのどの部分がその挙動を引き起こしているか

**解説：**

```csharp
if (employee == null) return NotFound();
```

`NotFound()` は HTTP ステータスコード **404** を返すメソッドです。
ブラウザには「このページは見つかりません」という意味の応答が返ります。

では、もし `if (employee == null) return NotFound();` の行を削除するとどうなるでしょうか？
`employee` が `null` のまま `return View(employee)` が実行され、View の中で `@Model.Id` にアクセスしたときに **NullReferenceException** が発生します。

このチェックは「DBに存在しないIDにアクセスされた場合のガード」として必要不可欠です。

**確認ポイント：**
- 存在しない ID でアクセスしたとき、404 画面が表示されること

---

### 問題 2-5: 部署名を JOIN で表示する

現在は **部署 ID** しか表示できません。`departments` テーブルと LEFT JOIN して **部署名** を表示してください。

**編集するファイル：**
- `Models/Employee.cs` — `DeptName` プロパティを追加する
- `Controllers/EmployeeController.cs` — LEFT JOIN する SQL に変更する
- `Views/Employee/Details.cshtml` — 部署名を表示する

**ヒント：**

**ステップ 1：Model にプロパティを追加する**

```csharp
// Models/Employee.cs に追加
public string? DeptName { get; set; }
```

**ステップ 2：SQL を LEFT JOIN に変更する**

```csharp
// Controllers/EmployeeController.cs の Details アクション
var employee = _db.QueryFirstOrDefault<Employee>(
    @"SELECT e.*, d.name AS dept_name
      FROM employees e
      LEFT JOIN departments d ON e.dept_id = d.id
      WHERE e.id = @id",
    new { id });
```

`AS dept_name` と書くことで SQL の結果の列名が `dept_name` になり、`Employee.DeptName` プロパティに自動的にマッピングされます。

> **tokkun-sql との接続**
> `LEFT JOIN` は tokkun-sql で学んだ JOIN とまったく同じ SQL です。

**ステップ 3：View に部署名を追加する**

```html
<dt class="col-sm-2">部署名</dt>
<dd class="col-sm-10">@Model.DeptName</dd>
```

**確認ポイント：**
- 部署に所属している社員の詳細画面に部署名が表示されること
- 部署に所属していない社員は部署名が空白になること（LEFT JOIN の効果）

---

### 問題 2-6: ページタイトルを社員名にする

ブラウザのタブに表示されるページタイトルを「**社員詳細 - 田中 太郎**」のように、社員名を含む形に変更してください。

**編集するファイル：**
- `Views/Employee/Details.cshtml`

**ヒント：**

現在の `Details.cshtml` の先頭部分を見てみましょう。

```csharp
@{
    ViewData["Title"] = "社員詳細";
}
```

`ViewData["Title"]` に社員名を含めます。

```csharp
@{
    ViewData["Title"] = $"社員詳細 - {Model.Name}";
}
```

> **tokkun-csharp との接続**
> `$"社員詳細 - {Model.Name}"` は tokkun-csharp で学んだ**文字列補間**（string interpolation）です。

`ViewData["Title"]` がどこで使われているか気になる方は `Views/Shared/_Layout.cshtml` の `<title>` タグを確認してみましょう。

**確認ポイント：**
- ブラウザのタブに「社員詳細 - 田中 太郎」のように社員名が表示されること
