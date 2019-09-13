using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text;
using Lab1_ED2.Models;
using Lab1_ED2.Helper;
namespace Lab1_ED2.Controllers
{
    public class HuffmanController : BaseController
    {
        const int bufferLength = 20;
        public List<Caracter> ListaCaracteresExistentes = new List<Caracter>();
        public List<Caracter> ListaCaracteresFinales = new List<Caracter>();
        public List<Nodo> ListaNodosArbol = new List<Nodo>();
        public Nodo cNodoRaiz;
        public int TotalDeCaracteres;
        public Dictionary<string, byte> DiccionarioIndices = new Dictionary<string, byte>();
        public string TextoEnBinario = "";
        public string nombreArchivo;

        public ActionResult Menu()
        {
            return View();
        }
        public ActionResult VerMisCompresiones()
        {
            return View(Datos.Instance.PilaArchivosComprimidos);
        }
        public ActionResult MenuHuffman()
        {
            return View();
        }
        public ActionResult CompresionHImportar()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CompresionHImportar(HttpPostedFileBase ArchivoImportado)
        {
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
            SumaCaracteres();
            //Crear Lista de Nodos
            CrearListaDeNodos();
            //Armar Arbol
            ArmarArbol();
            //Traducir A Binario
            TraductorABinario();
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
                    PropiedadesArchivoActual.RazonCompresion = Convert.ToDouble(PropiedadesArchivoActual.TamanoArchivoComprimido) / Convert.ToDouble(PropiedadesArchivoActual.TamanoArchivoDescomprimido);
                    PropiedadesArchivoActual.FactorCompresion = Convert.ToDouble(PropiedadesArchivoActual.TamanoArchivoDescomprimido) / Convert.ToDouble(PropiedadesArchivoActual.TamanoArchivoComprimido);
                    PropiedadesArchivoActual.PorcentajeReduccion = (Convert.ToDouble(1) - PropiedadesArchivoActual.RazonCompresion).ToString();
                    Datos.Instance.PilaArchivosComprimidos.Push(PropiedadesArchivoActual);
                }
            }
            Success(string.Format("Archivo comprimido exitosamente"), true);
            return View();
        }

        public ActionResult DescompresionHImportar()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DescompresionHImportar(HttpPostedFileBase ArchivoImportado)
        {
            var archivoLeer = string.Empty;
            var ArchivoMapeo = Server.MapPath("~/App_Data/ArchivosImportados/");
            if (ArchivoImportado != null)
            {
                archivoLeer = ArchivoMapeo + Path.GetFileName(ArchivoImportado.FileName);
                var extension = Path.GetExtension(ArchivoImportado.FileName);
                ArchivoImportado.SaveAs(archivoLeer);
                var Metadata = string.Empty;
                var NombreNuevoArchivo = string.Empty;
                var CantidadCaracteresCOnvertir = string.Empty;
                var DiccionarioText = string.Empty;
                var DiccionarioDescompresion = new Dictionary<string, char>();
                var ExtensionNuevoArchivo = "";
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
                            var ContadordeCaracteres = Convert.ToInt16(CantidadCaracteresCOnvertir);
                            Bs.BaseStream.Seek(caracteresCuenta + 4, SeekOrigin.Begin);
                            while (Bs.BaseStream.Position != fs.Length)
                            {
                                var Caracter = Bs.ReadByte();
                                int Decimal = Convert.ToInt32(Caracter);
                                var Binario = DecimalABinario(Decimal).ToCharArray();
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
                Danger("Formato de archivo no es 'huff'", true);
            }
            return View();
        }
        #region CrearLista
        void IntroducirALista(byte[] CaracteresAux)
        {
            for (int i = 0; i < CaracteresAux.Length; i++)
            {
                var ClaseAux = new Caracter();
                ClaseAux.CaracterTexto = CaracteresAux[i];
                ClaseAux.Frecuencia = 0;
                ListaCaracteresExistentes.Add(ClaseAux);
            }
        }
        void SumaCaracteres()
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
        #endregion
        #region CrearListaDeNodos
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
        #endregion
        #region ArmarArbol
        void ArmarArbol()
        {
            var MetodoCopara = new Comparar();
            //ordena la lista de mayor a menor
            int TamanoLista = ListaNodosArbol.Count;
            try
            {
                while (ListaNodosArbol[1] != null)
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
            }
            catch (Exception)
            {
                cNodoRaiz = ListaNodosArbol[0];
                cNodoRaiz.enOrden(cNodoRaiz);
                Diccionario(cNodoRaiz);
            }
        }
        #endregion
        #region crearDiccionario
        void Diccionario(Nodo nNodo)
        {
            if (nNodo != null)
            {
                if (nNodo.NodoHijoIzq == null)
                {
                    //diccionario += nNodo.caracter.CaracterTexto + nNodo.indice.ToString() + "|";
                    DiccionarioIndices.Add(nNodo.indice, nNodo.caracter.CaracterTexto);
                }
                Diccionario(nNodo.NodoHijoIzq);
                Diccionario(nNodo.NodoHijoDcha);
            }
        }
        #endregion
        #region MetodoOrdenarNodos
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
        #endregion
        #region Traducir a Binario
        void TraductorABinario()
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
        String DecimalABinario(int NUM)
        {
            int cont = 0;
            bool bandera = false;
            string regresa = "";
            while (bandera == false)
            {
                int a = Convert.ToInt32(Math.Pow(2, cont));
                if (NUM < a)
                {
                    bandera = true;
                }
                else
                {
                    cont++;
                }
            }
            var suma = 0;
            for (int i = cont - 1; i >= 0; i--)
            {
                int A = Convert.ToInt32(Math.Pow(2, i));
                if (suma + A > NUM)
                {
                    regresa = regresa + "0";
                }
                else
                {
                    regresa = regresa + "1";
                    suma += A;
                }
            }
            if (regresa.Length != 8)
            {
                string Aux = "";
                for (int i = regresa.Length; i < 8; i++)
                {
                    Aux = Aux + "0";
                }
                Aux = Aux + regresa;
                regresa = Aux;
            }
            return regresa;

        }

        #endregion
    }
}




