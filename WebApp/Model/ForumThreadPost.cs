namespace WebApp.Model
{
    public class ForumThreadPost
    {
        public long? Id { get; set; }
        public string Content { get; set; }
        public User Author { get; set; }
        public long ThreadId { get; set; }

        /// <summary>
        /// Should only be used when fetching from database.
        /// </summary>
        public ForumThreadPost() { }

        public ForumThreadPost(string Content, User Author, long ThreadId)
        {
            this.Content = Content;
            this.Author = Author;
            this.ThreadId = ThreadId;
        }
    }
}
