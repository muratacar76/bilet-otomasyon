@echo off
echo ========================================
echo Ucak Bileti Rezervasyon Sistemi
echo Tum Servisleri Baslat
echo ========================================
echo.
echo Backend baslat iliyor...
start cmd /k "cd src\FlightBooking.API && dotnet run"
timeout /t 5
echo.
echo Frontend baslatiliyor...
start cmd /k "cd client && npm run dev"
echo.
echo Tum servisler baslatildi!
echo Backend: http://localhost:5000
echo Frontend: http://localhost:3000
echo.
pause
