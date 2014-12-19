%using SimpleParser;
%using QUT.Gppg;
%using System.Linq;

%namespace SimpleScanner

Alpha 	[a-zA-Z_]
Digit   [0-9] 
AlphaDigit {Alpha}|{Digit}
INTNUM  {Digit}+
FLOATNUM {INTNUM}\.{INTNUM}
ID {Alpha}{AlphaDigit}*
STRING \"([^"\\]*|(\\\\)+|\\[^\\])*\"

%%

"+" { return (int)Tokens.PLUS; }
"-" { return (int)Tokens.MINUS; }
"*" { return (int)Tokens.MULT; }
"/" { return (int)Tokens.DIV; }

"<" { return (int)Tokens.LESS; }
">" { return (int)Tokens.GREATER; }
"==" { return (int)Tokens.EQUAL; }
"!=" { return (int)Tokens.NEQUAL; }
"<=" { return (int)Tokens.LEQUAL; }
">=" { return (int)Tokens.GEQUAL; }

"(" { return (int)Tokens.LBR; }
")" { return (int)Tokens.RBR; }
":" { return (int)Tokens.COLON; }
"," { return (int)Tokens.COMMA; }

{INTNUM} { 
  yylval.iVal = int.Parse(yytext); 
  return (int)Tokens.INUM; 
}

{STRING} { 
  yylval.sVal = yytext; 
  return (int)Tokens.STRING; 
}

{FLOATNUM} { 
  NumberFormatInfo nfi = new NumberFormatInfo();
  nfi.NumberDecimalSeparator = ".";
  yylval.fVal = float.Parse(yytext, nfi); 
  return (int)Tokens.FNUM;
}

{ID}  { 
  int res = ScannerHelper.GetIDToken(yytext);
  if (res == (int)Tokens.ID)
	yylval.sVal = yytext;
  return res;
}

":=" { return (int)Tokens.ASSIGN; }
";" { return (int)Tokens.SEMICOLON; }

[^ \r\n\t] {
	LexError();
}

%{
  yylloc = new LexLocation(tokLin, tokCol, tokELin, tokECol);
%}

%%

public override void yyerror(string format, params object[] args) // обработка синтаксических ошибок
{
  var ww = args.Skip(1).Cast<string>().ToArray();
  string errorMsg = string.Format("({0},{1}): Встречено {2}, а ожидалось {3}", yyline, yycol, args[0], string.Join(" или ", ww));
  throw new SyntaxException(errorMsg);
}

public void LexError()
{
  string errorMsg = string.Format("({0},{1}): Неизвестный символ {2}", yyline, yycol, yytext);
  throw new LexException(errorMsg);
}

class ScannerHelper 
{
  private static Dictionary<string,int> keywords;

  static ScannerHelper() 
  {
    keywords = new Dictionary<string,int>();
	keywords.Add("begin",(int)Tokens.BEGIN);
	keywords.Add("end",(int)Tokens.END);
	keywords.Add("var",(int)Tokens.VAR);
    keywords.Add("cycle",(int)Tokens.CYCLE);
	keywords.Add("if",(int)Tokens.IF);
	keywords.Add("else",(int)Tokens.ELSE);
	keywords.Add("while",(int)Tokens.WHILE);
	keywords.Add("write",(int)Tokens.WRITE);
	keywords.Add("true",(int)Tokens.TRUE);
	keywords.Add("false",(int)Tokens.FALSE);
  }
  public static int GetIDToken(string s)
  {
	if (keywords.ContainsKey(s.ToLower()))
	  return keywords[s];
	else
      return (int)Tokens.ID;
  }
  
}
