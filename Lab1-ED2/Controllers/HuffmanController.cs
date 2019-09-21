using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Lab1_ED2.Models;

namespace Lab1_ED2.Controllers
{

    public class HuffmanController : BaseController
    {
        const int bufferLength = 100;
        public List<Caracter> ListaCaracteresExistentes = new List<Caracter>();
        public List<Caracter> ListaCaracteresFinales = new List<Caracter>();
        public List<Nodo> ListaNodosArbol = new List<Nodo>();
        public Nodo cNodoRaiz;
        public int TotalDeCaracteres;
        public Dictionary<string, byte> DiccionarioIndices = new Dictionary<string, byte>();
        public string TextoEnBinario = "";
        public string nombreArchivo;
        public Dictionary<string, int> DiccionarioLZWCompresion = new Dictionary<string, int>();
        //Vistas
        public ActionResult Menu()
        {
            return View();
        }
        public ActionResult VerMisCompresiones()
        {
            return View(LeerMisCompresiones());
        }
        public ActionResult MenuHuffman()
        {
            return View();
        }
        public ActionResult MenuLZW()
        {
            return View();
        }
        //Compresiones
        #region LZW
        public ActionResult CompresionLZWImportar()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CompresionLZWImportar(HttpPostedFileBase ArchivoImportado)
        {
            var archivoLeer = string.Empty;
            var ArchivoMapeo = Server.MapPath("~/App_Data/ArchivosImportados/");
            archivoLeer = ArchivoMapeo + Path.GetFileName(ArchivoImportado.FileName);
            var extension = Path.GetExtension(ArchivoImportado.FileName);
            ArchivoImportado.SaveAs(archivoLeer);
            var PropiedadesArchivoActual = new PropiedadesArchivo();
            FileInfo ArchivoAnalizado = new FileInfo(archivoLeer);
            PropiedadesArchivoActual.TamanoArchivoDescomprimido = ArchivoAnalizado.Length;
            PropiedadesArchivoActual.NombreArchivoOriginal = ArchivoAnalizado.Name;
            nombreArchivo = ArchivoAnalizado.Name.Split('.')[0];
            using (var Lectura = new BinaryReader(ArchivoImportado.InputStream))
            {
                using (var writeStream = new FileStream(Server.MapPath(@"~/App_Data/Compresiones/" + nombreArchivo + ".lzw"), FileMode.OpenOrCreate))
                {
                    using (var writer = new BinaryWriter(writeStream))
                    {
                        var byteBuffer = new byte[bufferLength];
                        while (Lectura.BaseStream.Position != Lectura.BaseStream.Length)
                        {
                            byteBuffer = Lectura.ReadBytes(bufferLength);
                            foreach (var item in byteBuffer)
                            {
                                if (!DiccionarioLZWCompresion.ContainsKey((Convert.ToChar(item)).ToString()))
                                {
                                    DiccionarioLZWCompresion.Add((Convert.ToChar(item)).ToString(), DiccionarioLZWCompresion.Count + 1);
                                }
                            }
                        }
                        foreach (var item in DiccionarioLZWCompresion)
                        {
                            var Indice = ((item.Key)+(item.Value).ToString()+"|").ToCharArray();
                            writer.Write(Indice);
                        }
                        writer.Write("\r\n");
                        Lectura.BaseStream.Position = 0;
                        var CaracterActual = string.Empty;
                        var Output = string.Empty;
                        while (Lectura.BaseStream.Position != Lectura.BaseStream.Length)
                        {
                            byteBuffer = Lectura.ReadBytes(bufferLength);
                            foreach (byte item in byteBuffer)
                            {
                                string CadenaAnalizada = CaracterActual + Convert.ToChar(item);
                                if (DiccionarioLZWCompresion.ContainsKey(CadenaAnalizada))
                                {
                                    CaracterActual = CadenaAnalizada;
                                }
                                else
                                {
                                    writer.Write(Convert.ToByte(DiccionarioLZWCompresion[CaracterActual]));
                                    DiccionarioLZWCompresion.Add(CadenaAnalizada, DiccionarioLZWCompresion.Count + 1);
                                    CaracterActual = Convert.ToChar(item).ToString();
                                }
                            }
                        }
                        PropiedadesArchivoActual.TamanoArchivoComprimido = writeStream.Length;
                        PropiedadesArchivoActual.RazonCompresion = Convert.ToDouble(PropiedadesArchivoActual.TamanoArchivoComprimido) / Convert.ToDouble(PropiedadesArchivoActual.TamanoArchivoDescomprimido);
                        PropiedadesArchivoActual.FactorCompresion = Convert.ToDouble(PropiedadesArchivoActual.TamanoArchivoDescomprimido) / Convert.ToDouble(PropiedadesArchivoActual.TamanoArchivoComprimido);
                        PropiedadesArchivoActual.PorcentajeReduccion = (Convert.ToDouble(1) - PropiedadesArchivoActual.RazonCompresion).ToString();
                        PropiedadesArchivoActual.FormatoCompresion = ".lzw";
                        GuaradarCompresiones(PropiedadesArchivoActual);
                    }
                }
            }
            Success(string.Format("Archivo comprimido exitosamente"), true);
            var FileVirtualPath = @"~/App_Data/Compresiones/" + nombreArchivo + ".lzw";
            return File(FileVirtualPath, "application / force - download", Path.GetFileName(FileVirtualPath));
        }
        public ActionResult DecompresionLZWImportar()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DecompresionLZWImportar(HttpPostedFileBase ArchivoImportado)
        {
            return View();
        }
        #endregion 
        #region Huffman
        public ActionResult CompresionHImportar()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CompresionHImportar(HttpPostedFileBase ArchivoImportado)
        {
            Directory.CreateDirectory(Server.MapPath("~/App_Data/ArchivosImportados/"));
            Directory.CreateDirectory(Server.MapPath("~/App_Data/Compresiones/"));
            var archivoLeer = string.Empty;
            var ArchivoMapeo = Server.MapPath("~/App_Data/ArchivosImportados/");
            archivoLeer = ArchivoMapeo + Path.GetFileName(ArchivoImportado.FileName);
            var extension = Path.GetExtension(ArchivoImportado.FileName);
            ArchivoImportado.SaveAs(archivoLeer);
            //Obtener propiedades del archivo
            var PropiedadesArchivoActual = new PropiedadesArchivo();
            FileInfo ArchivoAnalizado = new FileInfo(archivoLeer);
            PropiedadesArchivoActual.TamanoArchivoDescomprimido = ArchivoAnalizado.Length;
            PropiedadesArchivoActual.NombreArchivoOriginal = ArchivoAnalizado.Name;
            nombreArchivo = ArchivoAnalizado.Name.Split('.')[0];
            //Leer Archivo
            using (var Lectura = new BinaryReader(ArchivoImportado.InputStream))
            {
                var byteBuffer = new byte[bufferLength];
                while (Lectura.BaseStream.Position != Lectura.BaseStream.Length)
                {
                    byteBuffer = Lectura.ReadBytes(bufferLength);
                    IntroducirALista(byteBuffer);
                }
            }
            TotalDeCaracteres = ListaCaracteresExistentes.Count();
            SumarCaracteres();
            //Crear Lista de Nodos
            CrearListaDeNodos();
            //Armar Arbol
            ArmarArbol();
            //Traducir A Binario
            TraducirABinario();
            while (TextoEnBinario.Length % 8 != 0)
            {
                TextoEnBinario += "0";
            }
            //Escritura Huffman
            using (var writeStream = new FileStream(Server.MapPath(@"~/App_Data/Compresiones/" + nombreArchivo + ".huff"), FileMode.OpenOrCreate))
            {
                using (var writer = new BinaryWriter(writeStream))
                {
                    writer.Write(ArchivoAnalizado.Name.ToCharArray());
                    var Txt = "." + TotalDeCaracteres.ToString();
                    writer.Write(Txt.ToCharArray());
                    writer.Write(Environment.NewLine);
                    foreach (var item in DiccionarioIndices)
                    {
                        var Texto = Convert.ToInt64(item.Value) + "&" + item.Key + "|";
                        writer.Write(Texto.ToCharArray());
                    }
                    writer.Write(Environment.NewLine);
                    var byteBuffer2 = new byte[bufferLength];
                    var contadorBits = 0;
                    var contadorBuffer = 0;
                    var contador = 0;
                    var TextoenByte = "";
                    while (contador != TextoEnBinario.Length)
                    {
                        TextoenByte += TextoEnBinario[contador];
                        contadorBits++;
                        if (contadorBits == 8)
                        {
                            if (contadorBuffer == bufferLength)
                            {
                                for (int i = 0; i < bufferLength; i++)
                                {
                                    writer.Write(byteBuffer2[i]);
                                }
                                byteBuffer2 = new byte[bufferLength];
                                contadorBuffer = 0;
                                byteBuffer2[contadorBuffer] = Convert.ToByte(TextoenByte, 2);
                                contadorBuffer++;
                                contadorBits = 0;
                                TextoenByte = "";
                            }
                            else
                            {
                                byteBuffer2[contadorBuffer] = Convert.ToByte(TextoenByte, 2);
                                contadorBuffer++;
                                contadorBits = 0;
                                TextoenByte = "";
                                if (contador == TextoEnBinario.Length - 1)
                                {
                                    for (int i = 0; i < bufferLength; i++)
                                    {
                                        writer.Write(byteBuffer2[i]);
                                    }
                                }
                            }
                        }
                        contador++;
                    }
                    PropiedadesArchivoActual.TamanoArchivoComprimido = writeStream.Length;
                    PropiedadesArchivoActual.FactorCompresion = Convert.ToDouble(PropiedadesArchivoActual.TamanoArchivoComprimido) / Convert.ToDouble(PropiedadesArchivoActual.TamanoArchivoDescomprimido);
                    PropiedadesArchivoActual.RazonCompresion = Convert.ToDouble(PropiedadesArchivoActual.TamanoArchivoDescomprimido) / Convert.ToDouble(PropiedadesArchivoActual.TamanoArchivoComprimido);
                    PropiedadesArchivoActual.PorcentajeReduccion = (Convert.ToDouble(1) - PropiedadesArchivoActual.FactorCompresion).ToString();
                    PropiedadesArchivoActual.FormatoCompresion = ".huff";

                    GuaradarCompresiones(PropiedadesArchivoActual);
                }
            }
            Success(string.Format("Archivo comprimido exitosamente"), true);
            var FileVirtualPath = @"~/App_Data/Compresiones/" + nombreArchivo + ".huff";
            return File(FileVirtualPath, "application / force - download", Path.GetFileName(FileVirtualPath));

        }

