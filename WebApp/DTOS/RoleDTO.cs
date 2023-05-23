using WebApp.Model;

namespace WebApp.DTOS
{
    public class RoleDTO
    {
        public long? Id { get; set; }
        public string roleName { get; set; }

        public RoleDTO(string roleName)
        {
            this.roleName = roleName;
        }

        public RoleDTO(Role role)
        {
            this.Id = role.Id;
            this.roleName = role.roleName;
        }
    }
}
