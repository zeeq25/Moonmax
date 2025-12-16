using Moonmax.Models;
using System.Collections.Generic;

namespace Moonmax.ViewModels
{
    public class UsersIndexViewModel
    {
        public List<User> Users { get; set; } = new List<User>();
        public List<Technicians>Technicians { get; set; }  = new List<Technicians> ();



        public CreateUserViewModel CreateModel { get; set; } = new CreateUserViewModel();
        public CreateTechnicianViewModel CreateTechnicianModel { get; set; } = new CreateTechnicianViewModel();

        public TechnicianEditViewModel EditTechnicianModel { get; set; } = new TechnicianEditViewModel();



    }

    



}
