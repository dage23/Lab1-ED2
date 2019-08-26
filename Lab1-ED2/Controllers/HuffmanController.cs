using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Lab1_ED2.Models;
namespace Lab1_ED2.Controllers
{
    public class HuffmanController : Controller
    {
        
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
                var ListaCaracteres = new List<char>();
                for (int i = 0; i < ArregloTexto.Length; i++)
                {
                    if (!(ListaCaracteres.Contains(ArregloTexto[i])))
                    {
                        ListaCaracteres.Add(ArregloTexto[i]);
                    }
                }
                var FrecuenciaCaracteres = new int[ListaCaracteres.Count];
                for (int i = 0; i < FrecuenciaCaracteres.Length; i++)
                {
                    FrecuenciaCaracteres[i] = 0;
                }

                for (int i = 0; i < ListaCaracteres.Count; i++)
                {
                    char CaracterEvaluando = ListaCaracteres[i];
                    for (int j = 0; j < ArregloTexto.Length; j++)
                    {
                        if (CaracterEvaluando==ArregloTexto[j])
                        {
                            FrecuenciaCaracteres[i]++;
                        }
                    }
                }

            }
                return View();
        }

        // GET: Huffman/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Huffman/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Huffman/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Huffman/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Huffman/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Huffman/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Huffman/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
