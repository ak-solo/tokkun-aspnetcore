# 第6章 絞り込み・ソート

## このチャプターで学ぶこと

- クエリパラメータ（`?keyword=田中`）をコントローラーで受け取る方法
- 動的な WHERE 句の組み立て方（Dapper の `DynamicParameters`）
- 動的な ORDER BY の実装とセキュリティ上の注意点
- ビューにソートリンクを追加して昇降順を切り替える方法

---

## 基礎知識

### クエリパラメータとは

URL の `?` 以降の部分を**クエリパラメータ**（クエリ文字列）といいます。

```
/Employee?keyword=田中&sortBy=salary&sortDir=desc
         ↑                ↑              ↑
    区切り文字         パラメータ名=値   複数は & でつなぐ
```

ブラウザのフォームで `method="get"` を使うと、入力値が自動的にクエリパラメータとして URL に付加されます。

---

### コントローラーでクエリパラメータを受け取る

アクションメソッドの引数名をクエリパラメータ名と一致させるだけで、ASP.NET Core が自動的に値をバインドします。

```csharp
// /Employee?keyword=田中 にアクセスすると keyword = "田中" になる
public IActionResult Index(string? keyword)
{
    // keyword を使って絞り込む
}
```

パラメータが省略された場合は `null` になります（`?` をつけて nullable 型にするのはそのため）。

---

### 動的な WHERE 句の組み立て

「検索ワードが入力されたときだけ WHERE 句に条件を追加する」という処理は、SQL 文字列を条件に応じて組み立てることで実現します。

```csharp
var sql = @"
    SELECT id, name, dept_id, salary, hire_date, manager_id
    FROM employees
    WHERE 1=1
";

var parameters = new DynamicParameters();

if (!string.IsNullOrWhiteSpace(keyword))
{
    sql += " AND name LIKE @keyword";
    parameters.Add("keyword", $"%{keyword}%");
}

var employees = _db.Query<Employee>(sql, parameters);
```

**ポイント：**

- `WHERE 1=1` は「常に真」の条件です。これを起点にすることで、後から `AND ...` を安全に追記できます
- `DynamicParameters` は条件に応じてパラメータを動的に追加できる Dapper のクラスです
- `$"%{keyword}%"` は SQL の部分一致検索（LIKE）に必要なワイルドカードを付加しています
- SQL インジェクションを防ぐため、条件の値は必ずパラメータ（`@keyword`）を使って渡してください

> **tokkun-sql との接続**
> `WHERE name LIKE '%田中%'` という構文は tokkun-sql で学んだものとまったく同じです。
> 違いは、検索ワードを SQL 文字列に直接埋め込むのではなく、`@keyword` というプレースホルダーで渡す点です。

---

### 動的な ORDER BY の実装

ORDER BY の列名は **パラメータで渡せません**。SQL の仕様上、`ORDER BY @col` のような書き方はできないためです。
代わりに、C# 側で「使ってよい列名の一覧」をホワイトリストとして定義し、その中から選ぶ方法を使います。

```csharp
public IActionResult Index(string? keyword, string? sortBy, string? sortDir)
{
    var validColumns = new[] { "name", "salary", "hire_date" };
    var column = validColumns.Contains(sortBy) ? sortBy : "hire_date";
    var direction = sortDir == "asc" ? "ASC" : "DESC";

    var sql = $@"
        SELECT id, name, dept_id, salary, hire_date, manager_id
        FROM employees
        WHERE 1=1
        ORDER BY {column} {direction}
    ";
    // ...
}
```

**ポイント：**

- `validColumns.Contains(sortBy)` で「ホワイトリストに含まれる列名かどうか」を確認します
- 含まれていない場合はデフォルト値（ここでは `"hire_date"`）を使います
- 方向（`ASC` / `DESC`）も同様に、`"asc"` のときだけ `ASC`、それ以外は `DESC` と固定します

---

### ビューにソートリンクを追加する

現在のソート状態を ViewData に保存しておき、ビューでリンクを生成します。

```csharp
// コントローラー
ViewData["SortBy"]  = column;
ViewData["SortDir"] = direction.ToLower();
```

