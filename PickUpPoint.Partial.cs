namespace Akhmetgareev41
{
    public partial class PickUpPoint
    {
        public string FullAddress
        {
            get
            {
                return $"{IndexOfAddress}, {City}, {Street}, {House}";
            }
        }
    }
}
