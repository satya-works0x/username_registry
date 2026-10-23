using System;
using System.Collections.Generic;
using System.Globalization;
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

        public async Task SaveItemAsync(User user)
        {
            if(string.IsNullOrWhiteSpace(user.Username))
            {
                throw new ArgumentException("username cannot be empty");
            }

            bool _newUser = _uniqueNames.Add(user.Username);
            if(!_newUser)
            {
                throw new InvalidOperationException($"the username {user.Username} already exist");
            }

            _userLookup[user.Username] = user;

            string rolesText = string.Join(",", user.Roles);
            string lineToSave = $"{user.Username}|{rolesText}" + Environment.NewLine;
            await File.AppendAllTextAsync(_filePath, lineToSave);

        }

        public async Task<List<User>> LoadAllItemsAsync()
        {
            List<User> loadedUsers = new List<User>();
            if(!File.Exists(_filePath))
            {
                return loadedUsers;
            }
            string[] lines = await File.ReadAllLinesAsync(_filePath);

            foreach(string line in lines)
            {
                if(string.IsNullOrWhiteSpace(line)) continue;
                string[] parts = line.Split('|');
                string username = parts[0];
                string[] roles = parts.Length > 1 ? parts[1].Split(',') : new string[]{"member"};
                _uniqueNames.Add(username);
                User loadedUser  = new User(username, roles);

                _userLookup[username] = loadedUser;
                loadedUsers.Add(loadedUser);
            }
            return loadedUsers;
        }

        public List<string> GetUsernamesStartsWith(char letter)
        {
            var matchingNames = from name in _uniqueNames
                                where name.StartsWith(letter.ToString(), StringComparison.OrdinalIgnoreCase)
                                orderby name ascending
                                select name;
                return matchingNames.ToList();
        }    
    }

    class Program
    {
        static async Task Main(string[] takingTerminalInputThroughArray)
        {
            UserStorageService storage = new UserStorageService("Database.txt");

            List<User> existingUsers = await storage.LoadAllItemsAsync();
            Console.WriteLine($"Loaded {existingUsers.Count} from Database.txt");
            Console.Write("\nenter a new username: ");
            string inputName = Console.ReadLine();

            string[] defaultRoles = new string[] {"Developer", "Admin"};
            User newUser = new User(inputName, defaultRoles);

            try
            {
                await storage.SaveItemAsync(newUser);
                Console.WriteLine($"Success! '{newUser.Username}' saved to Database.txt");
            
            }
            catch(ArgumentException ex)
            {
                Console.WriteLine($"[Input Error] : {ex.Message}");
            }
            catch(InvalidOperationException ex)
            {
                Console.WriteLine($"[Duplicate error] : {ex.Message}");
            }
            catch(IOException ex)
            {
                Console.WriteLine($"[Disk/file error] : {ex.Message}");
            }

            List<string> sUsers = storage.GetUsernamesStartsWith('s');
            Console.WriteLine($"\nUsers starting with 'S': {string.Join(", ", sUsers)}");
            
        }
    }
}
