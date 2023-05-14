namespace WebApp.Model
{
    public class ForumThreadPost
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public User Author { get; set; }
        public ForumThread Thread { get; set; }

        /// <summary>
        /// Should only be used when fetching from database.
        /// </summary>
        public ForumThreadPost() { }

        public ForumThreadPost(string Content, User Author, ForumThread Thread)
        {
            this.Content = Content;
            this.Author = Author;
            this.Thread = Thread;
        }
    }
}
