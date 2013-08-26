```
                                                   
  ____ ________  ______ ___  ____ _____  __________
 / __ `/ ___/ / / / __ `__ \/ __ `/ __ \/ ___/ ___/
/ /_/ / /  / /_/ / / / / / / /_/ / / / / /__/ /    
\__, /_/   \__, /_/ /_/ /_/\__,_/_/ /_/\___/_/     v0.1.0
  /_/     /____/                                   
```

A .NET library for conjuring LINQ expressions from URL querystrings.

## Background

The problem I was trying to address with this library is that I wanted the consumer of a REST API I was building to be able to make _slightly_ more complex ad-hoc queries on a resource. A common way to accomplish this is to define a "filter" query parameter that has it's own special query syntax. This is what the [OData](http://www.odata.org/documentation/odata-v3-documentation/url-conventions/) protocol does:

    // requests all products with the name 'milk' that also have a price less than 2.55
    http://api.myservice.com/products?$filter=Name eq 'milk' and Price lt '2.55M'

In most RESTful API's, _simple_ filtering of a resource is typically handled directly with a query parameter for the property you're trying to filter on:

    // requests all products with the name 'milk'
    http://api.myservice.com/products?name=milk

It seemed a bit ugly to me to break away from this convention for _slightly_ more complex queries and to have the consumer learn _yet another query syntax_. So why not just augment the simple querying convention shown above by adding support for common comparison operators and [CSS attribute selectors](https://developer.mozilla.org/en/CSS/Attribute_selectors)? Like so:

    // requests all products with the Name 'milk' that also have a Price less than 2.55
    http://api.myservice.com/products?name=milk&price<=2.55

    // requests all products whose name starts with 'wheat' (e.g. Wheaties, Wheat-Thins)
    http://api.myservice.com/products?name^=wheat

## Summary

Qrymancr is a .NET library that builds LINQ predicate expressions from URL querystrings. These querystrings can be simple or _slightly_ more complex as stated above. You can then use this predicate expression in a LINQ `where` clause to filter on a collection of items (or pass it to your LINQ-compatible ORM).

## Requirements

### Usage requirements

To use the Qrymancr library, you'll need the following:

 * Microsoft .NET Framework `4.0+`
 
### Development requirements

To start developing for the Qrymancr library, you'll need the following:

 * Microsoft .NET Framework `4.0+`
 * Microsoft Visual Studio `2010+`
 * Microsoft Powershell `2.0+`
 
## Supported comparison operators

Qrymancr supports the following comparison operators:

Operator | Description              | Example
---------|--------------------------|---------
=        | equals                   | ?prop=5
!=       | not equals               | ?prop!=0
>=       | greater than or equal to | ?prop>=7
<=       | less than or equal to    | ?prop<=10
^=       | starts with              | ?prop^=bo
$=       | ends with                | ?prop$=ed
*=       | contains                 | ?prop*=and

## Supported logical operators

Qrymancr has limited support for logical operators (for now). It currently supports:

Operator | Description              | Example
---------|--------------------------|---------
&        | logical AND              | ?prop1=5&prop2>=3&prop3!=bob
,        | logical OR*              | ?prop=michaelangelo,leornado,donatello,raphael

> NOTE: logical OR is only supported for checking a *single* property against multiple values
