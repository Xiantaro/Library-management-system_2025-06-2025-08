using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using test2.Models;

namespace test2.ViewComponents
{
    public class RecommendViewComponent : ViewComponent
    {
        #region field
        private readonly Test2Context _context;
        #endregion

        #region constructor
        public RecommendViewComponent(Test2Context context) { _context = context; }
        #endregion

        #region action
        public async Task<IViewComponentResult> InvokeAsync()
        {
            Random random = new Random();

            var idAll = await _context.Collections.Select(i => i.CollectionId).ToListAsync();
            var idRandom = idAll.OrderBy(id => random.Next()).Take(12).ToList();
            var recommendX = await _context.Collections.Where(i => idRandom.Contains(i.CollectionId)).ToListAsync();

            return View("~/Areas/Frontend/Views/Shared/_Partial/_carousel.cshtml", recommendX);
        }
        #endregion
    }
}