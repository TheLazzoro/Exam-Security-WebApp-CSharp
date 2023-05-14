namespace WebApp.Model
{
    public class ForumThread
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public User Author { get; set; }

        /// <summary>
        /// Should only be used when fetching from database.
        /// </summary>
        public ForumThread() { }

        public ForumThread(string Title, string Content, User Author)
        {
            this.Title = Title;
            this.Content = Content;
            this.Author = Author;
        }
    }
}
