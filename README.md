# NzbWishlist

Newznab API aggregator: Add 'wishes' and get notifications when new results for those wishes are found. Support for multiple newznab API providers.

[![Build Status](https://dev.azure.com/strideriidx/NZB%20Wishlist/_apis/build/status/NZB%20Wishlist?branchName=master)](https://dev.azure.com/strideriidx/NZB%20Wishlist/_build/latest?definitionId=1&branchName=master)

----------

## Technology Stack
* C# 7.3
* .NET Core 2.0 (netstandard2.0)
* FluentValidation
* Azure Functions v2 (w/ Durable Functions framework)
* Azure Storage v2 (Tables)
* xUnit
* Moq

## Getting Started in Development
You'll need the [Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator) with the [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/) for table/blob persistence.
Populate your local.settings.json in `NzbWishlist.Azure` to resemble this, filling in missing values accordingly.

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",

    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "APPSETTING_WEBSITE_SLOT_NAME": "Development",
    "WEBSITE_TIME_ZONE": "Pacific Standard Time",

    "PushoverAppToken": "",
    "PushoverUserKey": ""
  }
}
```