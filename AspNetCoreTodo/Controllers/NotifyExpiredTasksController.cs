using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreTodo.Models;
using AspNetCoreTodo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreTodo.Controllers
{

    [Authorize(Roles = "Administrator")]
    public class NotifyExpiredTasksController : Controller
    {

        private readonly ITodoItemService _todoItemService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public NotifyExpiredTasksController(ITodoItemService todoItemService, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _todoItemService = todoItemService;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> Index()
        {
            if (await ValidateUser() == null) return Challenge();

            return View();
        }

        public async Task<IActionResult> Done()
        {
            if (await ValidateUser() == null) return Challenge();
            return RedirectToAction("Index");
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendNotifications()
        {
            if (await ValidateUser() == null) return Challenge();

            var items = await _todoItemService.GetItemsToSendMailAsync();

            foreach (TodoItem item in items)
            {
                var userMail = _userManager.Users.FirstOrDefault(user => user.Id == item.UserId).UserName;
                if (item.DueAt < DateTime.Now)
                    await _emailSender.SendEmailAsync(userMail, "Tarea Vencida", $"La tarea < {item.Title} > se encuentra vencida.");
                else
                    await _emailSender.SendEmailAsync(userMail, "Tarea próxima a Vencer", $"Se esta por vencer la tarea < {item.Title} > en las próximas 24 hs");

            }

            return View("Done");
        }

        private async Task<ApplicationUser> ValidateUser()
        {
            return await _userManager.GetUserAsync(User);
        }
    }
}