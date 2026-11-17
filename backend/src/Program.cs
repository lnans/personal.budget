using Personal.Budget.App.Persistence.Entities;

Console.WriteLine("Big bang");
var test = Tag.Create("test", "#123456", DateTimeOffset.Now);
Console.WriteLine(test);
