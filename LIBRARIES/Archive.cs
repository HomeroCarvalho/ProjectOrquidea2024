using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace parser
{
    public class Archive:Objeto
    {

        /// <summary>
        /// le bytes de um arquivo.
        /// </summary>
        /// <param name="path">caminho do arquivo.</param>
        /// <param name="vecBytes">vector retorno com os bytes lidos.</param>
        /// <returns>retorna um vetor com os bytes lidos, se nao houver bytes, retorna um vetor vazio.</returns>
        public static Vector ReadBytes(string path, Vector vecBytes)
        {
            // le todos bytes de uma vez só! otimização sem duvida..          
            byte[] data = File.ReadAllBytes(path);
            vecBytes.Clear();

            if ((data != null) && (data.Length > 0))
            {
                for (int i = 0; i < data.Length; i++)
                {
                    vecBytes.Append(data[i]);
                }
            }

            return vecBytes;
        }


        /// <summary>
        /// grava bytes para um arquivo.
        /// </summary>
        /// <param name="path">caminho do arquivo.</param>
        /// <param name="data">vetor com os dados (byte)</param>
        public static void WriteBytes(string path, Vector data)
        {
            if ((data != null) && (data.size() > 0))
            {
                byte[] dataBytes = new byte[data.size()];
                for (int i = 0; i < dataBytes.Length; i++)
                {
                    dataBytes[i] = (byte)data.Get(i);
                }

                File.WriteAllBytes(path, dataBytes);
            }
        }

        /// <summary>
        /// le linhas de texto de um arquvivo.
        /// </summary>
        /// <param name="path">caminho do arquivo.</param>
        /// <returns>retorna um vetor contendo linhas de texto do arquivo.</returns>
        public static void ReadText(string path, Vector vectTexts)
        {

            string[] linhaDeTexto = File.ReadAllLines(path);
            if (linhaDeTexto!=null)
            {
              
                foreach (string line in linhaDeTexto)
                {
                    vectTexts.Append(line);
                }
            }

            
        }

        /// <summary>
        /// retorna true se o arquivo existe.
        /// </summary>
        /// <param name="path">caminho para o arquivo a verificar.</param>
        /// <returns></returns>
        public static bool Exists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// retorna o caminho completo do arquivo.
        /// </summary>
        /// <param name="nameFile">nome do arquivo.</param>
        /// <returns></returns>
        public static string GetFullPath(string nameFile)
        {
            return Path.GetFullPath(nameFile);
        }

        /// <summary>
        /// retorna o nome do arquivo.
        /// </summary>
        /// <param name="fullPath">caminho completo ate o arquivo.</param>
        /// <returns></returns>
        public static string GetNameFile(string fullPath)
        {
            return Path.GetFileName(fullPath);
        }


        /// <summary>
        /// grava linhas de texto, para um arquivo.
        /// </summary>
        /// <param name="path">caminho do arquivo.</param>
        /// <param name="data">dados de texto.</param>
        public static void WriteText(string path, Vector data)
        {
            if ((data != null) && (data.size() > 0)) 
            {
                List<string> lstData = new List<string>();
                for (int i = 0; i < data.size(); i++)
                {
                    lstData.Add((string)data.Get(i));
                }

                File.WriteAllLines(path, lstData.ToArray());
            }
           
        }

        public class Testes: SuiteClasseTestes
        {
            public Testes() : base("testes para biblioteca de arquivos")
            {

            }

            public void TesteProgramaAchive(AssercaoSuiteClasse assercao)
            {
       
                try
                {
                    SystemInit.InitSystem();


                    string aspas = "\"";

                    string path = "fileText2.txt";
                    string code_vector = "Vector palavras[1]; palavras.Clear(); palavras.Append(" + aspas + "Terra" + aspas + "); palavras.Append(" + aspas + "Azul" + aspas + ");";
                    string code_file_write = "Archive.WriteText(" + aspas + path + aspas + ", palavras);";
                    string code_file_read = "Vector palavras_leitura[1]; palavras_leitura.Clear(); Archive.ReadText(" + aspas + path + aspas + ", palavras_leitura);";


                    ProcessadorDeID compilador = new ProcessadorDeID(code_vector + code_file_write + code_file_read);
                    compilador.Compilar();



                    ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                    programa.Run(compilador.escopo);

                    assercao.IsTrue(((Vector)compilador.escopo.tabela.GetObjeto("palavras_leitura", compilador.escopo)).size() == 2, code_vector + code_file_write + code_file_read);
                }
                catch(Exception ex)
                {
                    assercao.IsTrue(false,"TESTE FALHOU:  "+ex.Message);    

                }

            }
            public void TesteGravaLeTextos(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string path = "fileText.txt";
                List<string> textos = new List<string>() { "A Terra eh azul", "A Amazonia eh verde das arvores", "as pessoas podem ser azul da Terra ou verde da Amazonia", "azul e verde sao cores" };
                Vector vtWrite = new Vector();
                vtWrite.Clear();
                for (int i = 0; i < textos.Count; i++)
                {
                    vtWrite.Append(textos[i]);
                }

                Archive.WriteText(path, vtWrite);

                Vector vtRead = new Vector();
                vtRead.Clear();
                Archive.ReadText(path, vtRead);

                try
                {
                    assercao.IsTrue(vtRead.size() == 4, "leitura e gravacao de textos em arquivos");
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }
            }

            public void TesteGravaLeBytes(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string path = "fileBytes.bin";
                Vector vWrite= new Vector();
                vWrite.Clear();
                Random rnd = new Random();

                for (int i = 0; i < 200; i++)
                {
                    byte umDado = (byte)rnd.Next(256);
                    vWrite.Append(umDado);
                }

                Archive.WriteBytes(path, vWrite);

                Vector vRead= new Vector();
                vRead = Archive.ReadBytes(path, vRead);

                try
                {
                    assercao.IsTrue(vRead.size() == 200,"contagem de bytes gravados e lidos");
                    assercao.IsTrue((byte)vRead.Get(0) == (byte)vWrite.Get(0), "conteudo dos arquivos");
                }
                catch(Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }

            }
    

        }
    }
}
