@echo off
setlocal

:: ������ȷ��·��������LocalLow�ļ��У�
set "TARGET_FILE=%USERPROFILE%\AppData\LocalLow\z\DesktopBalloon_BuildIn\saveData.sd"

:: ����ļ��Ƿ����
if not exist "%TARGET_FILE%" (
    echo �ļ� "%TARGET_FILE%" �����ڣ�����ɾ����
    goto :END
)

:: ��ʾ�û�ȷ�ϣ���ѡ��
echo ���棺����ɾ���ļ� "%TARGET_FILE%"���˲������ɳ�����
choice /C YN /M "�Ƿ������(Y/N)"
if errorlevel 2 (
    echo ������ȡ����
    goto :END
)

:: ɾ���ļ���/Q ������ʾ����ģʽ������ʾȷ�ϣ�
del /Q /F "%TARGET_FILE%"

if exist "%TARGET_FILE%" (
    echo ɾ��ʧ�ܣ�����ȱ��Ȩ�޻��ļ���ռ�á�
    echo �볢���Թ���Ա������д˽ű���
) else (
    echo �ɹ�ɾ���ļ� "%TARGET_FILE%"��
)

:END
endlocal
pause