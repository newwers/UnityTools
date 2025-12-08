@echo off
chcp 936 >nul
:start
cls
echo 1. 设置定时关机
echo 2. 取消定时关机
echo.
choice /c 12 /n /m "请按1或2: "

if errorlevel 2 goto cancel
if errorlevel 1 goto shutdown

:shutdown
cls
echo 已选择：设置定时关机
echo.
:input_minutes
set /p minutes=输入分钟数: 
if not defined minutes goto input_minutes
if %minutes% lss 1 goto input_minutes
set /a seconds=%minutes%*60
echo 将在%minutes%分钟（即%seconds%秒）后关机
shutdown -s -f -t %seconds%
pause
exit

:cancel
cls
shutdown -a
echo 已取消关机计划
pause
exit