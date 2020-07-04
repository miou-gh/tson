# <img align="left" width="48" height="48" src="https://i.imgur.com/HS4N2PR.png" />tson
[<img align="right" src="https://img.shields.io/nuget/v/tson.svg" />]("https://www.nuget.org/packages/tson/") <img align="right" src="https://img.shields.io/badge/license-MIT-blue.svg" />

A format like JSON, except now with embedded types!


# tson
A format like JSON, except now with embedded types!

### An example of serialized TSON.
```js
{
    "name": string("My Cool Adventure"),
    "plays": uint(150),
    "reputation": int(-2),
    "visible": bool(false),
    "data": bytes("UTIHCQsOEBIUFxkbHSAiJCYHCQsOEBIUFxkbHSAiJCYICAg="),
    "created": datetime("2020-05-13T10:06:09.5137659-04:00")
}
``` 

### Usage
```cs
// serialize an object into TSON.
var serialized = TsonConvert.Serialize(input, Formatting.Indented);

// deserialize a TSON string into a Dictionary<string, object>
var deserialized = TsonConvert.Deserialize(input);
```

### Supported Types
- [x] Object
- [x] Array
- [ ] Literals
  - [x] string
  - [x] boolean
  - [x] int
  - [x] uint
  - [x] long
  - [x] ulong
  - [x] char
  - [x] short
  - [x] ushort
  - [x] float
  - [x] double
  - [x] sbyte
  - [x] byte
  - [x] byte[]
  - [x] DateTime
  - [x] null
  
### TODO:
- [ ] Mapper (TSON -> Class)
