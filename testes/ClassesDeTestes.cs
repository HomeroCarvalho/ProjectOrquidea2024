using parser.ProgramacaoOrentadaAObjetos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using parser.textoFormatado;
using ParserLinguagemOrquidea.Wrappers;
using Wrappers;
using ModuloTESTES;
using SFML.Audio;
using Modulos;



namespace parser
{
    public class ClassesDeTestes
    {
        public ClassesDeTestes()
        {


            EvalExpression.Testes testesExpressoes= new EvalExpression.Testes();
            testesExpressoes.ExecutaTestes();

            //LoopGame.Testes testesJogo = new LoopGame.Testes();
            //testesJogo.ExecutaTestes();

            //Expressao.Testes testesExpressoes1= new Expressao.Testes();
            //testesExpressoes1.ExecutaTestes();

            //ExpressaoPorClassificacao.Testes testesExpressoes = new ExpressaoPorClassificacao.Testes();
            //testesExpressoes.ExecutaTestes();

            //Archive.Testes testesArquivos= new Archive.Testes();
            //testesArquivos.ExecutaTestes();

            //Prompt.Testes testeesPrompt= new Prompt.Testes();
            //testeesPrompt.ExecutaTestes();

            //ExpressaoPorClassificacao.Testes2 testesExpresoes = new ExpressaoPorClassificacao.Testes2();
            //testesExpresoes.ExecutaTestes();


            //FileHeader.Testes testesHeaders = new FileHeader.Testes();
            //testesHeaders.ExecutaTestes();

            //Metodo.Testes testesFuncoes = new Metodo.Testes();
            //testesFuncoes.ExecutaTestes();

            //TESTES_DESEMPENHO.Testes testesOtimizacao = new TESTES_DESEMPENHO.Testes();
            //testesOtimizacao.ExecutaTestes();

            //GameLibrary.Testes testesInput= new GameLibrary.Testes();
            //testesInput.ExecutaTestes();

            //ExtratoresOO.Testes testesPropriedades= new ExtratoresOO.Testes();
            //testesPropriedades.ExecutaTestes();



            //Vector.Testes testesMalhaFor = new Vector.Testes();
            //testesMalhaFor.ExecutaTestes();



            //ParserAFile.Testes testesLinhasDeCodigo = new ParserAFile.Testes();
            //testesLinhasDeCodigo.ExecutaTestes();


            //UtilTokens.Testes testesUtil = new UtilTokens.Testes();
            //testesUtil.ExecutaTestes();


            //BuildInstrucoes.Testes testesCreate= new BuildInstrucoes.Testes();
            //testesCreate.ExecutaTestes();



            //Expressao.Testes testesExpressoes = new Expressao.Testes();
            //testesExpressoes.ExecutaTestes();


            //ParserAFile.Testes testesParser = new ParserAFile.Testes();
            //testesParser.ExecutaTestes();

            //BuildInstrucoes.Testes testesIf = new BuildInstrucoes.Testes();
            //testesIf.ExecutaTestes();


            //Text.Testes testesTextos= new Text.Testes();
            //testesTextos.ExecutaTestes();


            //ProcessadorDeID.Testes testesIFElse= new ProcessadorDeID.Testes();
            //testesIFElse.ExecutaTestes();


            //Vector2D.Testes testesPropriedadesVector2D= new Vector2D.Testes();
            //testesPropriedadesVector2D.ExecutaTestes();




            //Sound.Testes testesArquivosSom = new Sound.Testes();
            //testesArquivosSom.ExecutaTestes();




            //TESTES_DESEMPENHO.Testes testesVectorAcess = new TESTES_DESEMPENHO.Testes();
            //testesVectorAcess.ExecutaTestes();


            //WrapperDataVector.Testes testesWrapperVector = new WrapperDataVector.Testes();
            //testesWrapperVector.ExecutaTestes();


            //Vector.Testes testesMalhaForEmVector = new Vector.Testes();
            //testesMalhaForEmVector.ExecutaTestes();


            //GameLibrary.Testes testesBibliotecaDeJogos = new GameLibrary.Testes();
            //testesBibliotecaDeJogos.ExecutaTestes();


            //Imagem.Testes testesDeImagens = new Imagem.Testes();
            //testesDeImagens.ExecutaTestes();

            //Sound.Testes testesDeSom = new Sound.Testes();
            //testesDeSom.ExecutaTestes();


            //LoopGameSFML.Testes  testesEsbocosDeJogos= new LoopGameSFML.Testes();
            //testesEsbocosDeJogos.ExecutaTestes();


            //TESTES_DESEMPENHO.Testes testesMedidasDeOtimizacao = new TESTES_DESEMPENHO.Testes();
            //testesMedidasDeOtimizacao.ExecutaTestes();







            //Vector2D.Testes testesVetores2D= new Vector2D.Testes();
            //testesVetores2D.ExecutaTestes();


            //Instrucao.Testes testesProgramasVM= new Instrucao.Testes();
            //testesProgramasVM.ExecutaTestes();


            //Expressao.Testes testesExpressoes= new Expressao.Testes();
            //testesExpressoes.ExecutaTestes();





            //MapLevelPixels.Testes testesMapa= new MapLevelPixels.Testes();
            //testesMapa.ExecutaTestes();

            //BuildInstrucoes.Testes testesFuncoesImportadas = new BuildInstrucoes.Testes();
            //testesFuncoesImportadas.ExecutaTestes();


            //FileHeader.Testes testesHeaders = new FileHeader.Testes();
            //testesHeaders.ExecutaTestes();


            //ProcessadorDeID.Testes testesCompilacao = new ProcessadorDeID.Testes();
            //testesCompilacao.ExecutaTestes();





            //UtilTokens.Testes testesFuncoes = new UtilTokens.Testes();
            //testesFuncoes.ExecutaTestes();



            //Vector.Testes testesVector = new Vector.Testes();
            //testesVector.ExecutaTestes();






            //Metodo.Testes  testesFuncoes= new Metodo.Testes();
            //testesFuncoes.ExecutaTestes();


            //ParserUniversal.Testes testesParser = new ParserUniversal.Testes();
            //testesParser.ExecutaTestes();




            //ExtratoresOO.Testes testesExtratorClasses= new ExtratoresOO.Testes();
            //testesExtratorClasses.ExecutaTestes();



            //WrapperDataJaggedArray.Testes testesDictionary = new WrapperDataJaggedArray.Testes();
            //testesDictionary.ExecutaTestes();

            //WrapperDataDictionaryText.Testes testesDictionary= new WrapperDataDictionaryText.Testes();
            //testesDictionary.ExecutaTestes();


            //BuildInstrucoes.Testes testesCompilacaoClasses= new BuildInstrucoes.Testes();
            //testesCompilacaoClasses.ExecutaTestes();


            //ProcessadorDeID.Testes testesCompilacao = new ProcessadorDeID.Testes();
            //testesCompilacao.ExecutaTestes();


            //WrapperDataMatriz.Testes testesWrapperMatriz= new WrapperDataMatriz.Testes();
            //testesWrapperMatriz.ExecutaTestes();


            //Instrucao.Testes testesInstrucao = new Instrucao.Testes();
            //testesInstrucao.ExecutaTestes();


            //ProcessadorDeID.Testes testesCompilacao = new ProcessadorDeID.Testes();
            //testesCompilacao.ExecutaTestes();

            //Vector2D.Testes testesVetor2D= new Vector2D.Testes();
            //testesVetor2D.ExecutaTestes();

            //Classificador.Testes testesClassificador = new Classificador.Testes();
            //testesClassificador.ExecutaTestes();





            //Expressao.Testes testesExpressao = new Expressao.Testes();
            //testesExpressao.ExecutaTestes();





            //Metodo.Testes testesFuncoes = new Metodo.Testes();
            //testesFuncoes.ExecutaTestes();


            //Tokens.Testes testesTokenizacao= new Tokens.Testes();
            //testesTokenizacao.ExecutaTestes();



            //ExpressionBase.Testes testesDeExpressaoBase= new ExpressionBase.Testes();
            //testesDeExpressaoBase.ExecutaTestes();


            //ExpressaoPorClassificacao.Testes testesExpressaoClassificacao = new ExpressaoPorClassificacao.Testes();
            //testesExpressaoClassificacao.ExecutaTestes();


            //Objeto.Testes testes1 = new Objeto.Testes();
            //testes1.ExecutaTestes();

            //ExpressaoPorClassificacao.Testes testesProcessamentoPorClassificacao = new ExpressaoPorClassificacao.Testes();
            //testesProcessamentoPorClassificacao.ExecutaTestes();


            //FileHeader.Testes testesExtracaoDeClasses = new FileHeader.Testes();
            //testesExtracaoDeClasses.ExecutaTestes();

            //Metodo.Testes testesFuncoes = new Metodo.Testes();
            //testesFuncoes.ExecutaTestes();

            //Expressao.Testes testesExpressoes= new Expressao.Testes();
            //testesExpressoes.ExecutaTestes();


            //ExpressionBase.Testes testesExpressao= new ExpressionBase.Testes();
            //testesExpressao.ExecutaTestes();


            //WrapperDataVector.Testes testesWrapperVector = new WrapperDataVector.Testes();
            //testesWrapperVector.ExecutaTestes();



            //ProcessadorDeID.Testes testesCompilacao= new ProcessadorDeID.Testes();
            //testesCompilacao.ExecutaTestes();


            //WrapperDataMatriz.Testes testesWrapperMatriz = new WrapperDataMatriz.Testes();
            //testesWrapperMatriz.ExecutaTestes();

            //WrapperDataJaggedArray.Testes testesWrapperJaggedArray= new WrapperDataJaggedArray.Testes();
            //testesWrapperJaggedArray.ExecutaTestes();

            //WrapperDataDictionaryText.Testes testesWrapperDictionary= new WrapperDataDictionaryText.Testes();
            //testesWrapperDictionary.ExecutaTestes();

            //WrapperDataVector.Testes testesWrapperVector= new WrapperDataVector.Testes();
            //testesWrapperVector.ExecutaTestes();



            //ParserAFile.Testes testesParserFile= new ParserAFile.Testes();
            //testesParserFile.ExecutaTestes();


            //ExpressaoGrupos.Testes testesExpressoes = new ExpressaoGrupos.Testes();
            ///testesExpressoes.ExecutaTestes();


            //BuildInstrucoes.Testes testesInstrucaoCases = new BuildInstrucoes.Testes();
            //testesInstrucaoCases.ExecutaTestes();




            //Instrucao.Testes testesProgramasVM = new Instrucao.Testes();
            //testesProgramasVM.ExecutaTestes();

            //UtilTokens.Testes testesFuncoes= new UtilTokens.Testes();
            //testesFuncoes.ExecutaTestes();

            //ProcessadorDeID.Testes testesCompilacao = new ProcessadorDeID.Testes();
            //testesCompilacao.ExecutaTestes();

            //EvalExpression.Testes testesEval = new EvalExpression.Testes();
            //testesEval.ExecutaTestes();

            //FileHeader.Testes testesHeader= new FileHeader.Testes();
            //testesHeader.ExecutaTestes();


            //Testes testesExpressoes = new ExpressaoGrupos.Testes();
            //testesExpressoes.ExecutaTestes();




            //UtilTokens.Testes testesParametros = new UtilTokens.Testes();
            //testesParametros.ExecutaTestes();




            //TextExpression.Testes testesReqexExpressions= new TextExpression.Testes();
            //testesReqexExpressions.ExecutaTestes();

            //Archive.Testes testesArquivos= new Archive.Testes();
            //testesArquivos.ExecutaTestes();


            //Objeto.Testes testesCasting= new Objeto.Testes();
            //testesCasting.ExecutaTestes();



            //Metodo.Testes testesFuncoes= new Metodo.Testes();
            //testesFuncoes.ExecutaTestes();


            //MetodosString.Testes testesFuncoesString = new MetodosString.Testes();
            //testesFuncoesString.ExecutaTestes();


            //WrapperDataVector.Testes testesWrapper= new WrapperDataVector.Testes();
            //testesWrapper.ExecutaTestes();





            //FileHeader.Testes testesheaders = new FileHeader.Testes();
            //testesheaders.ExecutaTestes();


            //UtilTokens.Testes testesUtilTokens = new UtilTokens.Testes();
            //testesUtilTokens.ExecutaTestes();





            //Metodo.Testes testesFuncoes= new Metodo.Testes();
            //testesFuncoes.ExecutaTestes();



            //WrapperData.Testes testesWrapperData = new WrapperData.Testes();
            //testesWrapperData.ExecutaTestes();


            //OperadoresImplementacao.TestesOperadoresBase testesOperadores = new OperadoresImplementacao.TestesOperadoresBase();
            //testesOperadores.ExecutaTestes();


            //ProcessadorDeID.Testes testesCompilacao= new ProcessadorDeID.Testes();
            //testesCompilacao.ExecutaTestes();



            //WrapperDataVector.Testes testesVector= new WrapperDataVector.Testes();
            //testesVector.ExecutaTestes();



            //WrapperDataJaggedArray.Testes testesWrapperJagged= new WrapperDataJaggedArray.Testes();
            //testesWrapperJagged.ExecutaTestes();



            //WrapperDataMatriz.Testes testesMatriz = new WrapperDataMatriz.Testes();
            //testesMatriz.ExecutaTestes();

            //WrapperDataDictionaryText.Testes testesWrapperDicitonary = new WrapperDataDictionaryText.Testes();
            //testesWrapperDicitonary.ExecutaTestes();






            //WrapperData.Testes testesWrapperDataExpressaoResultante = new WrapperData.Testes();
            //testesWrapperDataExpressaoResultante.ExecutaTestes();


            //Vector.Testes testesVetorWrapperStructure = new Vector.Testes();
            //testesVetorWrapperStructure.ExecutaTestes();


            //ClasseDeTesteRegex testesRegexParaCompilador = new ClasseDeTesteRegex();
            //testesRegexParaCompilador.ExecutaTestes();



            //FileHeader.Testes testesHeaders = new FileHeader.Testes();
            //testesHeaders.ExecutaTestes();


            //Matriz.Testes testesMatriz= new Matriz.Testes();
            //testesMatriz.ExecutaTestes();




            //DictionaryText.Testes testesDictionaryText = new DictionaryText.Testes();
            //testesDictionaryText.ExecutaTestes();




            //JaggedArray.Testes testesWrapperStdructureJaggedArray = new JaggedArray.Testes();
            //testesWrapperStdructureJaggedArray.ExecutaTestes();

            //WrapperDataVector.Testes testesVectorWrapper = new WrapperDataVector.Testes();
            //testesVectorWrapper.ExecutaTestes();




            //ExpressaoSearch.Testes testesExpressaoSearch = new ExpressaoSearch.Testes();
            //testesExpressaoSearch.ExecutaTestes();

            //MetodosDouble.Testes testesMetodosDouble = new MetodosDouble.Testes();
            //testesMetodosDouble.ExecutaTestes();

            //TextExpression.Testes testesExpression = new TextExpression.Testes();
            //testesExpression.ExecutaTestes();

            //SearchByRegexExpression.Testes testesRegexSearch = new SearchByRegexExpression.Testes();
            //testesRegexSearch.ExecutaTestes();



            //ParserUniversal.Testes testesParser = new ParserUniversal.Testes();
            //testesParser.ExecutaTestes();

            //ExtratoresOO.Testes testesObterClasses = new ExtratoresOO.Testes();
            //testesObterClasses.ExecutaTestes();


            //FileHeader.Testes testesHeardersFile = new FileHeader.Testes();
            //testesHeardersFile.ExecutaTestes();

            //Instrucao.Testes testesInstrucao = new Instrucao.Testes();
            //testesInstrucao.ExecutaTestes();


            //Funcao.Testes testesDeChamadaDeMetodo = new Funcao.Testes();
            //testesDeChamadaDeMetodo.ExecutaTestes();


            //parser.textoFormatado.Testes testesRegex = new Testes();
            //testesRegex.ExecutaTestes();




            //ProcessadorDeID.Testes testesCompilacao = new ProcessadorDeID.Testes();
            //testesCompilacao.ExecutaTestes();






        }

    }
}

