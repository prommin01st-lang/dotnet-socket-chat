namespace ChatBackend.Services.FileStorage
{
    public interface IFileStorageService
    {
        /// <summary>
        /// อัปโหลดไฟล์และคืนค่า URL ของไฟล์
        /// </summary>
        Task<string> UploadFileAsync(IFormFile file, string fileName);

        /// <summary>
        /// ลบไฟล์ (ถ้าจำเป็น)
        /// </summary>
        Task DeleteFileAsync(string fileName);
    }
}
