@rem $Id: Client.bat 5563 2007-11-08 09:23:07Z jacek.paszkowski $
@echo off
REM "Product GUID": [ProductGuid]

REM Adres WebServices np."http://zeus/Polkomtel-KalkulatorOfert/"
set KOURL=http://192.168.1.177/ko/

REM Adres Hermes np."http://kalkulator/hermes/"
set HERMESURL=http://kalkulator/hermes/

REM Sciezka instalacji programu, w przypadku wartosci innej niz
REM domyslna nalezy dodad zmienna jako parametr do instalatora MSI
set INSTALLDIR=""

REM Adres SMSGatewayService np. "http://localhost:9080/db2sms20/services/SMSGateway/",
REM w przypadku wartosci innej niz domyslna nalezy dodac
REM zmienna jako parametr do instalatora MSI
set DB2SMSURL=""

echo ===========================================================================
echo URL KalkulatorOfer.Web %KOURL% %HERMESURL%
echo ===========================================================================

[Client.msi] KOURL="%KOURL%" HERMESURL="%HERMESURL%"

