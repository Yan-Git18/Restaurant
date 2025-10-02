using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    }
}
        