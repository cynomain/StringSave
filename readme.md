# StringSave

A Dictionary<string,string> based format system. (for Unity)
It can save different values in a single class. (such as string with int and long and so on)

## Officially Supported types 
- String
- Int
- Float
- Short
- Double
- Long
- Bool
- Vector2
- Vector3

Other types will be formatted as JSON.

## Examples
```C#
//Set Methods
SaveStorage.Set<T>("key", [value:T]); //To set values.

//Get Methods (returns StringStorageObject)
SaveStorage["key"]; 
SaveStorage.Get("key"); 
SaveStorage.TryGet("key", out [value]) 
```
## StringStorageObject
```C#
//StringStorageObjects can be used as a container for the values.
//Types officially supported can be parsed with methods as such : 

SaveStorage["key"].AsVector2(); 
SaveStorage["key"].AsInt(); //etc. 
//Types NOT officially supported can be parsed with 
SaveStorage["key"].AsTypeJson<T>();
```
