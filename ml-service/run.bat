@echo off
chcp 65001 >nul
python -m pip install -r requirements.txt
python -m uvicorn main:app --host 127.0.0.1 --port 8001 --reload
pause
