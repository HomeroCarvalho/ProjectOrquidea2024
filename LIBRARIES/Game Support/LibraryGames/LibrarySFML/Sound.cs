using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Audio;

namespace parser
{
    /// <summary>
    /// classe de audio: sound, music. instanciação feito com path relativo do arquivo de som, musica.
    /// e o acesso é por um id de texto, como dicionarios C Sharp. PlayMusic("nomeAliasMusica");
    /// </summary>
    public class Sound
    {
        /// <summary>
        /// mapa de musicas possiveis de se tocar.
        /// </summary>
        private static Dictionary<string, SFML.Audio.Music> musicas = new Dictionary<string, Music>();
        /// <summary>
        /// mapa de sons sendo tocados simultaneamente.
        /// </summary>
        private static Dictionary<string, SFML.Audio.Sound> sonsTocando = new Dictionary<string, SFML.Audio.Sound>();


        /// <summary>
        /// id da musica que está sendo tocada.
        /// </summary>
        private string musicaNomeTocando = null;


        /// <summary>
        /// carrraga um arquivo de musica.
        /// </summary>
        /// <param name="filename">path para o arquivo da mmusica.</param>
        /// <param name="id_musica">id de identificacao da musica.</param>
        public void LoadMusic(string filename, string id_musica)
        {
            FileInfo arquivoMusica = new FileInfo(filename);

            SFML.Audio.Music? musica1 = new Music(arquivoMusica.FullName);
            SFML.Audio.Music? m_in= null;
            if (!musicas.TryGetValue(id_musica, out m_in))
             {
                musicas[id_musica] = musica1;
            }
        }



        /// <summary>
        /// loop continuo de tocar a musica.
        /// </summary>
        /// <param name="id">id de identificacao da musica.</param>
        public void RepeatMusic(string id)
        {
            musicas[id].Loop = true;
        }


        /// <summary>
        /// pausa da musica currente.
        /// </summary>
        /// <param name="id"></param>
        public void PauseMusic(string id)
        {
            musicas[id].Pause();
        }

        /// <summary>
        /// toca a musica.
        /// </summary>
        /// <param name="id"></param>
        public void PlayMusic(string id)
        {
            this.musicaNomeTocando = id;
            musicas[id].Play();
        }

        /// <summary>
        /// ajusta o volume da musica.
        /// </summary>
        /// <param name="id">identificacao da musica.</param>
        /// <param name="volume">valor de 0..100 do volume.</param>
        public void VolumeMusic(string id, float volume)
        {
            musicas[id].Volume = volume;
        }


        /// <summary>
        /// carrega um aquivo de soms.
        /// </summary>
        /// <param name="pathFile">path do arquivo de som.</param>
        /// <param name="id_sound">um nome que identifique o som.</param>
        /// <returns>retorna um id de identifacao para acesso ao som.</returns>
        public void LoadSound(string pathFile, string id_sound)
        {
            FileInfo arquivoSom = new FileInfo(pathFile);

            SoundBuffer buffer = new SoundBuffer(arquivoSom.FullName);
            SFML.Audio.Sound ?sound = new SFML.Audio.Sound(buffer);


            // verifica se já há o som no mapa de sons.
            if (!sonsTocando.TryGetValue(id_sound, out sound))
            {
                if (sound == null)
                {
                    sound = new SFML.Audio.Sound(buffer);

                    // adiciona o som para o mapa de sons tocando.
                    sonsTocando[id_sound] = sound;

                }
            }
            

        }

        /// <summary>
        /// carrega um som, a partir de outro som.
        /// </summary>
        /// <param name="sound">nome do som.</param>
        /// <param name="id_sound">id de identifcacao do som.</param>
        public void LoadSound(SFML.Audio.Sound sound, string id_sound)
        {
            SFML.Audio.Sound ?sound1= new SFML.Audio.Sound(sound);
            // verifica se já há o som no mapa de sons.
            if (!sonsTocando.TryGetValue(id_sound, out sound1))
            {
                if (sound1 == null)
                {
                    sound1 = new SFML.Audio.Sound(sound);
                    // adiciona o som para o mapa de sons tocando.
                    sonsTocando[id_sound] = sound1;

                }
            }

            
        }


        /// <summary>
        /// toca o som.
        /// </summary>
        /// <param name="id">nome que identifica o som.</param>
        public void PlaySound(string id)
        {
            sonsTocando[id].Play();
        }

        /// <summary>
        /// para de tocar o som.
        /// </summary>
        /// <param name="id">nome que identifica o som.</param>
        public void StopSound(string id)
        {
            sonsTocando[id].Stop();
        }

        /// <summary>
        /// ajuasta o volume do som.
        /// </summary>
        /// <param name="id">identificacao do som.</param>
        /// <param name="volume">volume de 0...100.</param>
        public void VolumeSound(string id, int volume)
        {
            sonsTocando[id].Volume= volume;
        }


        /// <summary>
        /// pausa de tocar o som.
        /// </summary>
        /// <param name="id">nome que identifica o som./param>
        public void PauseSound(string id)
        {
            sonsTocando[id].Pause();
        }


     

        /// <summary>
        /// obtem um som, a partir de um indice.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public SFML.Audio.Sound GetSound(string id_nameSound)
        {
            return sonsTocando[id_nameSound];
            
        }

        public class Testes: SuiteClasseTestes
        {
            public Testes():base("testes de classe sound")
            {

            }

            public void TesteTocarMusica(AssercaoSuiteClasse assercao)
            {
                Sound musica = new Sound();
                musica.LoadMusic(@"sounds and musics\round_5.ogg", "musica1");
                musica.PlayMusic("musica1");
                musica.RepeatMusic("musica1");
                while (true)
                {

                }
            }
            public void TesteFileInfoSound(AssercaoSuiteClasse assercao)
            {
                FileInfo file = new FileInfo(@"sounds and musics\AccessDeniedNow.wav");
                Console.WriteLine(file.FullName);


                Sound som = new Sound();
                som.LoadSound(file.FullName, "som1");

                
                som.PlaySound("som1");
                som.VolumeSound("som1", 70);
                while (true)
                {

                }
            }

          
       
          

            public void TesteTocarSom(AssercaoSuiteClasse assercao)
            {
                Sound som = new Sound();
                som.LoadSound(@"F:\BACUKPS\ProjectOrquidea2024\ProjectOrquidea2024\bin\Debug\net8.0\sounds and musics\AccessDeniedNow.wav", "som1");
                som.PlaySound("som1");
                som.VolumeSound("som1", 70);
                while (true)
                {

                }

            }
        }



    }
}
