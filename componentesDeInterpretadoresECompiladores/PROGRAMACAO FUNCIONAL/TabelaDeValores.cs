using System.Collections.Generic;
using System.Collections;
using System.Linq;
using parser.ProgramacaoOrentadaAObjetos;
using System.Security.Principal;
using System;
using System.IO;
using System.Reflection;

using parser.LISP;

using Microsoft.SqlServer.Server;
using Util;

namespace parser
{

    /// <summary>
    /// classe que armazena os objetos, funcoes, operadores, para um dado contexto. funciona como o HEAP da linguagem.
    /// </summary>
    public class TablelaDeValores
    {


       


        private List<Classe> Classes = new List<Classe>();
        private List<Operador> operadores = new List<Operador>();


        /// <summary>
        /// contém as expressões validadas no escopo currente.
        /// </summary>
        public static List<Expressao> expressoes { get; set; } 

        /// <summary>
        /// contém as funções do escopo currente.
        /// </summary>
        public List<Metodo> Funcoes = new List<Metodo>(); 

        /// <summary>
        /// contem variaveis vetor (obsoleto, substituido pela classe Wrapper [Vector].
        /// </summary>
        private List<Vetor> VariaveisVetor;


        /// <summary>
        /// contém uma lista de objetos instanciados, em um escopo.
        /// </summary>
        public List<Objeto> objetos = new List<Objeto>(); 
        
        private List<string> codigo { get; set; }

        /// <summary>
        /// retorna a lista de objetos registrados nesta tabela.
        /// </summary>
        /// <returns></returns>
        public List<Objeto> getObjetos()
        {
            return objetos;
        }

        /// <summary>
        /// clona a tabela de valores.
        /// </summary>
        /// <returns>retorna um clone desta tabela de valores.</returns>
        public TablelaDeValores Clone()
        {
            TablelaDeValores tabelaClone = new TablelaDeValores(this.codigo);
            if (tabelaClone != null)
            {
                tabelaClone.Classes = this.Classes.ToList<Classe>();
                tabelaClone.operadores = this.operadores.ToList<Operador>();
                TablelaDeValores.expressoes = expressoes.ToList<Expressao>();
                tabelaClone.Funcoes = this.Funcoes.ToList<Metodo>();
                tabelaClone.VariaveisVetor = new List<Vetor>();
            
                if ((this.objetos != null) && (this.objetos.Count > 0))
                {
                    for (int i = 0; i < this.objetos.Count; i++)
                    {
                        tabelaClone.objetos.Add(this.objetos[i]);
                    }
                }

                tabelaClone.codigo = this.codigo.ToList<string>();
                
            }
            return tabelaClone;
        }

        private static LinguagemOrquidea lng = LinguagemOrquidea.Instance();

        /// <summary>
        /// construtor. 
        /// </summary>
        /// <param name="_codigo">lista de linhas de codigo/tokens a compilar.</param>
        public TablelaDeValores(List<string> _codigo)
        {
            lng = LinguagemOrquidea.Instance();
            if ((_codigo != null) && (_codigo.Count > 0))
                this.codigo = _codigo.ToList<string>();
            else
                this.codigo = new List<string>();
            if (expressoes == null)
                expressoes = new List<Expressao>();
        } //TabelaDeValores()

        /// <summary>
        /// adiciona as expressoes formadas, para fins de otimização.
        /// </summary>
        /// <param name="escopo">escopo onde as expressoes estao.</param>
        /// <param name="expressoesAIncluir">expressoes a adicionar.</param>
        public void AdicionaExpressoes(Escopo escopo, params Expressao[] expressoesAIncluir)
        {
            if (expressoes == null)
            {
                expressoes = new List<Expressao>();
            }
            List<Expressao> expressFound = new List<Expressao>();
            expressoes.AddRange(expressoesAIncluir);
        }

      
        /// <summary>
        /// adiciona objetos para esta tabela.
        /// </summary>
        /// <param name="escopo">contexto onde os objetos estão.</param>
        /// <param name="objetos">array de objetos a adicionar.</param>
        public void AdicionaObjetos(Escopo escopo, params Objeto[] objetos)
        {
            if ((objetos != null) && (objetos.Length > 0))
            {
                this.objetos.AddRange(objetos);
               
            }
        }

