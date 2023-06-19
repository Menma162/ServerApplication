namespace HouseManagement.OtherClasses
{
    public class Urles
    {
        private const string BaseUrl = "https://localhost:7222/api/";

        public const string AuthenticateUrl = BaseUrl + "authenticate/login";

        public const string UserUrl = BaseUrl + "User";
        public const string FlatOwnerUrl = BaseUrl + "FlatOwner";
        public const string FlatUrl = BaseUrl + "Flat";
        public const string CounterUrl = BaseUrl + "Counter";
        public const string IndicationUrl = BaseUrl + "Indication";
        public const string ServicetUrl = BaseUrl + "Service";
        public const string SettingsServiceUrl = BaseUrl + "SettingsService";
        public const string PaymentUrl = BaseUrl + "Payment";
        public const string HouseUrl = BaseUrl + "House";
        public const string AdvertisementUrl = BaseUrl + "Advertisement";
        public const string ComplaintUrl = BaseUrl + "Complaint";
        public const string ResidenceUrl = BaseUrl + "Residence";
    }
}
