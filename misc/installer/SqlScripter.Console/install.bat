@rem $Id: install.bat 9237 2008-08-05 09:28:40Z wooyek $
@echo off
REM "Product GUID": [ProductGuid]

REM Prze³¹cznik "/l*v .\[Client.msi].log" pozwala na utworzenie logu instalacji,
REM w przypadku problemow pozwala je dokladnie zdiagnozowac.

REM ===========================================================================
REM Parametr definiuje nazwê pliku instalacyjnego 
set NAME=[Client.msi]

echo ===========================================================================
echo %NAME% 
echo ===========================================================================

msiexec /i %NAME% /l*v .\%NAME%.log 