FROM mcr.microsoft.com/devcontainers/dotnet:1-10.0

# ベースイメージに含まれる Yarn リポジトリの GPG キーが失効しているため削除
RUN rm -f /etc/apt/sources.list.d/yarn.list

# PostgreSQL クライアントツールをインストール
RUN apt-get update && apt-get install -y \
    postgresql-client \
    && rm -rf /var/lib/apt/lists/*

# .NET ツールのパスを通す
ENV PATH="${PATH}:/home/vscode/.dotnet/tools"
