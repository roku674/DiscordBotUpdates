set mypath=%cd%

if not exist %mypath%\Extensions mkdir /d %mypath%\Extenisons

cd Extensions
git.exe clone https://github.com/roku674/Algorithms-for-C-Sharp
git.exe clone https://github.com/roku674/Excel-C-Sharp
git.exe clone https://github.com/roku674/StarportHelperClasses

cd %mypath%
cd Objects
git.exe clone https://github.com/roku674/StarportObjects

@pause

