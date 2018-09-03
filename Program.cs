
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Requests;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace DemoGoogleDriveApi
{
    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/drive-dotnet-quickstart.json
        static string[] Scopes = {
            DriveService.Scope.DriveReadonly,
            DriveService.Scope.Drive,
            DriveService.Scope.DriveFile,
        };
        static string ApplicationName = "Drive API .NET Quickstart";

        public static void FileSharing(string fileId, DriveService driveService)
        {
            try
            {
                fileId = "1XY2zomDP2qYZJ-aTAPLHSMEu-v7RVjii";
                var batch = new BatchRequest(driveService);
                BatchRequest.OnResponse<Permission> callback = delegate (
                    Permission permission,
                    RequestError error,
                    int index,
                    System.Net.Http.HttpResponseMessage message)
                {
                    if (error != null)
                    {
                        // Handle error
                        Console.WriteLine(error.Message);
                    }
                    else
                    {
                        Console.WriteLine("Permission ID: " + permission.Id);
                    }
                };
                Permission userPermission = new Permission()
                {
                    Type = "user",
                    Role = "writer",
                    EmailAddress = "khalilmohammadmirza@gmail.com",
                };
                var request = driveService.Permissions.Create(userPermission, fileId);
                request.Fields = "id";
                
                batch.Queue(request, callback);

                //Permission domainPermission = new Permission()
                //{
                //    Type = "user",
                //    Role = "reader",
                //    Domain = "gmail.com"
                //};
                //request = driveService.Permissions.Create(domainPermission, fileId);
                //request.Fields = "id";
                //batch.Queue(request, callback);
                var task = batch.ExecuteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static string FileUpload(DriveService driveService)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = "photo.jpg"
            };
            FilesResource.CreateMediaUpload request;
            using (var stream = new System.IO.FileStream("Files/photo.jpg",
                                    System.IO.FileMode.Open))
            {
                request = driveService.Files.Create(
                    fileMetadata, stream, "image/jpeg");
                request.Fields = "id";
                request.Upload();
            }
            var file = request.ResponseBody;
            Console.WriteLine("File ID: " + file.Id);
            return file.Id;
        }

        public static string CreateFolder(DriveService driveService)
        {
            try
            {
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = "Invoices",
                    MimeType = "application/vnd.google-apps.folder"
                };
                var request = driveService.Files.Create(fileMetadata);
                request.Fields = "id";
                var file = request.Execute();
                Console.WriteLine("Folder ID: " + file.Id);

                return file.Id;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        static void Main(string[] args)
        {
          

            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name)";

            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;

            Console.WriteLine("Files:");
            var fileId = files[0].Id;

            CreateFolder(service);
            FileUpload(service);
            FileSharing(fileId, service);

            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    Console.WriteLine("{0} ({1})", file.Name, file.Id);
                }
            }
            

            //FileShare(files[0])
            else
            {
                Console.WriteLine("No files found.");
            }
            Console.Read();

        }

    }
}
