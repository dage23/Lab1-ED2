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
        public int TotalDeCaracteres;
        public Dictionary<string, byte> DiccionarioIndices = new Dictionary<string, byte>();
        public string TextoEnBinario = "";
        public string nombreArchivo;


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
        public ActionResult CompresionLZWImportar()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CompresionLZWImportar(HttpPostedFileBase ArchivoImportado)
        {
            Dictionary<string, int> DiccionarioLZWCompresion = new Dictionary<string, int>();
            Directory.CreateDirectory(Server.MapPath("~/App_Data/ArchivosImportados/"));
            Directory.CreateDirectory(Server.MapPath("~/App_Data/Compresiones/"));
            var DireccionArchivo = string.Empty;
            var ArchivoMapeo = Server.MapPath("~/App_Data/ArchivosImportados/");
            DireccionArchivo = ArchivoMapeo + Path.GetFileName(ArchivoImportado.FileName);
            var extension = Path.GetExtension(ArchivoImportado.FileName);
            ArchivoImportado.SaveAs(DireccionArchivo);
            var PropiedadesArchivoActual = new PropiedadesArchivo();
            FileInfo ArchivoAnalizado = new FileInfo(DireccionArchivo);
            PropiedadesArchivoActual.TamanoArchivoDescomprimido = ArchivoAnalizado.Length;
            PropiedadesArchivoActual.NombreArchivoOriginal = ArchivoAnalizado.Name;
            nombreArchivo = ArchivoAnalizado.Name.Split('.')[0];
            var listaCaracteresExistentes = new List<byte>();
            var listaCaracteresEscribir = new List<int>();
            var listaCaracteresBinario = new List<string>();
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
                                if (!listaCaracteresExistentes.Contains(item))
                                {
                                    listaCaracteresExistentes.Add(item);
                                }
                            }
                        }
                        listaCaracteresExistentes.Sort();
                        foreach (var item in listaCaracteresExistentes)
                        {
                            var caractreres = Convert.ToChar(item);
                            DiccionarioLZWCompresion.Add(caractreres.ToString(), DiccionarioLZWCompresion.Count + 1);
                        }
                        var TamanoDiccionario = Convert.ToString(DiccionarioLZWCompresion.LongCount()) + ".";
                        writer.Write(TamanoDiccionario.ToCharArray());
                        Lectura.BaseStream.Position = 0;
                        var CaracterActual = string.Empty;
                        var Output = string.Empty;
                        while (Lectura.BaseStream.Position != Lectura.BaseStream.Length)
                        {
                            byteBuffer = Lectura.ReadBytes(bufferLength);
                            foreach (byte item in byteBuffer)
                            {
                                var CadenaAnalizada = CaracterActual + Convert.ToChar(item);
                                if (DiccionarioLZWCompresion.ContainsKey(CadenaAnalizada))
                                {
                                    CaracterActual = CadenaAnalizada;
                                }
                                else
                                {
                                    listaCaracteresEscribir.Add(DiccionarioLZWCompresion[CaracterActual]);
                                    DiccionarioLZWCompresion.Add(CadenaAnalizada, DiccionarioLZWCompresion.Count + 1);
                                    CaracterActual = Convert.ToChar(item).ToString();
                                }
                            }
                        }
                        listaCaracteresEscribir.Add(DiccionarioLZWCompresion[CaracterActual]);
                        var TamanoTexto = Convert.ToString(DiccionarioLZWCompresion.LongCount()) + ".";
                        writer.Write(TamanoTexto.ToCharArray());
                        foreach (var item in listaCaracteresExistentes)
                        {
                            var Indice = Convert.ToByte(item);
                            writer.Write(Indice);
                        }
                        writer.Write(Environment.NewLine);
                        var mayorIndice = listaCaracteresEscribir.Max();
                        var bitsMayorIndice = (Convert.ToString(mayorIndice, 2)).Count();
                        writer.Write(bitsMayorIndice.ToString().ToCharArray());
                        writer.Write(extension.ToCharArray());
                        writer.Write(Environment.NewLine);
                        if (mayorIndice > 255)
                        {
                            foreach (var item in listaCaracteresEscribir)
                            {
                                var indiceBinario = Convert.ToString(item, 2);
                                while (indiceBinario.Count() < bitsMayorIndice)
                                {
                                    indiceBinario = "0" + indiceBinario;
                                }
                                listaCaracteresBinario.Add(indiceBinario);
                            }
                            var cadenaBits = string.Empty;
                            foreach (var item in listaCaracteresBinario)
                            {
                                for (int i = 0; i < item.Length; i++)
                                {
                                    if (cadenaBits.Count() < 8)
                                    {
                                        cadenaBits += item[i];
                                    }
                                    else
                                    {
                                        var cadenaDecimal = Convert.ToInt64(cadenaBits, 2);
                                        var cadenaEnByte = Convert.ToByte(cadenaDecimal);
                                        writer.Write((cadenaEnByte));
                                        cadenaBits = string.Empty;
                                        cadenaBits += item[i];
                                    }
                                }
                            }
                            if (cadenaBits.Length > 0)
                            {
                                var cadenaRestante = Convert.ToInt64(cadenaBits, 2);
                                writer.Write(Convert.ToByte(cadenaRestante));
                            }
                        }
                        else
                        {
                            foreach (var item in listaCaracteresEscribir)
                            {
                                writer.Write(Convert.ToByte(Convert.ToInt32(item)));
                            }
                        }
                        PropiedadesArchivoActual.TamanoArchivoComprimido = writeStream.Length;
                        PropiedadesArchivoActual.FactorCompresion = Convert.ToDouble(PropiedadesArchivoActual.TamanoArchivoComprimido) / Convert.ToDouble(PropiedadesArchivoActual.TamanoArchivoDescomprimido);
                        PropiedadesArchivoActual.RazonCompresion = Convert.ToDouble(PropiedadesArchivoActual.TamanoArchivoDescomprimido) / Convert.ToDouble(PropiedadesArchivoActual.TamanoArchivoComprimido);
                        PropiedadesArchivoActual.PorcentajeReduccion = (Convert.ToDouble(1) - PropiedadesArchivoActual.FactorCompresion).ToString();
                        PropiedadesArchivoActual.FormatoCompresion = ".lzw";
                        GuaradarCompresiones(PropiedadesArchivoActual);
                    }
                }
            }
            Success(string.Format("Archivo comprimido exitosamente"), true);
            var FileVirtualPath = @"~/App_Data/Compresiones/" + nombreArchivo + ".lzw";
            return File(FileVirtualPath, "application / force - download", Path.GetFileName(FileVirtualPath));
        }
        public ActionResult DescompresionLZWImportar()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DescompresionLZWImportar(HttpPostedFileBase ArchivoImportado)
        {
            Directory.CreateDirectory(Server.MapPath("~/App_Data/ArchivosImportados/"));
            Directory.CreateDirectory(Server.MapPath("~/App_Data/Descompresiones/"));
            var DireccionArchivo = string.Empty;
            var ArchivoMapeo = Server.MapPath("~/App_Data/ArchivosImportados/");
            var extensionArchivo = string.Empty;
            if (ArchivoImportado != null)
            {
                DireccionArchivo = ArchivoMapeo + Path.GetFileName(ArchivoImportado.FileName);
                var extension = Path.GetExtension(ArchivoImportado.FileName);
                ArchivoImportado.SaveAs(DireccionArchivo);
                var DiccionarioText = string.Empty;
                var DiccionarioCaracteres = new Dictionary<int, string>();
                var byteBuffer = new byte[bufferLength];
                FileInfo ArchivoAnalizado = new FileInfo(DireccionArchivo);
                nombreArchivo = ArchivoAnalizado.Name.Split('.')[0];
                if (extension == ".lzw")
                {
                    using (var Lectura = new BinaryReader(ArchivoImportado.InputStream))
                    {
                        var CaracterDiccionario = Convert.ToChar(Lectura.ReadByte());
                        var CantidadCaracteresDiccionatrio = string.Empty;
                        while (CaracterDiccionario != '.')
                        {
                            CantidadCaracteresDiccionatrio += CaracterDiccionario;
                            CaracterDiccionario = Convert.ToChar(Lectura.ReadByte());
                        }
                        var CantidadTexto = string.Empty;
                        CaracterDiccionario = Convert.ToChar(Lectura.ReadByte());
                        while (CaracterDiccionario != '.')
                        {
                            CantidadTexto += CaracterDiccionario;
                            CaracterDiccionario = Convert.ToChar(Lectura.ReadByte());
                        }
                        CaracterDiccionario = Convert.ToChar(Lectura.PeekChar());
                        var byteEscrito = Lectura.ReadByte();
                        while (DiccionarioCaracteres.Count!=Convert.ToInt32(CantidadCaracteresDiccionatrio))
                        {
                            if (!DiccionarioCaracteres.ContainsValue(Convert.ToString(Convert.ToChar(byteEscrito))))
                            {
                                DiccionarioCaracteres.Add(DiccionarioCaracteres.Count + 1, Convert.ToString(Convert.ToChar(byteEscrito)));
                            }
                            byteEscrito = Lectura.ReadByte();
                        }
                        Lectura.ReadByte();
                        Lectura.ReadByte();
                        CaracterDiccionario = Convert.ToChar(Lectura.ReadByte());
                        var TamanoBits = string.Empty;
                        while (CaracterDiccionario != '.')
                        {
                            TamanoBits += CaracterDiccionario;
                            CaracterDiccionario = Convert.ToChar(Lectura.ReadByte());
                        }
                        CaracterDiccionario = Convert.ToChar(Lectura.ReadByte());
                        while (CaracterDiccionario != '\u0002')
                        {
                            extensionArchivo += CaracterDiccionario;
                            CaracterDiccionario = Convert.ToChar(Lectura.ReadByte());
                        }
                        extensionArchivo = "." + extensionArchivo;
                        var byteAnalizado = string.Empty;
                        var listaCaracteresComprimidos = new List<int>();
                        Lectura.ReadByte();
                        Lectura.ReadByte();
                        while (Lectura.BaseStream.Position != Lectura.BaseStream.Length && listaCaracteresComprimidos.Count<Convert.ToInt32(CantidadTexto))
                        {
                            var byteLeido = Convert.ToString(Lectura.ReadByte(), 2);
                            while (byteLeido.Length < 8)
                            {
                                byteLeido = "0" + byteLeido;
                            }
                            byteAnalizado += byteLeido;
                            if (Convert.ToInt32(TamanoBits) > 8)
                            {
                                if (byteAnalizado.Length >= Convert.ToInt32(TamanoBits))
                                {
                                    var caracterComprimido = string.Empty;
                                    for (int i = 0; i < Convert.ToInt32(TamanoBits); i++)
                                    {
                                        caracterComprimido += byteAnalizado[i];
                                    }
                                    listaCaracteresComprimidos.Add(Convert.ToInt32(caracterComprimido, 2));
                                    byteAnalizado = byteAnalizado.Substring(Convert.ToInt32(TamanoBits));
                                }
                            }
                            else
                            {
                                listaCaracteresComprimidos.Add(Convert.ToInt32(byteAnalizado, 2));
                                byteAnalizado = string.Empty;
                            }
                        }
                        if (byteAnalizado.Length>0)
                        {
                            listaCaracteresComprimidos[listaCaracteresComprimidos.Count - 1] = listaCaracteresComprimidos[listaCaracteresComprimidos.Count - 1] + Convert.ToInt32(byteAnalizado, 2);
                        }
                        var primerCaracter = DiccionarioCaracteres[listaCaracteresComprimidos[0]];
                        listaCaracteresComprimidos.RemoveAt(0);
                        var decompressed = new System.Text.StringBuilder(primerCaracter);
                        using (var ArchivoNuevo = new FileStream(Server.MapPath(@"~/App_Data/Descompresiones/" + nombreArchivo + extensionArchivo), FileMode.OpenOrCreate))
                        {
                            using (var writer = new StreamWriter(ArchivoNuevo))
                            {
                                foreach (var item in listaCaracteresComprimidos)
                                {
                                    var cadenaAnalizada = string.Empty;
                                    if (DiccionarioCaracteres.ContainsKey(item))
                                    {
                                        cadenaAnalizada = DiccionarioCaracteres[item];
                                    }
                                    else if (item == DiccionarioCaracteres.Count + 1)
                                    {
                                        cadenaAnalizada = primerCaracter + primerCaracter[0];
                                    }
                                    decompressed.Append(cadenaAnalizada);
                                    DiccionarioCaracteres.Add(DiccionarioCaracteres.Count + 1, primerCaracter + cadenaAnalizada[0]);
                                    primerCaracter = cadenaAnalizada;
                                }
                                writer.Write(decompressed.ToString());
                            }
                        }
                    }
                }
                else
                {
                    Danger("Formato de archivo no es '.lzw'", true);
                }
            }
            else
            {
                Danger("El archivo es nulo.", true);
            }
            var FileVirtualPath = @"~/App_Data/Descompresiones/" + nombreArchivo + extensionArchivo;
            return File(FileVirtualPath, "application / force - download", Path.GetFileName(FileVirtualPath));
        }
        public ActionResult CompresionHImportar()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CompresionHImportar(HttpPostedFileBase ArchivoImportado)
        {
            var NodoRaiz = new Nodo();
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
            NodoRaiz.TotalDeCaracteres = TotalDeCaracteres;
            TextoEnBinario = NodoRaiz.ArmarArbolHuffman(ListaCaracteresExistentes);
            DiccionarioIndices = NodoRaiz.DiccionarioIndices;

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

        //Metodos
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
        List<PropiedadesArchivo> LeerMisCompresiones()
        {
            var Lista = new List<PropiedadesArchivo>();
            string archivoLeer = string.Empty;
            string ArchivoMapeo = Server.MapPath("~/App_Data/");
            archivoLeer = ArchivoMapeo + Path.GetFileName("ListaCompresiones");
            PropiedadesArchivo Prueba = new PropiedadesArchivo();
            GuaradarCompresiones(Prueba);
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
                        PorcentajeReduccion = Cadena.Split('|')[5],
                        FormatoCompresion = Cadena.Split('|')[6]
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
                if (!(Archivo.TamanoArchivoComprimido <= 0 && Archivo.TamanoArchivoDescomprimido <= 0))
                {
                    writer.WriteLine(Archivo.NombreArchivoOriginal + "|" + Archivo.TamanoArchivoDescomprimido + "|" + Archivo.TamanoArchivoComprimido + "|" + Archivo.FactorCompresion + "|" + Archivo.RazonCompresion + "|" + Archivo.PorcentajeReduccion + "|" + Archivo.FormatoCompresion);
                }
            }

        }
    }
}




