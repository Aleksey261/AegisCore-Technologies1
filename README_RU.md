# AegisCore C# / ASP.NET Core

Это переписанная версия проекта AegisCore на C# и ASP.NET Core MVC.

## Что исправлено при переносе

В исходном Java-проекте были обнаружены типовые проблемы:

- конфликт пакетов `com.aegis.scanner.*` и `com.aegis.modules.scanner.*`;
- поле `value` в JPA-entity `ScanTarget` конфликтовало с H2 SQL;
- обязательная инъекция `XmlMapper` могла ломать запуск Spring Context;
- отсутствовал статический файл `/js/scanner.js`, из-за чего браузер получал JSON 404 вместо JavaScript;
- не все маршруты `/`, `/fstec`, `/scanner`, `/scanner/reports`, `/scanner/start` были явно закрыты контроллерами.

В C#-версии эти проблемы убраны архитектурно: namespace единый `AegisCore`, статические JS-файлы существуют, маршруты явно объявлены, вместо SQL-колонки `value` используются `TargetValue` и `ItemValue`.

## Основные функции

- ASP.NET Core MVC frontend;
- cookie-аутентификация;
- дашборд;
- модуль ФСТЭК;
- импорт XML/CSV БДУ/OVAL;
- локальный сканер ПК: система, диски, процессы, программы, сеть;
- TCP-проверка типовых портов, включая RDP, SMB, WinRM, Modbus/TCP, Siemens S7, OPC UA, EtherNet/IP;
- проверка результатов сканирования по уязвимостям, угрозам и признакам АСУ ТП;
- HTML-отчёт и простой PDF-отчёт;
- интеграция с отдельным Python FastAPI ML-сервисом.

## Запуск

Требуется .NET SDK 8.0 или новее.

```powershell
dotnet restore
dotnet run
```

Открыть:

```text
http://localhost:5000
```

Логин:

```text
admin / admin123
```

Если ASP.NET Core выберет другой порт, он будет указан в консоли после запуска.

## ML-сервис

```powershell
cd ml-service
python -m pip install -r requirements.txt
python -m uvicorn main:app --host 127.0.0.1 --port 8001 --reload
```

## Сборка portable EXE для Windows

```powershell
scripts\publish-windows.bat
```

Готовые файлы появятся в:

```text
dist\win-x64
```
http://localhost:5000/audit/ml-analysis/dashboard