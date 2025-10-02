using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using RESTAURANT.Data;
using System;

namespace Restaurant.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CategoriasController : Controller
    {
        private readonly AppDbContext _context;

        public CategoriasController(AppDbContext appDBContext)
        {
            _context = appDBContext;
        }

        public async Task<IActionResult> Index()
        {
            var cat = await _context.Categorias.ToListAsync();
            return View(cat);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Rest_Categoria cat)
        {
            var existeCat = await _context.Categorias
                .FirstOrDefaultAsync(r => r.Nombre == cat.Nombre);

            if (existeCat != null)
            {
                TempData["Mensaje"] = "La categoría ya esta registrada.";
                TempData["Tipo"] = "error";
                return View(cat);
            }
            else
            {
                var ct = new Rest_Categoria
                {
                    Nombre = cat.Nombre,
                    Descripcion = cat.Descripcion,
                    Activo = true,
                    Orden = cat.Orden
                };
                await _context.Categorias.AddAsync(ct);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "La categoría se ha creado correctamente.";
                TempData["Tipo"] = "success";
                return RedirectToAction("Index");
            }

        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            var cat = _context.Categorias.Find(id);
            if (cat == null)
            {
                return NotFound();
            }
            return View(cat);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Rest_Categoria cat)
        {
            _context.Categorias.Update(cat);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "La categoría se ha actualizado correctamente.";
            TempData["Tipo"] = "success";
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public IActionResult Eliminar(int id)
        {
            var cat = _context.Categorias.Find(id);
            if (cat == null)
            {
                return NotFound();
            }
            return View(cat);
        }

        [HttpPost]
        public IActionResult EliminarConfirmar(int id)
        {
            var cat = _context.Categorias.Find(id);
            _context.Categorias.Remove(cat);
            _context.SaveChanges();

            TempData["Mensaje"] = "La categoría se ha eliminado correctamente.";
            TempData["Tipo"] = "success";
            return RedirectToAction(nameof(Index));
        }
    }
}

        

 