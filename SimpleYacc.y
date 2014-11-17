%{
// Ёти объ€вления добавл€ютс€ в класс GPPGParser, представл€ющий собой парсер, генерируемый системой gppg
    public BlockNode root; //  орневой узел синтаксического дерева 
    public Parser(AbstractScanner<ValueType, LexLocation> scanner) : base(scanner) { }
%}

%output = SimpleYacc.cs
%partial

%union { 
			public float fVal; 
			public int iVal;
			public string sVal; 
			public Node nVal;
			public ExprNode eVal;
			public StatementNode stVal;
			public BlockNode blVal;
       }

%using SimpleLang.MiddleEnd;

%namespace SimpleParser

%token BEGIN END CYCLE WHILE IF ELSE ASSIGN SEMICOLON WRITE COLON COMMA VAR
%token PLUS MINUS MULT DIV LBR RBR LESS GREATER EQUAL NEQUAL LEQUAL GEQUAL LESS GREATER
%token TRUE FALSE
%token <sVal> STRING
%token <iVal> INUM 
%token <fVal> FNUM 
%token <sVal> ID

%type <eVal> num_expr comp_expr ident T F expr
%type <stVal> assign statement cycle while if write vardef idlist
%type <blVal> stlist block

%%

progr   
	: block { root = $1; }
	;

stlist	
	: statement { $$ = new BlockNode($1); }
	| stlist SEMICOLON statement 
		{ 
			$1.Add($3); 
			$$ = $1; 
		}
	;

statement
	: vardef { $$ = $1; }
	| assign { $$ = $1; }
	| block   { $$ = $1; }
	| cycle   { $$ = $1; }
	| while { $$ = $1; }
	| if { $$ = $1; }
	| write { $$ = $1; }
	;

ident 	
	: ID { $$ = new IdNode($1); }	
	;
	
idlist
	: ident { $$ = new VarDefNode($1 as IdNode); }
	| idlist COMMA ident { 
			($1 as VarDefNode).Add($3 as IdNode);
			$$ = $1;
		}
	;
	
assign 	
	: ident ASSIGN expr { $$ = new AssignNode($1 as IdNode, $3); }
	;

vardef
	: VAR idlist COLON ident { 
			$$ = $2; 
			($$ as VarDefNode).TypeIdent = $4 as IdNode;
		}
	;
	
block	
	: BEGIN stlist END { $$ = $2; }
	;

cycle	
	: CYCLE num_expr statement { $$ = new CycleNode($2, $3); }
	;
		
while	
	: WHILE comp_expr statement { $$ = new WhileNode($2, $3); }
	;

if 
	: IF comp_expr statement { $$ = new IfNode($2, $3); }
	| IF comp_expr statement ELSE statement { $$ = new IfNode($2, $3, $5); }
	;
	
write
	: WRITE LBR expr RBR { $$ = new WriteNode($3); }
	;

expr
	: comp_expr { $$ = $1; }
	| num_expr { $$ = $1; }
	;
	
comp_expr
	: num_expr LESS num_expr { $$ = new BinOpNode($1, $3, BinOpType.Less); }
	| num_expr GREATER num_expr { $$ = new BinOpNode($1, $3, BinOpType.Greater); }
	| num_expr EQUAL num_expr { $$ = new BinOpNode($1, $3, BinOpType.Equal); }
	| num_expr NEQUAL num_expr { $$ = new BinOpNode($1, $3, BinOpType.NEqual); }
	| num_expr LEQUAL num_expr { $$ = new BinOpNode($1, $3, BinOpType.LEqual); }
	| num_expr GEQUAL num_expr { $$ = new BinOpNode($1, $3, BinOpType.GEqual); }
	| LBR comp_expr RBR { $$ = $2; }
	| TRUE { $$ = new BoolNode(true); }
	| FALSE { $$ = new BoolNode(false); }
	;
	
num_expr 
	: T { $$ = $1; }
    | num_expr PLUS T { $$ = new BinOpNode($1, $3, BinOpType.Plus); }
    | num_expr MINUS T { $$ = new BinOpNode($1, $3, BinOpType.Minus); }
    ;

T	
	: F { $$ = $1; }
    | T MULT F { $$ = new BinOpNode($1, $3, BinOpType.Mult); }
    | T DIV F { $$ = new BinOpNode($1, $3, BinOpType.Div); }
    ;

F    
	: ident  { $$ = $1 as IdNode; }
	| INUM { $$ = new IntNumNode($1); }
	| FNUM { $$ = new FloatNumNode($1); }
	| STRING { $$ = new StringNode($1); } 
    | LBR num_expr RBR { $$ = $2; }
    ;
	 
%%