        /// <summary>
        /// retorna uma lista de expressoes registradas nesta tabela.
        /// </summary>
        /// <returns></returns>
        private List<Expressao> GetExpressoes()
        {
            return expressoes;
        } // GetExpressoes()

        /// <summary>
        /// retorna uma lista de operadores registrados nesta tabela.
        /// </summary>
        /// <returns></returns>
        public List<Operador> GetOperadores()
        {
            return this.operadores;
        }

        /// <summary>
        /// retorna a lista de todos objetos registrados nesta tabela.
        /// </summary>
        /// <returns></returns>
        public List<Objeto> GetObjetos()
        {
            return this.objetos;
        }

        /// <summary>
        /// retorna a lista de todas funções registradas nesta tabela.
        /// </summary>
        /// <returns></returns>
        public List<Metodo> GetFuncoes()
        {
            return Funcoes;
        } // GetFuncoes()

        /// <summary>
        /// obtém a função nesta tabela de valores, e se não encontrar, obtém a função no repositorio de classes orquidea.
        /// </summary>
        /// <param name="nomeFuncao">nome da funcao.</param>
        /// <param name="classeDaFuncao">nome da classe da funcao.</param>
        /// <param name="escopo">contexto onde a funcao está.</param>
        public Metodo GetFuncao(string nomeFuncao, string classeDaFuncao, Escopo escopo)
        {
            if (escopo.ID == Escopo.tipoEscopo.escopoGlobal)
                  return FindFuncao(classeDaFuncao, nomeFuncao, escopo);

            Metodo fuctionFound = FindFuncao(classeDaFuncao, nomeFuncao, escopo);
            if (fuctionFound != null) 
                return fuctionFound;
            else
                return GetFuncao(nomeFuncao, classeDaFuncao, escopo.escopoPai);
        } // GetFuncao()



        private  Metodo FindFuncao(string classeDaFuncao, string nomeFuncao, Escopo escopo)
        {
        
            Metodo umaFuncaoDasTabelas = escopo.tabela.Funcoes.Find(k => k.nome == nomeFuncao);
            if (umaFuncaoDasTabelas != null)
                return escopo.tabela.Funcoes.Find(k => k.nome == nomeFuncao);

            if (RepositorioDeClassesOO.Instance().GetClasse(classeDaFuncao)!=null)
            {
                Metodo umaFuncaoDoRepositorio = RepositorioDeClassesOO.Instance().GetClasse(classeDaFuncao).GetMetodos().Find(k => k.nome.Equals(nomeFuncao));
                if (umaFuncaoDoRepositorio != null)
                    return umaFuncaoDoRepositorio;
            }
            return null;
        }


        /// <summary>
        /// obtem funcoes com o nome parâmetro, não importando a classe em que a função foi definida.
        /// </summary>
        /// <param name="nomeFuncao">none da funcao a obter.</param>
        public List<Metodo> GetFuncao(string nomeFuncao)        {

            List<Metodo> funcoesDaTabelaComMesmoNome = this.Funcoes.FindAll(k => k.nome.Equals(nomeFuncao));

            if (funcoesDaTabelaComMesmoNome != null)
                return funcoesDaTabelaComMesmoNome;

            List<Metodo> funcoesDoRepositorioDeClassesComMesmoNome = new List<Metodo>();
            List<Classe> lstClasses = RepositorioDeClassesOO.Instance().GetClasses();
            foreach (Classe umaClasse in lstClasses)
            {
                List<Metodo> lstFuncoes = umaClasse.GetMetodos().FindAll(k => k.nome.Equals(nomeFuncao));
               if (lstFuncoes!=null)
                {
                    funcoesDoRepositorioDeClassesComMesmoNome.AddRange(lstFuncoes);
                } // if
            } // foreach
            return funcoesDoRepositorioDeClassesComMesmoNome;
        } // GetFuncao()

     
        /// <summary>
        /// retorna a lista de classes registradas nesta tabela.
        /// </summary>
        /// <returns></returns>
        public List<Classe> GetClasses()
        {
           return Classes;
        }


