namespace Yammerly.Service.DataObjects
{
    public class StorageToken
    {
        public string Name { get; set; }
        public System.Uri Uri { get; set; }
        public string SasToken { get; set; }
    }
}