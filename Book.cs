namespace LB7
{
  internal class Book
  {
    public string Id { get; set; }
    public string Title { get; set; }

    public Book(string id, string title)
    {
      Id = id;
      Title = title;
    }
  }
}
