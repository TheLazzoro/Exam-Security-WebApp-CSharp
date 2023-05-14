using Newtonsoft.Json;
using WebApp.Model;

namespace WebApp.DTOS
{
    public class ForumThreadPostDTO
    {
        public string Content { get; set; }
        public UserSafeDTO? Author { get; set; }
        public ForumThreadDTO? Thread { get; set; }

        public ForumThreadPostDTO()
        {
        }

        public ForumThreadPostDTO(string Content, UserSafeDTO Author, ForumThreadDTO Thread)
        {
            this.Content = Content;
            this.Author = Author;
            this.Thread = Thread;
        }

        public ForumThreadPostDTO(ForumThreadPost post)
        {
            this.Content = post.Content;
            this.Author = new UserSafeDTO(post.Author);
            this.Thread = new ForumThreadDTO(post.Thread);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
