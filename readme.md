# Samples: Ecommerce

## Applications

### PublicApi
HTTP REST interface exposed to public applications such as single page apps and mobile apps.

### DomainApi
RPC over HTTP interface for executing commands on aggregate roots. All commands are POSTed to this HTTP. Do not call directly, but instead use Domain.Clients to execute commands from client applications.

### Cli
Console application (command line interface) for driving demos and observing system output such as business events as they occur.

## Supporting Libraries

### Domain.Events
Defines system-wide business events. No application logic here. All events are in a single context.

### Domain
Implements aggregate roots that handle commands, keep aggregate root state, and emit business events in response to commands.

### Domain.Clients
SDK for abstracting the execution of commands from client applications, which in this ecommerce sample sends commands to the appropriate HTTP endpoint. Include this assembly in applications that will execute commands, such as process managers.

### ProcessManagers
Implements process managers that coordinate short and long running business processes by executing commands in response to incoming business events.

### Integrations
Custom services that translate between the sample ecommerce system and external services. Integrations are responsible for communicating with the outside world so other internal services may remain as functional as possible without knowing the details of external implementations.