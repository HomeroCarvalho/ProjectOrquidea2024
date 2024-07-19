Project Orquidea 2024:

Este é um projeto de estudos holístico sobre criar uma linguagem de programação.
Contém um interpretador LISP, um interpretador PROLOG, uma linguagem de programação orientada
a objetos, e uma extensão de linguagem orientada a objetos de criação e utilização de Aspectos.

Possui um sistema de importação de classes C Sharp, via API Reflexão. Com isso, é fácil
criar bibliotecas para a linguagem. A escalabilidade de criar bibliotecas é algo muito interessante.
Com foco nas classes base orquidea, que cria objetos do tipo [Objeto], esta extensão do projeto
ProjectOrquidea2023 está muito mais completa. 

Como demonstrativo, execute o arquivo .exe do projeto (./ProjectOrquidea2024.exe), e veja
a versão de um jogo implementado na linguagem orientada a objetos.

Os comandos de programação estruturada: for,while,if/else,return, break,continue, e um novo
comando: casesOfUse (um swicth mais complexo, que permite comparações com objetos não constantes,
e com operadores que não sejam apenas o operador igual).

A linguagem orientada a objetos tem sintaxe muito semelhante a de linguagens C++,Java,C Sharp, porque
é um estudo, sem a intenção de lançar uma linguagem competitiva no universo das linguagens de programação.

second branch contains .exe files, and assets and libraries SFML.NET, to running the game Catch Meteors,
if you to execute the project (./ProjectOrquidea2024.exe), this a option to run this game.


Jogos completos feito na lingugem orquidea:
	1- SpaceInvadersSFML_ComImagens.txt (1.5 min para compilar)
	2- CatchMeteors.txt  (2-3min para compilar)
	


 
Segue nos proximos dias uma descrição completa de comandos de programação estruturada: for,if/else,while, casesOfUse,
return, break, continue.

Nos proximos dias também verificarei se a classe Aspecto ainda está funcional. Faz muito tempo desde que
realizei testes de programação orientada a Aspectos, que depende desta classe.

Olá, caras, bom dia.
Fiquei de dar um retorno de melhor documentação do projeto.
	A melhor descrição sobre como a linguagem funciona, já está num dos arquivos de documentação disponibilizado: 
	[ProjectOrquidea2024\Documentos do Projeto\MANUTENCAO CLASSES\ManutencaoAditivaBibliotecasOrquidea.txt].


	Outro fato é que há falhas no sistema de testes. Você muda a posição na sequência dos cenários, e os teste validam.
Isto é porque há um problema no classificador de tokens, não sanada. Mas não é falha fatal: o efeito ocorre
quando se execute de uma só vez, muitos pequenos programas. Numa situação real, isto não falha,é como se executasse
num só processo no Sistema Operacional, vários programas.

	Quanto ao tempo de compilação, já está se planejando um compilador de classes quando não há modificações
do código de classe. Isto será feito com uma serialização de classes, métodos, propriedades, instruções e expressões.
	

	
