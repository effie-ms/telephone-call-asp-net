using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using TelephoneCallsWebApplication.Data;
using TelephoneCallsWebApplication.Models;

namespace TelephoneCallsWebApplication.Controllers
{
    public class EventsController : Controller
    {
        private const int DEFAULT_PAGE_SIZE = 5;

        private readonly IStringLocalizer<HomeController> _localizer;
        private readonly TelephoneCallsContext _context;
        private IHostingEnvironment _hostingEnvironment;

        public EventsController(TelephoneCallsContext context, IHostingEnvironment hostingEnvironment, IStringLocalizer<HomeController> localizer)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _localizer = localizer;
        }

        // GET: Events
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, string searchBy, int? page, int? pageSize, List<string> eventTypes)
        {
            ViewData["CallerSortParm"] = String.IsNullOrEmpty(sortOrder) ? "caller_desc" : "";
            ViewData["ReceiverSortParm"] = sortOrder == "Receiver" ? "receiver_desc" : "Receiver";
            searchBy = !string.IsNullOrWhiteSpace(searchBy) ? searchBy : ((ViewData["SearchBy"] == null) ? "Caller" : ViewData["SearchBy"].ToString());
            ViewData["SearchBy"] = searchBy;
            ViewData["CurrentSort"] = sortOrder;

            foreach (var item in eventTypes)
            {
                ViewData["EventTypes"] += (item + ',');
            }
         
            if (!string.IsNullOrEmpty(searchString))
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var events = GetFilteredAndSortedData(eventTypes, searchString, searchBy, sortOrder);

            int recordsPerPage = pageSize.HasValue ? pageSize.Value : DEFAULT_PAGE_SIZE;
            ViewData["PageSize"] = recordsPerPage;
            return View(await PaginatedList<Event>.CreateAsync(events.AsNoTracking(), page ?? 1, recordsPerPage));
        }

        private IQueryable<Event> GetFilteredAndSortedData(List<string> eventTypes, string searchString, string searchBy, string sortOrder)
        {
            if (eventTypes.Count == 0)
            {
                eventTypes = new List<string>()
                {
                    EventType.EventDial.EventName,
                    EventType.EventHangUp.EventName,
                    EventType.EventPickUp.EventName,
                    EventType.EventCallEstablished.EventName,
                    EventType.EventCallEnd.EventName
                };
            }

            if (eventTypes.Count == 1)
            {
                eventTypes = eventTypes[0].Split(',').ToList();
            }

            var events = from _event in _context.Events.Include(x => x.Call).Include(x => x.EventType) select _event;
            events = events.Where(x => eventTypes.Contains(x.EventType.EventName));

            if (!string.IsNullOrEmpty(searchString))
            {
                if (!string.IsNullOrWhiteSpace(searchBy) && searchBy == "Receiver")
                {
                    events = events.Where(x => x.Call.Receiver.ToString().StartsWith(searchString));
                }
                else
                {
                    events = events.Where(x => x.Call.Caller.ToString().StartsWith(searchString));
                }
            }
            switch (sortOrder)
            {
                case "caller_desc":
                    events = events.OrderByDescending(x => x.Call.Caller);
                    break;
                case "Receiver":
                    events = events.OrderBy(x => x.Call.Receiver);
                    break;
                case "receiver_desc":
                    events = events.OrderByDescending(x => x.Call.Receiver);
                    break;
                default:
                    events = events.OrderBy(x => x.Call.Caller);
                    break;
            }
            return events;

        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var _event = await _context.Events
                .Include(x => x.Call).Include(x => x.Call.Events)
                .Include(x => x.EventType)
                .SingleOrDefaultAsync(m => m.ID == id);

            if (_event == null)
            {
                return NotFound();
            }

            _event.Call.Events = _event.Call.Events.OrderBy(x => x.Date).ToList();

            return View(_event);
        }

        // POST: Events/Export
        public async Task<IActionResult> Export(string sortOrder, string currentFilter, string searchBy, List<string> eventTypes)
        {
            string sWebRootFolder = _hostingEnvironment.WebRootPath;
            DateTime todayDate = DateTime.Now;
            string month = todayDate.Month.ToString();
            month = month.Length == 1 ? "0" + month : month;
            string day = todayDate.Day.ToString();
            day = day.Length == 1 ? "0" + day : day;
            string todayString = todayDate.Year.ToString() + month + day;
            string fileName = "all_records_" + todayString + ".csv";
            string sFileName = @fileName;
            string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, sFileName);
            FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            var memory = new MemoryStream();
            using (var fs = new FileStream(Path.Combine(sWebRootFolder, sFileName), FileMode.Create, FileAccess.Write))
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet(_localizer["All Records"]);
                IRow row = excelSheet.CreateRow(0);
                row.CreateCell(0).SetCellValue(_localizer["Caller"]);
                row.CreateCell(1).SetCellValue(_localizer["Event"]);
                row.CreateCell(2).SetCellValue(_localizer["Receiver"]);
                row.CreateCell(3).SetCellValue(_localizer["Timestamp"]);

                var events = GetFilteredAndSortedData(eventTypes, currentFilter, searchBy, sortOrder).ToList();
                for (int i = 1; i <= events.Count(); i++)
                {
                    row = excelSheet.CreateRow(i);
                    row.CreateCell(0).SetCellValue(events[i - 1].Call.Caller.ToString());
                    row.CreateCell(1).SetCellValue(events[i - 1].EventType.EventName);
                    row.CreateCell(2).SetCellValue(events[i - 1].Call.Receiver.HasValue ? events[i - 1].Call.Receiver.Value.ToString() : "-");
                    row.CreateCell(3).SetCellValue(events[i - 1].Date.ToString());
                }
                workbook.Write(fs);
            }
            using (var stream = new FileStream(Path.Combine(sWebRootFolder, sFileName), FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", sFileName);
        }
    }
}
