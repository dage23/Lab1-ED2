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
        public static int TotalDeCaracteres;
        public ActionResult Importar()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Importar(HttpPostedFileBase ArchivoImportado)
        {
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
                Datos.Instance.ListaCaracteresExistentes.Add(ClaseAux);
            }
        }
        #endregion

        #region OrdenarLista
        void OrdenarLista()
        {
            for (int i = 0; i < Datos.Instance.ListaCaracteresExistentes.Count - 1; i++)
            {
                for (int j = 0; j < Datos.Instance.ListaCaracteresExistentes.Count - 1; j++)
                {
                    if (Datos.Instance.ListaCaracteresExistentes[j].Frecuencia > Datos.Instance.ListaCaracteresExistentes[j + 1].Frecuencia)
                    {
                        int temp = Datos.Instance.ListaCaracteresExistentes[j].Frecuencia;
                        Datos.Instance.ListaCaracteresExistentes[j].Frecuencia = Datos.Instance.ListaCaracteresExistentes[j + 1].Frecuencia;
                        Datos.Instance.ListaCaracteresExistentes[j + 1].Frecuencia = temp;

                        char tempcChar = Datos.Instance.ListaCaracteresExistentes[j].CaracterTexto;
                        Datos.Instance.ListaCaracteresExistentes[j].CaracterTexto = Datos.Instance.ListaCaracteresExistentes[j + 1].CaracterTexto;
                        Datos.Instance.ListaCaracteresExistentes[j + 1].CaracterTexto = tempcChar;
                    }
                }
            }
        }
        #endregion

        #region CrearListaDeNodos
        void CrearListaDeNodos()
        {
            int CantTotalCaracteres = Datos.Instance.ListaCaracteresExistentes.Count;
            for (int i = 0; i < CantTotalCaracteres; i++)
            {
                Nodo NodoAux = new Nodo();
                NodoAux.caracter = Datos.Instance.ListaCaracteresExistentes.ElementAt(i);
                NodoAux.probabilidad = NodoAux.caracter.Frecuencia / TotalDeCaracteres;
                ArbolH.Instance.ListaNodosArbol.Add(NodoAux);
            }
        }
        #endregion

        #region ArmarArbol
        void ArmarArbol()
        {
            Comparar MetodoCopara = new Comparar();
            ArbolH.Instance.ListaNodosArbol.Sort(MetodoCopara); //ordena la lista de mayor a menor
            while (ArbolH.Instance.ListaNodosArbol[1]!=null)
            {
                int TamanoLista = ArbolH.Instance.ListaNodosArbol.Count;
                Nodo auxPadre = new Nodo();
                Nodo auxIzq = ArbolH.Instance.ListaNodosArbol[TamanoLista - 1];
                Nodo auxDcha = ArbolH.Instance.ListaNodosArbol[TamanoLista - 2];
                auxPadre.probabilidad = auxIzq.probabilidad + auxDcha.probabilidad;
                auxPadre.NodoHijoDcha = auxDcha;
                auxPadre.NodoHijoIzq = auxIzq;
                auxPadre.NodoHijoIzq.NodoPadre = auxPadre;
                auxPadre.NodoHijoDcha.NodoPadre = auxPadre;

                ArbolH.Instance.ListaNodosArbol[TamanoLista - 2] = auxPadre;
                ArbolH.Instance.ListaNodosArbol.RemoveAt(TamanoLista - 1);
            }
            Nodo.Instance.NodoRaiz = ArbolH.Instance.ListaNodosArbol[0];
            Nodo.Instance.NodoRaiz.enOrden(Nodo.Instance.NodoRaiz);
        }
        #endregion

        #region crearDiccionario
        void Diccionario ()
        {
            //Buscar los nodos hoja y asignar el valor del indice en la lista de caracteres para cada caracter
            //crear un diccionario a partir de eso

        }
        #endregion
    }

}