        public ActionResult DescompresionHImportar()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DescompresionHImportar(HttpPostedFileBase ArchivoImportado)
        {
            Directory.CreateDirectory(Server.MapPath("~/App_Data/ArchivosImportados/"));
            Directory.CreateDirectory(Server.MapPath("~/App_Data/Descompresiones/"));
            var archivoLeer = string.Empty;
            var ArchivoMapeo = Server.MapPath("~/App_Data/ArchivosImportados/");
            var NombreNuevoArchivo = "";
            var ExtensionNuevoArchivo = "";
            if (ArchivoImportado != null)
            {
                archivoLeer = ArchivoMapeo + Path.GetFileName(ArchivoImportado.FileName);
                var extension = Path.GetExtension(ArchivoImportado.FileName);
                ArchivoImportado.SaveAs(archivoLeer);
                var Metadata = string.Empty;
                var CantidadCaracteresCOnvertir = string.Empty;
                var DiccionarioText = string.Empty;
                var DiccionarioDescompresion = new Dictionary<string, char>();
                if (extension == ".huff")
                {
                    var fs = new FileStream(archivoLeer, FileMode.OpenOrCreate);
                    var Reader = new StreamReader(fs);
                    long caracteresCuenta = 0;
                    Metadata = Reader.ReadLine();
                    caracteresCuenta = Metadata.LongCount();
                    DiccionarioText = Reader.ReadLine();
                    caracteresCuenta += DiccionarioText.LongCount();
                    NombreNuevoArchivo = Metadata.Split('.')[0];
                    ExtensionNuevoArchivo = "." + Metadata.Split('.')[1];
                    CantidadCaracteresCOnvertir = Metadata.Split('.')[2];
                    CantidadCaracteresCOnvertir = CantidadCaracteresCOnvertir.Split('\u0002')[0];
                    var ArregloDiccionario = DiccionarioText.Split('|');
                    for (int i = 0; i < ArregloDiccionario.Length - 1; i++)
                    {
                        var Caracter = Convert.ToChar(Convert.ToByte(ArregloDiccionario[i].Split('&')[0]));
                        var Indice = ArregloDiccionario[i].Split('&')[1];
                        DiccionarioDescompresion.Add(Indice, Caracter);
                    }

                    var Bs = new BinaryReader(fs);
                    using (var writeStream = new FileStream(Server.MapPath(@"~/App_Data/Descompresiones/" + NombreNuevoArchivo + ExtensionNuevoArchivo), FileMode.OpenOrCreate))
                    {
                        using (var writer = new BinaryWriter(writeStream))
                        {
                            var ListaDeDecimalesFlotantes = new List<char>();
                            string numRetenido = "";
                            var ContadordeCaracteres = Convert.ToInt32(CantidadCaracteresCOnvertir);
                            Bs.BaseStream.Seek(caracteresCuenta + 4, SeekOrigin.Begin);
                            while (Bs.BaseStream.Position != fs.Length)
                            {
                                var Caracter = Bs.ReadByte();
                                int Decimal = Convert.ToInt32(Caracter);
                                var Binario = Convert.ToString(Decimal, 2);
                                while (Binario.Count() < 8)
                                {
                                    Binario = "0" + Binario;
                                }
                                for (int i = 0; i < Binario.Length; i++)
                                {
                                    ListaDeDecimalesFlotantes.Add(Binario[i]);
                                }

                                foreach (var item in ListaDeDecimalesFlotantes)
                                {
                                    numRetenido = numRetenido + Convert.ToString(item);
                                    try
                                    {
                                        if (ContadordeCaracteres != 0)
                                        {
                                            writer.Write(DiccionarioDescompresion[numRetenido]);
                                            numRetenido = "";
                                            ContadordeCaracteres--;
                                        }
                                    }
                                    catch (Exception)
                                    { }
                                }
                                ListaDeDecimalesFlotantes.Clear();
                            }
                        }
                    }
                    Bs.Close();
                    Reader.Close();
                    fs.Close();
                    Success(string.Format("Archivo descomprimido exitosamente"), true);
                }
                else
                {
                    Danger("Formato de archivo no es 'huff'", true);
                }
            }
            else
            {
                Danger("El archivo es nulo.", true);
            }
            var FileVirtualPath = @"~/App_Data/Descompresiones/" + NombreNuevoArchivo + ExtensionNuevoArchivo;
            return File(FileVirtualPath, "application / force - download", Path.GetFileName(FileVirtualPath));
        }

