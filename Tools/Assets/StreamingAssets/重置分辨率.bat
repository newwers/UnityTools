@echo off
title ɾ��ע�����
color 0A

echo ==============================
echo      ��ʼɾ��ע�����
echo ==============================
echo.

:: ����Ҫɾ����ע���·��
set "regPath=HKEY_CURRENT_USER\Software\Z\DesktopBalloon_BuildIn"

:: ���ע������Ƿ����
reg query "%regPath%" >nul 2>nul
if %errorLevel% neq 0 (
    echo ע����� "%regPath%" �����ڣ�����ɾ����
) else (
    echo ����ɾ��ע����� "%regPath%"...
    
    :: ɾ��ע����/f ������ʾǿ��ɾ��������ȷ�ϣ�
    reg delete "%regPath%" /f
    
    :: ���ɾ�����
    reg query "%regPath%" >nul 2>nul
    if %errorLevel% neq 0 (
        echo ע�����ɾ���ɹ���
    ) else (
        echo ע�����ɾ��ʧ�ܣ�����Ȩ�޻��ֶ�ɾ����
    )
)

echo.
echo ==============================
echo      ������ɣ���������˳�
echo ==============================
pause >nul