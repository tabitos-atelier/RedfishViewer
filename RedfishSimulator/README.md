# Redfish Simulator

Redfish シミュレータを用意しました。
RedfishViewer の README の作成にも利用しています。

## 起動

`uv` をインストールします。

```bash
$ curl -LsSf https://astral.sh/uv/install.sh | sh
```

仮想環境を作成します。

```bash
$ uv venv
Using CPython 3.x.x
Creating virtual environment at: .venv
Activate with: source .venv/bin/activate
```

仮想環境に入ります。

```bash
$ source .venv/bin/activate
```

FastAPI と uvicorn をインストールします。

```bash
$ uv pip install fastapi "uvicorn[standard]"
```

Redfish シミュレータを起動します。

```bash
$ uvicorn main:app --reload
INFO:     Will watch for changes in these directories: ['/home/tabito/RedfishSimulator']
INFO:     Uvicorn running on http://127.0.0.1:8000 (Press CTRL+C to quit)
INFO:     Started reloader process [554] using WatchFiles
INFO:     Started server process [556]
INFO:     Waiting for application startup.
INFO:     Application startup complete.
```

RedfishViewer に `http://localhost:8000/redfish/v1` を入力します。

## 2回目以降の起動

仮想環境への入り直しとシミュレータの起動だけで動作します。

```bash
$ source .venv/bin/activate
$ uvicorn main:app --reload
```
