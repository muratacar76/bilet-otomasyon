@echo off
cd /d "D:\MuratAcar\MuratAcar\BiletOtomasyonKiro"
set GIT="D:\MuratAcar\MuratAcar\Git\bin\git.exe"

echo Initializing Git repository...
%GIT% init

echo Adding remote...
%GIT% remote add origin https://github.com/muratacar76/bilet-otomasyon.git

echo Adding all files...
%GIT% add .

echo Committing...
%GIT% commit -m "feat: Kapsamli koltuk secimi ve rezervasyon sistemi"

echo Pushing to GitHub...
%GIT% branch -M main
%GIT% push -u origin main

echo Done!
pause
