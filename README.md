# CustomAutoAdapterMapper
In Organizations that synchronize information from different systems supplied by specified endpoints, mapping unknown types in real-time is a pain, considering C# is "strongly typed." Creating contracts for every third-party system to be implemented is also challenging, as development work is needed each time. 

Additionally, most properties or fields supplied might not match the expected properties or fields of the known type. Hence, custom mapping needs to be established.

This library solves the problems of mapping a JSON string to a known type.

