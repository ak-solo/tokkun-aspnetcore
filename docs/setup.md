# 環境構築手順

この手順では、学習環境のセットアップを行います。

---

## 前提条件

以下がインストール済みであることを確認してください。

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)（Windows / Mac）または Docker Engine（Linux）
- [Visual Studio Code](https://code.visualstudio.com/)
- VS Code 拡張機能：[Dev Containers](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)

---

## 手順 1：リポジトリをクローンする

```bash
git clone <リポジトリのURL>
cd tokkun-asp-dotnet
```

---

## 手順 2：Dev Container を起動する

1. VS Code でリポジトリフォルダを開く
2. 右下に「**Reopen in Container**」の通知が表示されたらクリック
   - 表示されない場合は `F1` キーを押してコマンドパレットを開き、「**Dev Containers: Reopen in Container**」を選択
3. 初回はコンテナのビルドに数分かかります。完了するまで待ってください

---

## 手順 3：PostgreSQL にデータベースとシードデータを投入する

コンテナ起動後、VS Code のターミナル（`` Ctrl + ` ``）を開いて以下を実行します。

```bash
# データベースを作成する
createdb -h db -U postgres employeeapp

# テーブルを作成する
psql -h db -U postgres -d employeeapp -f db/00_schema.sql

# シードデータを投入する
psql -h db -U postgres -d employeeapp -f db/01_seed.sql
```

> **PostgreSQL の接続情報**
> | 項目 | 値 |
> |------|-----|
> | ホスト | `db` |
> | ポート | `5432` |
> | ユーザー | `postgres` |
> | パスワード | `postgres` |
> | データベース | `employeeapp` |

---

## 手順 4：アプリケーションを起動する

```bash
cd src/EmployeeApp
dotnet run
```

ターミナルに以下のような表示が出れば起動成功です。

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

ブラウザで [http://localhost:5000](http://localhost:5000) を開き、社員一覧が表示されることを確認してください。

---

## トラブルシューティング

### `createdb: error: connection to server on socket failed`

PostgreSQL コンテナがまだ起動していない可能性があります。少し待ってから再試行してください。

### `dotnet run` でエラーが出る

依存関係の復元を試してください。

```bash
dotnet restore
```

### ポート 5000 がすでに使われている

他のプロセスがポートを占有しています。以下で別のポートを指定して起動できます。

```bash
dotnet run --urls "http://localhost:5001"
```

---

## 次のステップ

環境の準備ができたら [chapter00.md](chapter00.md) から学習を始めましょう。
