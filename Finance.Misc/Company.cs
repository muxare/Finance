// See https://aka.ms/new-console-template for more information
class Company
{
	public Guid Id { get; set; }
	public string Name { get; set; }

	public virtual ICollection<Quote> Quotes { get; set; }
}