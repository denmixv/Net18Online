using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebPortalEverthing.Models.Surveys.Profile
{
    public class ProfileViewModel
    {
        [DisplayName("Логин")]
        public string UserName { get; set; }
        [DisplayName("Аватар")]
        public string AvatarUrl { get; set; }
        [DisplayName("Возраст")]
        public int Age { get; set; }
        [DisplayName("Количество монет")]
        public decimal Coins { get; set; }
        [DisplayName("Загрузить новый аватар")]
        public IFormFile? NewAvatarFile { get; set; }
    }
}
