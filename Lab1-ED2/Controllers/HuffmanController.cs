using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lab1_ED2.Controllers
{
    public class HuffmanController : Controller
    {
        // GET: Huffman
        public ActionResult Index()
        {
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
