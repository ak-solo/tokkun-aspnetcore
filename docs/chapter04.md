# 第4章 編集

## このチャプターで学ぶこと

- DataAnnotations によるバリデーション（入力値の検証）
- `ModelState.IsValid` でバリデーション結果を確認する方法
- `asp-for` / `asp-validation-for` でフォームにエラーメッセージを表示する方法
- Dapper で UPDATE を実行する方法

---

## 基礎知識

### バリデーションとは

Web フォームでは、ユーザーが不正な値を入力しても正しく動作するようにする必要があります。
例えば「氏名が空のまま更新される」「給与にマイナス値が入力される」といったケースです。

このような入力チェックを **バリデーション（検証）** と呼びます。

ASP.NET Core MVC では **DataAnnotations** という仕組みを使って、
モデルクラスにアトリビュートを付けるだけでバリデーションを定義できます。

---

### DataAnnotations でバリデーションを定義する

`Models/Employee.cs` にバリデーション属性を追加します。

```csharp
// Models/Employee.cs
using System.ComponentModel.DataAnnotations;

public class Employee
{
    public int Id { get; set; }

    [Required(ErrorMessage = "氏名は必須です")]
    public string Name { get; set; } = string.Empty;

    [Range(0, 100_000_000, ErrorMessage = "給与は 0 以上 1 億以下で入力してください")]
    public decimal? Salary { get; set; }

    // ...
}
```

主なバリデーション属性：

| 属性 | 意味 |
|------|------|
| `[Required]` | 必須入力（空文字・null を許可しない） |
| `[Range(min, max)]` | 数値の範囲を制限する |
| `[StringLength(max)]` | 文字列の最大長を制限する |
| `[EmailAddress]` | メールアドレス形式かどうかを検証する |

各属性に `ErrorMessage = "..."` を指定すると、エラー時に表示するメッセージを日本語にできます。

---

### コントローラーで ModelState.IsValid を確認する

フォームが送信されると、ASP.NET Core がモデルのバリデーションを自動的に実行し、
結果を `ModelState.IsValid` に格納します。

```csharp
// Controllers/EmployeeController.cs
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Edit(Employee employee)
{
    if (!ModelState.IsValid)
    {
        return View(employee);   // エラーがあればフォームを再表示
    }

    // バリデーション OK → DB を更新して詳細画面へリダイレクト
    _db.Execute(
        @"UPDATE employees
          SET name = @Name, salary = @Salary
          WHERE id = @Id",
        employee);
    return RedirectToAction(nameof(Details), new { employee.Id });
}
```

**`if (!ModelState.IsValid)`** — バリデーションエラーがある場合は `false` になります。
`return View(employee)` でフォームを再表示し、入力値とエラーメッセージをそのまま残します。

バリデーション OK の場合だけ UPDATE を実行し、PRG パターンでリダイレクトします。

---

### View でエラーメッセージを表示する

`asp-for` と `asp-validation-for` タグヘルパーを使います。

```html
<!-- Views/Employee/Edit.cshtml -->
<form asp-action="Edit" method="post">
    <input type="hidden" asp-for="Id" />
    <div class="mb-3">
        <label asp-for="Name" class="form-label">氏名</label>
        <input asp-for="Name" class="form-control" />
        <span asp-validation-for="Name" class="text-danger"></span>
    </div>
    <div class="mb-3">
        <label asp-for="Salary" class="form-label">給与</label>
        <input asp-for="Salary" class="form-control" />
        <span asp-validation-for="Salary" class="text-danger"></span>
    </div>
    <button type="submit" class="btn btn-primary">更新</button>
    <a asp-action="Details" asp-route-id="@Model.Id" class="btn btn-secondary">キャンセル</a>
</form>
```

| タグヘルパー | 役割 |
|------------|------|
| `asp-for="Name"` | `<input>` の `name` と `value` をモデルのプロパティと自動連携する |
| `asp-validation-for="Name"` | バリデーションエラーがあるとき、ここにエラーメッセージを表示する |

**`asp-for`** は `<input name="Name" value="山田太郎" ...>` のような HTML を生成します。
`name=` 属性や `value=` 属性を自分で書く必要がなくなり、プロパティ名との対応も確実になります。

---

### Dapper で UPDATE を実行する

UPDATE は `Execute()` を使います（SELECT を返さないためです）。

```csharp
_db.Execute(
    @"UPDATE employees
      SET name = @Name, dept_id = @DeptId, salary = @Salary, hire_date = @HireDate
      WHERE id = @Id",
    employee);
```

`@Id`, `@Name` などのパラメータは `employee` オブジェクトのプロパティから自動対応します。
**`WHERE id = @Id` を必ず書くこと**。忘れると全レコードが更新されてしまいます。

> **tokkun-sql との接続**
> UPDATE 文の構文は tokkun-sql で学んだものとまったく同じです。
> `WHERE` 句を忘れない、という点も同じ注意事項です。

---

## スターターコードの確認

`dotnet run` でアプリを起動し、社員一覧から任意の社員の「詳細」→「編集」リンクをクリックすると編集フォームが表示されます。

```
社員編集
────────────────────
氏名   [ 山田太郎   ]
給与   [ 500000    ]

[更新]  [キャンセル]
────────────────────
```

現時点では「更新」ボタンをクリックすると **405 Method Not Allowed** エラーになります。
POST を受け取るアクションがまだ実装されていないためです。

