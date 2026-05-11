# 第1章 一覧表示

## このチャプターで学ぶこと

- Controller がリクエストを受け取り、DB からデータを取得して View に渡す流れ
- Dapper の `Query<T>()` で SQL を実行してデータを取得する方法
- View で受け取ったデータを表に表示する方法
- SQL の `SELECT` に列を追加する・`ORDER BY` を変更する方法

---

## 基礎知識

### Controller → Dapper → View のデータの流れ

```
ブラウザが /employees にアクセス
        │
        ▼
EmployeeController の Index() が呼ばれる
        │
        │  _db.Query<Employee>("SELECT ...")
        ▼
PostgreSQL が結果を返す
        │
        │  IEnumerable<Employee> が返ってくる
        ▼
return View(employees) でViewにデータを渡す
        │
        ▼
Views/Employee/Index.cshtml が HTML を生成する
        │
        ▼
ブラウザに HTML が届く
```

### Controller のコード

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

**`_db.Query<Employee>(sql)`** は「このSQLを実行して、結果を `Employee` 型のリストにして返して」という意味です。`Employee` は `Models/Employee.cs` で定義したクラスです。

**`return View(employees)`** は「`employees` というデータを View に渡してHTMLを作って返して」という意味です。

### SQL の結果 → C# オブジェクトへの変換（Dapper）

Dapper は SQL の結果を自動的に C# オブジェクトに変換します。

```
DBの結果                          Employee オブジェクト
┌────┬──────────┐               ┌────────────────────┐
│ id │   name   │               │ Id   = 1           │
├────┼──────────┤  ──────────►  │ Name = "田中 太郎"  │
│  1 │ 田中 太郎 │               └────────────────────┘
└────┴──────────┘
```

列名と C# のプロパティ名が自動的に対応します（大文字・小文字は区別しません）。
**SELECT に書いた列だけ**が C# オブジェクトに入ります。書かなかった列は `null` のままです。

> **tokkun-sql との接続**
> `SELECT id, name FROM employees` は tokkun-sql で書いたものと全く同じ SQL です。

### View のコード

`Views/Employee/Index.cshtml` を見てみましょう。

```html
@* Views/Employee/Index.cshtml *@
@model IEnumerable<EmployeeApp.Models.Employee>

<table class="table">
    <thead>
        <tr>
            <th>ID</th>
            <th>氏名</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var employee in Model)
        {
            <tr>
                <td>@employee.Id</td>
                <td>@employee.Name</td>
            </tr>
        }
    </tbody>
</table>
```

**`@model IEnumerable<Employee>`** — Controller から受け取るデータの型を宣言します。`IEnumerable<Employee>` は「Employee のリスト」という意味です。

**`Model`** — Controller が `View(employees)` で渡したデータそのものです。

**`@employee.Name`** — C# の変数の値を HTML に埋め込みます（Razor 構文）。

> **tokkun-csharp との接続**
> `@foreach` は tokkun-csharp で学んだ `foreach` 文と同じです。Razor では `@` を付けることで HTML の中に C# を書けます。

---

## スターターコードの確認

`dotnet run` でアプリを起動すると、社員一覧が表示されます。
現時点では **ID** と **氏名** の 2 列のみ表示されています。

```
| ID | 氏名      |
|----|-----------|
|  1 | 田中 太郎  |
|  2 | 鈴木 花子  |
| .. | ...       |
```

この章の練習問題では、この一覧に列を追加したり、ソート順を変えたりします。
編集するファイルは以下の 2 つです。

- `Controllers/EmployeeController.cs` — SQL を変更する
- `Views/Employee/Index.cshtml` — 表示する列を変更する

---

## 練習問題

### 問題 1-1: 一覧に「給与」を追加する

現在の一覧は ID と氏名しか表示されていません。**給与（salary）** 列を追加してください。

**編集するファイル：**
- `Controllers/EmployeeController.cs` — SQL に `salary` を追加する
- `Views/Employee/Index.cshtml` — 「給与」列のヘッダーとデータを追加する

**ヒント：**
- SQL の `SELECT id, name` に `, salary` を追加します
- View の `<th>` に「給与」を追加し、`<td>@employee.Salary</td>` を追加します

**確認ポイント：**
- 一覧に「給与」列が表示されること
- 各行に給与の金額が表示されること（給与が未設定の社員は空白になります）

---

### 問題 1-2: ソート順を「入社日の新しい順」に変更する

現在は `ORDER BY id`（ID の昇順）でソートされています。
**入社日が新しい順（`hire_date` の降順）** に変更してください。

**編集するファイル：**
- `Controllers/EmployeeController.cs` — SQL の `ORDER BY` を変更する

**ヒント：**
- `ORDER BY id` を `ORDER BY hire_date DESC` に変えます
- `DESC` は「降順（大きい順・新しい順）」を意味します

**確認ポイント：**
- 一覧が入社日の新しい順に並んでいること
- `hire_date` が NULL の社員は最後に表示されること（PostgreSQL の仕様）

---

### 問題 1-3: 一覧に「入社日」を追加する

一覧に **入社日（hire_date）** 列を追加してください。

**編集するファイル：**
- `Controllers/EmployeeController.cs` — SQL に `hire_date` を追加する
- `Views/Employee/Index.cshtml` — 「入社日」列を追加する

**ヒント：**
- SELECT に `, hire_date` を追加します
- View で `@employee.HireDate` と書くと日付が表示されます
- 問題 1-2 のソート変更と組み合わせると、入社日の新しい順に並んで見やすくなります

**確認ポイント：**
- 一覧に「入社日」列が表示されること

---

### 問題 1-4: 一覧に「部署 ID」を追加する

一覧に **部署 ID（dept_id）** 列を追加してください。

**編集するファイル：**
- `Controllers/EmployeeController.cs` — SQL に `dept_id` を追加する
- `Views/Employee/Index.cshtml` — 「部署 ID」列を追加する

**ヒント：**
- SELECT に `, dept_id` を追加します
- View で `@employee.DeptId` と書くと値が表示されます
- 部署に所属していない社員は `null` になるため、空白が表示されます

**確認ポイント：**
- 一覧に「部署 ID」列が表示されること
- 部署に所属していない社員の列が空白であること

---

### 問題 1-5: 一覧の先頭に社員数を表示する

表の上に「**全 XX 名**」という形式で社員の総数を表示してください。

**編集するファイル：**
- `Views/Employee/Index.cshtml` — 表の上に社員数を追加する

**ヒント：**

`Model` は `IEnumerable<Employee>` なので、`Model.Count()` で件数を取得できます。

```html
<p>全 @Model.Count() 名</p>
<table class="table">
    ...
```

> **tokkun-csharp との接続**
> `Count()` は tokkun-csharp で学んだ LINQ の拡張メソッドです。

**確認ポイント：**
- 表の上に「全 XX 名」と表示されること
- 件数がシードデータの社員数と一致すること
