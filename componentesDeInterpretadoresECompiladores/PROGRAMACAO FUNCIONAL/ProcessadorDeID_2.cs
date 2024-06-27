using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using parser.ProgramacaoOrentadaAObjetos;
namespace parser
{
    public class BuildInstrucoes
    {
        /// <summary>
        /// lista dos tokens de uma instrucao.
        /// </summary>
        private List<string> codigo;
        /// <summary>
        /// contexto onde a expressao está.
        /// </summary>
        private Escopo escopo;


        /// <summary>
        /// objeto contendo as definicoes da linguagem orquidea.
        /// </summary>
        private static LinguagemOrquidea linguagem;
        
        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="codigo">lista de tokens da instrucao.</param>
        public BuildInstrucoes(List<string> codigo)
        {
            if (linguagem == null)
                linguagem = LinguagemOrquidea.Instance();
            this.codigo = codigo.ToList<string>();
            this.escopo = new Escopo(codigo);
        }

        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="code">texto contendo todos tokens da instrucao.</param>
        public BuildInstrucoes(string code)
        {
            if (linguagem == null)
                linguagem = LinguagemOrquidea.Instance();
            this.codigo = new Tokens(code).GetTokens();
            this.escopo = new Escopo(this.codigo);
        }

        /// <summary>
        /// instrucao module. sintaxe: module nameLibrary;
        /// </summary>
        /// <param name="sequencia">sequencia da expressao.</param>
        /// <param name="escopo">contexto onde a expressao está.</param>
        /// <returns></returns>
        protected List<Instrucao> BuildInstrucaoModule(UmaSequenciaID sequencia, Escopo escopo)
        {
            // module nameLibrary;
            // .
            if (!sequencia.tokens.Contains("module"))
                return null;


            // faz uma copia dos tokens.
            List<string> tokensInstrucao = sequencia.tokens.ToList<string>();

            // remove o token "module".
            tokensInstrucao.RemoveAt(0);

            // remove o token ";", se tiver.
            if (tokensInstrucao[tokensInstrucao.Count - 1] == ";")
                tokensInstrucao.RemoveAt(tokensInstrucao.Count - 1);





            string nomeBiblioteca = Util.UtilString.UneLinhasLista(tokensInstrucao).Trim(' ');
            Classe classeBiblioteca = RepositorioDeClassesOO.Instance().GetClasse(nomeBiblioteca);
            if (classeBiblioteca == null)
            {
                ImportadorDeClasses importador = new ImportadorDeClasses(nomeBiblioteca + ".dll");
                if (RepositorioDeClassesOO.Instance().GetClasse(nomeBiblioteca) == null)
                {
                    UtilTokens.WriteAErrorMensage("instruction Module, has a internal error to load a file dll of module: " + Utils.OneLineTokens(sequencia.tokens) + nomeBiblioteca, sequencia.tokens, escopo);
                    return null;
                }

            }

            // registra a classe do modulo, no escopo.
            escopo.tabela.RegistraClasse(classeBiblioteca);




            Instrucao instrucaoImporter = new Instrucao();



            return new List<Instrucao>() { instrucaoImporter };
        }  // BuildInstrucaoConstructor()


        /// <summary>
        /// instrucao exporter. sintaxe: importer ( nomeAssembly).
        /// </summary> 
        /// <param name="sequencia"></param>
        /// <param name="escopo"></param>
        /// <returns></returns>
        protected List<Instrucao> BuildInstrucaoImporter(UmaSequenciaID sequencia, Escopo escopo)
        {
            // importer ( nomeAssembly).
            if (!sequencia.tokens.Contains("importer"))
            {
                return null;
            }
                
            if (sequencia.tokens.Count < 3)
            {
                return null;
            }
                


            // faz uma copia dos tokens.
            List<string> tokensInstrucao = sequencia.tokens.ToList<string>();

            // remove o nome da instrução, e os parenteses abre e fecha da instrucao, e o ponto e virgula do comando.
            tokensInstrucao.RemoveRange(0, 2);
            // remove o operador de final de instrucao.
            if (tokensInstrucao.Contains(";"))
            {
                tokensInstrucao.RemoveAt(tokensInstrucao.Count - 1);
            }
            // remoe o parenteses fecha, da instrução importer.
            tokensInstrucao.RemoveAt(tokensInstrucao.Count - 1);
            






            string nomeArquivoAsssembly = Util.UtilString.UneLinhasLista(tokensInstrucao);
            nomeArquivoAsssembly = nomeArquivoAsssembly.Replace(" ", "");




            // expressao da instrucao.
            Expressao exprss_comando = new Expressao(new string[] { "importer", nomeArquivoAsssembly }, escopo);
            escopo.tabela.AdicionaExpressoes(escopo, exprss_comando);


            // carrega previamente a biblioteca.
            ImportadorDeClasses importador = new ImportadorDeClasses(nomeArquivoAsssembly);
            importador.ImportAllClassesFromAssembly(); // importa previamente as classes do arquivo assembly.



            // constroi a instrucao.
            Instrucao instrucaoImporter = new Instrucao(ProgramaEmVM.codeImporter, new List<Expressao> { exprss_comando }, new List<List<Instrucao>>());



            return new List<Instrucao>() { instrucaoImporter };
        }  // BuildInstrucaoConstructor()


