using Newtonsoft.Json;
using WebApp.Model;

namespace WebApp.DTOS
{
    public class ForumThreadPostDTO
    {
        public long? Id { get; set; }
        public string Content { get; set; }
        public UserSafeDTO? Author { get; set; }
        public long ThreadId { get; set; }

        public ForumThreadPostDTO()
        {
        }

        public ForumThreadPostDTO(string Content, UserSafeDTO Author, ForumThreadDTO Thread)
        {
            this.Content = Content;
            this.Author = Author;
            this.ThreadId = ThreadId;
        }

        public ForumThreadPostDTO(ForumThreadPost post)
        {
            this.Id = post.Id;
            this.Content = post.Content;
            this.Author = new UserSafeDTO(post.Author);
            this.ThreadId = post.ThreadId;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
