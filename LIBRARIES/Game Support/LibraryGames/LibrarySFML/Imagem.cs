using parser;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace parser
{
    /// <summary>
    /// classe para imagens.
    /// </summary>
    public class Imagem: Objeto
    {
        /// <summary>
        /// imagem sfml.
        /// </summary>
        public Sprite spr_image;

        /// <summary>
        /// id de identidade no loop game.
        /// </summary>
        private int id;

        /// <summary>
        /// path para o arquivo de imagem.
        /// </summary>
        private string path;

        /// <summary>
        /// construtor vazio.
        /// </summary>
        public Imagem()
        {
            this.valor = this;
        }

        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="path"></param>
        public Imagem(string path)
        {
            if (path == null)
            {
                return;
            }
            this.path = path;
            this.spr_image = new Sprite(new Texture(path));
         
            LoopGame.imagens.Add(this);
            this.id = LoopGame.imagens.Count - 1;
            this.SET("id", "int", this.id);
            this.valor = this;
        
        }

        /// <summary>
        /// construtor, carrega do arquivo a imagem, e seta suas dimensoes.
        /// </summary>
        /// <param name="path">path do arquivo de imagem.</param>
        /// <param name="width">dimensao x da imagem.</param>
        /// <param name="height">dimensao y da imagem.</param>
        public Imagem(string path, int width, int height)
        {
            this.path = path;
            this.spr_image = new Sprite(new Texture(path, new IntRect(0, 0, width, height)));


            LoopGame.imagens.Add(this);
            this.id = LoopGame.imagens.Count - 1;

            this.SET("id", "int", this.id);
            this.valor = this;
        }

        /// <summary>
        /// obtem o id de identidade da imagem.
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return (int)this.GET("id");
        }


        public new class Testes:SuiteClasseTestes
        {
            public Testes() : base("testes de imagens")
            {

            }

            public void TestePropriedades(AssercaoSuiteClasse assercao)
            {
                string code_0_0 = "Imagem img1= create(\"images\\set1\\Eyeball_1.png\"); Imagem img2= create(\"images\\set1\\Eyeball_2.png\");int n= img2.GetID();";
                ProcessadorDeID compilador = new ProcessadorDeID(code_0_0);
                compilador.Compilar();

                ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                programa.Run(compilador.escopo);

                Imagem img2 = (Imagem)compilador.escopo.tabela.GetObjeto("img2", compilador.escopo).valor;

                assercao.IsTrue(img2.propriedades[0].valor.ToString() == "1", code_0_0);
                assercao.IsTrue(compilador.escopo.tabela.GetObjeto("n", compilador.escopo).valor.ToString() == "1", code_0_0);

            }

            public void TesteImagem(AssercaoSuiteClasse assercao)
            {
                string pathImage1 = @"images\set1\Eyeball_1.png";
                string pathImage2 = @"images\set1\Eyeball_2.png";

                Imagem image1= new Imagem(pathImage1);
                Imagem image2 = new Imagem(pathImage2);

                assercao.IsTrue(image2.GetID().ToString() == "1", "teste de imagem");

            }

        }
    }

    
}
