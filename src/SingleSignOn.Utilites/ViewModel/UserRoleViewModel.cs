using System.Collections.Generic;

namespace SingleSignOn.Utilites.ViewModel
{
    public class UserRoleViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Dob { get; set; }
        public string CreateDate { get; set; }
        public string LastModifiedDate { get; set; }
        public List<string> UserRoles { get; set; }
        public List<UserRoleTempViewModels> Roles { get; set; }
        public string AvatarUri { get; set; }
    }

}
