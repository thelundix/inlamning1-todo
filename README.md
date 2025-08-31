# ✅ Inlämning 1 — Todo API & Klient

Detta projekt innehåller en server (Web API i .NET 8 + SQLite + JWT) och en klient (HTML, CSS, JavaScript). Följ stegen nedan för att köra igång allt lokalt.

---

## 🚀 Starta projektet

Gå in i server-projektet:
```bash
cd server/TodoApi
Återställ paket och skapa dev-certifikat:

bash
Kopiera kod
dotnet restore
dotnet dev-certs https --trust
Sätt JWT-nyckeln (krävs minst 32 tecken):

bash
Kopiera kod
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "<lång-stark-hemlighet-minst-32-tecken>"
Kör API:et:

bash
Kopiera kod
dotnet run
API:et startar på:

https://localhost:7088

http://localhost:5088

När databasen skapas första gången seedas ett administratörskonto automatiskt:

Användarnamn: admin

Lösenord: Admin!12345

Öppna en ny terminal, gå till klientmappen och starta en enkel webbserver:

bash
Kopiera kod
cd client
npx http-server -p 5500
Alternativt kan du använda VS Code Live Server (högerklicka på index.html → Open with Live Server).
