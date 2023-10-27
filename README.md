# Console Application Boilerplate

.NET Core has radically changed the way developers build console applications.  I was struggling to find a good "starter template" that contained all the key features I wanted in a modern .NET 6+ console application.  The features included with this boilerplate are:

1. Uses the [.NET Generic Host](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host) pattern which levearges both `HostApplicationBuilder` for initialization and `IHostedService` for the actual execution logic.  This provides much faster overall performance and makes it easier to call `async` logic.
2. Uses a "layered" configuration architecture which allows developers to define a base `appsettings.json` file and layer down environemntal changes in `appsettings.{env}.json` files.
3. Already wired-up for dependency injection.  Easy to extend with your own interfaces and services.
4. Leverages the `IOptions<MyConfig>` cofiguration architecture where you can define complex configuation objects in `appsettings.json` files.
5. Preconfigured with [Serilog](https://serilog.net/), an excellent logging framework for .NET Core.

## Example Logic In Boilerplate

This boilerplate console application contains execution logic which makes a REST API call to a free, public API called [Bored API](https://www.boredapi.com/).  This particular API provides activity suggestions for one or more people.  The sample code in this boilerplate demostrates some very clean architectural approachs, specifically:

1. Demonstrates how to differentiate between upstream data models and service models and how to extrapolate and map between the two.
2. Demonstrates how to compartmentalize the upstream client code and wrap it via a C# service.
3. Demonstrates best practices for calling upstream REST APIs using .NETs `IHttpClient` and supporting extension methods.
4. Demonstrates how to create a "mock" client for an upstream API.  This "mock" client will not actually make a call to the upstream API, but it will return data in the same structure.  This can be useful for when access to an API is intermittent and/or the upstream API is still in active development.
5. Uses a free, public API [Bored API](https://www.boredapi.com/) as a demonstration of how to wrap an upstream API.

The diagram below outlines the high level architecture of the console application boilerplate:

![Architecture Diagram](https://github.com/rstrube/dotnet-console-app-boilerplate/blob/main/doc/img/console-app-architecture.png)

## Initialization

The initialization logic leverages the new .NET `HostApplicationBuilder` (as opposed to using the older `IHostBuilder`).  You can examine the code in [Program.cs](https://github.com/rstrube/dotnet-console-app-boilerplate/blob/main/src/ConsoleAppBoilerplate/Program.cs) to better understand the initialization steps.  You'll most likely want to extend the services that are registered for dependency injection purposes, and/or register additional configuration objects. This is where the initialization and bootstrapping for the console application is contained.

## Execution Logic

The execution logic is contained within a class that implements `IHostedService`.  You can examine the code in [AppHostedService.cs](https://github.com/rstrube/dotnet-console-app-boilerplate/blob/main/src/ConsoleAppBoilerplate/AppHostedService.cs).  This pattern of separating out the execution logic for the initialization logic is defined by the [.NET Generic Host](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host).

## Configuration

The code also establishes the best practice of using strongly-typed configuration objects that can be injected using dependency injection.  In the boilerplate code base we have strongly typed configuration objects for both activity parameter configuration [ActivityParamsConfig.cs](https://github.com/rstrube/dotnet-console-app-boilerplate/blob/main/src/ConsoleAppBoilerplate/Configuration/ActivityParamsConfig.cs) and for the upstream API we are wrapping [BoreClientConfig.cs](https://github.com/rstrube/dotnet-console-app-boilerplate/blob/main/src/ConsoleAppBoilerplate/Configuration/BoredClientConfig.cs) (for [Bored API](https://www.boredapi.com/)).  These classes directly tie configuration objects that are locate in `appsettings.json` to strongly typed `IOptions<MyConfig>` objects that can be passed in via dependency injection into constructors.

### Bored API Configuration

This configuration object (in `appsettings.json`) is used to configure the upstream API ([Bored API]((https://www.boredapi.com/))).  It also serves as an excellent example for how to define configuration for upstream APIs.  This configuration is injected as `IOptions<BoredClientConfig>` into classes via dependency injection.

```
"BoredClient": {
    "PooledConnectionLifetime" : 15,
    "UseMock": false,
    "BaseAddress": "https://www.boredapi.com/",
    "ActivityPath": "api/activity"
},
```

* `PooledConnectionLifetime`: The length of time in minutes that connections will be pooled by a given `IHttpClient` instance.  Pooling connections improves overall performance, but can lead to complications if DNS entries change/go stale.  Setting this to a reasonable time (like 15 minutes) will ensure DNS changes will be propagated and taken into account within 15 minutes.  Please see [this article](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines) for more information.
* `UseMock`: If set to `true` the API application will use the `MockBoredClient` as opposed to `BoredClient`.  The "mock" client will not actually make upstream API calls, but does return data matching the same expected format.
* `BaseAddress`: The base address (protocol & hostname) for the upstream API.
* `ActivityPath` The path to the bored API endpoint for retrieving a list of activity suggestions.

### Activity Parameter Configuration

This configuration object (in `appsettings.json`) defines a minimum and maximum number of participants that the console application will leverage when requesting an activity.  It will randomly select from between this range.  This configuration is injected as `IOptions<ActivityParamsConfig>` into classes via dependency injection.

```
"ActivityParams": {
    "MinNumberOfParticipants": 1,
    "MaxNumberOfParticipants": 5
}
```

## Note on Microsoft.AspNet.WebApi.Client and Newtonsoft.Json

Microsoft provided a the `Microsoft.AspNet.WebApi.Client` library which included a variety of extension methods on the `HttpContent` class (and used `Newtonsoft.Json` behind the scenes).  `Newtonsoft.Json` was for many years the defacto and best JSON serializer/deserializer. Although it's still possible to use the same combination, it's largely been superseded by `System.Net.Http.Json` (for extension methods on `HttpContent`) and `System.Text.Json` for JSON serialization/deserialization.  This combination is now recommended for .NET 5+ applications, what the boilerplate currently uses.

## Upstream vs. Service Datamodels

This console application also demonstrates how to separate out the the upstream data models, from the data models you want to work with in your console application's business logic. It does this by introducinng a C# service [ActivityService.cs](https://github.com/rstrube/dotnet-console-app-boilerplate/blob/main/src/ConsoleAppBoilerplate/Services/ActivityService.cs) which wraps all upstream interaction (via the [IBoredClient.cs]((https://github.com/rstrube/dotnet-console-app-boilerplate/blob/main/src/ConsoleAppBoilerplate/Upstream/IBoredClient.cs))).

In the boilerplate [BoredActivity.cs](https://github.com/rstrube/dotnet-console-app-boilerplate/blob/main/src/ConsoleAppBoilerplate/Upstream/Models/BoredActivity.cs) represents the upstream model, and [Activity.cs](https://github.com/rstrube/dotnet-console-app-boilerplate/blob/main/src/ConsoleAppBoilerplate/Services/Models/Activity.cs) represents the simplified data model that's exposed to the console application via the C# service.  You'll also notice that the `Activity` class extrapolates some data (specifically the accessibility score) into a string value `Easy` `Intermediate` or `Hard`.  This demonstrates how to take an upstream data model and enhance / extrapolate the data for your own business logic.
