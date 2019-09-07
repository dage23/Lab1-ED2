﻿using System;
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
        //public string diccionario;
        public Dictionary<string, char> DiccionarioIndices = new Dictionary<string, char>();
        public string TextoEnBinario = "";
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
            using (var writeStream = new FileStream(Server.MapPath(@"~/App_Data/" + nombreArchivo[0] + ".huff"), FileMode.OpenOrCreate))
            {
                using (var writer = new BinaryWriter(writeStream))
                {
                    writer.Write(nombreArchivo[1]);
                    writer.Write((TotalDeCaracteres).ToString()+Environment.NewLine);
                    foreach (var item in DiccionarioIndices)
                    {
                        writer.Write((item.Value).ToString() + "&" + item.Key + "|");
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
                            if (contadorBuffer==bufferLength)
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
                                if (contador==TextoEnBinario.Length-1)
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
                    writer.Write(Environment.NewLine);
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
        void SumaCaracteres()
        {
            for (int i = 0; i < ListaCaracteresExistentes.Count; i++)
            {
                for (int j = 0; j < ListaCaracteresExistentes.Count; j++)
                {
                    if (ListaCaracteresExistentes[i].CaracterTexto == ListaCaracteresExistentes[j].CaracterTexto && !ListaCaracteresExistentes[j].CaracterYaRecorrido)
                    {
                        ListaCaracteresExistentes[i].Frecuencia += 1;
                        ListaCaracteresExistentes[i].CaracterAUsar = true;
                        ListaCaracteresExistentes[j].CaracterYaRecorrido = true;
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
        #endregion
    }
}




