using ProjectManagement.DTOs;

namespace ProjectManagement.Services;

public interface IFileUploadService
{
    Task<AttachmentResponseDto> UploadAsync(IFormFile file, int projectId, int userId);
    Task<IEnumerable<AttachmentResponseDto>> GetByProjectAsync(int projectId, int userId);
    Task<bool> DeleteAsync(int attachmentId, int userId);
    Task<(byte[] Data, string ContentType, string FileName)> DownloadAsync(int attachmentId, int userId);
}
