using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Lab1_ED2.Models;
using Lab1_ED2.Helper;
namespace Lab1_ED2.Controllers
{
    public class HuffmanController : Controller
    {
        public List<Caracter> ListaCaracteresExistentes = new List<Caracter>();
        public List<Nodo> ListaNodosArbol = new List<Nodo>();
        public Nodo cNodoRaiz;
        public static int TotalDeCaracteres;
        public string diccionario;
        public static Dictionary<string, char> DiccionarioIndices = new Dictionary<string, char>();
        public ActionResult Importar()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Importar(HttpPostedFileBase ArchivoImportado)
        {
            var ListaCaracteresExistentes = new List<Caracter>();
       
            var ListaNodosArbol = new List<Nodo>();


        string Rutaarchivo = string.Empty;
            if (ArchivoImportado != null)
            {
                string Ruta = Server.MapPath("~/Uploads/");
                if (!Directory.Exists(Ruta))
                {
                    Directory.CreateDirectory(Ruta);
                }
                Rutaarchivo = Ruta + Path.GetFileName(ArchivoImportado.FileName);
                string extension = Path.GetExtension(ArchivoImportado.FileName);
                ArchivoImportado.SaveAs(Rutaarchivo);
                string TextoArchivo = System.IO.File.ReadAllText(Rutaarchivo);
                char[] ArregloTexto = TextoArchivo.ToCharArray();
                TotalDeCaracteres = ArregloTexto.Length;
                //Crear Lista
                CrearLista(ArregloTexto);
                //Ordenamiento
                OrdenarLista();
                //Crear Lista de Nodos
                CrearListaDeNodos();
                //Armar Arbol
                ArmarArbol();
                //Crear Diccionario
            }
            return View();
        }
        #region CrearLista
        void CrearLista(char[] ArregloTexto)
        {
            var ListaCaracteres = new List<char>();
            for (int i = 0; i < ArregloTexto.Length; i++)
            {
                if (!(ListaCaracteres.Contains(ArregloTexto[i])))
                {
                    ListaCaracteres.Add(ArregloTexto[i]);
                }
            }
            var FrecuenciaCaracteres = new int[ListaCaracteres.Count];
            for (int q = 0; q < FrecuenciaCaracteres.Length; q++)
            {
                FrecuenciaCaracteres[q] = 0;
            }
            for (int w = 0; w < ListaCaracteres.Count; w++)
            {
                char CaracterEvaluando = ListaCaracteres[w];
                for (int j = 0; j < ArregloTexto.Length; j++)
                {
                    if (CaracterEvaluando == ArregloTexto[j])
                    {
                        FrecuenciaCaracteres[w]++;
                    }
                }
            }
            for (int i = 0; i < FrecuenciaCaracteres.Length; i++)
            {
                var ClaseAux = new Caracter();
                ClaseAux.CaracterTexto = ListaCaracteres[i];
                ClaseAux.Frecuencia = FrecuenciaCaracteres[i];
                ListaCaracteresExistentes.Add(ClaseAux);
            }
        }
        #endregion

        #region OrdenarLista
        void OrdenarLista()
        {
            for (int i = 0; i < ListaCaracteresExistentes.Count - 1; i++)
            {
                for (int j = 0; j < ListaCaracteresExistentes.Count - 1; j++)
                {
                    if (ListaCaracteresExistentes[j].Frecuencia > ListaCaracteresExistentes[j + 1].Frecuencia)
                    {
                        int temp = ListaCaracteresExistentes[j].Frecuencia;
                        ListaCaracteresExistentes[j].Frecuencia = ListaCaracteresExistentes[j + 1].Frecuencia;
                        ListaCaracteresExistentes[j + 1].Frecuencia = temp;

                        char tempcChar = ListaCaracteresExistentes[j].CaracterTexto;
                        ListaCaracteresExistentes[j].CaracterTexto = ListaCaracteresExistentes[j + 1].CaracterTexto;
                        ListaCaracteresExistentes[j + 1].CaracterTexto = tempcChar;
                    }
                }
            }
        }
        #endregion

        #region CrearListaDeNodos
        void CrearListaDeNodos()
        {
            int CantTotalCaracteres = ListaCaracteresExistentes.Count;
            for (int i = 0; i < CantTotalCaracteres; i++)
            {
                Nodo NodoAux = new Nodo
                {
                    caracter = ListaCaracteresExistentes.ElementAt(i)
                };
                NodoAux.probabilidad = Convert.ToDouble(NodoAux.caracter.Frecuencia) / Convert.ToDouble(TotalDeCaracteres);
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
                    Nodo auxPadre = new Nodo();
                    Nodo auxIzq = ListaNodosArbol[TamanoLista - 1];
                    Nodo auxDcha = ListaNodosArbol[TamanoLista - 2];
                    auxPadre.probabilidad = auxIzq.probabilidad + auxDcha.probabilidad;
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

    }
}





