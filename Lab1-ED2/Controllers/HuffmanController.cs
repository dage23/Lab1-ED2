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
                //Crear Lista de Nodos
                CrearListaDeNodos();
                //Armar Arbol
                ArmarArbol();
                //Crear Diccionario
                Diccionario();
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
                Nodo NodoAux = new Nodo
                {
                    caracter = Datos.Instance.ListaCaracteresExistentes.ElementAt(i)
                };
                NodoAux.probabilidad = Convert.ToDouble(NodoAux.caracter.Frecuencia) / Convert.ToDouble(TotalDeCaracteres);
                Datos.Instance.ListaNodosArbol.Add(NodoAux);
            }
        }
        #endregion

        #region ArmarArbol
        void ArmarArbol()
        {
            var MetodoCopara = new Datos.Comparar();
            Datos.Instance.ListaNodosArbol.Sort(MetodoCopara); //ordena la lista de mayor a menor

            try
            {
                while (Datos.Instance.ListaNodosArbol[1] != null)
                {
                    int TamanoLista = Datos.Instance.ListaNodosArbol.Count;
                    Nodo auxPadre = new Nodo();
                    Nodo auxIzq = Datos.Instance.ListaNodosArbol[TamanoLista - 1];
                    Nodo auxDcha = Datos.Instance.ListaNodosArbol[TamanoLista - 2];
                    auxPadre.probabilidad = auxIzq.probabilidad + auxDcha.probabilidad;
                    auxPadre.NodoHijoDcha = auxDcha;
                    auxPadre.NodoHijoIzq = auxIzq;
                    auxPadre.NodoHijoIzq.NodoPadre = auxPadre;
                    auxPadre.NodoHijoDcha.NodoPadre = auxPadre;

                    Datos.Instance.ListaNodosArbol[TamanoLista - 2] = auxPadre;
                    Datos.Instance.ListaNodosArbol.RemoveAt(TamanoLista - 1);
                }
            }
            catch (Exception)
            {

                Datos.Instance.cNodoRaiz = Datos.Instance.ListaNodosArbol[0];
                Datos.Instance.enOrden(Datos.Instance.cNodoRaiz);
            }
        }
        #endregion
        #region crearDiccionario
        void Diccionario()
        {
        }
    }
    #endregion
}





