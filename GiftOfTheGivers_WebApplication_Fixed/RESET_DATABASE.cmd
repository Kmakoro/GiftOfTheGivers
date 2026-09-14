@echo off
setlocal
cd /d "%~dp0"
echo Resetting local application data...
for %%F in ("gift-of-the-givers.db" "gift-of-the-givers-app.db" "gift-of-the-givers-web.db") do (
  if exist %%F del /q %%F
)
echo Local database reset completed.
echo A fresh database and the required Employee and Donor accounts will be created on the next run.
pause
