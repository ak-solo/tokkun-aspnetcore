# 第3章 新規登録

## このチャプターで学ぶこと

- GET と POST の違い
- `<form>` タグでデータを送信する仕組みと `[HttpPost]` アクション
- PRG パターン（Post-Redirect-Get）でブラウザリロード問題を防ぐ方法
- Dapper で INSERT を実行する方法
- `RETURNING id` で登録した行の ID を取得する方法

---

## 基礎知識

### GET と POST の違い

HTTP には「メソッド」という概念があり、リクエストの目的を区別します。
これまで学んできた一覧・詳細ページはすべて **GET** リクエストでした。

| メソッド | 用途 | 特徴 |
|---------|------|------|
| **GET** | データを取得する（読む） | URL にパラメータが含まれる。ブックマーク・履歴に残る |
| **POST** | データを送信する（書く・登録） | データは URL ではなくリクエストの本文に入る |

登録フォームでは POST を使います。理由は 2 つあります。

1. **セキュリティ**：入力内容（名前・給与など）が URL に露出しない
2. **安全性**：ブラウザの「戻る」ボタンや再読み込みで誤って再送信されにくい

### フォームからデータを送信する

`Create.cshtml` の `<form>` タグを見てみましょう。

```html
<!-- Views/Employee/Create.cshtml -->
<form asp-action="Create" method="post">
    <div class="mb-3">
        <label class="form-label">氏名</label>
        <input type="text" name="name" class="form-control" />
    </div>
    <div class="mb-3">
        <label class="form-label">給与</label>
        <input type="number" name="salary" class="form-control" />
    </div>
    <button type="submit" class="btn btn-primary">登録</button>
</form>
```

**`asp-action="Create"`** — フォームの送信先（アクション名）を指定します。これにより `<form action="/Employee/Create" ...>` という HTML が生成されます。

**`method="post"`** — POST メソッドで送信することを指定します。

**`name="name"` / `name="salary"`** — 各フィールドの `name` 属性がコントローラーで受け取るプロパティ名になります。

---

コントローラー側では `[HttpPost]` 属性をつけたアクションがこのリクエストを受け取ります。

```csharp
// Controllers/EmployeeController.cs
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Create(Employee employee)
{
    // ここで employee.Name、employee.Salary などが利用できる
}
```

**`[HttpPost]`** — POST リクエストにのみ対応するアクションであることを示します。
`Create` という名前のアクションが 2 つある（GET 用と POST 用）ため、この属性で区別します。

**`[ValidateAntiForgeryToken]`** — CSRF（クロスサイトリクエストフォージェリ）攻撃を防ぐための検証を行います。
`<form asp-action="Create" method="post">` を使うと、隠しフィールドにトークンが自動的に埋め込まれます。

**引数 `Employee employee`** — フォームの各フィールドの値が `Employee` オブジェクトに自動的にマッピングされます（**モデルバインディング**）。

```
フォームフィールド      Employee プロパティ
──────────────         ──────────────────
name="name"     ──►   employee.Name
name="salary"   ──►   employee.Salary
name="hire_date" ──►  employee.HireDate
```

ASP.NET Core のモデルバインディングはフィールド名の大文字・小文字を区別しません。

---

### PRG パターン（Post-Redirect-Get）

登録処理が完了したあと、そのまま View を返してしまうと問題が起きます。

```
❌ PRG なし（NG）
────────────────────────────────────────
ブラウザ → POST /Employee/Create → Controller
                                    INSERT 実行
                               ◄── return View() ← HTML を直接返す
ここでブラウザをリロードすると...
ブラウザ → 「フォームを再送信しますか？」 → 同じ INSERT が再実行される！
```

これを防ぐために **PRG パターン** を使います。

```
✅ PRG あり（OK）
────────────────────────────────────────
ブラウザ → POST /Employee/Create → Controller
                                    INSERT 実行
                               ◄── return RedirectToAction("Details", new { id })
ブラウザ → GET /Employee/Details/5 （リダイレクト先へ移動）
                               ◄── return View() ← HTML を返す
ここでリロードしても GET リクエストになるので INSERT は再実行されない
```

**`return RedirectToAction(nameof(Details), new { id })`** — コントローラーの別のアクションへリダイレクトします。ブラウザには HTTP 302 が返され、ブラウザが自動的に指定した URL へ GET リクエストを送ります。

> **なぜ `nameof(Details)` を使うか**
> `"Details"` とハードコードするより `nameof(Details)` を使うと、メソッド名を変更したときにコンパイルエラーになるため安全です。

---

### Dapper で INSERT を実行する

#### INSERT するだけでよい場合：`Execute()`

登録後に ID が不要な場合は `Execute()` を使います。

```csharp
_db.Execute(
    "INSERT INTO employees (name, salary) VALUES (@Name, @Salary)",
    employee);
```

#### 登録した行の ID が必要な場合：`RETURNING id` + `QuerySingle<int>()`