この章の練習問題では：
- `Models/Employee.cs` — バリデーション属性を追加する
- `Views/Employee/Edit.cshtml` — タグヘルパーとエラー表示を追加する
- `Controllers/EmployeeController.cs` — POST アクションを実装する

---

## 練習問題

### 問題 4-1: 氏名を必須項目にする

`Employee` モデルの `Name` プロパティに `[Required]` 属性を追加して、
氏名が空のまま送信されたときにエラーになるようにしてください。

**編集するファイル：**
- `Models/Employee.cs`

**ヒント：**

ファイル先頭に `using System.ComponentModel.DataAnnotations;` を追加し、
プロパティの上に属性を記述します。

```csharp
using System.ComponentModel.DataAnnotations;

public class Employee
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;
    // ...
}
```

**確認ポイント：**
- この段階ではまだフォームは動きません（POST アクションが未実装のため）
- コンパイルエラーがないこと

---

### 問題 4-2: エラーメッセージを日本語にする

問題 4-1 で追加した `[Required]` のエラーメッセージを日本語にしてください。
また、給与（`Salary`）に `[Range]` で 0 以上 1 億以下の制限を追加してください。

**編集するファイル：**
- `Models/Employee.cs`

**ヒント：**

`[Required(ErrorMessage = "...")]` と `[Range(min, max, ErrorMessage = "...")]` を使います。

```csharp
[Required(ErrorMessage = "氏名は必須です")]
public string Name { get; set; } = string.Empty;

[Range(0, 100_000_000, ErrorMessage = "給与は 0 以上 1 億以下で入力してください")]
public decimal? Salary { get; set; }
```

**確認ポイント：**
- コンパイルエラーがないこと

---

### 問題 4-3: フォームにエラー表示を追加する

`Edit.cshtml` のフォームを `asp-for` / `asp-validation-for` を使った形式に書き換えて、
バリデーションエラーが表示されるようにしてください。

**編集するファイル：**
- `Views/Employee/Edit.cshtml`

**ヒント：**

`<input name="name" value="@Model.Name" />` を `<input asp-for="Name" />` に書き換えます。
また `<span asp-validation-for="Name" class="text-danger"></span>` を追加します。

```html
<div class="mb-3">
    <label asp-for="Name" class="form-label">氏名</label>
    <input asp-for="Name" class="form-control" />
    <span asp-validation-for="Name" class="text-danger"></span>
</div>
```

`asp-for` は `name=` 属性と `value=` 属性を自動設定してくれるので、
`name="name"` や `value="@Model.Name"` を自分で書く必要がなくなります。

**確認ポイント：**
- 見た目は変わらないこと（フォームに現在の値が表示されること）

---

### 問題 4-4: UPDATE を実装する

「更新」ボタンをクリックしたときに社員情報を DB に保存する処理を実装してください。
バリデーションエラーがある場合はフォームを再表示し、
成功したら詳細画面にリダイレクトしてください。

**編集するファイル：**
- `Controllers/EmployeeController.cs`

**ヒント：**

`[HttpPost]` アクションを追加します。まず `ModelState.IsValid` を確認してから UPDATE します。

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Edit(Employee employee)
{
    if (!ModelState.IsValid)
    {
        return View(employee);
    }

    _db.Execute(
        @"UPDATE employees
          SET name = @Name, salary = @Salary
          WHERE id = @Id",
        employee);
    return RedirectToAction(nameof(Details), new { employee.Id });
}
```

**確認ポイント：**
- 正しい値を入力して「更新」をクリックすると詳細画面に移動すること
- 氏名を空にして「更新」をクリックするとエラーメッセージが表示されること
- 給与に負の値を入力するとエラーメッセージが表示されること

---

### 問題 4-5: UPDATE で全カラムを更新する

問題 4-4 では氏名と給与だけを更新していました。
部署 ID（`dept_id`）・入社日（`hire_date`）も更新対象に追加してください。
また、フォームに「部署 ID」と「入社日」の入力フィールドも追加してください。

**編集するファイル：**
- `Views/Employee/Edit.cshtml`
- `Controllers/EmployeeController.cs`

**ヒント：**

フォームにフィールドを追加します（ch03 の問題 3-1・3-2 と同じパターンです）。

```html
<div class="mb-3">
    <label asp-for="DeptId" class="form-label">部署 ID</label>
    <input asp-for="DeptId" class="form-control" />
</div>
<div class="mb-3">
    <label asp-for="HireDate" class="form-label">入社日</label>
    <input asp-for="HireDate" class="form-control" />
</div>
```

UPDATE 文にも `dept_id = @DeptId, hire_date = @HireDate` を追加します。

**確認ポイント：**
- フォームに部署 ID・入社日が表示されること
- 値を変更して「更新」をクリックすると詳細画面の表示も変わること

---

### 問題 4-6: 詳細画面に「編集」リンクを追加する

詳細画面から編集画面に移動できる「編集」ボタンをすでに追加済みの場合は確認してください。
追加していない場合は追加してください。

**編集するファイル：**
- `Views/Employee/Details.cshtml`

**ヒント：**

```html
<a asp-action="Edit" asp-route-id="@Model.Id" class="btn btn-secondary">編集</a>
```

`asp-route-id="@Model.Id"` により `/Employee/Edit/5` のような URL が生成されます。

**確認ポイント：**
- 詳細画面に「編集」ボタンが表示されること
- クリックすると編集フォームに遷移すること
