using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO.Compression;
using System.Xml.Schema;


namespace XmlToHtml
{
    class Program
    {
        public static String path = "E:\\LEW\\XML_1\\excels\\";
        public static String nombreArchivo = "prueba";
        public static String xmlToRead = "\\docProps\\core.xml";
        public static XmlReader xml;

        public static StreamWriter fileWriter;

        static void Main(string[] args)
        {
            fileWriter = new StreamWriter("metadata.txt");

            //Establecemos como DirectoryInfo el path donde se encuentran los excels
            DirectoryInfo d = new DirectoryInfo(path); 

            //Cogemos como files todos los ficheros con extensión xlsx
            FileInfo[] Files = d.GetFiles("*.xlsx"); 

            try {
                foreach(FileInfo file in Files )
                    {
                        //Separamos la extensión del archivo del nombre, almacenando este último ya que 
                        //compartirá el nombrecon la carpeta que se generará al descomprimir
                        string fileName = file.Name.Split(".")[0];
                        nombreArchivo = fileName;
                        fileWriter.WriteLine("Nombre del archivo: " + fileName);
                        //Procesamos el archivo, primero descomprimiendo y luego leyendo 
                        //el xml con la ruta donde se ha descomprimido el archivo
                        processFile();
                    }
            } finally {
                fileWriter.Close();
            }
 
        }


        public static void unZip() {
            string zipFilePath = path + nombreArchivo + ".xlsx";
            string extractionPath = path + nombreArchivo;
            ZipFile.ExtractToDirectory(zipFilePath, extractionPath);
        }

        public static void processFile() {
            try
            {
                unZip();
                //Tenemos que ir al xml que tiene los metadatos
                xml = XmlReader.Create(path + nombreArchivo + xmlToRead);
                bool nextIsCreator = false;
                bool nextIsLastModified = false;
                bool nextIsCreated = false;
                bool nextIsModified = false;

                while (xml.Read())
                {    
                    switch (xml.NodeType)
                    {
                        case XmlNodeType.Element:
                            switch (xml.Name) {
                                case "dc:creator":
                                    nextIsCreator = true;
                                    break;
                                case "cp:lastModifiedBy":
                                    nextIsLastModified = true;
                                    break;
                                case "dcterms:created":
                                    nextIsCreated = true;
                                    break;
                                case "dcterms:modified":
                                    nextIsModified = true;
                                    break;
                            }
                        break;
                        case XmlNodeType.Text:
                            if (nextIsCreator) {
                                fileWriter.WriteLine("Autor: " + xml.Value);
                                nextIsCreator = false;
                            } else if (nextIsLastModified) {
                                fileWriter.WriteLine("Última modificación: " + xml.Value);
                                nextIsLastModified = false;
                            } else if (nextIsCreated) {
                                fileWriter.WriteLine("Fecha de creación: " + xml.Value);
                                nextIsCreated = false;
                            } else if (nextIsModified) {
                                fileWriter.WriteLine("Fecha de modificación: " + xml.Value);
                                nextIsModified = false;
                            }
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
                fileWriter.WriteLine("---------------------");
                xml.Close();
                var dir = new DirectoryInfo(path+nombreArchivo);
                dir.Delete(true);
            }


        }

    }
}
