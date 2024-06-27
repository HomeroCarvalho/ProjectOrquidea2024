using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace parser
{
    /// <summary>
    /// classe do propriedades do sistema de lingaguagem.
    /// </summary>
    public class SystemInit
    {
        /// <summary>
        /// se [true], o file header é calculado.
        /// </summary>
        public static bool isResetFILE_HEADER = true;
        /// <summary>
        /// se [true], o classficador será resetado.
        /// </summary>
        public static bool isResetClassificador = false;

        /// <summary>
        /// se [true] as classes do codigo orquidea sao compilados no header.
        /// </summary>
        public static bool isResetLoadClasseCode = true;

        /// <summary>
        /// lista de classes que podem ter o 1o, argumento como o objeto caller.
        /// </summary>
        public static List<string> classeFirstArgumntAsObjectCaller = new List<string>() { "double", "string" };

        /// <summary>
        /// lista de erros encontrados na compilação.
        /// </summary>
        public static List<string> errorsInCopiling = new List<string>();

        /// <summary>
        /// lista de linhas, afim de contar com linhas de comentarios.
        /// </summary>
        public static List<int> contadorDeLinhas = new List<int>();



        public static void InitSystem()
        {

            ProcessadorDeID.lineInCompilation = 0;
            // reseta a lista de contador de linhas de instrucao.
            contadorDeLinhas.Clear();
            // reseta a lista de erros em compilação.
            errorsInCopiling.Clear();
            // reseta o file header.
            isResetFILE_HEADER = true;
            // reseta a construcao do escopo raiz.
            //Escopo.escopoROOT = null;
            // inicializa o escopo currente.
            Escopo.escopoCURRENT = new Escopo("");
            // reseta a tabela hash do classificador.
            Classificador.TabelaHash.isReset = true;
            // reseta a tabela de expressoes possiveis de otimizar.
            TablelaDeValores.expressoes = null;

            // reseta as bibliotecas carregadas externamente.
            LinguagemOrquidea.libraries = new List<System.Reflection.Assembly>();
        }
    }
}
