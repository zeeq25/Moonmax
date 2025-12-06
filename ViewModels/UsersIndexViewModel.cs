using Moonmax.Models;
using System.Collections.Generic;

namespace Moonmax.ViewModels
{
    public class UsersIndexViewModel
    {
        public List<User> Users { get; set; } = new List<User>();
        public CreateUserViewModel CreateModel { get; set; } = new CreateUserViewModel();
    }
}
