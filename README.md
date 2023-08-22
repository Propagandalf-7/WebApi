# WebApi

This is a simple code first implementation of a database, with a api and an web interface. The databse contains a simple structure of User, Groups and Permissions tables as well as a UserGroup and GroupPermission junction tables. Each user can be part of multiple groups and each group can have multiple permissions.

The api allows for creation, update, delete and query of the users and groups, and the creation, delete and query of permissions.


## Installed Packages

  Microsoft.EntityFrameworkCore.InMemory
  
  Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore
  
  Microsoft.EntityFrameworkCore

## References

  Creating minimal web api:
  
  https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-7.0&tabs=visual-studio

  DBContext:

  https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext?view=efcore-7.0

  Setting up relationships:

  https://learn.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api

  Language Integrated Query (LINQ)

  https://learn.microsoft.com/en-us/dotnet/csharp/linq/
  
  Blazor App:
  
  https://dotnet.microsoft.com/en-us/learn/aspnet/blazor-tutorial/next

  ### Disclaimer
  ChatGPT was used for quick debugging, asking questions that was then verified by the official docs before implementation. ChatGPT also made the dummy data to seed the database.
