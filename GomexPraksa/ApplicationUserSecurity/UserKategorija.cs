namespace GomexPraksa.ApplicationUserSecurity
{
    public class UserKategorija
    {
        public string UserId { get; set; } = string.Empty;

        public int KategorijaId { get; set; }

        public ApplicationUser User { get; set; } = null!;
    }
}