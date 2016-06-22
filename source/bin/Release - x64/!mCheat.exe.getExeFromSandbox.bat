taskkill /im mcheat.exe /t /f
call :sleep 2
copy "E:\Sandboxes\Programming\drive\D\Documents\My_Progs\mCheats\MemoryEditor_0.04\bin\Release\mCheat.exe" .\
start "" ".\mcheat.exe"








:sleep
timeout /t %1 /nobreak >nul
exit /b 0