

namespace AnjUx.Client.Shared
{
    [AttributeUsage(AttributeTargets.Field)]
    public class IconNameAttribute(string name) : Attribute
    {
        public string Name { get; set; } = name;

        public static string GetIconName<TEnum>(TEnum? value) where TEnum : struct, Enum
        {
            return GetIconNameInternal(value) ?? "Stop";
        }

        private static string? GetIconNameInternal<TEnum>(TEnum? value) where TEnum : struct, Enum
        {
            if (value == null)
                return null;

            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name != null)
            {
                var field = type.GetField(name);
                if (field != null)
                {
                    if (GetCustomAttribute(field, typeof(IconNameAttribute)) is not IconNameAttribute attr) return null;
                    
                    return attr.Name.ToLower().Replace(" ", "_");
                }
            }
            return null;
        }
    }

    public enum Icon : int
    {
        [IconName("Edit")]
        Edit = 0,

        [IconName("Delete")]
        Delete = 1,

        [IconName("Add Circle Outline")]
        Create = 2,

        [IconName("Done")]
        Confirm = 3,

        [IconName("Close")]
        Cancel = 4,

        [IconName("Add")]
        Add = 5,

        [IconName("Remove")]
        Subtract = 6,

        [IconName("Search")]
        Search = 7,

        [IconName("Person")]
        User = 8,

        [IconName("Group")]
        Users = 9,

        [IconName("Settings")]
        Settings = 10,

        [IconName("Stop")]
        Stop = 11,

        [IconName("Play Arrow")]
        Play = 12,

        [IconName("Pause")]
        Pause = 13,

        [IconName("Arrow Back")]
        Back = 14,

        [IconName("Home")]
        Home = 15,

        [IconName("Arrow Forward")]
        Forward = 16,

        [IconName("Arrow Upward")]
        Up = 17,

        [IconName("Arrow Downward")]
        Down = 18,

        [IconName("Login")]
        Login = 19,

        [IconName("Logout")]
        Logout = 20,

        [IconName("Menu")]
        Menu = 21,

        [IconName("Menu Open")]
        MenuOpen = 22,

        [IconName("More Vert")]
        MoreVert = 23,

        [IconName("More Horiz")]
        MoreHoriz = 24,

        [IconName("Expand More")]
        ExpandMore = 25,

        [IconName("Expand Less")]
        ExpandLess = 26,

        [IconName("Chevron Left")]
        ChevronLeft = 27,

        [IconName("Chevron Right")]
        ChevronRight = 28,

        [IconName("List")]
        List = 29,

        [IconName("View List")]
        ViewList = 30,

        [IconName("Filter List")]
        FilterList = 31,

        [IconName("Sort")]
        Sort = 32,

        [IconName("Sort By Alpha")]
        SortByAlpha = 33,

        [IconName("Favorite")]
        Favorite = 34,

        [IconName("Favorite Border")]
        FavoriteBorder = 35,

        [IconName("Star")]
        Star = 36,

        [IconName("Star Border")]
        StarBorder = 37,

        [IconName("Thumb Up")]
        ThumbUp = 38,

        [IconName("Thumb Down")]
        ThumbDown = 39,

        [IconName("Thumbs Up Down")]
        ThumbsUpDown = 40,

        [IconName("Visibility")]
        Visibility = 41,

        [IconName("Visibility Off")]
        VisibilityOff = 42,

        [IconName("Lock")]
        Lock = 43,

        [IconName("Lock Open")]
        LockOpen = 44,

        [IconName("Lock Clock")]
        LockClock = 45,

        [IconName("Block")]
        Block = 46,

        [IconName("Check Circle")]
        CheckCircle = 47,

        [IconName("Error")]
        Error = 48,

        [IconName("Error Outline")]
        ErrorOutline = 49,

        [IconName("Warning")]
        Warning = 50,

        [IconName("Warning Amber")]
        WarningOutline = 51,

        [IconName("Info")]
        Info = 52,

        [IconName("Help")]
        Help = 53,

        [IconName("Help Outline")]
        HelpOutline = 54,

        [IconName("Priority High")]
        ExclamationMark = 55,

        [IconName("Report")]
        Report = 56,

        [IconName("Sync Problem")]
        SyncProblem = 57,

        [IconName("Notifications")]
        Notifications = 58,

        [IconName("Notifications Active")]
        NotificationsActive = 59,

        [IconName("Notifications Off")]
        NotificationsOff = 60,

        [IconName("Notifications None")]
        NotificationsNone = 61,

        [IconName("Notifications Paused")]
        NotificationsPaused = 62,

        [IconName("Autorenew")]
        Autorenew = 63,

        [IconName("Cached")]
        Cached = 64,

        [IconName("Refresh")]
        Refresh = 65,

        [IconName("Sync")]
        Sync = 66,

        [IconName("Replay")]
        Replay = 67,

        [IconName("Sync Disabled")]
        SyncDisabled = 68,

        [IconName("Paid")]
        Paid = 69,

        [IconName("Monetization On")]
        MonetizationOn = 70,

        [IconName("File Download")]
        FileDownload = 71,

        [IconName("File Upload")]
        FileUpload = 72,

        [IconName("Cloud Download")]
        CloudDownload = 73,

        [IconName("Cloud Upload")]
        CloudUpload = 74,

        [IconName("Cloud")]
        Cloud = 75,

        [IconName("Cloud Off")]
        CloudOff = 76,

        [IconName("Publish")]
        Publish = 77,

        [IconName("Description")]
        File = 78,

        [IconName("Download")]
        Download = 79,

        [IconName("Upload")]
        Upload = 80,

        [IconName("File Copy")]
        FileCopy = 81,

        [IconName("Shopping Cart")]
        ShoppingCart = 82,

        [IconName("Add Shopping Cart")]
        AddShoppingCart = 83,

        [IconName("Remove Shopping Cart")]
        RemoveShoppingCart = 84,

        [IconName("Shopping Cart Checkout")]
        ShoppingCartCheckout = 85,

        [IconName("Credit Card")]
        CreditCard = 86,

        [IconName("Payment")]
        Payment = 87,

        [IconName("Attach Money")]
        AttachMoney = 88,

        [IconName("Receipt")]
        Receipt = 89,

        [IconName("Receipt Long")]
        ReceiptLong = 90,

        [IconName("Store")]
        Store = 91,

        [IconName("Sell")]
        Sell = 92,

        [IconName("Pix")]
        Pix = 93,

        [IconName("Qr Code")]
        QrCode = 94,

        [IconName("Qr Code Scanner")]
        QrCodeScanner = 95,

        [IconName("Local Atm")]
        LocalAtm = 96,

        [IconName("Local Offer")]
        LocalOffer = 97,

        [IconName("Local Shipping")]
        LocalShipping = 98,

        [IconName("Point Of Sale")]
        PointOfSale = 99,

        [IconName("Redeem")]
        Redeem = 100,

        [IconName("Shopping Bag")]
        ShoppingBag = 101,

        [IconName("Shopping Basket")]
        ShoppingBasket = 102,

        [IconName("Price Check")]
        PriceCheck = 103,

        [IconName("Payments")]
        Payments = 104,

        [IconName("Currency Exchange")]
        CurrencyExchange = 105,

        [IconName("Card Membership")]
        CardMembership = 106,

        [IconName("Price Change")]
        PriceChange = 107,

        [IconName("Credit Score")]
        CreditScore = 108,

        [IconName("Request Quote")]
        RequestQuote = 109,

        [IconName("Account Balance")]
        Bank = 110,

        [IconName("Question Mark")]
        QuestionMark = 111,

        [IconName("Hub")]
        Hub = 112,

        [IconName("Apps")]
        Apps = 113,

        [IconName("Undo")]
        Undo = 114,

        [IconName("Flag")]
        Flag = 115,

        [IconName("Save")]
        Save = 116,

        [IconName("Link")]
        Link = 117,

        [IconName("Print")]
        Print = 118,

        [IconName("Email")]
        Email = 119,

        [IconName("Send")]
        Send = 120,

        [IconName("Alternate Email")]
        AlternateEmail = 121,

        [IconName("Share")]
        Share = 122,

        [IconName("Redo")]
        Redo = 123,

        [IconName("Attach File")]
        AttachFile = 124,

        [IconName("Checklist")]
        Checklist = 125,

        [IconName("Desktop Windows")]
        DesktopWindows = 126,

        [IconName("Desktop Mac")]
        DesktopMac = 127,

        [IconName("Laptop")]
        Laptop = 128,

        [IconName("Laptop Mac")]
        LaptopMac = 129,

        [IconName("Tablet Mac")]
        Tablet = 130,

        [IconName("Phone Android")]
        PhoneAndroid = 131,

        [IconName("Phone Iphone")]
        PhoneIphone = 132,

        [IconName("Smartphone")]
        Smartphone = 133,

        [IconName("Computer")]
        Computer = 134,

        [IconName("Security")]
        Security = 135,

        [IconName("Storage")]
        Storage = 136,

        [IconName("Manage Search")]
        ManageSearch = 137,
    }
}
