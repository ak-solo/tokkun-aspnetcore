# 第5章 削除

## このチャプターで学ぶこと

- DELETE の実装（Dapper での `Execute` 活用）
- 削除確認を挟む理由（GET で削除してはいけない理由）
- 削除後に PRG パターンで一覧にリダイレクトする方法
- 一覧画面に「削除」ボタンを追加する方法

---

## 基礎知識

### なぜ GET で削除してはいけないのか

「`/Employee/Delete/3` という URL にアクセスするだけで削除できる」という設計は危険です。
その理由を整理します。

| 問題 | 説明 |
|------|------|
| ブックマーク・共有 | URL を保存・共有するだけで意図せず削除が起きる |
| ブラウザのリロード | 削除後に F5 を押すと同じ DELETE が再実行される |
| リンクのプリフェッチ | ブラウザや検索エンジンがリンクをたどるだけで削除される |

**HTTP の規約**として、GET リクエストは「データを取得するだけで副作用がない」操作に使います。
レコードの削除のようにデータを変更する操作は **POST（または DELETE）リクエスト** で行うのが正しい設計です。

---

### 削除確認フローの設計

安全な削除フローは次の 3 ステップです。

```
① GET /Employee/Delete/3
   → 削除確認画面を表示（削除はまだしない）

② 確認画面で「削除」ボタンをクリック
   → POST /Employee/Delete（フォーム送信）

③ POST アクション
   → DELETE を実行して一覧にリダイレクト（PRG パターン）
```

これで「意図しない削除」と「ブラウザのリロードで再削除」の両方を防げます。

---

### Dapper で DELETE を実行する

DELETE は UPDATE と同様、`Execute()` を使います。

```csharp
// Controllers/EmployeeController.cs
[HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
public IActionResult DeleteConfirmed(int id)
{
    _db.Execute("DELETE FROM employees WHERE id = @id", new { id });
    return RedirectToAction(nameof(Index));
}
```

**ポイント：**

- `[HttpPost, ActionName("Delete")]` — HTTP POST のみ受け付け、URL は `/Employee/Delete` のままにする
- `[ValidateAntiForgeryToken]` — CSRF 攻撃を防ぐ（フォームの `__RequestVerificationToken` と照合）
- `WHERE id = @id` — 必ず ID で絞り込む。書き忘れると全件削除になるので注意

> **tokkun-sql との接続**
> DELETE 文の構文は tokkun-sql で学んだものとまったく同じです。
> `WHERE` 句を忘れない点も同じです。

---

### なぜ `[HttpPost, ActionName("Delete")]` と書くのか

GET と POST で同じ名前のアクション（`Delete`）を持たせたいときに使います。

C# では同じシグネチャのメソッドは定義できません。
そこで POST 用のメソッドは `DeleteConfirmed` という名前にしつつ、
`ActionName("Delete")` で URL とフォームの `asp-action="Delete"` に対応させます。

```csharp
// GET: /Employee/Delete/3 → 確認画面
[HttpGet]
public IActionResult Delete(int id) { ... }

// POST: /Employee/Delete → 削除実行
[HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
public IActionResult DeleteConfirmed(int id) { ... }
```

---

### PRG パターンで一覧にリダイレクトする

削除後は一覧にリダイレクトします。こうすることで、ブラウザのリロードで削除が再実行されるのを防げます（ch03 で学んだ PRG パターンと同じです）。

```csharp
return RedirectToAction(nameof(Index));
```

---

## スターターコードの確認

`dotnet run` でアプリを起動し、社員一覧から任意の社員の「詳細」をクリックします。
詳細画面に「削除」リンクがあり、クリックすると削除確認画面が表示されます。

```
社員削除
────────────────────────────────
以下の社員を削除しますか？

ID    3
氏名  鈴木花子

[削除]  [キャンセル]
────────────────────────────────
```

現時点では「削除」ボタンをクリックすると **405 Method Not Allowed** エラーになります。
POST を受け取るアクションがまだ実装されていないためです。

