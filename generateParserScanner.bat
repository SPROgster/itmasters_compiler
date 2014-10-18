cls
gplex.exe /unicode SimpleLex.lex
gppg.exe /no-lines /gplex SimpleYacc.y
move /Y  SimpleLex.cs "Parsers/SimpleLex.cs"
move /Y SimpleYacc.cs "Parsers/SimpleYacc.cs"
pause