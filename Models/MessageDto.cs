namespace ChatBackend.Models 
{
    public class MessageDto
    {
        public Guid Id { get; set; }
        public required string Content { get; set; }
        public DateTime Timestamp { get; set; }

        // --- ข้อมูลผู้ส่ง ---
        public required string SenderId { get; set; }
        public required string SenderFirstName { get; set; }
        public required string SenderLastName { get; set; }
        public string? SenderProfilePictureUrl { get; set; }

        public Guid ConversationId { get; set; } 

    }
}