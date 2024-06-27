using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics.Tracing;
using System.Security.Policy;
using System.Runtime.CompilerServices;
using parser.ProgramacaoOrentadaAObjetos;
namespace parser
{
    /// <summary>
    /// classe que reune objetos, funcões, dentro de um determinado contexto, como classes, metodos, blocos de instrucoes.
    /// </summary>
    public class Escopo
    {

        /// <summary>
        /// tipos de escopos.
        /// </summary>
        public enum tipoEscopo { escopoGlobal, escopoNormal};


        /// <summary>
        ///  tipo do escopo currente.
        /// </summary>
        public tipoEscopo ID = tipoEscopo.escopoNormal;

        /// <summary>
        /// primeiro escopo do codigo, acessso a todos outros escopos de classes, funcoes, blocos.
        /// </summary>
        public static Escopo escopoROOT;

        /// <summary>
        /// escopo onde o codigo currente está sendo executado.
        /// </summary>
        public static Escopo escopoCURRENT;

        /// <summary>
        /// nome id do escopo.
        /// </summary>
        public string nome = "";

        /// <summary>
        /// escopos de mesmo nivel horizontal.
        /// </summary>
        public List<Escopo> escopoFolhas = null;

        /// <summary>
        /// escopo um nivel acima.
        /// </summary>
        private Escopo _escopoPai = null;

        /// <summary>
        /// lista de sequencias id encontradas neste escopo.
        /// </summary>
        public List<UmaSequenciaID> sequencias;

        /// <summary>
        /// nome do escopo.
        /// </summary>
        public string nomeEscopo = "";

        /// <summary>
        /// escopo raiz do escopo currente. Se o escopo pai for null, e o escopo não for escopoGlobal, retorna o escopoGlobal.
        /// </summary>
        public Escopo escopoPai
        {
            get
            {
                if (this.ID == tipoEscopo.escopoNormal)
                    return _escopoPai;
                if (this.ID == tipoEscopo.escopoGlobal)
                    return escopoROOT;
                return _escopoPai;
            }
            set
            {
                _escopoPai = value;
            } 
        } // escopoPai

        /// <summary>
        /// nome da classe sendo compilada.
        /// </summary>
        public static string nomeClasseCurrente;


        /// <summary>
        /// contém as variáveis, funções, métodos, propriedades, classes registradas neste escopo.       
        /// </summary>
        public TablelaDeValores tabela { get; set; }



        /// <summary>
        /// lista de erros na escrita de codigo feito pelo programador quando utiliza a linguagem orquidea.
        /// </summary>
        public List<string> MsgErros = new List<string>();

        /// <summary>
        /// codigo contido no contexto do escopo;
        /// </summary>
        public List<string> codigo;

        private static  UmaGramaticaComputacional linguagem = LinguagemOrquidea.Instance();


        /// <summary>
        /// retorna a lista de mensagens de erros, encontrados no processamento deste escopo. LEGADO, 
        /// os erros estão em [SystemInit.errorsInCopiling].
        /// </summary>
        /// <returns></returns>
        public List<string> GetMsgErros()
        {
            
            return MsgErros;
        }


        /// <summary>
        /// escopo deste contexto.
        /// </summary>
        /// <param name="code">linha de todo codigo, para fins de encontrar falhas.</param>
        public Escopo(string code)
        {
            if (code == null)
                codigo = new List<string>();
            else
                codigo = new Tokens(code).GetTokens();

            this.ID = tipoEscopo.escopoNormal;
            this.MsgErros = new List<string>();

            this.nomeEscopo = code;
            this.tabela = new TablelaDeValores(codigo);
            this.escopoFolhas = new List<Escopo>();
            this.sequencias = new List<UmaSequenciaID>();

            if (PosicaoECodigo.lineCurrentProcessing == 0)
                PosicaoECodigo.AddLineOfCode(Utils.OneLineTokens(codigo));

        }


