# EventCore

EventCore is framework for building microservices with .NET Core and event sourcing / CQRS for intraservice communication.

It's assumed that you're familiar with the basic concepts behind domain driven design, event sourcing, and command-query-responsibility-segregation.

EventCore was created as a code demonstration and training tool to be used in a guided setting. It is not production ready, although core classes have reasonable test coverage and are suitable for prototyping. 

Basic functionality exists for building aggregate roots and projections, but not advanced functionality such as process management and external service integration.


## Getting Started

This code repository is structured as a single .NET Core/Standard solution with three main folders:

- **/samples** - Contains application samples that consume and extend core EventCore classes.
- **/src** - Contains core EventCore classes.
- **/tests** - Contains test projects that cover core EventCore classes only. No tests are provided for sample code.

The fastest way to get up and running is to use the provided bash scripts to run the pre-built Ecommerce sample. No scripts are provided for Windows.

**Requirements:**
- Bash / Linux / Mac
- .NET Core 2.2
- Docker

There is one main sample application and a secondary/independent client library.

### EventCore.Samples.EventStore

A client libary that implements `EventCore.EventSourcing.IStreamClient` with Greg Young's Event Store as an event sourcing database. This client library can be used independently of other samples.

### EventCore.Samples.Ecommerce

A very simple set of microservices containing:
 - A single Sales Order aggregate root.
 - A single Sales Report projection.
 - A service host (ASP.NET Core app) that provides access to aggregate roots via HTTP API (RPC) and runs projectors as background services.
 - A barebones ASP.NET UI app that creates and lists sales orders.
 - A CLI (command line interface) console app that provides various options, including initialization of supporting infrastructure and a listening mode to watch events as they arrive in real time.

(Uses the EventStore sample client library.)

### Running the Ecommerce Sample

Set up supporting infrastucture, including Docker containers for Greg Young's Event Store and SQL Server:

```
cd samples/Ecommerce/_scripts

# Stops and removes existing containers, for starting with a clean slate.
sh reset_infrastructure.sh

# Run Docker containers.
sh run_infrastructure.sh

# Wait 30 seconds to make sure containers have started,
# then initialize SQL Server databases and Event Store.
sh initialize_infrastructure.sh
```

In a separate tab/terminal window, run the service host:
```
sh dotnet_run_serviceapi.sh
```

In a separate tab/terminal window, run the UI app:
```
sh dotnet_run_publicui.sh
```

In a separate tab/terminal window, run the CLI to watch a real-time event feed:
```
sh listen_all_events.sh
```

Interact with the application:
- Open the web UI: https://localhost:5512
- Click on "New Sales Order" in the nav bar.
- Enter data and submit.
- The new sales order will show up in the list. If not, check the terminal for logged errors, or try refreshing.

As in any eventually consistent system, projections experience latency between the time events are committed and the time the projection is updated. Latency should be on the order of 100ms or less when developing locally.

Useful URLs:
 - http://localhost:2113 - Greg Young's Event Store Web Admin (u/p : admin/changeit)
 - http://localhost:1113 - Greg Young's Event Store TCP
 - http://localhost:5633 - SQL Server DB - Aggregate Root States
 - http://localhost:5733 - SQL Server DB - Projections
 - https://localhost:5502 - Service API (5501 for non-https)
 - https://localhost:5512 - Public UI (5511 for non-https)


## EventCore Core Classes

### EventCore.EventSourcing
Provides interfaces and implementations for event-based communication and stream concepts.

### EventCore.StatefulSubscriber
Provides interfaces and implementations for a listener that subscribes to a given event subscription, deserializes and handles incoming events in parallel, and stores last known event positions for every stream in the subscription so subscribing services can be restarted/interrupted without losing their processing state.

### EventCore.AggregateRoots
Provides interfaces and implementations for building aggregate root services that are agnostic to where they are hosted. They can be hosted in ASP.NET Core apps, Azure Functions, etc.

### EventCore.Projectors
Provides interfaces and implementations for building projector services that take incoming events and create durable state projections that can be queried more efficiently than event streams.

### EventCore.Utilities
A few supporting classes shared by all EventCore components.


## What's Not Provided

Event sourcing / CQRS systems are complicated. They take otherwise trivial use cases and add multiple moving parts, presumably in exchange for better scalability and cleaner separation of code responsibilities with long term benefits.

In a traditional n-tier application it's common to build a single codebase on top of a single read/write database and be done with it. With ES / CQRS (and in general any distributed system), one part becomes many - a data store for events, a data store for projections (potentially for each one), and various caching databases to deal with event sourcing's elephant in the room - latency. Plus all the various services that make up "microservices" - aggregate roots, projectors, projections, process managers, integration managers.

Because of this, there are practically unlimited ways to build an ES/CQRS system. Process managers and integration managers are good examples of services that can be (should be) simple to build, but vary in their requirements - specifically in how they hydrate/replay events after a service disruption and at what point they start acting on incoming events again, which makes them impractical to build in a catch-all demonstration.

EventCore is meant to be a basic demonstration of concepts - therefore, process managers and integration managers have not been included.