        /// <summary>
        /// retorna a classe registrada nesta tabela.
        /// </summary>
        /// <param name="nomeDaClasse">nome da classe.</param>
        /// <param name="escopo">contexto onde a classe está.</param>
        /// <returns></returns>
        public Classe GetClasse(string nomeDaClasse, Escopo escopo)
        {
            if (escopo.ID == Escopo.tipoEscopo.escopoGlobal)
                return escopo.tabela.Classes.Find(k => k.nome.Equals(nomeDaClasse));
            else
            {
                Classe classe = escopo.tabela.Classes.Find(k => k.GetNome().Equals(nomeDaClasse));
                if (classe != null)
                    return classe;

                if (classe == null)
                    return GetClasse(nomeDaClasse, escopo.escopoPai);
            }
            return null;
        }


        /// <summary>
        /// clona a tabela de valores parametro..
        /// </summary>
        /// <param name="tabela">tabela a clonar.</param>
        /// <returns></returns>
        public static TablelaDeValores Clone(TablelaDeValores tabela)
        {
            TablelaDeValores tabelaClone = new TablelaDeValores(tabela.codigo);
            if (tabela.GetClasses().Count > 0)
                tabelaClone.GetClasses().AddRange(tabela.GetClasses().ToList<Classe>());
            
            if (tabela.GetExpressoes().Count > 0)
                tabelaClone.GetExpressoes().AddRange(tabela.GetExpressoes().ToList<Expressao>());

            if (tabela.GetFuncoes().Count > 0)
                tabelaClone.GetFuncoes().AddRange(tabela.GetFuncoes().ToList<Metodo>());

            if (tabela.GetObjetos().Count > 0)
                tabelaClone.GetObjetos().AddRange(tabela.GetObjetos().ToList<Objeto>());

            if (tabela.GetOperadores().Count > 0)
                tabelaClone.GetOperadores().AddRange(tabela.GetOperadores().ToList<Operador>());

          
            return tabelaClone;
            
        }

        /// <summary>
        /// registra uma classe nesta tabela de valores.
        /// </summary>
        /// <param name="umaClasse"></param>
        public void RegistraClasse(Classe umaClasse)
        {
            if (Classes.Find(k => k.nome == umaClasse.nome) == null)
                Classes.Add(umaClasse); 

            this.operadores.AddRange(umaClasse.GetOperadores());
        }


        /// <summary>
        /// obtem o objeto guardado na tabela de valores do escopo parametro.
        /// </summary>
        /// <param name="nomeObjeto">nome do objeto a obter.</param>
        /// <param name="escopo">contexto onde está a tabela de valores que contem o objeto.</param>
        /// <returns></returns>
        public Objeto GetObjeto(string nomeObjeto, Escopo escopo)
        {
            if ((escopo == null) || (escopo.tabela == null) || (escopo.tabela.objetos == null) || (escopo.tabela.objetos.Count == 0)) 
            {
                return null;
            }

            
            for (int i = 0; i < escopo.tabela.objetos.Count; i++)
            {
                if ((escopo.tabela.objetos[i] != null) && (escopo.tabela.objetos[i].nome == nomeObjeto)) 
                {
                    return escopo.tabela.objetos[i];
                }
            }
            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return GetObjeto(nomeObjeto, escopo.escopoPai);

            return null;
        } // GetObjeto().


        /// <summary>
        /// obtem o objeto, pelo nome e tipo.
        /// </summary>
        /// <param name="nomeObjeto">nome do objeto.</param>
        /// <param name="tipoDoObjeto">classe do objeto.</param>
        /// <param name="escopo">contexto que contem a tabela de valores em que o objeto está.</param>
        /// <returns></returns>
        public Objeto GetObjeto(string nomeObjeto, string tipoDoObjeto, Escopo escopo)
        {
            if (escopo == null)
                return null;
            Objeto objetoRetorno = escopo.tabela.objetos.Find(k => k.GetNome() == nomeObjeto && k.GetTipo()==tipoDoObjeto);
            if (objetoRetorno != null)
                return objetoRetorno;

            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return GetObjeto(nomeObjeto,tipoDoObjeto, escopo.escopoPai);

            return null;
        } // GetObjeto().



        /// <summary>
        /// registra o objeto na tabela de valores currente.
        /// </summary>
        /// <param name="objeto">objeto a ser guardado.</param>
        public void RegistraObjeto(Objeto objeto)
        {
            this.objetos.Add(objeto);
        } // RegistraObjeto().



        /// <summary>
        /// remove a ultima instancia do objeto com o nome de entrada.
        /// </summary>
        /// <param name="nomeObjeto">nom do objeto a ser removido.</param>
        public void RemoveObjeto(string nomeObjeto)
        {

            int index = this.objetos.FindLastIndex(k => k.GetNome() == nomeObjeto);
            if (index != -1)
                this.objetos.RemoveAt(index);

        } // RemoveObjeto()

