@echo off
REM "Product GUID": [ProductGuid]

REM Prze��cznik "/l*v .\[Client.msi].log" pozwala na utworzenie logu instalacji,
REM w przypadku problemow pozwala je dokladnie zdiagnozowac.

REM ===========================================================================
REM Parametr definiuje nazw� pliku instalacyjnego dla aplikacji WWW
set NAME=[Client.msi]

REM ===========================================================================

echo ===========================================================================
echo %NAME%
echo ===========================================================================

msiexec /x %NAME% /l*v .\%NAME%.log