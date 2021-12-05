namespace SingleSignOn.Utilites.RequestModel
{
    public class UserPasswordRequestModel
    {
        public string CurrentPassword { get; set; }

        public string NewPassword { get; set; }
        public string CheckPassword { get; set; }
    }
}
