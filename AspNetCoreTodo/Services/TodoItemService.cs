using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreTodo.Data;
using AspNetCoreTodo.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreTodo.Services
{
    public class TodoItemService : ITodoItemService
    {
        private readonly ApplicationDbContext _context;

        public TodoItemService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TodoItem[]> GetIncompleteItemsAsync(
            ApplicationUser user)
        {
            return await _context.Items
                .Where(x => x.IsDone == false && x.UserId == user.Id)
                .ToArrayAsync();
        }

        public bool AddItems(IEnumerable<TodoItem> listItems, string creator)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    foreach (var item in listItems)
                    {

                        item.Id = Guid.NewGuid();
                        item.IsDone = false;
                        item.CreationTaskDate = DateTime.Now;
                        if (item.DueAt == null)
                        {
                            item.DueAt = DateTimeOffset.Now.AddDays(3);
                        }
                        item.UserCreateTask = creator;
                        _context.Add(item);
                        _context.SaveChanges();

                    }
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public async Task<bool> AddItemAsync(TodoItem newItem, ApplicationUser user, string creator)
        {
            newItem.Id = Guid.NewGuid();
            newItem.IsDone = false;
            newItem.DueAt = DateTimeOffset.Now.AddDays(3);
            newItem.UserId = user.Id;
            newItem.CreationTaskDate = DateTime.Now;

            if (string.IsNullOrWhiteSpace(creator))
            {
                newItem.UserCreateTask = user.UserName;
            }
            else
            {
                newItem.UserCreateTask = creator;
            }
            _context.Items.Add(newItem);

            var saveResult = await _context.SaveChangesAsync();
            return saveResult == 1;
        }
        public async Task<bool> MarkDoneAsync(Guid id, ApplicationUser user)
        {
            var item = await _context.Items
                .Where(x => x.Id == id && x.UserId == user.Id)
                .SingleOrDefaultAsync();

            if (item == null) return false;

            item.IsDone = true;
            item.ClosedTaskDate = DateTime.Now;

            var saveResult = await _context.SaveChangesAsync();
            return saveResult == 1; // One entity should have been updated
        }
        public async Task<TodoItem[]> GetItemsToSendMailAsync()
        {
            var limitDate = DateTime.Today.AddDays(2);
            var items = await _context.Items
                .Where(x => !x.IsDone && x.DueAt < limitDate)
                .ToArrayAsync();

            return items; 
        }


    }
}