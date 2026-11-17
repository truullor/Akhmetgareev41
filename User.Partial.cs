namespace Akhmetgareev41
{
    public partial class User
    {
        public string FullName
        {
            get
            {
                return ($"{UserSurname} {UserName} {UserPatronymic}").Trim();
            }
        }
    }
}
