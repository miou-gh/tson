# TSON

A relatively light (<1000 LOC) format similar to JSON, except with the addition of embedded property types in values.

# Examples

## To serialize an object to a TSON formatted string.

```csharp
public class Book
{
	public string Title { get; set; }
	public string Author { get; set; }
}

var book = new Book()
{
	Title = "Paths of Glory",
	Author = "Humphrey Cobb"
};

TsonConvert.SerializeObject(book, Formatting.Indented);

// becomes:

{
    "Author": string("Humphrey Cobb"),
    "Title": string("Paths of Glory")
}
```

## To deserialize a TSON string into an object of a matching type
```csharp
var encoded = "{"Author":string("Humphrey Cobb"),"Title":string("Paths of Glory")}";
var decoded = TsonConvert.DeserializeObject<Book>(encode);
```

## To deserialize a TSON string into an object of a dynamic type
```csharp
dynamic book = TsonConvert.DeserializeObject("{\"Author\":string(\"Humphrey Cobb\"),\"Title\":string(\"Paths of Glory\")}");

var title = book.Title; // "Paths of Glory"
var atuhor = book.Author; // "Humphrey Cobb"
```

## To serialize a list of objects to a TSON formatted string.
```csharp
public class Person
{
	public string Name { get; set; }
	public uint Age { get; set; }
	public bool Alive { get; set; }

	public DateTime Birthday { get; set; }
	public List<string> Pets { get; set; }
}

var people = new List<Person>
{
	new Person()
	{
		Name = "Alice Smith",
		Age = 21,
		Alive = true,
		Birthday = new DateTime(1998, 01, 01),
		Pets = new List<string>() { "Spot", "Tazz" }
	},

	new Person()
	{
		Name = "Mike Tyson",
		Age = 47,
		Alive = true,
		Birthday = new DateTime(1972, 01, 01),
		Pets = new List<string>() { "Fuzzy", "Bear" }
	}
};

TsonConvert.SerializeObject(people, Formatting.Indented);

// becomes:

[
    {
        "Age": uint(21),
        "Alive": bool(True),
        "Birthday": datetime("1998-01-01T00:00:00.0000000"),
        "Name": string("Alice Smith"),
        "Pets": [
            string("Spot"),
            string("Tazz")
        ]
    },
    {
        "Age": uint(47),
        "Alive": bool(True),
        "Birthday": datetime("1972-01-01T00:00:00.0000000"),
        "Name": string("Mike Tyson"),
        "Pets": [
            string("Fuzzy"),
            string("Bear")
        ]
    }
]
```
