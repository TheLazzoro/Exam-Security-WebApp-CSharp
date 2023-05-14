using Newtonsoft.Json;
using WebApp.Model;

namespace WebApp.DTOS
{
    public class ForumThreadDTO
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public UserSafeDTO? Author { get; set; }

        public ForumThreadDTO()
        {
        }

        public ForumThreadDTO(string Title, string Content, UserSafeDTO Author)
        {
            this.Title = Title;
            this.Content = Content;
            this.Author = Author;
        }

        public ForumThreadDTO(ForumThread forumThread)
        {
            this.Title = forumThread.Title;
            this.Content = forumThread.Content;
            this.Author = new UserSafeDTO(forumThread.Author);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
