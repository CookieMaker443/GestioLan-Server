## Items endpoints:

- *GET* */api/Items/GetItems* [Authorize]
  - [FromQuery] bool? has_category
  - [FromQuery] int? id_category
  - [FromQuery] string? name
  - [FromQuery] bool? has_image
  - [FromQuery] int? quantity
  - [FromQuery] int? id_quantity_unit
- *GET* */api/Items/GetItems/{id}* [Authorize]
  - [FromURL] int id
- *POST* */api/Items/CreateItem* [Authorize]
  - [FromBody] Item item
- *DELETE* */api/Items/DeleteItem/{id}* [Authorize]
  - [FromURL] int id
- *PUT* */api/Items/ModifyItem/{id}* [Authorize]
  - [FromURL] int id
  - [FromBody] Item updatedItem


## User endpoints:

*eliminato*
- *GET* */api/Users/AllUsers* [Authorize]

- *POST* */api/Users/Login*
  - [FromBody] User loginUserData
  - Risposta: `{ User, Token }`
- *POST* */api/Users/Register*
  - [FromBody] User user
- *DELETE* */api/Users/DeleteUser* [Authorize(Policy = "AdminOnly")]
  - [FromQuery] string username
- *PUT* */api/Users/{targetUsername}* [Authorize]
  - [FromURL] string targetUsername
  - [FromBody] User newUser
- *GET* */api/Users/image/{username}* [Authorize]
  - [FromURL] string username
- *POST* */api/Users/image/{username}* [Authorize]
  - [FromURL] string username
  - [FromForm] IFormFile file (multipart/form-data)


## Categories endpoints:

- *GET* */api/Categories/AllCategories* [Authorize]
- *POST* */api/Categories/AddCategory* [Authorize(Policy = "AdminOnly")]
  - [FromBody] Category category (obbligatorio `NameCategory`, opzionale `CategoryColor`)
- *PUT* */api/Categories/UpdateCategory/{id}* [Authorize(Policy = "AdminOnly")]
  - [FromURL] int id
  - [FromBody] Category category
- *DELETE* */api/Categories/DeleteCategory/{id}* [Authorize(Policy = "AdminOnly")]
  - [FromURL] int id


## QuantityUnit endpoints:

- *GET* */api/QuantityUnit/GetQtyUnits* [Authorize]
- *GET* */api/QuantityUnit/GetQtyUnits/{id}* [Authorize]
  - [FromURL] int id
- *POST* */api/QuantityUnit/AddQtyUnit* [Authorize(Policy = "AdminOnly")]
  - [FromBody] QuantityUnit quantityUnit
  - Risposta: 201 Created (header Location verso la GET dell'unità)
- *PUT* */api/QuantityUnit/UpdateQtyUnit/{id}* [Authorize(Policy = "AdminOnly")]
  - [FromURL] int id
  - [FromBody] QuantityUnit quantityUnit
- *DELETE* */api/QuantityUnit/DeleteQtyUnit/{id}* [Authorize(Policy = "AdminOnly")]
  - [FromURL] int id
  - Risposta: 204 No Content; 409 Conflict se l'unità è usata da qualche item


## Metadata endpoints:

- *GET* */api/Metadata/Providers* [Authorize]
  - Restituisce i nomi dei provider (plugin) caricati in memoria
- *PUT* */api/Metadata/AssociateProvider/{idCategory}* [Authorize(Policy = "AdminOnly")]
  - [FromURL] int idCategory
  - [FromQuery] string providerName


## Images endpoints:

- *GET* */api/Images/AllImagesInfo* [Authorize]
- *GET* */api/Images/ImageName/{itemImageName}* [Authorize]
  - [FromURL] string itemImageName
- *GET* */api/Images/IdImage/{idImage}* [Authorize]
  - [FromURL] int idImage
- *GET* */api/Images/ItemsCount/{qty}* [Authorize]
  - [FromURL] int qty
- *POST* */api/Images/CreateImage* [Authorize]
  - [FromQuery] string? itemName
  - [FromForm] IFormFile file (multipart/form-data)
- *PUT* */api/Images/UpdateImage/{id}* [Authorize]
  - [FromURL] int id
  - [FromQuery] string? itemName
  - [FromForm] IFormFile file (multipart/form-data)
- *PUT* */api/Images/RenameImage/{id}* [Authorize]
  - [FromURL] int id
  - [FromQuery] string? itemName
- *DELETE* */api/Images/DeleteImage/{id}* [Authorize(Policy = "AdminOnly")]
  - [FromURL] int id


## Models:

**Category**
- int IdCategory
- string NameCategory
- string CategoryColor (default "#FFFFFF")
- string? AssociatedProviderName

**Image**
- int IdImage
- string FileName
- int ItemsCount (default 0)
- DateTime? LastModified

**Item**
- int IdItem
- string ItemName
- string? Description
- int? IdImage
- string? ImageName
- int? IdCategory
- int Quantity
- int? IdQuantityUnit
- QuantityUnit? QuantityUnit
- string? Barcode

**QuantityUnit**
- int IdQuantityUnit
- string Name (es. "Chilogrammi")
- string Symbol (es. "kg")

**User**
- string Username
- string? Email
- string Password
- bool? IsAdmin (default false)
- DateTime? CreateTime