        /// <summary>
        /// registra uma funcao nesta tabela.
        /// </summary>
        /// <param name="funcao"></param>
        /// <returns></returns>
        public bool RegistraFuncao(Metodo funcao)
        {
            this.Funcoes.Add(funcao);
            return true;
        }


        /// <summary>
        /// atualiza um objeto de mesmo nome, com o objeto parâmetro, com valores atualizados.
        /// </summary>
        /// <param name="objetoAtualizado">objeto a ser atualizado.</param>
        public void UpdateObjeto(Objeto objetoAtualizado)
        {
            if (objetoAtualizado == null) { return; }
            else
            {
                this.RemoveObjeto(objetoAtualizado.nome);
                this.RegistraObjeto(objetoAtualizado.Clone());
            }
        }

        /// <summary>
        /// atualiza um objeto. se não tiver no escopo, registra e atualiza.
        /// </summary>
        /// <param name="objetoAtualizado">objeto a ser registrado/atualizado.</param>
        public void UpdateWithCreateObjeto(Objeto objetoAtualizado)
        {
            if (objetoAtualizado == null) 
            {
                this.RegistraObjeto(objetoAtualizado);
            }
            else
            {
                this.RemoveObjeto(objetoAtualizado.nome);
                this.RegistraObjeto(objetoAtualizado.Clone());
            }
        }


        /// <summary>
        /// retorna true se econtrar um operador registrado com: nome, tipo parametros.
        /// </summary>
        /// <param name="nomeOperador">nome do operador.</param>
        /// <param name="tipoDoOPerador">tipo do operador.</param>
        /// <param name="escopo">contexto onde o operador está.</param>
        /// <returns></returns>
        public bool ValidaOperador(string nomeOperador, string tipoDoOPerador, Escopo escopo)
        {

            if (RepositorioDeClassesOO.Instance().ExisteClasse(tipoDoOPerador))
            {
                if (Expressao.headers != null)
                {
                    HeaderClass header = Expressao.headers.cabecalhoDeClasses.Find(k => k.nomeClasse == tipoDoOPerador);
                    if (header == null)
                        return false;
                    else
                    {
                        return header.operators.Find(k => k.name == nomeOperador) != null;
                    }
                }

            }
            if (escopo.tabela.GetOperadores().Find(k => k.nome.Equals(nomeOperador) && (k.tipoRetorno.Equals(tipoDoOPerador))) != null)
                return true;

            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return ValidaOperador(nomeOperador, tipoDoOPerador, escopo.escopoPai);
            return false;
        } // ValidaOperador()
    
    
        /// <summary>
        /// retorna true se o nome de classe parametro é de uma classe registrada.
        /// </summary>
        /// <param name="nomeClasse">nome da classe.</param>
        /// <param name="escopo">contexto onde a classe está.</param>
        /// <returns></returns>
        public bool IsClasse(string nomeClasse, Escopo escopo)
        {
            if (escopo == null)
                return false;
            int indexClasse = escopo.tabela.GetClasses().FindIndex(k => k.GetNome() == nomeClasse);
            if (indexClasse != -1)
                return true;
            if (escopo.ID == Escopo.tipoEscopo.escopoNormal)
                return IsClasse(nomeClasse, escopo.escopoPai);
            return false;

        }  // IsClasse()

    
 

    

     
        /// <summary>
        /// adiciona um objeto na tabela de valores.
        /// </summary>
        /// <param name="acessor">tipo de acessor: public, protected, private.</param>
        /// <param name="nome">nome do objeto.</param>
        /// <param name="tipo">classe do objeto.</param>
        /// <param name="valor">valor que o objeto guarda.</param>
        /// <param name="escopo">contexto onde o objeto está.</param>
        public void AddObjeto(string acessor,string nome, string tipo, object valor, Escopo escopo)
        {
            Objeto objeto = new Objeto(acessor, tipo, nome, valor);
            objeto.SetTipoElement(tipo);
            escopo.tabela.objetos.Add(objeto);
        }

  
        /// <summary>
        /// retorna o tipo de retorno de uma funcao.
        /// </summary>
        /// <param name="nomeClasseDaFuncao">nome da classe da função.</param>
        /// <param name="nomeFuncao">nome da função.</param>
        /// <param name="escopo">contexto onde a função está.</param>
        /// <returns></returns>
        public string GetTypeFunction(string nomeClasseDaFuncao, string nomeFuncao, Escopo escopo)
        {
            if (escopo == null)
                return null;
            Metodo funcao = escopo.tabela.Funcoes.Find(k => k.nome == nomeFuncao);
            if (funcao != null)
                return funcao.tipoReturn;

            Metodo umaFuncaoDoRepositorioDeClasses = RepositorioDeClassesOO.Instance().GetClasse(nomeClasseDaFuncao).GetMetodos().Find(k => k.nome.Equals(nomeFuncao));
            if (umaFuncaoDoRepositorioDeClasses != null)
                return umaFuncaoDoRepositorioDeClasses.tipoReturn;

            if (escopo.escopoPai.ID != Escopo.tipoEscopo.escopoGlobal)
                return GetTypeFunction(nomeClasseDaFuncao, nomeFuncao, escopo.escopoPai);
            return null;
        }

      
        /// <summary>
        /// retorna o objeto com nome parametro, se estiver registrado nesta tabela.
        /// </summary>
        /// <param name="nomeObjeto">nome do objeto.</param>
        /// <param name="escopo">contexto onde o objeto está.</param>
        /// <returns></returns>
        public Objeto IsObjetoRegistrado(string nomeObjeto, Escopo escopo)
        {
            if (escopo == null)
                return null;
            Objeto objeto = escopo.tabela.objetos.Find(k => k.GetNome() == nomeObjeto);
            if (objeto != null)
                return objeto;
            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return IsObjetoRegistrado(nomeObjeto, escopo.escopoPai);
            return null;
        } // IsObjectRegistrade()



