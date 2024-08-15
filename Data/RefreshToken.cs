using System.ComponentModel.DataAnnotations;

namespace JwtAuth.Data
{
    public class RefreshToken
    {
        [Key]
        public string Token { get; set; }
        public int UserId { get; set; }
        public DateTime issuedAt { get; set; }
        public DateTime expiresAt { get; set; }

    }
}
