using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreTodo.Models;
using AspNetCoreTodo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AspNetCoreTodo.Controllers
{

    [Authorize]
    public class TodoController : Controller
    {
        private readonly ITodoItemService _todoItemService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        private readonly ILogger _logger;

        public TodoController(ITodoItemService todoItemService, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _todoItemService = todoItemService;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await ValidateUser();
            if (currentUser == null) return Challenge();

            var items = await _todoItemService
                .GetIncompleteItemsAsync(currentUser);

            var model = new TodoViewModel()
            {
                Items = items
            };

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult MassiveItemLoad(string MassiveLoad)
        {
            try
            {
                var MassiveLoadItems = JsonConvert.DeserializeObject<IEnumerable<TodoItem>>(MassiveLoad);

                if (!VerifyRegs(MassiveLoadItems))
                {
                    return Json(new { Resultado = "No se validó la carga masiva." });
                }

                ReemplazarIdsExt(MassiveLoadItems);

                _todoItemService.AddItems(MassiveLoadItems, "MassiveLoad");

                return Json(new { Resultado = "Sin error, la carga fue sastifactoria." });

            }
            catch (Exception)
            {
                return Json(new { Resultado = "Ocurrió un error al ejecutar la carga masiva." });
            }

        }

        private void ReemplazarIdsExt(IEnumerable<TodoItem> massiveLoadItems)
        {
            foreach (TodoItem item in massiveLoadItems)
            {
                item.UserId = _userManager.Users.FirstOrDefault(user => user.UserName == item.UserId).Id;
            }

        }

        #region validaciones 
        private bool VerifyRegs(IEnumerable<TodoItem> load)
        {

            string Resultado = string.Empty;
            var existenUsuarios = load.All(itemLoad => _userManager.Users.Any(user => user.UserName == itemLoad.UserId));
            if (!existenUsuarios)
                Resultado += "ERROR: Uno o más usuarios ingresados no existen en el sistema. -\r\n";
            var TareasCompletas = load.All(itemLoad => !string.IsNullOrWhiteSpace(itemLoad.Title));
            if (!TareasCompletas)
                Resultado += "ERROR: Uno o más Tareas se encuentran en blanco. -\r\n";

            return existenUsuarios && TareasCompletas;
        }
        #endregion

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(TodoItem newItem)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }

            var currentUser = await ValidateUser();
            if (currentUser == null) return Challenge();

            var successful = await _todoItemService
                .AddItemAsync(newItem, currentUser, null);

            if (!successful)
            {
                return BadRequest("Could not add item.");
            }

            return RedirectToAction("Index");
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkDone(Guid id)
        {
            if (id == Guid.Empty)
            {
                return RedirectToAction("Index");
            }
            var currentUser = await ValidateUser();
            if (currentUser == null) return Challenge();

            var successful = await _todoItemService
                .MarkDoneAsync(id, currentUser);

            if (!successful)
            {
                return BadRequest("Could not mark item as done.");
            }

            return RedirectToAction("Index");
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMailToExpiredTasksUsers()
        {
            var items = await _todoItemService
     .GetItemsToSendMailAsync();
            foreach (TodoItem item in items)
            {
                var userMail = _userManager.Users.FirstOrDefault(user => user.Id == item.UserId).UserName;
                if (item.DueAt < DateTime.Now)
                    await _emailSender.SendEmailAsync(userMail, "Tarea Vencida", "La tarea < " + item.Title + " > se encuentra vencida.");
                else
                    await _emailSender.SendEmailAsync(userMail, "Tarea próxima a Vencer", "Se esta por vencer la tarea < " + item.Title + " > en las próximas 24 hs");

            }

            return RedirectToAction("Index");
        }
        private async Task<ApplicationUser> ValidateUser()
        {
            return await _userManager.GetUserAsync(User);
        }

    }
}