// See https://aka.ms/new-console-template for more information

using TableConverter.DataGeneration;

var data = Faker.Create()
    .Add("Dish", x => x.Food.Dish())
    .Add("Description", x => x.Food.Description())
    .WithRowCount(10)
    .Build();
    
Console.WriteLine(string.Join(",", data.Headers));

foreach (var row in data.Rows)
{
    Console.WriteLine(string.Join(",", row));
}