namespace ProjectManagement.DTOs;

public record AttachmentResponseDto(
    int      Id,
    int      ProjectId,
    string   FileName,
    string   ContentType,
    long     FileSize,
    int      UploadedByUserId,
    DateTime UploadedAt
);