        /// <summary>
        /// instrucao constructorUP.
        /// </summary>
        /// <param name="sequencia">sequencia de tokens da instrucao.</param>
        /// <param name="escopo">contexto onde a instrucao está.</param>
        /// <returns></returns>
        protected List<Instrucao> BuildInstrucaoConstrutorUP(UmaSequenciaID sequencia, Escopo escopo)
        {
            /// template= classeHerdeira.construtorUP(nomeClasseHerdada, List<Expressao> parametrosDoConstrutor).
            /// pode ser o objeto "actual";

            string nomeClasseHedeira = sequencia.tokens[0];
            string nomeClasseHerdada = sequencia.tokens[4];

            if (!RepositorioDeClassesOO.Instance().ExisteClasse(nomeClasseHedeira))
            {
                UtilTokens.WriteAErrorMensage("instructor constructorUP: internal error, instruction construtorUP, super class do not exists, ou sintaxe error in name class. " + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                return null;
            }
            if (!RepositorioDeClassesOO.Instance().ExisteClasse(nomeClasseHerdada))
            {
                UtilTokens.WriteAErrorMensage("instruction construtorUP: internal error, sub class do not exists, ou sintax error in code." + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                return null;
            }



            int indexStartParametros = sequencia.tokens.IndexOf("(");
            if (indexStartParametros == 1)
            {
                UtilTokens.WriteAErrorMensage("instruction construtorUP, internal error, sintaxe error, without parentesis initial or final." + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                return null;
            }

            List<string> tokensParametros = sequencia.tokens.GetRange(indexStartParametros, sequencia.tokens.Count - indexStartParametros);
            List<Expressao> expressoesParametros = null;

            if ((tokensParametros != null) && (tokensParametros.Count > 0))
            {
                tokensParametros.Remove(";");
                tokensParametros.Remove(","); // remove da lista de parâmetros, o primeiro token ",", pois faz parte da especificação do token da classe herdada.
                tokensParametros.RemoveAt(0);
                tokensParametros.RemoveAt(tokensParametros.Count - 1);


                tokensParametros.Remove(nomeClasseHerdada);  // remove da lista de parâmetros, o token do nome da classe herdada..


                expressoesParametros = Expressao.ExtraiExpressoes(tokensParametros, escopo);
                if (expressoesParametros == null)
                {
                    UtilTokens.WriteAErrorMensage("instruction construtorUP,  internal error in parameters expressions, in contrutorUP instruction" + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                    return null;
                }
            }
            else
                expressoesParametros = new List<Expressao>(); // sem parametros para passar ao construtor.

            int indexConstrutor = FoundACompatibleConstructor(nomeClasseHerdada, expressoesParametros);
            if (indexConstrutor < 0)
            {
                UtilTokens.WriteAErrorMensage("instruction constructor up, not found super class constructor, not match with types of parameters." + Utils.OneLineTokens(sequencia.tokens),
                    sequencia.tokens, escopo);
                return null;
            }


            Expressao pacoteParametros = new Expressao();
            pacoteParametros.Elementos.AddRange(expressoesParametros);

            Expressao expressaoCabecalho = new Expressao();
            expressaoCabecalho.Elementos.Add(new ExpressaoElemento(nomeClasseHedeira));
            expressaoCabecalho.Elementos.Add(new ExpressaoElemento(nomeClasseHerdada));
            expressaoCabecalho.Elementos.Add(new ExpressaoElemento(indexConstrutor.ToString()));
            expressaoCabecalho.Elementos.Add(pacoteParametros);

            escopo.tabela.AdicionaExpressoes(escopo, expressoesParametros.ToArray()); // adiciona as expressoes-parametros, para fins de otimização.

            Instrucao instrucaoConstrutorUP = new Instrucao(ProgramaEmVM.codeConstructorUp, expressaoCabecalho.Elementos, new List<List<Instrucao>>());
            return new List<Instrucao>() { instrucaoConstrutorUP };
        }


        /// <summary>
        /// Cria um novo objeto. pode criar um objeto simples, ou um objeto de variavel vetor.
        /// sintaxe: tipo objeto= create(parametros), ou objeto=create(parametros), parametros pode se null ou vazio.
        /// </summary>
        /// <param name="sequencia">sequencia de tokens da instrucao.</param>
        /// <param name="escopo">contexto onde a instrucao está.</param>
        protected List<Instrucao> BuildInstrucaoCreate(UmaSequenciaID sequencia, Escopo escopo)
        {


            /// EXPRESSOES: a lista de expressões da instrução foi feita na seguinte sequência:
            /// 0- NOME "create"
            /// 1- tipo do objeto
            /// 2- nome do objeto
            /// 3- tipo template do objeto: Objeto/Vetor.
            /// 4- expressoes indices vetor.
            /// 5- expressoes parametros.
            /// 6- indice do construtor.
            /// 7- nome do objeto caller.

            string TIPODoObjeto = "";
            string NOMEDoObjeto = "";

            /// ID ID = create ( ID , ID ) --> exemplo: int m= create(1,1).
            if (RepositorioDeClassesOO.Instance().ExisteClasse(sequencia.tokens[0]))
            {
                TIPODoObjeto = sequencia.tokens[0].ToString();
                NOMEDoObjeto = sequencia.tokens[1];

            }
            else
            {

                NOMEDoObjeto = sequencia.tokens[0];
                Objeto objetoJaInicializado = escopo.tabela.GetObjeto(NOMEDoObjeto, escopo);
                if (objetoJaInicializado != null)
                {
                    TIPODoObjeto = objetoJaInicializado.GetTipo();
                }
                else
                {
                    NOMEDoObjeto = sequencia.tokens[0];
                    objetoJaInicializado = escopo.tabela.GetObjeto(NOMEDoObjeto, escopo);

                    // PROCESSAMENTO DE UM ELEMENTO DE UM VECTOR WRAPPER OBJECT. (multiplas instrucoes).
                    if (objetoJaInicializado != null)
                    {
                        if (objetoJaInicializado.isWrapperObject)
                        {
                            TIPODoObjeto = objetoJaInicializado.tipoElemento;
                            NOMEDoObjeto = objetoJaInicializado.nome + ExpressaoPorClassificacao.countObjetosElementosDeWrapperObject++;


                            int indexBegin = sequencia.tokens.IndexOf("[");
                            if (indexBegin == -1)
                            {
                                UtilTokens.WriteAErrorMensage("bad format to vector elment construction", sequencia.tokens, escopo);
                                return null;
                            }

                            List<string> tokensIndicesElemento = UtilTokens.GetCodigoEntreOperadores(indexBegin, "[", "]", sequencia.tokens);
                            if ((tokensIndicesElemento==null) || (tokensIndicesElemento.Count< 2))
                            {
                                UtilTokens.WriteAErrorMensage("bad format in vector element construction!", sequencia.tokens, escopo);
                                return null;
                            }
                            tokensIndicesElemento.RemoveAt(0);
                            tokensIndicesElemento.RemoveAt(tokensIndicesElemento.Count - 1);


                            int indexBeginParamsCreate = sequencia.tokens.IndexOf("(");
                            List<string> tokensCreate = UtilTokens.GetCodigoEntreOperadores(indexBeginParamsCreate, "(", ")", sequencia.tokens);
                            if ((tokensCreate == null) || (tokensCreate.Count < 2))
                            {
                                UtilTokens.WriteAErrorMensage("bad format in vector element construction!", sequencia.tokens, escopo);
                                return null;
                            }
                            tokensCreate.RemoveAt(0);
                            tokensCreate.RemoveAt(tokensCreate.Count - 1);

                            


                            string codeCreate = TIPODoObjeto + "  " + NOMEDoObjeto + " = create(" + Utils.OneLineTokens(tokensCreate) + ");";
                            string codeSetElement = objetoJaInicializado.nome + ".SetElement(" + Utils.OneLineTokens(tokensIndicesElemento) + "," + NOMEDoObjeto + ");";


                            // OBTENDO A INSTRUCAO CREATE DO ELEMENTO DO VECTOR.
                            ProcessadorDeID compiladorParaCreate = new ProcessadorDeID(codeCreate);
                            compiladorParaCreate.escopo.tabela.objetos.AddRange(escopo.tabela.objetos);
                            compiladorParaCreate.Compilar();
                            if ((compiladorParaCreate.instrucoes == null) || (compiladorParaCreate.instrucoes.Count == 0))
                            {
                                UtilTokens.WriteAErrorMensage("fail in constructor of a vector element.", sequencia.tokens, escopo);
                                return null;
                            }

                            // adiciona objetos criados na instrucao.
                            for (int i = 0; i < compiladorParaCreate.escopo.tabela.objetos.Count; i++)
                            {
                                int index = escopo.tabela.objetos.FindIndex(k => k.nome == compiladorParaCreate.escopo.tabela.objetos[i].nome);
                                if (index == -1)
                                {
                                    escopo.tabela.objetos.Add(compiladorParaCreate.escopo.tabela.objetos[i]);
                                }
                            }


                            // instrucao create.
                            Instrucao instrucaoCreate2 = compiladorParaCreate.instrucoes[0];

                            // CONSTRUINDO A EXPRESSAO SETELEMENT.
                            ProcessadorDeID compiladorParaSetElement = new ProcessadorDeID(codeSetElement);
                            compiladorParaSetElement.escopo.tabela.objetos.AddRange(escopo.tabela.objetos);
                            compiladorParaSetElement.Compilar();
                            if ((compiladorParaSetElement.instrucoes == null) || (compiladorParaSetElement.instrucoes.Count == 0))
                            {
                                UtilTokens.WriteAErrorMensage("fail in a set element function. verify inddex to element-vector", sequencia.tokens, escopo);
                                return null;
                            }

                            // adiciona objetos criados na instrucao.
                            for (int i = 0; i< compiladorParaSetElement.escopo.tabela.objetos.Count; i++)
                            {
                                int index = escopo.tabela.objetos.FindIndex(k => k.nome == compiladorParaSetElement.escopo.tabela.objetos[i].nome);
                                if (index == -1)
                                {
                                    escopo.tabela.objetos.Add(compiladorParaSetElement.escopo.tabela.objetos[i]);
                                }
                            }




                            // instrucao set element.
                            Instrucao instrucaoSetElement = compiladorParaSetElement.instrucoes[0];
                            

                            


                            return new List<Instrucao>() { instrucaoCreate2, instrucaoSetElement };

                        }
                        else
                        {
                            TIPODoObjeto = objetoJaInicializado.tipo;
                        }
                    }
                }


            }

            if (!ValidaTokensDaSintaxeDaInstrucao(sequencia, escopo))
            {
                UtilTokens.WriteAErrorMensage("instructor create: bad format instruction in create, not found signal equals our parenteses" + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                return null;
            }






            // EXTRAI OS PARAMETROS da instrução.
            int indexFirstParenteses = sequencia.tokens.IndexOf("(");
            List<string> tokensParametros = UtilTokens.GetCodigoEntreOperadores(indexFirstParenteses, "(", ")", sequencia.tokens);
            if ((tokensParametros==null) || (tokensParametros.Count<2))
            {
                UtilTokens.WriteAErrorMensage("bad format in create instruction." + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                return null;
            }
            tokensParametros.RemoveAt(0);
            tokensParametros.RemoveAt(tokensParametros.Count - 1);


            
            List<Expressao> expressoesParametros = Expressao.ExtraiExpressoes(tokensParametros, escopo);
            if ((expressoesParametros == null) ||
                (expressoesParametros.Count == 0) ||
                ((expressoesParametros[0].Elementos != null) && (expressoesParametros[0].Elementos.Count == 0)))
            {
                expressoesParametros = new List<Expressao>();
            }
            

            Objeto objetoInstanciado = new Objeto();
            int indexConstrutor = -1;

            List<object> parametrosObject = new List<object>();



            //__________________________________________________________________________________________________________________________________
            // PROCESSAMENTO DE OBJETO DE CLASSE IMPORTADA.
            Classe classObjetoInstanciado = RepositorioDeClassesOO.Instance().GetClasse(TIPODoObjeto);
            if (classObjetoInstanciado == null)
            {
                RepositorioDeClassesOO.Instance().GetClasse(Escopo.nomeClasseCurrente);
            }
            if ((classObjetoInstanciado != null) && (classObjetoInstanciado.isImport))
            {
                // obtem o construtor compativel com a lista de parametros.
                Metodo construtor = UtilTokens.FindConstructorCompatible(TIPODoObjeto, expressoesParametros, ref indexConstrutor);
                if (construtor == null)
                {
                    construtor = UtilTokens.FindConstructorCompatible(Escopo.nomeClasseCurrente, expressoesParametros, ref indexConstrutor);
                    if (construtor == null)
                    {
                        UtilTokens.WriteAErrorMensage("instruction create, constructor to object: " + NOMEDoObjeto + " not found!   " + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                        return null;
                    }
                    
                }
                else
                {

                    EvalExpression eval = new EvalExpression();
                    
                    for (int i = 0; i < expressoesParametros.Count; i++)
                    {
                        object umParametro = eval.EvalPosOrdem(expressoesParametros[i], escopo);
                        parametrosObject.Add(umParametro);
                    }
                    // faz uma instanciacao previa. objetos importados não são instanciados no create previamente, porque pode resultar em falhas colaterais de instanciacao.
                    objetoInstanciado = new Objeto("private", TIPODoObjeto, NOMEDoObjeto, null);
                    objetoInstanciado.nome = NOMEDoObjeto;
                    objetoInstanciado.classePertence = TIPODoObjeto;
                    objetoInstanciado.indexConstrutor = indexConstrutor;
                   
                  

                    // obtem o tipo do objeto.
                    Type tipoClasseObjeto = UtilTokens.GetType(TIPODoObjeto);
                    // validação se o tipo existe.
                    if (tipoClasseObjeto == null)
                    {
                        // 2a. validação, mas com o nome do objeto dentro do namespace [parser].
                        tipoClasseObjeto = Type.GetType("parser." + TIPODoObjeto);

                        
                        if (tipoClasseObjeto == null)
                        {

                            Classe classeDoObjeto = RepositorioDeClassesOO.Instance().GetClasse(TIPODoObjeto);
                            if (classeDoObjeto == null)
                            {

                                UtilTokens.WriteAErrorMensage("in instruction create,  type: " + TIPODoObjeto + " of object: " + NOMEDoObjeto + " not found!" + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                                return null;

                            }

                        }
                    }
                    
                    if (escopo.tabela.GetObjeto(NOMEDoObjeto, escopo) != null)
                    {
                        escopo.tabela.UpdateObjeto(objetoInstanciado);
                       
                    }
                    else
                    {
                        escopo.tabela.RegistraObjeto(objetoInstanciado);
                    }
                    
                }
                
            }
            else
            //__________________________________________________________________________________________________________________________________
            // PROCESSAMENTO DE OBJETO ORQUIDEA (nao importado)
            if (escopo.tabela.GetObjeto(NOMEDoObjeto, escopo) == null)
            {
               

                // o construtor pode não estar já construido, procede a instanciacao, sem validação.
                indexConstrutor = FoundACompatibleConstructor(TIPODoObjeto, expressoesParametros);

                if (indexConstrutor < 0)
                {
                    List<Classe> classesRegistradas = RepositorioDeClassesOO.Instance().classesRegistradas;
                    Classe classeDoObjeto = classesRegistradas.Find(k => k.nome == TIPODoObjeto);
                    if (classeDoObjeto != null)
                    {
                        int indexPropriedade = classeDoObjeto.propriedades.FindIndex(k => k.nome == NOMEDoObjeto);
                        if (indexPropriedade >= 0)
                        {
                            TIPODoObjeto = classeDoObjeto.propriedades[indexPropriedade].tipo;
                            // o construtor pode não estar já construido, procede a instanciacao, sem validação.
                            indexConstrutor = FoundACompatibleConstructor(TIPODoObjeto, expressoesParametros);

                        }
                        
                    }
                   
                }
                objetoInstanciado = new Objeto("public", TIPODoObjeto, NOMEDoObjeto, null);
                objetoInstanciado.classePertence = Escopo.nomeClasseCurrente;
                objetoInstanciado.indexConstrutor = indexConstrutor;

                // se o objeto não já foi criado, instancia o objeto, no escopo.
                escopo.tabela.GetObjetos().Add(objetoInstanciado); 
            }




            // construção da instrução, em suas expressoes.
            Expressao exprssDaIntrucao = new Expressao();
            Expressao parametros = new Expressao();
            parametros.Elementos.AddRange(expressoesParametros);


            exprssDaIntrucao.Elementos.Add(new ExpressaoElemento("create"));
            exprssDaIntrucao.Elementos.Add(new ExpressaoElemento(TIPODoObjeto));
            exprssDaIntrucao.Elementos.Add(new ExpressaoElemento(NOMEDoObjeto));
            exprssDaIntrucao.Elementos.Add(new ExpressaoElemento("Objeto"));
            exprssDaIntrucao.Elementos.Add(new ExpressaoElemento("reservado para vetor"));
            exprssDaIntrucao.Elementos.Add(parametros);
            exprssDaIntrucao.Elementos.Add(new ExpressaoElemento(indexConstrutor.ToString()));
            exprssDaIntrucao.Elementos.Add(new ExpressaoElemento(objetoInstanciado.GetNome()));




            // registra as expressões, a fim de otimização de modificação.
            if (expressoesParametros.Count > 0)
            {
                escopo.tabela.AdicionaExpressoes(escopo, expressoesParametros.ToArray());
            }


            // cria a instrucao do objeto.
            Instrucao instrucaoCreate = new Instrucao(ProgramaEmVM.codeCreateObject, new List<Expressao>() { exprssDaIntrucao }, new List<List<Instrucao>>());
            return new List<Instrucao>() { instrucaoCreate };

        } // BuildInstrucaoCreate()



   


        protected bool ValidaTokensDaSintaxeDaInstrucao(UmaSequenciaID sequencia, Escopo escopo)
        {
            int indexSignalEquals = sequencia.tokens.IndexOf("=");
            if (indexSignalEquals == -1)
            {
                UtilTokens.WriteAErrorMensage("instruction create: internal error in instruction create, not found operator equals for instantiation." + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                return false;
            } // if


            int indexParenteses = sequencia.tokens.IndexOf("(");
            if (indexParenteses == -1)
            {
                UtilTokens.WriteAErrorMensage("instruction create: internal error in instruction create, not found parentesis, sintaxe error." + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                return false;
            } // if
            return true;
        } // ValidaTokensDaSintaxeDaInstrucao()

        /// <summary>
        /// encontra um construtor compativel com a lista de parametros.
        /// </summary>
        /// <param name="tipoObjeto">tipo do objeto a instanciar.</param>
        /// <param name="parametros">lista de expressoes que compoem os parametros.</param>
        /// <returns>retorna o indice do construtor.</returns>
        public static int FoundACompatibleConstructor(string tipoObjeto, List<Expressao> parametros)
        {
            if ((tipoObjeto == null) || (tipoObjeto == ""))
            {
                return -1;
            }
            Classe classeComConstrutores = RepositorioDeClassesOO.Instance().GetClasse(tipoObjeto);

            if (classeComConstrutores == null)
            {
                return -1;
            }

            List<Metodo> construtores = classeComConstrutores.construtores;
            int contadorConstrutores = 0;

            if ((parametros == null) || (parametros.Count == 0))
            {
                int indexConstrutorSemParametros = construtores.FindIndex(k => k.parametrosDaFuncao == null || k.parametrosDaFuncao.Length == 0);
                return indexConstrutorSemParametros;

            }
            int x_parametros = 0;
            foreach (Metodo umConstrutor in construtores)
            {

                if (umConstrutor.parametrosDaFuncao == null)
                {
                    if ((parametros == null) || (parametros.Count == 0) || (parametros[x_parametros].Elementos.Count == 0))
                        return contadorConstrutores;
                }
                else
                if (parametros.Count != umConstrutor.parametrosDaFuncao.Length)
                    continue;

                bool isFoundConstrutor = true;
                for (int x = 0; x < parametros.Count; x++)
                {
                    if ((parametros[x] != null) && (parametros[x].Elementos != null) && (parametros[x].Elementos.Count > 0))
                    {
                        if (parametros[x].Elementos[0].tipoDaExpressao.ToLower() == umConstrutor.parametrosDaFuncao[x].GetTipo().ToLower()) 
                        {
                            continue;
                        }
                        else
                        {
                            isFoundConstrutor = false;
                            break;
                        }

                    }
                    else
                    {
                        continue;
                    }


                }
                if (isFoundConstrutor)
                    return contadorConstrutores;

                contadorConstrutores++;
            }

            return -1;
        }


        /// <summary>
        /// instrucao SetVar. sintaxe: SetVar(nomeObjeto,valorDoObjeto);
        /// </summary>
        /// <param name="sequencia">sequencia de tokens.</param>
        /// <param name="escopo">contexto onde a sequencia está.</param>
        /// <returns></returns>
        protected List<Instrucao> BuildInstrucaoSetVar(UmaSequenciaID sequencia, Escopo escopo)
        {
            // template: 
            // SetVar ( ID , ID)

            if ((sequencia == null) || (sequencia.tokens.Count == 0))
                return null;
            if (sequencia.tokens[0] != "SetVar")
                return null;
            string nomeVar = sequencia.tokens[2];
            string valorVar = sequencia.tokens[4];

            Objeto v = escopo.tabela.GetObjeto(nomeVar, escopo);
            if (v == null)
            {
                UtilTokens.WriteAErrorMensage("instruction SetVar, object not found.  " + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                return null;
            }
                
            Expressao expressaoNumero = new Expressao(new string[] { valorVar }, escopo);
            ProcessadorDeID.SetValorNumero(v, expressaoNumero, escopo);

            Expressao exprss = new Expressao(sequencia.tokens.ToArray(), escopo);

            // adiciona a expressao para a lista de expressoes do escopo, para fins de otimização.
            escopo.tabela.AdicionaExpressoes(escopo, exprss);

            Instrucao instrucaoSet = new Instrucao(ProgramaEmVM.codeSetObjeto, new List<Expressao>() { exprss }, new List<List<Instrucao>>());
            return new List<Instrucao>() { instrucaoSet };
        }


        /// <summary>
        /// instrucao GetObjeto. sintaxe: GetObjeto(nomeObjeto). retorna uma [ExpressaoObjeto] contendo o objeto.
        /// </summary>
        /// <param name="sequencia">sequencia de tokens da instrucao.</param>
        /// <param name="escopo">contexto onde a instrucao está.</param>
        /// <returns></returns>
        protected List<Instrucao> BuildInstrucaoGetObjeto(UmaSequenciaID sequencia, Escopo escopo)
        {
            // template: 
            // ID GetObjeto ( ID )

            if ((sequencia == null) || (sequencia.tokens.Count == 0))
                return null;
            if (sequencia.tokens[1] != "GetObjeto")
                return null;

            string nomeVar = sequencia.tokens[3];
            Objeto v = escopo.tabela.GetObjeto(nomeVar, escopo);
            if (v == null)
            {
                UtilTokens.WriteAErrorMensage("instruction GetObjeto: internal error in instruction GetObjeto, object not found." + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                return null;
            }
            Expressao expressaoGetObjeto = new Expressao(sequencia.tokens.ToArray(), escopo);

            // adiciona a expressao para a lista de expressoes, para fins de otimizações.
            escopo.tabela.AdicionaExpressoes(escopo, expressaoGetObjeto);

            Instrucao instrucaoGet = new Instrucao(ProgramaEmVM.codeGetObjeto, new List<Expressao>() { expressaoGetObjeto }, new List<List<Instrucao>>());
            return new List<Instrucao>() { instrucaoGet };
        }

        /// <summary>
        /// instrucao while. sintaxe: while(expressaoCondicional){blocoDeInstrucoes}.
        /// </summary>
        /// <param name="sequencia">sequencia de tokens da instrução.</param>
        /// <param name="escopo">contexto onde a instrucao está.</param>
        /// <returns></returns>
        protected List<Instrucao> BuildInstrucaoWhile(UmaSequenciaID sequencia, Escopo escopo)
        {
            List<Expressao> expressoesWhile = null;
            /// while (exprss) bloco .
            if (sequencia.tokens[0] == "while")
            {
                int indexStartExpressionCondicional = sequencia.tokens.IndexOf("(");

                if (indexStartExpressionCondicional == -1)
                {
                    UtilTokens.WriteAErrorMensage("instruction while: internal error in instruction while, not found a conditional expression." + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                    return null;
                }
                List<string> tokensExpressaoCondicional = UtilTokens.GetCodigoEntreOperadores(indexStartExpressionCondicional, "(", ")", sequencia.tokens);
                if ((tokensExpressaoCondicional == null) || (tokensExpressaoCondicional.Count == 0))
                {
                    UtilTokens.WriteAErrorMensage("instruction while:internal error, not found a conditional expression. " + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                    return null;
                }
                tokensExpressaoCondicional.RemoveAt(0);
                tokensExpressaoCondicional.RemoveAt(tokensExpressaoCondicional.Count - 1);


                expressoesWhile = Expressao.ExtraiExpressoes(tokensExpressaoCondicional, escopo);

                if ((expressoesWhile == null) || (expressoesWhile.Count == 0))
                {
                    UtilTokens.WriteAErrorMensage("instruction while: internal error, bad formation in conditional expression." + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                    return null;
                }


                if (!Expressao.ValidaExpressaoCondicional(expressoesWhile[0]))   // valida se a expressão contém um operador operacional.
                {

                    UtilTokens.WriteAErrorMensage("instruction while: internal error, bad formation in conditional control expression." + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                    return null;
                }



                escopo.tabela.AdicionaExpressoes(escopo, expressoesWhile.ToArray()); // registra a expressão na lista de expressões do escopo currente.

                ProcessadorDeID processador = null;
                Instrucao instrucaoWhile = new Instrucao(ProgramaEmVM.codeWhile, new List<Expressao>() { expressoesWhile[0] }, new List<List<Instrucao>>());
                BuildBloco(0, sequencia.tokens, ref escopo,instrucaoWhile, ref processador); // constroi as instruções contidas num bloco.

                return new List<Instrucao>() { instrucaoWhile };

            } // if
            return null;
        } // InstrucaoWhileSemBloco()

        /// <summary>
        /// instrucao for. sintaxe: for (int x=0; x<3; x++){ bloco de instrucoes }; 
        /// </summary>
        /// <param name="sequencia">sequencia de tokens.</param>
        /// <param name="escopo">contexto onde a instrucao está.</param>
        /// <returns></returns>
        protected List<Instrucao> BuildInstrucaoFor(UmaSequenciaID sequencia, Escopo escopo)
        {

            // for (int x=0; x<3; x++){ }; 
            if ((sequencia.tokens[0] == "for") && (sequencia.tokens.IndexOf("(") != -1))
            {



                List<string> tokensDaInstrucao = sequencia.tokens.ToList<string>();
                tokensDaInstrucao.RemoveAt(0); // remove o termo-chave: "for"


                List<string> tokensExpressoes = UtilTokens.GetCodigoEntreOperadores(0, "(", ")", tokensDaInstrucao);
                tokensExpressoes.RemoveAt(0);
                tokensExpressoes.RemoveAt(tokensExpressoes.Count - 1);



                Objeto variavelMalha = null;
                int indexClasseVariavelMalha = sequencia.tokens.IndexOf("=");
                if ((indexClasseVariavelMalha - 2) >= 0) // verifica se a variavel de malha é definida dentro da instrução for.
                {
                    
                    int indexFirstComma = sequencia.tokens.IndexOf(";");
                    if (!RepositorioDeClassesOO.Instance().ExisteClasse(sequencia.tokens[indexClasseVariavelMalha - 2]))
                    {
                        if (escopo.tabela.GetObjeto(sequencia.tokens[indexClasseVariavelMalha - 1], escopo)==null)
                        {
                            UtilTokens.WriteAErrorMensage("instructio for, control variable not found, not defined.  " + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                            return null;
                        }
                    
                    }
                }

                // obtem as expressoes da instrução "for".
                List<Expressao> expressoesDaInstrucaoFor = Expressao.ExtraiExpressoes(tokensExpressoes, escopo);





                if ((expressoesDaInstrucaoFor == null) || (expressoesDaInstrucaoFor.Count == 0))
                {
                    UtilTokens.WriteAErrorMensage("instruction for: internal error, not found expressions of instruction.  " + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                    return null;
                }

                if (expressoesDaInstrucaoFor.Count == 1)
                {
                    // houve um nao processamento de todas expressoes, pois a instanciacao da  variavel de malha esta entre as expressoes. Faz
                    // o processamento da instanciacao da variavel de malha, e extrai as expressoes novamente.
                    ProcessadorDeID processadorVariavelDaMalha = new ProcessadorDeID(new List<string>() { expressoesDaInstrucaoFor[0].ToString() });
                    processadorVariavelDaMalha.Compilar();
                    expressoesDaInstrucaoFor = Expressao.ExtraiExpressoes(tokensExpressoes, escopo);
                }



                if (RepositorioDeClassesOO.Instance().ExisteClasse(expressoesDaInstrucaoFor[0].ToString()))
                {
                    // se a Objeto malha for definida na instrucao for, extrai a Objeto e adiciona no escopo esta Objeto.
                    // as expressoes posteriorees da instrucao for utilizam esta Objeto, ela já foi registrada.
                    Classe tipoDaObjeto = RepositorioDeClassesOO.Instance().GetClasse(expressoesDaInstrucaoFor[0].Elementos[0].ToString());
                    string nomeObjeto = expressoesDaInstrucaoFor[0].Elementos[1].ToString();
                    object valorObjeto = expressoesDaInstrucaoFor[0].Elementos[3].ToString();

                    escopo.tabela.GetObjetos().Add(new Objeto("private", tipoDaObjeto.GetNome(), nomeObjeto, valorObjeto));
                }

                if (!Expressao.Instance.IsExpressionAtibuicao(expressoesDaInstrucaoFor[0])) // valida a expressao de atribuicao
                {
                    UtilTokens.WriteAErrorMensage("instruction for: internal error in atribution expression", expressoesDaInstrucaoFor[0].tokens, escopo);
                }



                if (!Expressao.ValidaExpressaoCondicional(expressoesDaInstrucaoFor[1])) // valida a expressão de controle condicional.
                {
                    UtilTokens.WriteAErrorMensage("instruction for: internal error in conditional expression" + expressoesDaInstrucaoFor[1].ToString() + "is not a valid expression!  " + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                }

                if (!Expressao.Instance.IsExpressaoAritimeticoUnario(expressoesDaInstrucaoFor[2], escopo)) // valida a expressão de incremento/decremento.
                {
                    UtilTokens.WriteAErrorMensage("instruction for: internal error in increment expression, not valid expression: " + expressoesDaInstrucaoFor[2].ToString()+"in:  "+Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                }


                
                    

                Instrucao instrucaoFor = null;
                int offsetIndexBloco = sequencia.tokens.FindIndex(k => k == "{"); // calcula se há um token de operador bloco abre.
                if (offsetIndexBloco == -1)
                {
                    UtilTokens.WriteAErrorMensage("instruction for: must provide a instructions block, uses { } operator for definition a block, include in none block instrutions." + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                    return null;
                }
                else
                {
                    ProcessadorDeID processador = null;
                    instrucaoFor = new Instrucao(ProgramaEmVM.codeFor, expressoesDaInstrucaoFor, new List<List<Instrucao>>()); // cria a instrucao for principal.
                    BuildBloco(0, sequencia.tokens, ref escopo, instrucaoFor, ref processador); // adiciona as instruções do bloco.

                    instrucaoFor.expressoes = new List<Expressao>();

                    // adiciona as expressoes do "for" para a instrução VM "for".
                    instrucaoFor.expressoes.AddRange(expressoesDaInstrucaoFor);


                    return new List<Instrucao>() { instrucaoFor };

                } //if
            } // if
            return null;
        } 


        /// <summary>
        /// instrucao if/else.
        /// sintaxe: if (expressoesCondicionais){bloco de instrucao}, ou if (expressoesCondicionais){bloco de instrucao if} else {bloco de instrucao else};
        /// 
        /// </summary>
        /// <param name="sequencia">sequencia de tokens da instrucao.</param>
        /// <param name="escopo">contexto onde a expressao está.</param>
        /// <returns></returns>
        protected List<Instrucao> BuildInstrucaoIFsComOuSemElse(UmaSequenciaID sequencia, Escopo escopo)
        {


          
            /// if (exprss) {} else {}.
            if (sequencia.tokens[0] == "if")
            {
                int indexInitTokens = sequencia.tokens.IndexOf("(");
                List<string> tokensDeExpressoes = UtilTokens.GetCodigoEntreOperadores(indexInitTokens, "(", ")", sequencia.tokens);
                if ((tokensDeExpressoes == null) || (tokensDeExpressoes.Count < 2))
                {
                    UtilTokens.WriteAErrorMensage("instruction if, bad sintax for conditional expression.  " + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                    return null;
                }
                tokensDeExpressoes.RemoveAt(0);
                tokensDeExpressoes.RemoveAt(tokensDeExpressoes.Count - 1);

                Expressao expressoesIf = new Expressao(tokensDeExpressoes.ToArray(), escopo);





                // valida se há expressões validas para a instrução.
                if (expressoesIf == null)
                {

                    UtilTokens.WriteAErrorMensage("instruction if, sintax error.", sequencia.tokens, escopo);
                    return null;
                }
                // valida a expressão de atribuição da instrução "if".
                if (expressoesIf == null) 
                {
                    UtilTokens.WriteAErrorMensage("instruction if,  internal error, control expression is in bad formation." + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                    return null;
                }// valida se a expressão contém um operador operacional.
                if (!Expressao.ValidaExpressaoCondicional(expressoesIf))
                {
                    UtilTokens.WriteAErrorMensage("instruction if, internal error, control expression is not a conditional expression:  " + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                    return null;
                }





                


                // offset para o primeiro token de bloco.
                int offsetBlocoIf = sequencia.tokens.IndexOf("{");
                // se não for uma instrução com bloco, é uma instrução sem bloco, retornando null, pois a instrucao nao foi construida.
                if (offsetBlocoIf == -1)
                {
                    return null;
                }



                ProcessadorDeID processador = null;

                int offsetBlocoElse = sequencia.tokens.IndexOf("else", offsetBlocoIf);
                if (offsetBlocoElse == -1) // instrução if sem bloco de uma instrução else.
                {

                    Instrucao instrucaoIfSemElse = new Instrucao(ProgramaEmVM.codeIfElse, new List<Expressao>() { expressoesIf }, new List<List<Instrucao>>());
                    
                    BuildBloco(0, sequencia.tokens, ref escopo, instrucaoIfSemElse, ref processador);

                    return new List<Instrucao>() { instrucaoIfSemElse }; // ok , é um comando if sem instrução else.
                } // if
                else // instrução if com bloco de uma instrução else.
                {

                    // CONSTRUCAO DO BLOCO DE IF.
                    List<string> tokensIf = UtilTokens.GetCodigoEntreOperadores(offsetBlocoIf, "{", "}", sequencia.tokens);
                    if ((tokensIf == null) || (tokensIf.Count < 2))
                    {
                        UtilTokens.WriteAErrorMensage("instruction if: sintax error in if block", sequencia.tokens, escopo);
                        return null;
                    }

                    Instrucao instrucaoElse = new Instrucao(ProgramaEmVM.codeIfElse, new List<Expressao>() { expressoesIf }, new List<List<Instrucao>>());

                    // constroi o bloco da instrução if.
                    BuildBlocoIfElse(0, tokensIf, ref escopo, instrucaoElse, ref processador);



                    // obtem o indice do bloco else.
                    offsetBlocoElse = sequencia.tokens.IndexOf("{", offsetBlocoIf + tokensIf.Count);
                    // CONSTRUCAO DE BLOCO DE ELSE;
                    List<string> tokensElse = UtilTokens.GetCodigoEntreOperadores(offsetBlocoElse, "{", "}", sequencia.tokens);
                    if ((tokensElse != null) && (tokensElse.Count >= 2))
                    {
                        BuildBlocoIfElse(1, tokensElse, ref escopo, instrucaoElse, ref processador);
                    }
                    

                    return new List<Instrucao>() { instrucaoElse };
                } // else
            } // if
            return null;
        } // BuildInstrucaoIFsComOuSemElse

        /// <summary>
        /// instrucao casesOfUse: sintaxe: casesOfUse y: { (case < x): { y = 2; }; {mais cases}} 
        /// </summary>
        /// <param name="sequencia">sequencia de tokens.</param>
        /// <param name="escopo">contexto onde a expressao está.</param>
        /// <returns></returns>
        protected List<Instrucao> BuildInstrucaoCasesOfUse(UmaSequenciaID sequencia, Escopo escopo)
        {


            // sintaxe: "casesOfUse y: { (case < x): { y = 2; }; } ";
            int iCabecalho = sequencia.tokens.IndexOf("(");
            if (iCabecalho == -1)
            {
                UtilTokens.WriteAErrorMensage("instruction casesOfUse: sintaxe error, not found parentesis.  " + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                return null;
            }

            // obtem as listas de cases, cada um contendo o bloco de um item case.
            List<List<List<string>>> listaDeCases = UtilTokens.GetCodigoEntreOperadoresCases(sequencia.tokens);

            // obtem a variavel principal, e valida.
            string nomeObjetoPrincipal = sequencia.tokens[1];
            Objeto vMain = escopo.tabela.GetObjeto(nomeObjetoPrincipal, escopo);
            if (vMain == null)
            {
                UtilTokens.WriteAErrorMensage("instruction cases of use, main variable: " + nomeObjetoPrincipal + " not defined. " + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                return null;
            } // if


            List<Expressao> EXPRESSOES_TODOS_CASE = new List<Expressao>();
      
            List<List<Instrucao>> BLOCOS_DE_INSTRUCOES_CORPO_CASE = new List<List<Instrucao>>(); // inicializa as listas de blocos de instrução.


            // percorre as listas, calculando: 1- a expressão condicional do case, 2- o bloco de instruções para o case.
            for (int UM_CASE = 0; UM_CASE < listaDeCases.Count; UM_CASE++)
            {
                // verifica se o case tem tokens suficientes.
                if (listaDeCases[UM_CASE][0].Count<5)
                {
                    UtilTokens.WriteAErrorMensage("instruction casesOfUse: error in one of cases, index case: " + UM_CASE+", in: " + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                    return null;
                }

                int indexCabecalhoCase = listaDeCases[UM_CASE][0].IndexOf("(");
                string OPERADOR_CONDICIONAL_UM_CASE = listaDeCases[UM_CASE][0][indexCabecalhoCase + 2];

                // SUBSTITUI a palavra case pelo nome do objeto principal, para formar a expressao condicional completa.
                listaDeCases[UM_CASE][0][indexCabecalhoCase + 1] = nomeObjetoPrincipal;
                listaDeCases[UM_CASE][0].RemoveAt(0);
                listaDeCases[UM_CASE][0].RemoveAt(listaDeCases[UM_CASE][0].Count - 1);

                // se o operador não pertencer ao tipo do objeto principal, retorna null.
                if (RepositorioDeClassesOO.Instance().GetClasse(vMain.GetTipo()).operadores.Find(k => k.nome == OPERADOR_CONDICIONAL_UM_CASE) == null)
                {
                    UtilTokens.WriteAErrorMensage("instruction casesOfUse, operator: " + OPERADOR_CONDICIONAL_UM_CASE + " not found at class: " + vMain.GetTipo() + ", of " + vMain.GetNome() + "object, in:  " + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                    return null;
                }



                List<string> tokensExprssHeader = listaDeCases[UM_CASE][0].ToList<string>();

                Expressao expressaoCABECALHO = null;
                if ((tokensExprssHeader != null) && (tokensExprssHeader.Count > 0))
                {
                    expressaoCABECALHO = new Expressao(tokensExprssHeader.ToArray(), escopo);
                    if (expressaoCABECALHO == null)
                    {
                        UtilTokens.WriteAErrorMensage("instruction casesOfUse,  error in expression.  " + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                        return null;

                    }
                }
                else
                {
                    UtilTokens.WriteAErrorMensage("instruction casesOfUse, error in expression." + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                    return null;

                }


                //______________________________________________________________________________________________

                List<string> tokensCORPO_DO_CASE = listaDeCases[UM_CASE][1];
                if (tokensCORPO_DO_CASE[0] == "{")
                {
                    tokensCORPO_DO_CASE.RemoveAt(0);
                    tokensCORPO_DO_CASE.RemoveAt(tokensCORPO_DO_CASE.Count - 1);
                }

                ProcessadorDeID compilador = new ProcessadorDeID(tokensCORPO_DO_CASE);
                // ADICIONA TODOS OBJETOS DO ESCOPO CURRENTE, NECESSARIO A COMPILACAO DO BLOCO DE INSTRUCAO DE UM CASE.
                compilador.escopo.tabela.AdicionaObjetos(compilador.escopo, escopo.tabela.objetos.ToArray());
                compilador.Compilar();
                if ((compilador.GetInstrucoes() == null) || (compilador.GetInstrucoes().Count == 0))
                {
                    UtilTokens.WriteAErrorMensage("instruction casesOfUse,error in instructions of body case.  " + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                    return null;
                }


                // adiciona as instrucoes do corpo do case currente.
                BLOCOS_DE_INSTRUCOES_CORPO_CASE.Add(compilador.GetInstrucoes());

               
          

                
                
                EXPRESSOES_TODOS_CASE.Add(expressaoCABECALHO);

           
           

                // formato:
                /// exprss[umCaseIndex]:a expressao condicional do case.
                /// instrucao[umCaseIndex]: instrucoes.

            }

            Instrucao instrucaoCase = new Instrucao(ProgramaEmVM.codeCasesOfUse, EXPRESSOES_TODOS_CASE, BLOCOS_DE_INSTRUCOES_CORPO_CASE);
            return new List<Instrucao>() { instrucaoCase };


        } // BuildInstrucaoCasesOfUse(()

    

        /// <summary>
        /// constroi blocos de if/else,
        /// </summary>
        /// <param name="numeroDoBloco">indice do bloco.</param>
        /// <param name="tokens">tokens de if, ou tokens de else.</param>
        /// <param name="escopoDaInstrucao">escopo da instrucao.</param>
        /// <param name="instrucaoPrincipal">instrucao if/else.</param>
        /// <param name="processadorBloco">compilador.</param>
       protected void BuildBlocoIfElse(int numeroDoBloco, List<string> tokens, ref Escopo escopoDaInstrucao, Instrucao instrucaoPrincipal, ref ProcessadorDeID processadorBloco)
        {

            



            // remove os operadores bloco dos tokens do bloco.
            tokens.RemoveAt(0);
            tokens.RemoveAt(tokens.Count - 1);

            Escopo escopoBloco = new Escopo(tokens);

            processadorBloco = new ProcessadorDeID(tokens);

            if (escopoDaInstrucao != null)
            {   // copia a tabela de valores do escopo currente.
                processadorBloco.escopo.tabela.objetos.AddRange(escopoDaInstrucao.tabela.objetos);
            }
            else
            {
                processadorBloco.escopo.tabela = new TablelaDeValores(tokens);
            }

            // +1 linha compilado, do operador de bloco abre.
            ProcessadorDeID.lineInCompilation += 1;

            processadorBloco.Compilar(); 

            // +1 linha compilada, do operador de bloco fecha.
            ProcessadorDeID.lineInCompilation += 1;


            // sobe para o escopo pai, eventuais erros levantados no escopo do bloco de instruções.
            if ((processadorBloco.escopo != null) && (processadorBloco.escopo.GetMsgErros() != null) && (processadorBloco.escopo.GetMsgErros().Count > 0))
            {

                escopoDaInstrucao.GetMsgErros().AddRange(processadorBloco.escopo.GetMsgErros());

            }


            // remove os objetos presentes no escopo da instrucao, e também no escopo do bloco. a remoção é no escopo do bloco.
            List<Objeto> objetosDaInstrucao = escopoDaInstrucao.tabela.objetos;
            if (processadorBloco.escopo.tabela.objetos != null) 
            {
                for (int i = 0;i<objetosDaInstrucao.Count;i++)
                {
                    int index = processadorBloco.escopo.tabela.objetos.FindIndex(k => k.nome == objetosDaInstrucao[i].nome);
                    if (index >= 0)
                    {
                        processadorBloco.escopo.tabela.objetos.RemoveAt(index);
                    }
                }
            }



            List<Instrucao> instrucoesBLOCO = processadorBloco.GetInstrucoes();
            instrucaoPrincipal.blocos.Insert(numeroDoBloco, instrucoesBLOCO);
            // adiciona o escopo do bloco de instruções.
            instrucaoPrincipal.escoposDeBloco.Add(processadorBloco.escopo.Clone());

            // faz a ligacao de escopo-pai (escopo do metodo) ----> escopo no-filho (escopo do bloco).
            UtilTokens.LinkEscopoPaiEscopoFilhos(escopoDaInstrucao, processadorBloco.escopo);
        }



        /// <summary>
        /// constroi um bloco de instruções, em  instrucoes: for, while, casesOfUse.
        /// </summary>
        /// <param name="numeroDoBloco">indice do bloco.</param>
        /// <param name="tokens">tokens das instruções do bloco.</param>
        /// <param name="escopoDaInstrucao">contexto onde a expressao está.</param>
        /// <param name="instrucaoPrincipal">instrucao do bloco.</param>
        /// <param name="processadorBloco">objeto compilador.</param>
        protected void BuildBloco(int numeroDoBloco, List<string> tokens, ref Escopo escopoDaInstrucao, Instrucao instrucaoPrincipal, ref ProcessadorDeID processadorBloco)
        {
      
            if ((!tokens.Contains("{")) || (!tokens.Contains("}")))
                return;

            int indexStart = 0;
            int offsetStart = 0;
            for (int x = 0; x <= numeroDoBloco; x++)
            {
                indexStart = tokens.IndexOf("{", offsetStart);
                if (indexStart == -1)
                    break;


                List<string> blocoAnterior = UtilTokens.GetCodigoEntreOperadores(indexStart, "{", "}", tokens);
                offsetStart = blocoAnterior.Count;

            }

            List<string> bloco = UtilTokens.GetCodigoEntreOperadores(indexStart, "{", "}", tokens);


            bloco.RemoveAt(0); // remove os operadores bloco dos tokens do bloco.
            bloco.RemoveAt(bloco.Count - 1);


            processadorBloco = new ProcessadorDeID(bloco);

            // copia os objetos do escopo superior, ao escopo do bloco.
            if ((escopoDaInstrucao.tabela.objetos != null) && (escopoDaInstrucao.tabela.objetos.Count > 0))
            {
                processadorBloco.escopo.tabela.objetos.AddRange(escopoDaInstrucao.tabela.objetos);
            }


            // +1 do operador de blocos abre.
            ProcessadorDeID.lineInCompilation += 1; 


            // faz a compilacao do bloco.
            processadorBloco.Compilar();


            // remove os objetos presentes no escopo da instrucao, presentes também  no escopo do bloco.
            if (processadorBloco.escopo.tabela.objetos.Count > 0)
            {
                List<Objeto> objetosEscopoDoMetodo = escopoDaInstrucao.tabela.objetos;

                for (int i = 0; i < objetosEscopoDoMetodo.Count; i++)
                {
                    int index = processadorBloco.escopo.tabela.objetos.FindIndex(k => k.nome == objetosEscopoDoMetodo[i].nome);
                    if (index >= 0)
                    {
                        processadorBloco.escopo.tabela.objetos.RemoveAt(index);
                    }

                }
            }


            // sobe para o escopo pai, eventuais erros ocorridos no escopo do bloco de instrucoes.
            if ((processadorBloco.escopo != null) && (processadorBloco.escopo.GetMsgErros() != null) && (processadorBloco.escopo.GetMsgErros().Count > 0))
            {
                escopoDaInstrucao.GetMsgErros().AddRange(processadorBloco.escopo.GetMsgErros());
            }

            // +1 do operador de blocos fecha.
            ProcessadorDeID.lineInCompilation += 1;  
            

            // faz a ligação dos escopos pai->escopo no filho.
            UtilTokens.LinkEscopoPaiEscopoFilhos(escopoDaInstrucao, processadorBloco.escopo);

            List<Instrucao> instrucoesBLOCO = processadorBloco.GetInstrucoes();
            instrucaoPrincipal.blocos.Add(instrucoesBLOCO);

            // adiciona o escopo do bloco de instruções.
            instrucaoPrincipal.escoposDeBloco.Add(processadorBloco.escopo.Clone());


        }


        /// <summary>
        /// instrucao return. sintax: return expressao.
        /// </summary>
        /// <param name="sequencia">sequencia de tokens da instrucao.</param>
        /// <param name="escopo">contexto onde a instrucao está.</param>
        /// <returns></returns>
        protected List<Instrucao> BuildInstrucaoReturn(UmaSequenciaID sequencia, Escopo escopo)
        {



            List<string> tokensExpressoes = sequencia.tokens.ToList<string>();
            tokensExpressoes.RemoveAt(0); // retira o nome da instrucao: token "return", para compor o corpo da expressão.
            tokensExpressoes.RemoveAt(tokensExpressoes.Count - 1);
            Expressao exprssRETURN = new Expressao(tokensExpressoes.ToArray(), escopo);
            if (exprssRETURN != null)
            {
                Instrucao instrucaoReturn = new Instrucao(ProgramaEmVM.codeReturn, new List<Expressao>() { exprssRETURN }, new List<List<Instrucao>>());
                return new List<Instrucao>() { instrucaoReturn };
            }
            else
            {
                UtilTokens.WriteAErrorMensage("instruction return, error in expression" + Utils.OneLineTokens(sequencia.tokens), sequencia.tokens, escopo);
                return null;
            }
            
        }



        public class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes para instrucoes da linguagem orquidea.")
            {
            }

            public void TesteChamadasDeFuncaoAninhados(AssercaoSuiteClasse assercao)
            {

                SystemInit.InitSystem();
                try
                {
                    string class_entity = "public class entity{ public int a; public int b; public entity(){a=1; b=2;}; public void metodoB() { a=3;};};";
                    string create = "entity[] v1[20]; v1[0]= create();v1[0].metodoB();";


                    ProcessadorDeID compilador = new ProcessadorDeID(class_entity + create);
                    compilador.Compilar();

                    ProgramaEmVM programa = new ProgramaEmVM(compilador.instrucoes);
                    programa.Run(compilador.escopo);

                    Objeto elementoVector = (Objeto)(((Vector)compilador.escopo.tabela.GetObjeto("v1", compilador.escopo).valor).Get(0));
                    assercao.IsTrue(elementoVector != null && elementoVector.propriedades[0].valor.ToString() == "3", class_entity + " " + create);

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }
            }


            public void TesteInstrucaoForComChamadasDeFuncao(AssercaoSuiteClasse assercao)
            {
                try
                {
                    SystemInit.InitSystem();
                    string code_0_0 = "int[] v[15]; int i=0; for (i=0;i<v.size();i++){v[i]=i;};";
                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0); ;
                    compilador.Compilar();

                    assercao.IsTrue(SystemInit.errorsInCopiling.Count == 0, code_0_0);

                    ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                    programa.Run(compilador.escopo);


                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }
            }

            public void TesteInstrucaoIfElse(AssercaoSuiteClasse assercao)
            {
                try
                {
                    SystemInit.InitSystem();

                    string code_0_0 = "int x=1; int y=2; if ((x<1) && (y>5)){x=1;};";
                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0);
                    compilador.Compilar();

                    assercao.IsTrue(SystemInit.errorsInCopiling.Count == 0, code_0_0);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }
                

            }

          
            public void TesteInstrucaoCasesOfUse(AssercaoSuiteClasse assercao)
            {

                SystemInit.InitSystem();

                // sintaxe: "casesOfUse y: { (case < x): { y = 2; }; } ";
                string varsCreate = "int x=1; int y=2;";
                string instrucaoCase = "casesOfUse y: { (case < x): { y = 2; } (case < x): { y = 2; } };";

                List<string> tokensTeste = new Tokens(varsCreate + instrucaoCase).GetTokens();
                ProcessadorDeID compilador = new ProcessadorDeID(tokensTeste);
                compilador.Compilar();

                try
                {
                    Instrucao instCase = compilador.GetInstrucoes()[2];
                    assercao.IsTrue(instCase.blocos.Count == 2 && instCase.expressoes.Count == 2, "casesOfUse y:  (case < x): { y = 2; };");
       
                }
                catch (Exception ex)
                {
                    string msgError = ex.Message;
                }
                

            }
        } // class

    }
} // namespace
