@echo off
set current_dir=%~dp0
reg add HKCU\Software\Microsoft\Windows\CurrentVersion\Run /v PalCat /t REG_SZ /d %current_dir%�ӳ������ű�.bat
echo "���ÿ�������ӳ�������·��:" %current_dir%�ӳ������ű�.bat
pause
#reg add HKCU\Software\Microsoft\Windows\CurrentVersion\Run /v PalCat /t REG_SZ /d "D:\Work\UnityProjects\DesktopPet\PalCat\PalCat.exe"