        void IntroducirALista(byte[] CaracteresAux)
        {
            for (int i = 0; i < CaracteresAux.Length; i++)
            {
                var ClaseAux = new Caracter
                {
                    CaracterTexto = CaracteresAux[i],
                    Frecuencia = 0
                };
                ListaCaracteresExistentes.Add(ClaseAux);
            }
        }

        void SumarCaracteres()
        {
            ListaCaracteresExistentes[0].Frecuencia = 1;
            ListaCaracteresExistentes[0].CaracterAUsar = true;
            if (ListaCaracteresExistentes.Count > 1)
            {
                for (int i = 0; i < ListaCaracteresExistentes.Count; i++)
                {
                    for (int j = 1; j < ListaCaracteresExistentes.Count; j++)
                    {
                        if (ListaCaracteresExistentes[i].CaracterTexto == ListaCaracteresExistentes[j].CaracterTexto && !ListaCaracteresExistentes[j].CaracterYaRecorrido)
                        {
                            ListaCaracteresExistentes[i].Frecuencia += 1;
                            ListaCaracteresExistentes[i].CaracterAUsar = true;
                            ListaCaracteresExistentes[j].CaracterYaRecorrido = true;
                        }

                    }
                }
            }
            for (int i = 0; i < ListaCaracteresExistentes.Count; i++)
            {
                if (ListaCaracteresExistentes[i].CaracterAUsar)
                {
                    ListaCaracteresFinales.Add(ListaCaracteresExistentes[i]);
                }
            }
        }

