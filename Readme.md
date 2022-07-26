## Proof-of-concept бот для выдачи инвайтов на основе файла о регистрации дду в росреестре.

Нет проверок на повторное использование, код для простоты скинут в один большой метод.
Проверка "росреестровости" осуществляется только по CN в подписи без проверки корневого сертификата.

В файл `config.json` прописать токен бота и id чата, для которого будут выданы инвайты.

В целевой чат данный бот должен быть доавлен как админ. Регистрация и получения токена бота - через `https://t.me/BotFather` телеграмма.

Как получить id чата после добавления бота тут - https://stackoverflow.com/a/32572159/11992513

## Сборка и запуск:

### 1. Устанавливаем dotnet 6:
https://dotnet.microsoft.com/en-us/download/dotnet/6.0

### 2. Выполняем
```cmd
dotnet run
```

### 3. Ждём сообщения вида
```cmd
 Start listening for @имя_бота
```
