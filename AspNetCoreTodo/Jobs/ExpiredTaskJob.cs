
using System;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreTodo.Models;
using AspNetCoreTodo.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;
namespace AspNetCoreTodo.Jobs
{
    public class ExpiredTaskJob : IJob
    {
        private readonly ITodoItemService _todoItemService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        public ExpiredTaskJob(ITodoItemService todoItemService, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _todoItemService = todoItemService;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"Por enviar emails - {DateTime.Now:r}");
            await SendMailToExpiredTasksUsers();
        }
        public async Task SendMailToExpiredTasksUsers()
        {
            var items = await _todoItemService
                .GetItemsToSendMailAsync();
            //SendMailToExpiredTasksUserAsync
            foreach (TodoItem item in items)
            {
                var userMail = _userManager.Users.FirstOrDefault(user => user.Id == item.UserId).UserName;
                if (item.DueAt < DateTime.Now)
                    await _emailSender.SendEmailAsync(userMail, "Tarea Vencida", "La tarea < " + item.Title + " > se encuentra vencida.");
                else
                    await _emailSender.SendEmailAsync(userMail, "Tarea próxima a Vencer", "Se esta por vencer la tarea < " + item.Title + " > en las próximas 24 hs");

            }
        }
    }
}