namespace SingleSignOn.Utilites.RequestModel
{
    public class ApiResourceSecretRequestModel
    {
        public string Value { get; set; }
        public string Type { get; set; }
        public string HashType { get; set; }
        public string Expiration { get; set; }
        public string Description { get; set; }
    }
}
