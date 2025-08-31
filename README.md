## 🚀 Starta API (servern)

1. Gå in i server-projektet:
   ```bash
   cd server/TodoApi

Återställ paket och skapa dev-certifikat:

dotnet restore
dotnet dev-certs https --trust

Sätt JWT-nyckeln (krävs minst 32 tecken):

dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "<lång-stark-hemlighet-minst-32-tecken>"

Kör API:et:

dotnet run


API:et startar på:

https://localhost:7088

http://localhost:5088

När databasen skapas första gången seedas ett administratörskonto automatiskt:

Användarnamn: admin

Lösenord: Admin!12345
