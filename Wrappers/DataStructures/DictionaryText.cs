using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using parser;
namespace parser
{
    /// <summary>
    /// classe mapa de chave string e valor generico.
    /// </summary>
    public class DictionaryText: Objeto
    {

        /// <summary>
        /// dicionario contendo o mapa.
        /// </summary>
        public Dictionary<string, object> _dict;


        /// <summary>
        /// faz uma copia do objeto parametro.
        /// </summary>
        /// <param name="objAClonar">objeto a clonar (sendo DictionaryText também)</param>
        /// <returns>retorna uma copia DicitonaryText do objeto parametro.</returns>
        public static DictionaryText Clone(Objeto objAClonar)
        {
            DictionaryText d1= (DictionaryText)objAClonar;
            DictionaryText dClone= new DictionaryText();
            dClone._dict = new Dictionary<string, object>();
            foreach(KeyValuePair<string, object> valores in d1._dict)
            {
                dClone._dict[valores.Key] = valores.Value;
            }

            dClone.nome = d1.nome;
            dClone.tipoElemento = d1.tipoElemento;
            dClone.tipo = d1.tipo;
            dClone.isWrapperObject = true;
            dClone.valor = d1.valor;
            dClone.isReadOnly = d1.isReadOnly;
       
            return dClone;
        }

        /// <summary>
        /// converte um texto, todos elementos do dicionario.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = "";
            if (_dict != null)
            {

                foreach (KeyValuePair<string, object> umItem in _dict)
                {
                    if ((umItem.Key != null) && (umItem.Value != null)) 
                    {
                        str += "[" + umItem.Key + ":" + umItem.Value + "],";
                    }
                    
                }
                str = str.Remove(str.Length - 1);
                
            }
            return str;
        }


        /// <summary>
        /// faz o casting de um object para o dicionaryText;
        /// </summary>
        /// <param name="vt">dictionary text parametro.</param>
        public void Casting(object vt)
        {
            DictionaryText dictToCopy=(DictionaryText)vt;
            if (dictToCopy._dict != null)
            {
                if (this._dict == null)
                {
                    this._dict = new Dictionary<string, object>();
                }
                

                foreach(KeyValuePair<string,object> umItem in dictToCopy._dict)
                {
                    this._dict.Add(umItem.Key, umItem.Value);
                }
            }
            else
            {
                this._dict=null;
            }
        }

        /// <summary>
        /// construtor vazio.
        /// </summary>
        public DictionaryText()
        {
            _dict = new Dictionary<string, object>();
            this.tipo = "DictionaryText";
            this.tipoElemento = "string";
        }

        /// <summary>
        /// seta o elemento com [chave]=[valor].
        /// </summary>
        /// <param name="chave">chave de identificação.</param>
        /// <param name="novoValorDoElemento">novoValor.</param>
        public void SetElement(string chave, object novoValorDoElemento)
        {
            if (this.valor != null)
            {
                DictionaryText dicAtualizado= (DictionaryText)this.valor;
                dicAtualizado._dict[chave]= novoValorDoElemento;
                _dict[chave] = novoValorDoElemento;
            }
            else
            {
                _dict[chave] = novoValorDoElemento;
            }
            
           
        }

        /// <summary>
        /// obtem o elemento com a chave parametro.
        /// </summary>
        /// <param name="chave">chave de identificação.</param>
        /// <returns></returns>
        public object GetElement(string chave)
        {
            if (this.valor != null)
            {
                DictionaryText dicAtualizado= (DictionaryText)this.valor;
                return dicAtualizado._dict[chave];
            }
            else
            {
                return _dict[chave];
            }
            
        }
        /// <summary>
        /// remove um elemento.
        /// </summary>
        /// <param name="chave">chave do elemento a remover.</param>
        public void RemoveElement(string chave)
        {
            _dict.Remove(chave);
        }

        /// <summary>
        /// retorna quantos elementos tem o dicionario.
        /// </summary>
        /// <returns></returns>
        public int Size()
        {
            return _dict.Count;
        }

        /// <summary>
        /// retorna um vetor com todos valores de chave do dicionario.
        /// </summary>
        /// <returns></returns>
        public Vector GetKeys()
        {
            int contKeys = 0;
            Vector vt_retorno = new Vector(Size());
            Dictionary<string, object>.Enumerator iterator = _dict.GetEnumerator();
            while (iterator.MoveNext())
            {
                vt_retorno.SetElement(iterator.Current.Key, contKeys);
                contKeys++;
            }
            return vt_retorno;
        }

        /// <summary>
        /// retorna um vetor com todos value do dicionario.
        /// </summary>
        /// <returns></returns>
        public Vector GetValues()
        {
            int contKeys = 0;
            Vector vt_retorno = new Vector(Size());
            Dictionary<string, object>.Enumerator iterator = _dict.GetEnumerator();
            while (iterator.MoveNext())
            {
                vt_retorno.SetElement(iterator.Current.Value, contKeys);
                contKeys++;
            }
            return vt_retorno;
        }


        /// <summary>
        /// instancia um dictionary text, a partir de uma chamada de metodo. 
        /// </summary>
        /// <param name="obj">objeto instanciado e registrado, em tempo de compilação.</param>
        public void Create()
        {
            this._dict = new Dictionary<string, object>();
            this.tipo= "DictionaryText";
            this.isWrapperObject = true;
          
 
        }


        /// <summary>
        /// retorna o tipo dos elementos constituinte.
        /// </summary>
        /// <returns></returns>
        public new string GetTipoElement()
        {
            return "Object";
        }


        /// <summary>
        /// atualiza um elemento do dictionary text, com dados de um objeto [actual].
        /// </summary>
        /// <param name="wrapperObject">wrapper dictionary text.</param>
        /// <param name="caller">objeto actual contendo dados atualizados de processamento.</param>
        /// <param name="index">indices de acesso ao elemento.</param>
        public void UpdateFromActualObject(Objeto caller, Objeto wrapperObject, int index1, int index2)
        {
            Objeto objetoAtual = caller.Clone();
            objetoAtual.nome = wrapperObject.nome + index1.ToString();

            DictionaryText dictionary = (DictionaryText)wrapperObject;

            dictionary.SetElement(index1.ToString(), objetoAtual);

        }




        public new class Testes : SuiteClasseTestes
        {
            public Testes() : base("teste classe wrapper dictionary text")
            {
            }

            public void TesteInstanciacao(AssercaoSuiteClasse assercao)
            {
                DictionaryText dict = new DictionaryText();
                dict.SetNome("b");

                assercao.IsTrue(dict != null, "validacao para instanciacao de objeto dictionary text.");
                assercao.IsTrue(dict.GetNome() == "b", "validacao para nome de objeto dictionary text.");
            }


            public void TesteSETElementGETElement(AssercaoSuiteClasse assercao)
            {
                DictionaryText dict = new DictionaryText();
                dict.SetNome("b");

                dict._dict["fruta"] = "uva";

                assercao.IsTrue(dict != null, "validacao para instanciacao de objeto dictionary text.");
                assercao.IsTrue(dict._dict != null && dict._dict["fruta"] != null);


            }
        }
    }
}
