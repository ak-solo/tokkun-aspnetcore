# コーディング規約・スターターコードの方針

## コーディング規約（教材内コードの統一ルール）

- C# は PascalCase（クラス・メソッド・プロパティ）、camelCase（ローカル変数）
- SQL は予約語を大文字（tokkun-sql に準拠）
- Razor ビューは Bootstrap 5 で最低限のスタイルのみ。CSS は深く触れない
- バリデーションは DataAnnotations を使う（ch04 以降）
- 非同期（`async/await`）は ch03 以降で導入し、それ以前は同期処理
- エラーハンドリングは最低限（try-catch は扱わない。入力検証のみ）

## スターターコードの方針

- `src/` 以下に動作する状態の ASP.NET Core MVC アプリを用意する
- DB マイグレーション・シードデータは `db/` 以下に SQL ファイルとして置く
- 学習者が触るファイルは `Controllers/` `Views/` `Models/` のみ（`Program.cs` 等は触らない）
- フェーズ 1 の各章ごとにスターターブランチ or タグを切る（`ch01-start` 等）
