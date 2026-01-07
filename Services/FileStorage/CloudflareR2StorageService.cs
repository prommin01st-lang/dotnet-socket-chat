using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace ChatBackend.Services.FileStorage
{
    public class CloudflareR2StorageService : IFileStorageService
    {
        private readonly string _bucketName;
        private readonly string _serviceUrl;
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _publicUrl;

        public CloudflareR2StorageService(IConfiguration configuration)
        {
            var section = configuration.GetSection("CloudflareR2");
            _bucketName = section["BucketName"]!;
            _serviceUrl = section["ServiceUrl"]!; // https://<ACCOUNT_ID>.r2.cloudflarestorage.com
            _accessKey = section["AccessKey"]!;
            _secretKey = section["SecretKey"]!;
            _publicUrl = section["PublicUrl"]!;   // https://pub-<ID>.r2.dev หรือ Custom Domain
        }

        private IAmazonS3 GetS3Client()
        {
            var config = new AmazonS3Config
            {
                ServiceURL = _serviceUrl,
                ForcePathStyle = true // ต้องเปิดสำหรับ R2 (S3 Compatible)
            };
            return new AmazonS3Client(_accessKey, _secretKey, config);
        }

        public async Task<string> UploadFileAsync(IFormFile file, string fileName)
        {
            using var client = GetS3Client();
            using var newMemoryStream = new MemoryStream();
            await file.CopyToAsync(newMemoryStream);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = newMemoryStream,
                Key = fileName,
                BucketName = _bucketName,
                CannedACL = S3CannedACL.PublicRead, // หรือ Private ขึ้นอยู่กับการตั้งค่า R2
                
                // (Optional) Disable Payload Signing สำหรับ R2 บางกรณี
                // DisablePayloadSigning = true 
            };
            uploadRequest.Headers.ContentType = file.ContentType; // ตั้ง Content-Type ให้ถูกต้อง

            var fileTransferUtility = new TransferUtility(client);
            await fileTransferUtility.UploadAsync(uploadRequest);

            return $"{_publicUrl}/{fileName}";
        }

        public async Task DeleteFileAsync(string fileName)
        {
             using var client = GetS3Client();
             await client.DeleteObjectAsync(new DeleteObjectRequest
             {
                 BucketName = _bucketName,
                 Key = fileName
             });
        }
    }
}
