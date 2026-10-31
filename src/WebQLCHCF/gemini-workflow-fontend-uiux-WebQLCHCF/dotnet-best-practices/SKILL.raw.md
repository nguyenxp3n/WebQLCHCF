---
name: dotnet-best-practices
description: 'Ensure .NET/C# code meets best practices for the solution/project.'
---

# .NET/C# Best Practices

Your task is to ensure .NET/C# code in ${selection} meets the best practices specific to this solution/project. This includes:

## Documentation & Structure
- Create comprehensive XML documentation comments for public classes, interfaces, methods, and properties when working on library/API code.
- Follow established namespace and folder structure.

## Design Patterns & Architecture
- Use constructor dependency injection.
- Use interface segregation when the project already follows it.
- Keep methods focused and cohesive.
- Do not introduce large architecture changes unless requested.

## Dependency Injection & Services
- Register services with appropriate lifetimes.
- Implement service interfaces for testability when appropriate.

## Async/Await Patterns
- Use async/await for I/O operations.
- Return Task or Task<T> from async methods.
- Handle async exceptions properly.

## Testing Standards
- Follow Arrange, Act, Assert.
- Test success and failure scenarios.

## Configuration & Settings
- Use strongly typed configuration where appropriate.
- Use appsettings.json configuration.

## Error Handling & Logging
- Use structured logging.
- Throw specific exceptions with descriptive messages.
- Show user-friendly error messages in UI.

## Performance & Security
- Validate and sanitize input.
- Use parameterized queries/stored procedures.
- Avoid exposing sensitive data.

## Code Quality
- Follow SOLID principles.
- Avoid duplication.
- Use meaningful names.
- Implement disposal patterns when needed.
