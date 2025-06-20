# Redfish Server

Redfish シミュレータを用意しました。
RedfishViewer の README の作成にも利用しています。

## 起動

pip が使えるようにします。

```bash
$ sudo apt install python3-pip
```

FastAPI を公式ページの通りにインストールします。

```bash
$ pip3 install fastapi
$ pip3 install "uvicorn[standard]"
```

Redfish シミュレータを起動します。

```bash
$ uvicorn main:app --reload
INFO:     Will watch for changes in these directories: ['/home/tabito/RedfishServer']
INFO:     Uvicorn running on http://127.0.0.1:8000 (Press CTRL+C to quit)
INFO:     Started reloader process [554] using WatchFiles
INFO:     Started server process [556]
INFO:     Waiting for application startup.
INFO:     Application startup complete.
```

RedfishViewer に `http://localhost:8000/redfish/v1` を入力します。
