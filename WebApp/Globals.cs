namespace WebApp
{
    public static class Globals
    {
        public static bool IsDevelopment { get; set; }
        public static readonly string LocalImageDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Images");
    }
}
