
using MathNet.Numerics.Interpolation;
using parser;
using parser.ProgramacaoOrentadaAObjetos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modulos
{

    /// <summary>
    /// class to read/write text, numbers, in terminal.
    /// read too arguments passed via terminal command.
    /// all methods are static, dont instantiate object
    /// this class.
    /// this methods are executable like functions and pass to code orquidea.
    /// Homero T. Carvalho 2022, vs. 1.0.
    /// </summary>
    public class Prompt
    {

        /// <summary>
        /// caracter de literais constantes.
        /// </summary>
        public static string aspas = "\"";

        /// <summary>
        /// caracter delimitador para char constantes.
        /// </summary>
        public static string singleQuote = "\'";



        /// <summary>
        /// imprime textos a partir de um vetor multi-argumento.
        /// </summary>
        /// <param name="text">texto base.</param>
        /// <param name="subTextos">subtextos inseridos no texto base.</param>
        public static void xWrite(string text, Vector subTextos)
        {
            List<int> indexOcurrences = new List<int>();
            int offset = 0;
            int index = text.IndexOf("%d");
            while (index != -1)
            {
                indexOcurrences.Add(index);
                offset = index + 1;
                if (offset < text.Length)
                {
                    index = text.IndexOf("%d", offset);
                }
                else
                {
                    break;
                }
            }

            // reserva espaço suficiente para inserção.
            int lngth = 0;
            for (int i = 0; i < subTextos.size(); i++)
            {
                lngth += subTextos.Get(i).ToString().Length;      
            }

            for (int i = 0; i < lngth; i++)
            {
                text += " ";
            }

            // remove a formatacao do texto.
            text = text.Replace("%d", "");


            // insere os sub-textos contido no vector parametro.
            if (indexOcurrences.Count > 0)
            {
                int offfsetOcrurrence = 0;
                

                for (int i = 0; i < indexOcurrences.Count; i++)
                {
                    string aTextElemento = subTextos.Get(i).ToString();
                    text = text.Insert(indexOcurrences[i] + offfsetOcrurrence, aTextElemento);
                    offfsetOcrurrence+= aTextElemento.Length;
                }

                
            }
            System.Console.WriteLine(text);
        }


        /// <summary>
        /// um prompt de saldacao. se precisar de escrever algo na tela, mas há a possibilidade de erro de formatacao,
        /// utilizar esta funcao.
        /// </summary>
        public static void xWriteHelloWorld()
        {
            System.Console.WriteLine("hello world!");
        }



        /// <summary>
        /// Read a text from terminal.
        /// </summary>
        /// <param name="caption">text info.</param>
        /// <returns>return a text read from terminal.</returns>
        public static string sRead(string caption)
        {
            caption = RemoveAspas(caption);

            Console.Write(caption + ":   ");
            string textRead = Console.ReadLine();

            return textRead;

        }

        /// <summary>
        /// Read an int number from terminal.
        /// </summary>
        /// <param name="caption"></param>
        /// <returns>return an int, if valid.throw a execption for invalid data input .</returns>
        public static int iRead(string caption)
        {
            caption = RemoveAspas(caption);

            Console.WriteLine(caption + ":   ");
            string textRead = Console.ReadLine();

            int number = 0;
            if (!int.TryParse(textRead, out number))
            {
                throw new Exception("Input for int invalid.");
            }

            return number;
        }


        /// <summary>
        /// Read an float number from terminal.
        /// </summary>
        /// <param name="caption">text info.</param>
        /// <returns>return a float, if valid. throw a execption for invalid data input.</returns>
        public static float fRead(string caption)
        {
            Console.WriteLine(caption + "  :");
            string textRead = Console.ReadLine();

            float number = 0.0f;
            if (!float.TryParse(textRead, out number))
            {
                throw new Exception("Input for int invalid.");
            }

            return number;
        }

        /// <summary>
        /// Read an double number from terminal.
        /// </summary>
        /// <param name="caption">text info.</param>
        /// <returns>return an double, if valid. throw a execption for invalid data  input.</returns>
        public static double dRead(string caption)
        {
            caption = RemoveAspas(caption);

            Console.WriteLine(caption + ":  ");
            string textRead = Console.ReadLine();

            double number = 0.0;
            if (!double.TryParse(textRead, out number))
            {
                throw new Exception("Input for int invalid.");
            }

            return number;
        }






        /// <summary>
        /// Write a text to terminal.
        /// </summary>
        /// <param name="writeText">text to write. rise if text to write is null.</param>
        public static void sWrite(string writeText)
        {
            writeText = RemoveAspas(writeText);

            if (writeText == null)
            {
                return;
            }
            else
            {
                Console.WriteLine(writeText);
            }

        }

        /// <summary>
        /// Read line arguments CLI. 
        /// </summary>
        /// <returns>return a array of args passed in terminal console.</returns>
        public static string[] GetArgs()
        {
            return Environment.GetCommandLineArgs();
        }


        /// <summary>
        /// remove as aspas de literais constantes.
        /// </summary>
        /// <param name="tex">texto com aspas.</param>
        /// <returns>retorna o texto parametro sem a primeira e ultima aspas, se houver.</returns>
        private static string RemoveAspas(string tex)
        {
            if (tex == null){
                return tex;
            }


            string text = (string)tex.Clone();


            int indexFirstAspas = text.IndexOf(aspas);

            if (indexFirstAspas != -1)
            {
                text = text.Remove(indexFirstAspas, 1);
            }

            int indexLastAspas = text.LastIndexOf(aspas);
            if (indexLastAspas != -1)
            {
                text = text.Remove(indexLastAspas, 1);
            }

            return text;
        }

        /// <summary>
        /// inicializacao da classe, ajustando alguns parametros.
        /// </summary>
        public static void Init()
        {
            
            // mmodificacao de vetor desta funcao, para multi-argumento.
            Classe classePrompt = RepositorioDeClassesOO.Instance().GetClasse("Prompt");
            if (classePrompt != null)
            {
                Metodo funcaoWrite = classePrompt.metodos.Find(
                    k => k.nome == "xWrite" &&
                    k.parametrosDaFuncao.Length > 1
                    && k.parametrosDaFuncao[1].tipo == "Vector");
                if (funcaoWrite != null)
                {
                    classePrompt.metodos.Remove(funcaoWrite);
                    funcaoWrite.parametrosDaFuncao[1].isMultArgument = true;
                    funcaoWrite.parametrosDaFuncao[1].tipoElemento = "string";
                    classePrompt.metodos.Add(funcaoWrite);
                    RepositorioDeClassesOO.Instance().RemoveClasse("Prompt");
                    RepositorioDeClassesOO.Instance().RegistraUmaClasse(classePrompt);
                }


            }

        }


        public class Testes : SuiteClasseTestes
        {

            public Testes():base("testes unitarios classe Prompt")
            {

            }

            public void TestexWrite(AssercaoSuiteClasse assercao)
            {
                string aspas = "\"";

                SystemInit.InitSystem();

                string format = "string format=" + aspas + "Hello, %d  %d  %d" + aspas + ";";
                string primeiroNome = "string firstName=" + aspas + "homero " + aspas + ";"; ;
                string segundoNome = "string lastName=" + aspas + " carvalho" + aspas + ";"; ;
                string formacao= "string formacao=" + aspas + " engenheiro de software" + aspas + ";";


                string code_0_0 = "Prompt.xWrite(format,firstName,lastName,formacao);";
                string code_0_1 = "Prompt.xWrite(format," + aspas + "homero" + aspas + "," + aspas + " carvalho" + aspas + "," + aspas + " engenheiro" + aspas + ");";
                Console.WriteLine(code_0_1);

                ProcessadorDeID compilador = new ProcessadorDeID(format + primeiroNome + segundoNome + formacao + code_0_0 + code_0_1);
                compilador.Compilar();

                ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                programa.Run(compilador.escopo);

            }

        }
    }
}