```html
<!-- ビュー：氏名列のヘッダーをクリックするとソートが切り替わる -->
@{
    var currentSortBy  = ViewData["SortBy"]  as string;
    var currentSortDir = ViewData["SortDir"] as string;
    var nameSortDir    = (currentSortBy == "name" && currentSortDir == "asc") ? "desc" : "asc";
}

<th>
    <a asp-action="Index"
       asp-route-keyword="@ViewData["Keyword"]"
       asp-route-sortBy="name"
       asp-route-sortDir="@nameSortDir">氏名</a>
</th>
```

**ポイント：**

- `asp-route-*` を使うと、任意のクエリパラメータを URL に付加できます
- 現在「氏名で昇順ソート中」なら次のクリックで「降順」に切り替わるよう、`nameSortDir` を計算しています
- 既存の検索キーワード（`keyword`）も一緒に引き継ぐことで、絞り込み状態を保ったままソートできます

---

## スターターコードの確認

`dotnet run` でアプリを起動し、`/Employee` にアクセスします。
一覧の上部に検索フォームが表示されています。

```
社員一覧
────────────────────────────────
[_________________] [検索]

[新規登録]

ID  氏名  部署ID  給与  入社日  上司ID
────────────────────────────────
```

現時点では検索フォームに氏名を入力して「検索」しても、絞り込みは動作しません（全件が表示されたままになります）。

この章の練習問題では：
- `Controllers/EmployeeController.cs` — 動的 WHERE と ORDER BY を実装する
- `Views/Employee/Index.cshtml` — フォームにフィールドを追加し、ソートリンクを設置する

---

## 練習問題

### 問題 6-1: 名前で部分一致検索する

検索フォームに入力したキーワードで社員名を絞り込む機能を実装してください。
キーワードが空のときは全件を表示してください。

**編集するファイル：**
- `Controllers/EmployeeController.cs`

**ヒント：**

現在の `Index` アクションは `keyword` 引数を受け取っていますが、SQL には使っていません。
`DynamicParameters` を使って動的に WHERE 句を追加してください。

```csharp
public IActionResult Index(string? keyword)
{
    var sql = @"
        SELECT id, name, dept_id, salary, hire_date, manager_id
        FROM employees
        WHERE 1=1
    ";

    var parameters = new DynamicParameters();

    if (!string.IsNullOrWhiteSpace(keyword))
    {
        sql += " AND name LIKE @keyword";
        parameters.Add("keyword", $"%{keyword}%");
    }

    sql += " ORDER BY hire_date DESC";

    ViewData["Keyword"] = keyword;
    var employees = _db.Query<Employee>(sql, parameters);
    return View(employees);
}
```

**確認ポイント：**
- 「田中」と入力して検索すると、氏名に「田中」を含む社員のみ表示されること
- 検索フォームをクリアして検索すると全件が表示されること
- 検索後に URL が `/Employee?keyword=田中` のようになること

---

### 問題 6-2: 部署 ID でフィルタする

部署 ID を指定して絞り込む機能を追加してください。
名前の絞り込みと組み合わせて使えるようにしてください。

**編集するファイル：**
- `Controllers/EmployeeController.cs`
- `Views/Employee/Index.cshtml`

**ヒント（コントローラー）：**

`deptId` パラメータを引数に追加し、値があれば WHERE 条件を追加します。

```csharp
public IActionResult Index(string? keyword, int? deptId)
{
    // ...

    if (deptId.HasValue)
    {
        sql += " AND dept_id = @deptId";
        parameters.Add("deptId", deptId.Value);
    }

    // ...
    ViewData["DeptId"] = deptId;
    // ...
}
```

**ヒント（ビュー）：**

フォームに部署 ID の入力欄を追加します。

```html
<form method="get" class="mb-3">
    <div class="d-flex gap-2" style="max-width: 600px;">
        <input type="text" name="keyword" value="@ViewData["Keyword"]"
               class="form-control" placeholder="氏名で検索..." />
        <input type="number" name="deptId" value="@ViewData["DeptId"]"
               class="form-control" style="max-width: 100px;" placeholder="部署ID" />
        <button type="submit" class="btn btn-outline-secondary">検索</button>
    </div>
</form>
```