        /// <summary>
        /// constroi a rede de escopos para um programa.
        /// </summary>
        /// <param name="codigo">trecho de código bruto, sem conversao de tokens.</param>
        /// 
        public Escopo(List<string> codigo)
        {
            if (codigo != null)
                this.codigo = codigo.ToList<string>();
            else
                this.codigo = new List<string>();
      
            this.ID = tipoEscopo.escopoNormal;
            this.MsgErros = new List<string>();

            if (codigo != null)
            {
                this.nomeEscopo = Util.UtilString.UneLinhasLista(codigo);
            }
            

            this.tabela = new TablelaDeValores(codigo);
            this.escopoFolhas = new List<Escopo>();
            this.sequencias = new List<UmaSequenciaID>();

            if (PosicaoECodigo.lineCurrentProcessing == 0)
                PosicaoECodigo.AddLineOfCode(Utils.OneLineTokens(codigo));
        } // ContextoEscopo()

        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="escopo">escopo a copiar.</param>
        public Escopo(Escopo escopo)
        {
           
            this.ID = escopo.ID;
            this.tabela = new TablelaDeValores(codigo);
            this.MsgErros = new List<string>();
            this.nomeEscopo = escopo.nomeEscopo;
            
            
            this.codigo = escopo.codigo.ToList<string>();

            
            this.tabela = escopo.tabela.Clone();
           
            this.escopoFolhas = escopo.escopoFolhas.ToList<Escopo>();
            this.sequencias = escopo.sequencias.ToList<UmaSequenciaID>();

            for (int x = 0; x < escopo.escopoFolhas.Count; x++)
                this.escopoFolhas.Add(new Escopo(escopo.escopoFolhas[x]));

            
        } // Escopo()

        /// <summary>
        /// construtor vazio.
        /// </summary>
        private Escopo()
        {
           
        }


   
        /// <summary>
        /// retorna um clone deste escopo.
        /// </summary>
        /// <returns></returns>
        public Escopo Clone()
        {
            Escopo escopo = new Escopo(this.codigo);
            if (this.tabela != null)
            {
                escopo.tabela = this.tabela.Clone();
            }
            else
            {
                escopo.tabela = new TablelaDeValores(this.codigo);
            }


            // copia os escopos folha para o escopo clone.
            if (this.escopoFolhas != null)
            {
                for (int x = 0; x < this.escopoFolhas.Count; x++)
                {
                    escopo.escopoFolhas.Add(this.escopoFolhas[x].Clone());
                }
            }
            return escopo;
        } // Clone()

        /// <summary>
        /// verifica se um objeto está presente neste escopo.
        /// </summary>
        /// <param name="nomeObjeto">nome do objeto a verificar.</param>
        /// <returns>true se o objeto está no escopo.</returns>
        public bool isPresentObject(string nomeObjeto)
        {
            return this.tabela.GetObjeto(nomeObjeto, this) != null;
        }


        public void Dispose()
        {
            this.codigo = null;
            this.tabela = null;
            this.sequencias = null;
            this.MsgErros = null;
            this.escopoFolhas = null;

        }
        /// <summary>
        /// escreve no terminal, os tokens de chamadas de metodo. LEGADO.
        /// </summary>
        /// <param name="chamadaAMetodo"></param>
        /// <param name="indexChamada"></param>
        /// <returns></returns>
        public string WriteCallAtMethod(List<List<string>> chamadaAMetodo, int indexChamada)
        {
            string str = "";
            string nomeFuncaoChamada = chamadaAMetodo[indexChamada][0];
            str += nomeFuncaoChamada;
            str += "( ";
            for (int x = 1; x < chamadaAMetodo[indexChamada].Count - 1; x++)
                str += chamadaAMetodo[indexChamada][x] + ",";
            str += chamadaAMetodo[indexChamada][chamadaAMetodo[indexChamada].Count - 1] + ")";
            return str;
        }  //WriteCallMethods()


 
    } // class ContextoEscopo
} // namespace
