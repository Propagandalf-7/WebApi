# WebApi

This is a simple code first implementation of a database, with a api and an web interface. The databse contains a simple structure of User, Groups and Permissions tables as well as a UserGroup and GroupPermission junction tables. Each user can be part of multiple groups and each group can have multiple permissions.

The api allows for creation, update, delete and query of the users and groups, and the creation, delete and query of permissions. The postman collection in the repo allows for testing of the api endpoints.

The web app is a blazor app that interacts with the database through the api. The web app is simple only to demonstrate the usability.

Most of the website was built using documentation, and help from chatgpt, as my experience with this level of .NET is very limited.

The theme of the database is of the Pentagon and a miltitary database, just for fun.

## How to use:
- 

## Know issues:
-  Password management of the users aren't safe.
-  Lack of documentation and comments in code. I did try to make the code clear using good variable names.
-  Canceling an update using the web app mid update can cause the web page to throw an error which has not been addressed.
-  There is inconsitency on the format of the api responses for errors.
-  API errors do not propagate to the web interface.


# Pentagon API Documentation

## Collection Information
- **Name**: Pentagon
- **Postman ID**: ac855c15-2923-40a3-b3ae-fa1971d626e3
- **Schema**: [https://schema.getpostman.com/json/collection/v2.1.0/collection.json](https://schema.getpostman.com/json/collection/v2.1.0/collection.json)
- **Exporter ID**: 29282734

---

## Users API

### 1. Retrieve All Users
- **Method**: GET
- **URL**: `https://localhost:7097/pentagon/user`

### 2. Retrieve a User by ID
- **Method**: GET
- **URL**: `https://localhost:7097/pentagon/user/{id}`

### 3. Delete a User by ID
- **Method**: DELETE
- **URL**: `https://localhost:7097/pentagon/user/{id}`

### 4. Update User's Group
- **Method**: PUT
- **URL**: `https://localhost:7097/pentagon/user/{id}/groups`
- **Body**:

    ```json
    {
        "groupids": ["level_x", "level_x"]
    }
    ```

### 5. Update a User by ID
- **Method**: PUT
- **URL**: `https://localhost:7097/pentagon/user/{id}`
- **Body**:

    ```json
    {
        "name":"john",
        "surname":"do",
        "email":"john.doe@email.com",
        "newpassword":"1234",
        "oldpassword":"1234",
        "GroupIds": ["a_group"]
    }
    ```

### 6. Add a New User
- **Method**: POST
- **URL**: `https://localhost:7097/pentagon/user`
- **Body**:

    ```json
    {
        "name":"john",
        "surname":"do",
        "email":"john.doe@email.com",
        "password":"1234",
        "GroupIds": ["a_group","a_group"]
    }
    ```

---

## Groups API

### 1. Retrieve All Groups
- **Method**: GET
- **URL**: `https://localhost:7097/pentagon/group`

### 2. Retrieve a Group by ID
- **Method**: GET
- **URL**: `https://localhost:7097/pentagon/group/{id}`

### 3. Delete a Group by ID
- **Method**: DELETE
- **URL**: `https://localhost:7097/pentagon/group/{id}`

### 4. Update Group's Permissions
- **Method**: PUT
- **URL**: `https://localhost:7097/pentagon/group/{id}/permissions`
- **Body**:

    ```json
    {
        "PermissionIds": ["level_x", "level_x"]
    }
    ```

### 5. Add a New Group
- **Method**: POST
- **URL**: `https://localhost:7097/pentagon/group`
- **Body**:

    ```json
    {
        "GroupName":"a_group",
        "PermissionIds":["level_x"]
    }
    ```

---

## Permissions API

### 1. Retrieve All Permissions
- **Method**: GET
- **URL**: `https://localhost:7097/pentagon/permission`

### 2. Retrieve a Permission by ID
- **Method**: GET
- **URL**: `https://localhost:7097/pentagon/permission/{id}`

### 3. Delete a Permission by ID
- **Method**: DELETE
- **URL**: `https://localhost:7097/pentagon/permission/{id}`

### 4. Add a New Permission
- **Method**: POST
- **URL**: `https://localhost:7097/pentagon/permission`
- **Body**:

    ```json
    {
        "PermissionName":"level_x"
    }
    ```

---


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