        /// <summary>
        /// retorna true se a funcao de nome parametro, está registrada nesta tabela.
        /// </summary>
        /// <param name="escopo">contexto onde a funcao está.</param>
        /// <param name="nameFuncion">nome da função.</param>
        /// <returns></returns>
        public bool IsFunction(Escopo escopo, string nameFuncion)
        {
            return (IsFunction(nameFuncion, escopo) != null);
        }


        /// <summary>
        /// verifica se um token é nome de uma função.
        /// </summary>
        /// <param name="nomeFuncao">nome da funcao.</param>
        /// <param name="escopo">contexto onde a funcao está.</param>
        public Metodo IsFunction(string nomeFuncao, Escopo escopo)
        {
            if (escopo == null)
                return null;
            Metodo funcao = escopo.tabela.Funcoes.Find(k => k.nome == nomeFuncao);
            if (funcao != null)
                return funcao;

            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return IsFunction(nomeFuncao, escopo.escopoPai);
            return null;
        } // IsFunction()

       

        /// <summary>
        /// retorna true se o objeto de nome parametro está registrado.
        /// </summary>
        /// <param name="nomeObjeto">nome do objeto.</param>
        /// <param name="escopo">contexto onde o objeto está.</param>
        /// <returns></returns>
        public bool ValidaObjeto(string nomeObjeto, Escopo escopo)
        {

            if (escopo == null)
                return false;
            if (escopo.tabela.objetos.Find(k => k.GetNome() == nomeObjeto) != null)
                return true;

            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return ValidaObjeto(nomeObjeto, escopo.escopoPai);
            return false;
        } // ValidaVariavel()

        /// <summary>
        /// valida um objeto, se está registrado com o tipo parametro.
        /// </summary>
        /// <param name="tipoVariavel">classe do objeto.</param>
        /// <param name="escopo">contexto onde o objeto está.</param>
        /// <returns></returns>
        public bool ValidaTipoObjeto( string tipoVariavel, Escopo escopo)
        {
            if (escopo == null)
                return false;
            foreach (Classe umaClasse in escopo.tabela.Classes)
            {
                if (umaClasse.GetNome() == tipoVariavel)
                    return true;
            }// foreach

            
            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return (ValidaTipoObjeto(tipoVariavel, escopo.escopoPai));
            return false;
        } // ValidaTipoVariavel()

        

    } // class TabelaDeValores

    public class Vetor: Objeto
    {
        // o valor de um elemento do vetor é o valor do Objeto associado.
        public new string nome;
        private new string tipo;

        public List<Vetor> tailVetor { get; set; }

