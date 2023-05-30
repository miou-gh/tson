# <img align="left" width="48" height="48" src="https://i.imgur.com/HS4N2PR.png" />tson
[![Nuget](https://img.shields.io/nuget/v/tson.svg)](https://www.nuget.org/packages/tson/)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT)

# Description
TSON is a human-readable data interchange format with a syntax similar to JSON. TSON preserves type information for every value.

TSON supports POCO serialization/deserialization. You can configure them using the `TsonProperty` and `TsonIgnore` attributes.
A configuration object can also be passed in to the serialization and deserialization methods. Common usage example: To include private members during serialization.

# Example
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
```cs
// serialize an object into a TSON string.
var serialized = TsonConvert.Serialize(input, Formatting.Indented);

// deserialize a TSON string into a Dictionary<string, object>
var deserialized = TsonConvert.Deserialize(input);

// deserialize a TSON string into a class.
var deserialized = TsonConvert.Deserialize<ShopItems>();
```

### Supported Types
- [x] Object
- [x] Array
- [x] Literals
  - [x] string
  - [x] bool
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
  - [x] bytes
  - [x] null
  - [x] datetime
  - [x] uri
