FROM mcr.microsoft.com/devcontainers/dotnet:1-8.0

# PostgreSQL クライアントツールをインストール
RUN apt-get update && apt-get install -y \
    postgresql-client \
    && rm -rf /var/lib/apt/lists/*

# .NET ツールのパスを通す
ENV PATH="${PATH}:/home/vscode/.dotnet/tools"
