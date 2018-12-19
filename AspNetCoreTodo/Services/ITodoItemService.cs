using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AspNetCoreTodo.Models;

namespace AspNetCoreTodo.Services {
    public interface ITodoItemService {

        Task<TodoItem[]> GetIncompleteItemsAsync (
            ApplicationUser user);

        bool AddItems (IEnumerable<TodoItem> listItems, string creator);

        Task<bool> AddItemAsync (TodoItem newItem, ApplicationUser user, string creator);

        Task<bool> MarkDoneAsync (Guid id, ApplicationUser user);
        Task<TodoItem[]> GetItemsToSendMailAsync ();
    }
}