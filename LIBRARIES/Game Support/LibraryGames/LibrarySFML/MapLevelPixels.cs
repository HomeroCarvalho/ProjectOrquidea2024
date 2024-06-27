using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
namespace parser
{
    /// <summary>
    /// classe de processamento de mapas de niveis, atraves de cores de pixels.
    /// </summary>
    public unsafe class MapLevelPixels
    {

        /// <summary>
        /// caminho do arquivo de imagem do mapa.
        /// </summary>
        private string pathFileMap = null;
        /// <summary>
        /// imagem do mapa.
        /// </summary>
        private SFML.Graphics.Image mapa;

       
        /// <summary>
        /// cores de tiles identificados.
        /// </summary>
        private List<Color> coresTiles= new List<Color>();
        private List<string> nameTiles= new List<string>();

        /// <summary>
        /// items identificados no mapa.
        /// </summary>
        public Vector items = new Vector();





        /// <summary>
        /// instancia, e faz o processamento dos tiles marcados no mapa.
        /// </summary>
        /// <param name="pathFileMap">path do arquivo de imagem mapa.</param>
        public MapLevelPixels(string pathFileMap)
        {
            this.pathFileMap = pathFileMap;
        }

        /// <summary>
        /// faz o processamento de idenficação de tiles no mapa.
        /// </summary>
        public void Processing()
        {
            // carrega a imagem do mapa.
            this.mapa = new SFML.Graphics.Image(this.pathFileMap);
         
            // extrai as cores de pixels do mapa.
            //this.pixels = Raylib.LoadImageColors(this.mapa);

            // muda o comportamento do vetor, de array para list.
            this.items.Clear();

            for (int x = 0; x < mapa.Size.X; x++)
            {
                for (int y = 0; y < mapa.Size.X; y++)
                {

                    // DESABILITADO PARA IMPLEMENTACAO ADIANTE.
                    // obtem o pixel em (x,y)
                    SFML.Graphics.Color corPixel = this.Pixel(x, y);

                    // procura da cor do pixel, entre as cores registradas.
                    int index = this.coresTiles.FindIndex(k => k.R == corPixel.R && k.G == corPixel.G && k.B == corPixel.B);
                    if (index != -1)
                    {

                        // encontrou uma cor registrada, constroi um [ItemMap].
                        ItemMap umItem = new ItemMap(nameTiles[index], x, y, corPixel);
                        // adiciona o item identificado a lista de itens.
                        this.items.Append(umItem);

                    }

                    
                }
            }

        }

        /// <summary>
        /// adiciona dados de um tile representado no mapa.
        /// </summary>
        /// <param name="nameTile">nome id do tile.</param>
        /// <param name="corID">cor de identificacao.</param>
        public void AddTile(string nameTile, Color corID)
        {
            this.coresTiles.Add(corID);
            this.nameTiles.Add(nameTile);
        }


        /// <summary>
        /// retorna a cor do pixels numa coordenadas.
        /// </summary>
        /// <param name="x">coordenada x</param>
        /// <param name="y">coordenada y</param>
        /// <returns>retorna a cor do pixel em (x,y).</returns>
        private SFML.Graphics.Color Pixel(int x, int y)
        {
            return this.mapa.GetPixel((uint)x, (uint)y);
           
        }

        public class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes de mapa de pixels")
            {

            }

            public void IdentificarPixelNoMapa(AssercaoSuiteClasse assercao)
            {
                try
                {
                    string pathFileMap = @"images\mapLevel1Example.png";
                    MapLevelPixels mapa1 = new MapLevelPixels(pathFileMap);


                    // adiciona um tipo de tile mapeado.
                    mapa1.AddTile("grass", Color.FromArgb(0, 255, 0, 255));

                    // faz o processamento do mapa.
                    mapa1.Processing();


                    assercao.IsTrue(mapa1.items != null && mapa1.items.size() == 2);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }
            }
        }

    }

    /// <summary>
    /// classe de mapeamento de tiles.
    /// </summary>
    public class ItemMap: Objeto
    {
        public string nameTile;
        public int x;
        public int y;
        public SFML.Graphics.Color corID;

        /// <summary>
        /// constroi um item map.
        /// interessante aplicacao de prpopriedades de [Objeto]: o codigo só dá processamento com propriedades do objeto,
        /// e se setar as propriedades com os campos da classe, obtemos GETTERs!
        /// </summary>
        /// <param name="nameTile">texto id do tile.</param>
        /// <param name="x">coordenada x do tile identificado.</param>
        /// <param name="y">coordenada y do tile identificado.</param>
        /// <param name="corID">cor que o tile é representado no mapa.</param>
        public ItemMap(string nameTile, int x, int y, SFML.Graphics.Color corID)
        {
            this.nameTile = nameTile;
            this.x = x;
            this.y = y;
            this.corID = corID;

            this.SET("nameTile", "string", this.nameTile);
            this.SET("x", "int", this.x);
            this.SET("y", "int", this.y);
            this.SET("corID", "SFML.Graphics.Color", this.corID);
            
        }

        public override string ToString()
        {
            return "name: " + nameTile + "  color: [" + corID.R + "," + corID.G + "," + corID.B + "] at < " + x + "," + y + ">";
        }


    }

   

}