**確認ポイント：**
- 部署 ID に `1` を入力して検索すると、`dept_id = 1` の社員のみ表示されること
- 氏名と部署 ID を両方入力すると、両方の条件で絞り込まれること
- どちらか一方だけ入力した場合も正しく動作すること

---

### 問題 6-3: 給与の高い順にソートする

一覧を給与の高い順に並び替えるソート機能を追加してください。
`?sortBy=salary` のクエリパラメータでソート列を切り替えられるようにしてください。

**編集するファイル：**
- `Controllers/EmployeeController.cs`

**ヒント：**

`sortBy` 引数を追加し、ホワイトリストで検証してから ORDER BY に使います。

```csharp
public IActionResult Index(string? keyword, int? deptId, string? sortBy)
{
    var validColumns = new[] { "name", "salary", "hire_date" };
    var column = validColumns.Contains(sortBy) ? sortBy : "hire_date";

    // ... WHERE 句の組み立て ...

    sql += $" ORDER BY {column} DESC";

    ViewData["SortBy"] = column;
    // ...
}
```

**確認ポイント：**
- `/Employee?sortBy=salary` にアクセスすると給与の高い順に並ぶこと
- `/Employee?sortBy=name` にアクセスすると名前の降順に並ぶこと
- 不正な値（`/Employee?sortBy=DROP TABLE` 等）では `hire_date` でソートされること

---

### 問題 6-4: ソートの昇降順を切り替えるリンクを追加する

列ヘッダーをクリックすると昇順・降順を切り替えられるリンクを追加してください。
「氏名」「給与」「入社日」の 3 列に対応してください。

**編集するファイル：**
- `Controllers/EmployeeController.cs`
- `Views/Employee/Index.cshtml`

**ヒント（コントローラー）：**

`sortDir` 引数を追加します。

```csharp
public IActionResult Index(string? keyword, int? deptId, string? sortBy, string? sortDir)
{
    var validColumns = new[] { "name", "salary", "hire_date" };
    var column    = validColumns.Contains(sortBy) ? sortBy : "hire_date";
    var direction = sortDir == "asc" ? "ASC" : "DESC";

    // ...
    sql += $" ORDER BY {column} {direction}";

    ViewData["SortBy"]  = column;
    ViewData["SortDir"] = direction.ToLower();
    // ...
}
```

**ヒント（ビュー）：**

各列ヘッダーにリンクを追加します。現在のソート列と同じ列をクリックしたら方向を反転させます。

```html
@{
    var currentSortBy  = ViewData["SortBy"]  as string;
    var currentSortDir = ViewData["SortDir"] as string;

    string NextDir(string col) =>
        (currentSortBy == col && currentSortDir == "asc") ? "desc" : "asc";
}

<th>
    <a asp-action="Index"
       asp-route-keyword="@ViewData["Keyword"]"
       asp-route-deptId="@ViewData["DeptId"]"
       asp-route-sortBy="name"
       asp-route-sortDir="@NextDir("name")">氏名</a>
</th>
```

「給与」列と「入社日」列のヘッダーにも同様のリンクを追加してください。

**確認ポイント：**
- 「氏名」ヘッダーをクリックするたびに昇順・降順が切り替わること
- ソート後も検索キーワード・部署 ID が URL に引き継がれること
- 現在ソートしている列以外をクリックするとその列の降順になること

---

### 問題 6-5: 検索結果の件数を表示する

一覧の上部に「X 件見つかりました」という件数を表示してください。
検索していない場合は「全 X 件」と表示してください。

**編集するファイル：**
- `Views/Employee/Index.cshtml`

**ヒント：**

`Model` は `IEnumerable<Employee>` なので、`Model.Count()` で件数を取得できます。
LINQ を使うには `@using System.Linq` が必要です（Razor では `@using` ディレクティブで追加します）。

```html
@{
    var count   = Model.Count();
    var keyword = ViewData["Keyword"] as string;
    var label   = string.IsNullOrWhiteSpace(keyword) ? $"全 {count} 件" : $"{count} 件見つかりました";
}
<p class="text-muted">@label</p>
```

**確認ポイント：**
- 検索なしの場合「全 XX 件」と表示されること
- キーワードで絞り込んだ場合「X 件見つかりました」と表示されること
- 0 件の場合「0 件見つかりました」と表示されること