        public int[] dimensoes;


        public string GetTiposElemento()
        {
            return tipo;
        }


        public Vetor()
        {
            this.tailVetor = new List<Vetor>();
            this.nome = "";
            this.tipo = "";
           
            this.dimensoes = new int[1]; 
        }


        public Vetor(string acessor, string nome, string tipoElementoVetor, Escopo escopo, params int[] dims) : base(acessor, "Vetor", nome, null)
        {
            Init(nome, tipoElementoVetor, dims);

            for (int x = 0; x < dims.Length; x++) // inicializa as variaveis vetor de elementos, para evitar recursão inifinita.
            {
                this.tailVetor.Add(new Vetor());
                this.tailVetor[tailVetor.Count - 1].Init(nome, tipoElementoVetor, dims);
            }

        }
        public void AddElements(int qtdDeElementos)
        {
            if (this.tailVetor == null)
                this.tailVetor = new List<Vetor>();
            if (qtdDeElementos <= 0)
                return;
            else
                for (int x = 0; x < qtdDeElementos; x++)
                    this.tailVetor.Add(new Vetor());
        }



        private void Init(string nomeVariavel, string tipoElemento, int[] dims)
        {
            this.tipo = tipoElemento;
            this.nome = nomeVariavel;
            this.dimensoes = dims;

            this.tailVetor = new List<Vetor>();

            if (this.dimensoes == null)
                this.dimensoes = new int[1];
        }



        /// <summary>
        /// seta o elemento com os indices matriciais de entrada. 
        /// Util para modificar um elemento com dimensões bem definidas: M[1,5,8] um vetor, e queremos acessar
        /// a variavel: m[0,0,3].
        /// </summary>
        public void SetElementoPorOffset(List<Expressao> exprssIndices, object newValue, Escopo escopo)
        {
            EvalExpression eval = new EvalExpression();
            List<int> indices = new List<int>();
            for (int x = 0; x < exprssIndices.Count; x++)
            {
                exprssIndices[x].isModify = true;
                indices.Add(int.Parse(eval.EvalPosOrdem(exprssIndices[x], escopo).ToString()));
            }
            int indexOffet = this.BuildIndex(indices.ToArray());
            this.tailVetor[indexOffet].SetValor(newValue);
            
        }

        /// <summary>
        /// seta elemento com elementos vetor dentro de vetores, como: [[1,5],2,6,8,[1,3,5]].
        /// Para futuros novos tipos de vetor, como JaggedArray.
        /// </summary>
        public void SetElementoAninhado(object newValue, Escopo escopo, params Expressao[] exprssoesIndices)
        {
            List<int> indices = new List<int>();
            EvalExpression eval = new EvalExpression();
            for (int k = 0; k < exprssoesIndices.Length; k++)
            {
                exprssoesIndices[k].isModify = true;
                indices.Add(int.Parse(eval.EvalPosOrdem(exprssoesIndices[k], escopo).ToString()));
            }

            Vetor vDinamico = this;
            for (int x = 0; x < indices.Count - 1; x++)
                if (vDinamico.tailVetor[indices[x]].GetType() == typeof(Vetor))
                  vDinamico = vDinamico.tailVetor[indices[x]]; // o elemento do vetor eh outro vetor.
                else
                {
                    vDinamico.tailVetor[indices[x]].SetValor(newValue); // o elemento eh um objeto, não um Vetor.
                    return;
                }

            vDinamico.tailVetor[indices.Count - 1].SetValor(newValue);
        }
        

        /// <summary>
        /// constroi um indice de acessso de vetores com varias dimensoes. Eh um offset de
        /// endereço onde esta localizado a variavel que queremos acessar, dentro da variavel vetor.
        /// Util quando temos um vetor como: vetor[4,5,7], e queremos o elemento vetor[1,2,5].
        /// </summary>
        public int BuildIndex(int[] indices)
        {
            if (indices.Length != this.dimensoes.Length)
                return -1;

            int indiceTotal = 0;
            for (int x = 0; x < this.dimensoes.Length; x++)
            {
                indiceTotal += (this.dimensoes[x] - 1) * indices[x];
            }
            return indiceTotal;
        }


        public object GetElemento(Escopo escopo, params int[] indices)
        {

            int indexElemento = BuildIndex(indices);
            return this.tailVetor[indexElemento].GetValor();
        }


     
    }
} // namespace
