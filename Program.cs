using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UsernameApp
{
    public interface IStorageService<T>
    {
        Task SaveItemAsync(T item);
        Task<List<T>> LoadAllItemsAsync();
    }

    public class User
    {
        public string Username{get; set;}
        public string[] Roles {get; set;}

        public User(string username, string[] roles)
        {
            Username = username;
            Roles = roles;
        }
    }

    public class UserStorageService : IStorageService<User>
    {
        private readonly string _filePath;
        private readonly HashSet<string> _uniqueNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, User> _userLookup = new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase);

        public UserStorageService(string filePath)
        {
            _filePath = filePath;
        }

        
    }
}
