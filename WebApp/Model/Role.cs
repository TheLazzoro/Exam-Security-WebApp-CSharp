namespace WebApp.Model
{
    public class Role
    {
        public long? Id { get; set; }
        public string? roleName { get; set; }

        /// <summary>
        /// Should only be used when constructing object from the DB.
        /// </summary>
        public Role()
        {
            
        }

        public Role(string roleName)
        {
            this.roleName = roleName;
        }
    }
}
