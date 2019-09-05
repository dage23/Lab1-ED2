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
    public class HuffmanController : Controller
    {
        const int bufferLength = 10;
        public List<Caracter> ListaCaracteresExistentes = new List<Caracter>();
        public List<Caracter> ListaCaracteresFinales = new List<Caracter>();
        public List<Nodo> ListaNodosArbol = new List<Nodo>();
        public Nodo cNodoRaiz;
        public int TotalDeCaracteres;
        public string diccionario;
        public Dictionary<string, char> DiccionarioIndices = new Dictionary<string, char>();
        public string TextoBinarioTRY = "";
        public string[] nombreArchivo;
        public ActionResult Importar()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Importar(HttpPostedFileBase ArchivoImportado)
        {
            nombreArchivo = (ArchivoImportado.FileName).Split('.');
            using (var Lectura = new BinaryReader(ArchivoImportado.InputStream))
            {
                var byteBuffer = new byte[bufferLength];
                while (Lectura.BaseStream.Position != Lectura.BaseStream.Length)
                {
                    byteBuffer = Lectura.ReadBytes(bufferLength);
                    var result = Encoding.UTF8.GetChars(byteBuffer);
                    IntroducirALista(result);
                }
            }
            TotalDeCaracteres = ListaCaracteresExistentes.Count();
            SUma();
            //Crear Lista de Nodos
            CrearListaDeNodos();
            //Armar Arbol
            ArmarArbol();
            //Traducir A Binario
            TradBinario();
            while (TextoBinarioTRY.Length % 8 != 0)
            {
                TextoBinarioTRY += "0";
            }
            //Escritura Huffman
            using (var writeStream = new FileStream("C://Users//allan//Downloads//" + nombreArchivo[0] + ".huff", FileMode.OpenOrCreate))
            {
                using (var writer = new StreamWriter(writeStream))
                {
                    writer.WriteLine(TotalDeCaracteres.ToString());
                    var byteBuffer2 = new int[bufferLength];
                    var contadorBits = 0;
                    var contadorCaracteres = 0;
                    var contadorBuffer = 0;
                    var contador = 0;
                    var TextoenByte = "";
                    while (contador != TextoBinarioTRY.Length)
                    {
                        TextoenByte += TextoBinarioTRY[contador];
                        contadorBits++;

                        if (contadorBits == 8)
                        {
                            if (contadorCaracteres <= bufferLength)
                            {
                                var Caracteres = new char[bufferLength];
                                for (int i = 0; i < bufferLength; i++)
                                {
                                    Caracteres[i] = Convert.ToChar(byteBuffer2[i]);
                                }
                                for (int i = 0; i < bufferLength; i++)
                                {
                                    writer.Write(Caracteres[i]);
                                }

                                byteBuffer2 = new int[bufferLength];
                                contadorCaracteres = 0;
                            }
                            var TextoDecimal = Convert.ToInt32(TextoenByte, 2);
                            byteBuffer2[contadorCaracteres] = TextoDecimal;
                            contadorBuffer++;
                            contadorCaracteres++;
                            contadorBits = 0;
                            TextoenByte = "";
                        }
                        contador++;
                    }
                    writer.WriteLine();
                    foreach (var item in DiccionarioIndices)
                    {
                        writer.Write(Convert.ToString(item.Value) + "&" + item.Key + "|");
                    }
                }
            }

            return View();
        }
        #region CrearLista
        void IntroducirALista(char[] CaracteresAux)
        {
            for (int i = 0; i < CaracteresAux.Length; i++)
            {
                var ClaseAux = new Caracter();
                ClaseAux.CaracterTexto = CaracteresAux[i];
                ClaseAux.Frecuencia = 1;
                ListaCaracteresExistentes.Add(ClaseAux);
            }
        }
        void SUma()
        {
            for (int i = 0; i < ListaCaracteresExistentes.Count; i++)
            {
                for (int j = 0; j < ListaCaracteresExistentes.Count; j++)
                {
                    if (ListaCaracteresExistentes[i].CaracterTexto == ListaCaracteresExistentes[j].CaracterTexto && !ListaCaracteresExistentes[j].Recorrido)
                    {
                        ListaCaracteresExistentes[i].Frecuencia += 1;
                        ListaCaracteresExistentes[i].Tomar = true;
                        ListaCaracteresExistentes[j].Recorrido = true;
                    }

                }
            }
            for (int i = 0; i < ListaCaracteresExistentes.Count; i++)
            {
                if (ListaCaracteresExistentes[i].Tomar)
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
                NodoAux.probabilidad = Convert.ToDouble(NodoAux.caracter.Frecuencia) / (CantTotalCaracteres);
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
                    diccionario += nNodo.caracter.CaracterTexto + nNodo.indice.ToString() + "|";
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
        #region TraductorABinario
        void TradBinario()
        {
            for (int i = 0; i < ListaCaracteresExistentes.Count; i++)
            {
                foreach (var item in DiccionarioIndices)
                {
                    if (item.Value == ListaCaracteresExistentes[i].CaracterTexto)
                    {
                        TextoBinarioTRY += item.Key;
                        ListaCaracteresExistentes[i].binarioText = item.Key;
                    }
                }
            }
        }
        #endregion
    }
}