        void CrearListaDeNodos()
        {
            int CantTotalCaracteres = ListaCaracteresFinales.Count;
            for (int i = 0; i < CantTotalCaracteres; i++)
            {
                var NodoAux = new Nodo
                {
                    caracter = ListaCaracteresFinales.ElementAt(i)
                };
                NodoAux.probabilidad = Convert.ToDouble(NodoAux.caracter.Frecuencia) / (TotalDeCaracteres);
                ListaNodosArbol.Add(NodoAux);
            }
        }

        void ArmarArbol()
        {
            var MetodoCopara = new Comparar();
            //ordena la lista de mayor a menor
            int TamanoLista = ListaNodosArbol.Count;
            while (ListaNodosArbol.Count() > 1)
            {
                ListaNodosArbol.Sort(MetodoCopara);
                var auxPadre = new Nodo();
                Nodo auxIzq = ListaNodosArbol[TamanoLista - 1];
                Nodo auxDcha = ListaNodosArbol[TamanoLista - 2];
                auxPadre.probabilidad = Convert.ToDouble(auxIzq.probabilidad) + Convert.ToDouble(auxDcha.probabilidad);
                auxPadre.NodoHijoDcha = auxDcha;
                auxPadre.NodoHijoIzq = auxIzq;
                auxPadre.NodoHijoIzq.NodoPadre = auxPadre;
                auxPadre.NodoHijoDcha.NodoPadre = auxPadre;

                ListaNodosArbol[TamanoLista - 2] = auxPadre;
                ListaNodosArbol.RemoveAt(TamanoLista - 1);
                TamanoLista = ListaNodosArbol.Count;
            }
            cNodoRaiz = ListaNodosArbol[0];
            cNodoRaiz.enOrden(cNodoRaiz);
            CrearDiccionario(cNodoRaiz);

        }

