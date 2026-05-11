# 第8章 新画面作成（CRUD完成）

## このチャプターで学ぶこと

- 新規登録フォームを一から作成する（GET/POST・INSERT・`RETURNING id`）
- 編集フォームで既存データを表示し更新する（UPDATE）
- 削除確認画面を経由して安全に削除する（DELETE）
- `DataAnnotations` でバリデーションを追加する
- 日付の大小関係など、属性だけでは表現しにくい入力チェックをコントローラーで実装する

---

## フェーズ 2 後半へ

ch07 ではプロジェクトの一覧・詳細画面を作成しました。
ch08 では登録・編集・削除を追加して、完全な CRUD を完成させてください。

ch07 で作成したファイルに加えて、次のファイルを新規作成・修正します。

```
src/EmployeeApp/
├── Controllers/
│   └── ProjectController.cs      ← Create / Edit / Delete アクションを追加
├── Models/
│   └── Project.cs                ← DataAnnotations を追加
└── Views/
    └── Project/
        ├── Index.cshtml          ← 「新規登録」ボタン・「編集」「削除」リンクを追加
        ├── Details.cshtml        ← 「編集」「削除」リンクを追加
        ├── Create.cshtml         ← 新規作成
        ├── Edit.cshtml           ← 新規作成
        └── Delete.cshtml         ← 新規作成
```

---

## 練習問題

> **問題の進め方：** 実装すべき内容を箇条書きで示します。どのように実装するかは自分で考えてください。
> 詰まったときは ch03・ch04・ch05 の `EmployeeController` と対応するビューを参考にしてください。

---

### 問題 8-1：プロジェクト新規登録画面を作成する

**URL：**

| メソッド | URL | 説明 |
|---|---|---|
| `GET` | `/projects/create` | 空の登録フォームを表示する |
| `POST` | `/projects/create` | 入力内容を受け取り DB に保存する |

**フォームの入力項目：**

| 項目 | バリデーション |
|---|---|
| プロジェクト名 | 必須 |
| 開始日 | 必須 |
| 終了日 | 空欄可。入力した場合は開始日より後の日付であること |
| 予算 | 必須。1 以上の整数であること |

**バリデーションのヒント：**

- プロジェクト名・予算の必須チェックと範囲チェックは `DataAnnotations` を `Models/Project.cs` に追加することで実装できます
- 終了日が開始日より前という条件は `DataAnnotations` の属性だけでは表現しにくいため、コントローラーの `ModelState.AddModelError` で追加することを推奨します

```csharp
// コントローラーでの複合バリデーション例
if (project.EndDate.HasValue && project.EndDate <= project.StartDate)
{
    ModelState.AddModelError(nameof(project.EndDate), "終了日は開始日より後の日付を入力してください");
}
if (!ModelState.IsValid)
{
    return View(project);
}
```

**使用する SQL（INSERT）：**

```sql
INSERT INTO projects (name, start_date, end_date, budget)
VALUES (@Name, @StartDate, @EndDate, @Budget)
RETURNING id
```

**リダイレクト先：** 登録完了後は詳細画面（`/projects/details/{id}`）へ遷移すること

**その他の要件：**

- 一覧画面（`Views/Project/Index.cshtml`）に「新規登録」ボタンを追加すること

**確認ポイント：**

- `/projects/create` にアクセスするとフォームが表示されること
- すべての項目を正しく入力して送信すると、DB に新しいレコードが追加されること
- 登録後、追加したプロジェクトの詳細画面に遷移すること
- プロジェクト名を空のまま送信するとエラーメッセージが表示されること
- 予算に 0 または負の数を入力するとエラーメッセージが表示されること
- 終了日に開始日より前の日付を入力するとエラーメッセージが表示されること

---

### 問題 8-2：プロジェクト編集画面を作成する

**URL：**

| メソッド | URL | 説明 |
|---|---|---|
| `GET` | `/projects/edit/{id}` | 既存データを入力済みの編集フォームを表示する |
| `POST` | `/projects/edit` | 更新内容を受け取り DB を更新する |

**フォームの入力項目：**

問題 8-1 の新規登録フォームと同じ項目・バリデーション条件を適用すること。

**使用する SQL：**

既存データの取得（GET）：

```sql
SELECT id, name, start_date, end_date, budget
FROM projects
WHERE id = @id
```

