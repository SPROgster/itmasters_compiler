mono Gplex.exe /unicode SimpleLex.lex
mono Gppg.exe /no-lines /gplex SimpleYacc.y
mv -f SimpleLex.cs "Parsers/SimpleLex.cs"
mv -f SimpleYacc.cs "Parsers/SimpleYacc.cs"