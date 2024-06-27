using ParserLinguagemOrquidea.Wrappers;
using System.Collections.Generic;
using System.Linq;

using Wrappers;

namespace parser
{
    public class GerenciadorWrapper
    {
        List<WrapperData> wrappers;


        public GerenciadorWrapper()
        {
            this.wrappers = new List<WrapperData>();

            this.wrappers.Add(new WrapperDataDictionaryText());
            this.wrappers.Add(new WrapperDataVector());
            this.wrappers.Add(new WrapperDataMatriz());
            this.wrappers.Add(new WrapperDataJaggedArray());

        }


        /// <summary>
        /// instancia objetos wrapper estrutures, se forem objetos wrappers.
        /// </summary>
        /// <param name="tokensExpresssao">tokens contendo a instanciacao wrapper.</param>
        /// <param name="escopo">contexto onde os tokens estão.</param>
        /// <returns>[true] se os tokens são de uma instanciacao wrapper, [false] se  não.</returns>
        public List<string> IsToWrapperInstantiate(string[] tokensExpresssao, Escopo escopo)
        {
            for (int i = 0; i < wrappers.Count; i++)
            {
                // verifica se a expressão é uma instanciação de wrapper data objeto.
                if (wrappers[i].IsInstantiateWrapperData(tokensExpresssao.ToList<string>()))
                {
                    List<string> tokensProcessed = new List<string>();
                    string exprssCreate = Util.UtilString.UneLinhasLista(tokensExpresssao.ToList<string>());
                    // chamada a instanciação do wrapper data objeto, identificado.
                    List<string> tokensInstanciacao = wrappers[i].CREATE(ref exprssCreate, escopo, ref tokensProcessed);
                    return tokensInstanciacao;
                }

            }
            return null;
        }




    }





}