この章の練習問題では：
- `Views/Employee/Delete.cshtml` — 確認画面の表示内容を充実させる
- `Controllers/EmployeeController.cs` — POST アクションを実装する
- `Views/Employee/Index.cshtml` — 一覧に「削除」ボタンを追加する

---

## 練習問題

### 問題 5-1: 削除確認画面に情報を追加する

削除確認画面に現在は「ID」と「氏名」しか表示されていません。
誰が見ても間違いなく削除対象とわかるよう、「給与」と「入社日」も表示してください。

**編集するファイル：**
- `Views/Employee/Delete.cshtml`

**ヒント：**

`<dl class="row">` の中に `<dt>` / `<dd>` のペアを追加します。

```html
<dt class="col-sm-2">給与</dt>
<dd class="col-sm-10">@Model.Salary</dd>

<dt class="col-sm-2">入社日</dt>
<dd class="col-sm-10">@Model.HireDate</dd>
```

**確認ポイント：**
- 削除確認画面に給与と入社日が表示されること

---

### 問題 5-2: DELETE を実装する

「削除」ボタンをクリックしたとき、DB からレコードを削除する処理を実装してください。
削除後は一覧画面にリダイレクトしてください。

**編集するファイル：**
- `Controllers/EmployeeController.cs`

**ヒント：**

`[HttpPost, ActionName("Delete")]` アクションを追加します。

```csharp
[HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
public IActionResult DeleteConfirmed(int id)
{
    _db.Execute("DELETE FROM employees WHERE id = @id", new { id });
    return RedirectToAction(nameof(Index));
}
```

**確認ポイント：**
- 「削除」ボタンをクリックすると一覧画面に移動すること
- 削除した社員が一覧に表示されなくなること
- ブラウザのリロードをしても再削除されないこと

---

### 問題 5-3: 削除後のリダイレクト先を変更する

問題 5-2 では削除後に一覧画面へリダイレクトしました。
次の要件を満たすよう変更してください。

- 削除が成功した場合は一覧画面へリダイレクトする（変更なし）
- 指定した ID の社員が存在しない場合は 404 を返す

**編集するファイル：**
- `Controllers/EmployeeController.cs`

**ヒント：**

削除前に対象レコードの存在確認を行います。

```csharp
[HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
public IActionResult DeleteConfirmed(int id)
{
    var exists = _db.QueryFirstOrDefault<Employee>(
        "SELECT id FROM employees WHERE id = @id", new { id });
    if (exists == null) return NotFound();

    _db.Execute("DELETE FROM employees WHERE id = @id", new { id });
    return RedirectToAction(nameof(Index));
}
```

**確認ポイント：**
- 通常の削除は引き続き動作すること
- 存在しない ID（例：`/Employee/Delete/9999`）に POST すると 404 になること

---

### 問題 5-4: 一覧に「削除」ボタンを追加する

現在の一覧画面には「詳細」リンクしかありません。
各行に「削除」ボタンを追加してください。ただし、確認を省かずに削除確認画面を経由してください。

**編集するファイル：**
- `Views/Employee/Index.cshtml`

**ヒント：**

リンクは削除確認画面（GET）に遷移するだけで構いません。
実際の削除は確認画面のフォームが担います。

```html
<td>
    <a asp-action="Details" asp-route-id="@employee.Id">詳細</a>
    <a asp-action="Delete" asp-route-id="@employee.Id" class="text-danger">削除</a>
</td>
```

**確認ポイント：**
- 一覧の各行に「削除」リンクが表示されること
- リンクをクリックすると削除確認画面に遷移すること
- 削除確認画面で「削除」をクリックすると一覧に戻り、対象行が消えること

---

### 問題 5-5: 詳細画面に「削除」リンクを追加する

詳細画面にも削除確認画面への「削除」リンクを追加してください。

**編集するファイル：**
- `Views/Employee/Details.cshtml`

**ヒント：**

```html
<a asp-action="Delete" asp-route-id="@Model.Id" class="btn btn-danger">削除</a>
```

**確認ポイント：**
- 詳細画面に「削除」ボタンが表示されること
- クリックすると削除確認画面に遷移すること
