# ✅ Inlämning 1 — Todo API & Klient

Detta projekt består av:  
- ⚙️ **Server:** Web API i **.NET 8** + **SQLite** + **JWT-autentisering**  
- 🌍 **Klient:** Byggd med **HTML, CSS och JavaScript**  

Följ guiden nedan för att köra igång projektet lokalt.  

---

## 🚀 Kom igång

### 1. Starta API-servern

📂 Gå in i server-projektet: 

cd server/TodoApi

📦 Återställ paket och skapa dev-certifikat:

dotnet restore

dotnet dev-certs https --trust

🔑 Sätt JWT-nyckeln (minst 32 tecken):

dotnet user-secrets init

dotnet user-secrets set "Jwt:Key" "<lång-stark-hemlighet-minst-32-tecken>"

✅ Verifiera att nyckeln sparats:
dotnet user-secrets list

▶️ Kör API:et: dotnet run

🌐 API:et startar på:

https://localhost:7088
http://localhost:5088

👤 När databasen skapas första gången seedas ett administratörskonto:

Användarnamn: admin
Lösenord: Admin!12345

2. Starta klienten
📂 Öppna en ny terminal och gå till klientmappen:

cd client

🌍 Starta en enkel webbserver:

(Högerklicka på index.html → Open with Live Server)