        void CrearDiccionario(Nodo nNodo)
        {
            if (nNodo != null)
            {
                if (nNodo.NodoHijoIzq == null)
                {
                    DiccionarioIndices.Add(nNodo.indice, nNodo.caracter.CaracterTexto);
                }
                CrearDiccionario(nNodo.NodoHijoIzq);
                CrearDiccionario(nNodo.NodoHijoDcha);
            }
        }

        public class Comparar : IComparer<Nodo> // clase para ordenar la lista de nodos
        {
            public int Compare(Nodo N1, Nodo N2)
            {
                double x = N1.probabilidad;
                double y = N2.probabilidad;
                if (x == 0 || y == 0)
                {
                    return 0;
                }
                return y.CompareTo(x);

            }
        }

        void TraducirABinario()
        {
            for (int i = 0; i < ListaCaracteresExistentes.Count; i++)
            {
                foreach (var item in DiccionarioIndices)
                {
                    if (item.Value == ListaCaracteresExistentes[i].CaracterTexto)
                    {
                        TextoEnBinario += item.Key;
                        ListaCaracteresExistentes[i].binarioText = item.Key;
                    }
                }
            }
        }
        List<PropiedadesArchivo> LeerMisCompresiones()
        {
            var Lista = new List<PropiedadesArchivo>();
            string archivoLeer = string.Empty;
            string ArchivoMapeo = Server.MapPath("~/App_Data/");
            archivoLeer = ArchivoMapeo + Path.GetFileName("ListaCompresiones");
            using (var Lectura = new StreamReader(archivoLeer))
            {
                while (!Lectura.EndOfStream)
                {
                    var Cadena = Lectura.ReadLine();
                    var Auxiliar = new PropiedadesArchivo
                    {
                        NombreArchivoOriginal = Cadena.Split('|')[0],
                        TamanoArchivoComprimido = Convert.ToDouble(Cadena.Split('|')[2]),
                        TamanoArchivoDescomprimido = Convert.ToDouble(Cadena.Split('|')[1]),
                        RazonCompresion = Convert.ToDouble(Cadena.Split('|')[4]),
                        FactorCompresion = Convert.ToDouble(Cadena.Split('|')[3]),
                        PorcentajeReduccion = Cadena.Split('|')[5]
                    };
                    Lista.Add(Auxiliar);
                }
            }
            return Lista;
        }

        void GuaradarCompresiones(PropiedadesArchivo Archivo)
        {
            string archivoLeer = string.Empty;
            string ArchivoMapeo = Server.MapPath("~/App_Data/");
            archivoLeer = ArchivoMapeo + Path.GetFileName("ListaCompresiones");
            using (var writer = new StreamWriter(archivoLeer, true))
            {
                writer.WriteLine(Archivo.NombreArchivoOriginal + "|" + Archivo.TamanoArchivoDescomprimido + "|" + Archivo.TamanoArchivoComprimido + "|" + Archivo.FactorCompresion + "|" + Archivo.RazonCompresion + "|" + Archivo.PorcentajeReduccion);
            }
        }
    }
}




