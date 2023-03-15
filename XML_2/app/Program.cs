using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace XmlToHtml
{
    class Program
    {
        public static StreamWriter file;
        public static String cssToUse = "";
        public static bool hideTitles = false;

        static void Main(string[] args)
        {
            String pathLocalArchivo = "E:\\LEW\\XML_2\\XmlToHtml\\peliculas3.xml";
            String pathOutput = "E:\\LEW\\XML_2\\XmlToHtml\\peliculas3.html";
            try
            {
                if (args.Length > 1)
                {
                    pathLocalArchivo = args[0];

                }
                file = new StreamWriter(pathOutput);

                XmlReader xml = XmlReader.Create(pathLocalArchivo);
                while (xml.Read())
                {
                    switch (xml.NodeType)
                    {
                        case XmlNodeType.Element:
                            string xmlNameToParse = AddSpacesToSentence(xml.Name).ToLower();
                            if (xml.HasAttributes)
                            {
                                switch (xmlNameToParse) {
                                    case "pagina":
                                        cssToUse = (xml.GetAttribute("estilo"));
                                        hideTitles = bool.Parse(xml.GetAttribute("hide"));
                                        break;
                                }
                            }
                        break;
                    }
                }
                WriteMetaData();
                BeginBody();
                xml = XmlReader.Create(pathLocalArchivo);

                int counterreview = 0;
                bool nextIsImage = false;
                bool nextIsVideo = false;
                bool listaProtagonistasAbierta = false;
                bool nextIsDuration = false;


                String valueField = "";

                while (xml.Read())
                {
                    switch (xml.NodeType)
                    {
                        case XmlNodeType.Element:
                            string xmlNameToParse = AddSpacesToSentence(xml.Name).ToLower();
                            switch (xmlNameToParse) {
                                case "reviews":
                                    toHeader3(xmlNameToParse);
                                    break;
                                case "noticias":
                                    toHeader3(xmlNameToParse);
                                    break;
                                case "protagonistas":
                                    listaProtagonistasAbierta = true;
                                    toParagraph(emphasize(xmlNameToParse));
                                    OpenList();
                                    break;
                                case "referencias":
                                    toParagraph(emphasize(xmlNameToParse));
                                    OpenList();
                                    break;
                                case "galeria de fotos":
                                    toParagraph(emphasize(xmlNameToParse));
                                    break;
                                case "galeria de videos":
                                    toParagraph(emphasize(xmlNameToParse));
                                    break;
                                case "tabla actores":
                                    startTableActores();
                                    break;
                                case "tabla puntuaciones":
                                    startTablePuntuaciones();
                                    break;
                                 case "banda sonora":
                                    toParagraph(emphasize(xmlNameToParse));
                                    startTableOST();
                                    break;
                                case "imagen":
                                    nextIsImage = true;
                                    break;
                                case "video":
                                    nextIsVideo = true;
                                    break;
                                case "peliculas":
                                    break;
                                case "duracion":
                                    nextIsDuration = true;
                                    break;
                                default:
                                    if (!xml.HasAttributes)
                                    {
                                        if (!hideTitles) {
                                            valueField += xmlNameToParse.ToUpper() + ": ";
                                        }
                                    }
                                    break;
                            }
                            if (xml.HasAttributes)
                            {
                                switch (xmlNameToParse) {
                                    case "pelicula":
                                        openSection();
                                        counterreview = 0;
                                        toHeader2(xml.GetAttribute("nombre"));
                                        break;
                                    case "review":
                                        toHeader4(xml.GetAttribute("nombre"));
                                        break;
                                    case "noticia":
                                        toHeader4(xml.GetAttribute("nombre"));
                                        break;
                                    case "pagina":
                                        toHeader1(xml.GetAttribute("nombre"));
                                        break;
                                    case "actor":
                                        toTable(xml.GetAttribute("nombre"), xml.GetAttribute("personaje"));
                                        break;
                                    case "puntuacion":
                                        toTablePuntuaciones(xml.GetAttribute("imagen"), xml.GetAttribute("propio"),xml.GetAttribute("publico"), xml.GetAttribute("veredicto"));
                                        break;
                                    case "cancion":
                                        toMusicTable(xml.GetAttribute("audio"), xml.GetAttribute("descripcion"));
                                        break;
                                    case "enlace":
                                        toLink(xml.GetAttribute("descripcion"), xml.GetAttribute("enlace"));
                                        break;
                                }
                            }
                            break;
                        case XmlNodeType.EndElement:
                            string xmlEndNameToParse = AddSpacesToSentence(xml.Name).ToLower();
                            switch (xmlEndNameToParse) {
                                case "tabla actores":
                                    endTable();
                                    break;
                                case "tabla puntuaciones":
                                    endTable();
                                    break;
                                case "banda sonora":
                                    endTable();
                                    break;
                                case "referencias":
                                    CloseList();
                                    break;
                                case "protagonistas":
                                    listaProtagonistasAbierta = false;
                                    CloseList();
                                    break;
                                case "galeria de fotos":
                                    nextIsImage = false;
                                    break;
                                case "pelicula":
                                    closeSection();
                                    break;
                            }
                            break;
                        case XmlNodeType.Text:
                            if (nextIsVideo) {
                                toVideo(xml.Value);
                                nextIsVideo = false;
                            } else if (listaProtagonistasAbierta && !nextIsImage) {
                                toListElement(emphasize(valueField)  + xml.Value);
                            }
                            else if (nextIsImage && listaProtagonistasAbierta) {
                                toListImage(valueField, xml.Value);
                                nextIsImage = false;
                            } else if (nextIsImage && !listaProtagonistasAbierta) {
                                toImage(valueField, xml.Value);
                                nextIsImage = false;
                            } else if (nextIsDuration) {
                                var duration = xml.Value.Split("P")[1];
                                toParagraph(emphasize("DURACIÓN: ") + duration);
                                nextIsDuration = false;
                            }
                            else
                            {
                                toParagraph(emphasize(valueField) + xml.Value);
                            }
                            valueField = "";
                            break;
                        case XmlNodeType.Comment:
                            file.WriteLine("Comment.Value: {0}", xml.Value);
                            break;
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("error");
            }
            finally
            {
                endHtml();
                file.Close();
            }
        }

        public static void toVideo(String text) {
            file.WriteLine("<video controls>");
            file.WriteLine("<source src = \"" + text + "\" type =\"video/mp4\">");
            file.WriteLine("</video>");
        }

        public static string AddSpacesToSentence(string text)
        {
        if (string.IsNullOrWhiteSpace(text))
           return "";
        StringBuilder newText = new StringBuilder(text.Length * 2);
        newText.Append(text[0]);
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                newText.Append(' ');
            newText.Append(text[i]);
        }
        return newText.ToString();
}

        public static String emphasize(String text)
        {
            return "<em>" + text + "</em>";
        }

        public static void toHeader1(String text)
        {
            file.WriteLine("<h1>" + text + "</h1>");
        }

        public static void toHeader2(String text)
        {
            file.WriteLine("<h2>" + text + "</h2>");
        }

        public static void toHeader3(String text)
        {
            file.WriteLine("<h3>" + text + "</h3>");
        }

        public static void toHeader4(String text)
        {
            file.WriteLine("<h4>" + text + "</h4>");
        }

        public static void toParagraph(String text)
        {
            file.WriteLine("<p>" + text + "</p>");
        }
        public static void toLink(String descripcion, String link)
        {
            file.WriteLine("<li><a href = " + link + ">" +  descripcion + "</a></li>");
        }
        public static void toListElement(String text)
        {
            file.WriteLine("<li>" + text + "</li>");
        }
        public static void OpenList()
        {
            file.WriteLine("<ul>");
        }
        public static void CloseList()
        {
            file.WriteLine("</ul>");
        }

        public static void WriteMetaData()
        {
            file.WriteLine("<!DOCTYPE HTML>");
            file.WriteLine("<html lang=\"es\">");
            file.WriteLine("<head>");
            file.WriteLine("<meta charset=\"UTF-8\"/>");
            file.WriteLine("<title>Reviews de películas</title>");
            file.WriteLine("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + cssToUse +  "\"/>");
            file.WriteLine("</head>");
        }

        public static void startTableActores()
        {
            file.WriteLine("<table>");
            file.WriteLine("<thead>");
            file.WriteLine("<tr>");
            file.WriteLine("<th>Actor</th>");
            file.WriteLine("<th>Personaje que interpreta</th>");
            file.WriteLine("</tr>");
            file.WriteLine("</thead>");
            file.WriteLine("<tbody>");
        }

        
        public static void startTablePuntuaciones()
        {
            file.WriteLine("<table>");
            file.WriteLine("<thead>");
            file.WriteLine("<tr>");
            file.WriteLine("<th>Carátula</th>");
            file.WriteLine("<th>Nuestra puntuación</th>");
            file.WriteLine("<th>La puntuación de la crítica</th>");
            file.WriteLine("<th>Veredicto final</th>");
            file.WriteLine("</tr>");
            file.WriteLine("</thead>");
            file.WriteLine("<tbody>");
        }

        public static void startTableOST()
        {
            file.WriteLine("<table>");
            file.WriteLine("<thead>");
            file.WriteLine("<tr>");
            file.WriteLine("<th>Canción</th>");
            file.WriteLine("<th>Descripción</th>");
            file.WriteLine("</tr>");
            file.WriteLine("</thead>");
            file.WriteLine("<tbody>");
        }

        public static void toTable(String cell1, String cell2)
        {
            file.WriteLine("<tr>");
            file.WriteLine("<td>" + cell1 + "</td>");
            file.WriteLine("<td>" + cell2 + "</td>");
            file.WriteLine("</tr>");
        }

        public static void toMusicTable(String cell1, String cell2)
        {
            file.WriteLine("<tr>");
            file.WriteLine("<td><audio controls src=" + cell1 + "></audio></td>");
            file.WriteLine("<td>" + cell2 + "</td>");
            file.WriteLine("</tr>");
        }

        public static void toTablePuntuaciones(String cell1, String cell2, String cell3, String cell4)
        {
            file.WriteLine("<tr>");
            file.WriteLine("<td>" + "<img src=" + "\"" +  cell1 + "\"" + " alt=" + cell4  + " >" + "</td>");
            file.WriteLine("<td>" + cell2 + "</td>");
            file.WriteLine("<td>" + cell3 + "</td>");
            file.WriteLine("<td>" + cell4 + "</td>");
            file.WriteLine("</tr>");
        }

        public static void endTable()
        {
            file.WriteLine("</tbody>");
            file.WriteLine("</table>");
        }

        public static void endHtml()
        {
            file.WriteLine("<footer>");
            file.WriteLine("<p>Autor: Javier Carrillo González</p>");
            file.WriteLine("<p><a href=\"mailto:uo269412@uniovi.es\">uo269412@uniovi.es</a></p>");
            file.WriteLine("</footer>");
            file.WriteLine("</body>");
            file.WriteLine("</html>");
        }

        public static void BeginBody()
        {
            file.WriteLine("<body>");
        }

        public static void CloseBody()
        {
            file.WriteLine("</body>");
        }

        public static void toImage(String name, String value)
        {
            file.WriteLine("<img src=" + "\"" +  value + "\"" + " alt=" + name + "imagen"  + " >");   
        }

        public static void toListImage(String name, String value)
        {
            file.WriteLine("<li><img src=" + "\"" +  value + "\"" + " alt=" + name + "imagen" + " ></li>");   
        }


        public static void openSection()
        {
            file.WriteLine("<section>");
        }

        public static void closeSection()
        {
            file.WriteLine("</section>");
        }

    }
}