更新処理（POST）：

```sql
UPDATE projects
SET name = @Name, start_date = @StartDate, end_date = @EndDate, budget = @Budget
WHERE id = @Id
```

**リダイレクト先：** 更新完了後は詳細画面（`/projects/details/{id}`）へ遷移すること

**その他の要件：**

- 存在しない ID にアクセスした場合は `NotFound()` を返すこと
- 一覧画面（`Index.cshtml`）の各行に「編集」リンクを追加すること
- 詳細画面（`Details.cshtml`）に「編集」リンクを追加すること

**確認ポイント：**

- `/projects/edit/1` にアクセスすると「基幹システム刷新」の情報がフォームに表示されること
- 内容を変更して送信すると DB が更新されること
- 更新後、詳細画面に遷移すること
- バリデーションエラーが発生した場合、フォームに入力内容が保持されたままエラーメッセージが表示されること
- `/projects/edit/999` にアクセスすると 404 になること

---

### 問題 8-3：プロジェクト削除確認画面を作成する

**URL：**

| メソッド | URL | 説明 |
|---|---|---|
| `GET` | `/projects/delete/{id}` | 削除対象のプロジェクト情報と確認ボタンを表示する |
| `POST` | `/projects/delete` | 確認後に DB から削除する |

**削除確認画面の表示内容：**

| 項目 | 取得元 |
|---|---|
| プロジェクト名 | `projects.name` |
| 開始日 | `projects.start_date` |
| 終了日 | `projects.end_date`（`NULL` の場合は「未定」） |
| 予算 | `projects.budget` |

「削除する」ボタンと「キャンセル」リンク（詳細画面に戻る）を配置すること。

**使用する SQL：**

削除対象の取得（GET）：

```sql
SELECT id, name, start_date, end_date, budget
FROM projects
WHERE id = @id
```

削除処理（POST）：

```sql
DELETE FROM projects WHERE id = @id
```

**リダイレクト先：** 削除完了後は一覧画面（`/projects`）へ遷移すること

**その他の要件：**

- 存在しない ID にアクセスした場合は `NotFound()` を返すこと
- 一覧画面（`Index.cshtml`）の各行に「削除」リンクを追加すること
- 詳細画面（`Details.cshtml`）に「削除」リンクを追加すること

**確認ポイント：**

- `/projects/delete/1` にアクセスすると「基幹システム刷新」の削除確認画面が表示されること
- 「削除する」ボタンを押すと DB から該当レコードが削除されること
- 削除後、一覧画面に遷移し、削除したプロジェクトが表示されないこと
- 「キャンセル」リンクを押すと詳細画面に戻ること
- `/projects/delete/999` にアクセスすると 404 になること

---

## ヒント

### Project.cs に DataAnnotations を追加する

ch07 で作成した `Models/Project.cs` に属性を追加します。
`Employee.cs` の `[Required]` や `[Range]` と同じ書き方が使えます。

```csharp
using System.ComponentModel.DataAnnotations;

public class Project
{
    public int Id { get; set; }

    [Required(ErrorMessage = "プロジェクト名は必須です")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "開始日は必須です")]
    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "予算は 1 以上の整数を入力してください")]
    public int Budget { get; set; }

    // ステータスや MemberCount など ch07 で追加したプロパティはそのまま残す
}
```

> **注意：** `StartDate` が `DateOnly`（非 nullable）の場合、フォームで空欄のまま送信すると `0001-01-01` などのデフォルト値になることがあります。
> `[Required]` だけでは空欄を防げないケースがあるため、必要に応じてコントローラーで `StartDate == default` の追加チェックを行ってください。

### 日付の入力フォーム

Razor の `asp-for` を使うと、`DateOnly` 型のプロパティに対して `type="date"` のフォームが自動生成されます。

```html
<input asp-for="StartDate" type="date" class="form-control" />
<span asp-validation-for="StartDate" class="text-danger"></span>
```

### クライアントサイドバリデーション

`Views/Shared/_Layout.cshtml` にすでに jQuery Validation が含まれている場合は、フォームのビューの末尾に次のセクションを追加するとフォーム送信前にブラウザ上でエラーを表示できます。

```html
@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

### ビルドエラーの確認

新しいアクションやファイルを追加したら、実行前にビルドしてコンパイルエラーを確認してください。

```
dotnet build
```