登録後に詳細画面にリダイレクトするには、新しく採番された ID が必要です。
PostgreSQL では `RETURNING id` を使うと INSERT した行の ID を取得できます。

```csharp
var id = _db.QuerySingle<int>(
    @"INSERT INTO employees (name, salary)
      VALUES (@Name, @Salary)
      RETURNING id",
    employee);
return RedirectToAction(nameof(Details), new { id });
```

**`QuerySingle<int>()`** — 結果が必ず 1 行だとわかっている SELECT / INSERT ... RETURNING に使います。

> **tokkun-sql との接続**
> `RETURNING id` は tokkun-sql で学んだ DML の応用です。`INSERT` 文の末尾に追加するだけで、INSERT した行のカラム値を返せます。

---

## スターターコードの確認

`dotnet run` でアプリを起動し、一覧画面の右上「新規登録」リンク（または `/Employee/Create`）にアクセスすると登録フォームが表示されます。

```
社員登録
────────────────────
氏名   [ テキスト入力 ]
給与   [ 数値入力   ]

[登録]  [キャンセル]
────────────────────
```

現時点では「登録」ボタンをクリックすると **405 Method Not Allowed** エラーになります。
POST を受け取るアクションがまだ実装されていないためです。

この章の練習問題では：
- `Views/Employee/Create.cshtml` — フォームにフィールドを追加する
- `Controllers/EmployeeController.cs` — POST アクションを実装する

---

## 練習問題

### 問題 3-1: フォームに「入社日」を追加する

登録フォームに **入社日（hire_date）** の入力フィールドを追加してください。

**編集するファイル：**
- `Views/Employee/Create.cshtml`

**ヒント：**

HTML の `<input type="date">` を使うと日付の入力フィールドになります。

```html
<div class="mb-3">
    <label class="form-label">入社日</label>
    <input type="date" name="hire_date" class="form-control" />
</div>
```

`name="hire_date"` と書くと `Employee.HireDate` プロパティに自動でマッピングされます。

**確認ポイント：**
- フォームに「入社日」のフィールドが表示されること
- （この段階では「登録」ボタンはまだ動きません）

---

### 問題 3-2: フォームに「部署 ID」を追加する

登録フォームに **部署 ID（dept_id）** の入力フィールドも追加してください。

**編集するファイル：**
- `Views/Employee/Create.cshtml`

**ヒント：**

`type="number"` を使います。給与フィールドと同じパターンです。

**確認ポイント：**
- フォームに「部署 ID」のフィールドが表示されること

---

### 問題 3-3: INSERT を実装する

「登録」ボタンをクリックしたときに社員を DB に登録する処理を実装してください。
登録後は **一覧画面** にリダイレクトしてください。

**編集するファイル：**
- `Controllers/EmployeeController.cs`

**ヒント：**

`[HttpPost]` アクションを追加します。

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Create(Employee employee)
{
    _db.Execute(
        @"INSERT INTO employees (name, dept_id, salary, hire_date)
          VALUES (@Name, @DeptId, @Salary, @HireDate)",
        employee);
    return RedirectToAction(nameof(Index));
}
```

**確認ポイント：**
- フォームに入力して「登録」をクリックすると一覧画面に移動すること
- 一覧画面に登録した社員が表示されること

---

### 問題 3-4: 登録後に詳細画面へリダイレクトする

問題 3-3 では一覧画面にリダイレクトしていました。
これを **登録した社員の詳細画面** にリダイレクトするよう変更してください。

**編集するファイル：**
- `Controllers/EmployeeController.cs`

**ヒント：**

`RETURNING id` を使って INSERT した行の ID を取得し、`RedirectToAction` で詳細画面に渡します。

```csharp
var id = _db.QuerySingle<int>(
    @"INSERT INTO employees (name, dept_id, salary, hire_date)
      VALUES (@Name, @DeptId, @Salary, @HireDate)
      RETURNING id",
    employee);
return RedirectToAction(nameof(Details), new { id });
```

`new { id }` は `new { id = id }` の省略形です（C# のオブジェクト初期化子）。
これにより `/Employee/Details/5` のような URL が生成されます。

**確認ポイント：**
- 登録後に「いま登録した社員」の詳細画面が表示されること
- ブラウザをリロードしても社員が 2 件登録されないこと（PRG パターンが効いている）

---

### 問題 3-5: 一覧画面に「新規登録」ボタンを追加する

社員一覧画面の上部に「新規登録」ボタン（リンク）を追加してください。
クリックすると登録フォーム（`/Employee/Create`）に移動するようにしてください。

**編集するファイル：**
- `Views/Employee/Index.cshtml`

**ヒント：**

一覧表の上に `<a>` タグを追加します。Bootstrap の `btn btn-primary` クラスをつけるとボタンのように表示できます。

```html
<a asp-action="Create" class="btn btn-primary mb-3">新規登録</a>
```

`asp-action="Create"` で `/Employee/Create` への URL が自動生成されます。

**確認ポイント：**
- 一覧画面に「新規登録」ボタンが表示されること
- クリックすると登録フォームに移動すること
