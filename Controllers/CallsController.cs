using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TelephoneCallsWebApplication.Data;
using TelephoneCallsWebApplication.Models;

namespace TelephoneCallsWebApplication.Controllers
{
    public class CallsController : Controller
    {
        private readonly TelephoneCallsContext _context;
        private readonly IStringLocalizer<HomeController> _localizer;

        public CallsController(TelephoneCallsContext context, IStringLocalizer<HomeController> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        // GET: Calls/Details/3333333
        public async Task<IActionResult> Details(int? phoneNumber)
        {
            if (phoneNumber == null)
            {
                return NotFound();
            }

            var calls = await _context.Calls
                .Include(x => x.Events).Where(x => x.Caller == phoneNumber).ToListAsync();

            if (calls == null)
            {
                return NotFound();
            }

            ViewData["phoneNumber"] = phoneNumber.Value;

            var vcalls = calls.ToList().OrderByDescending(x => x.Timestamp).ToList();

            return View(vcalls);
        }
    }
}
