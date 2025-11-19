// 'using' ถูกต้องแล้ว (ชี้ไปที่ .Entities)
using ChatBackend.Entities; 
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChatBackend.Data
{
    /// <summary>
    /// ใช้ IdentityDbContext แทน DbContext ธรรมดา
    /// และระบุ ApplicationUser เป็นคลาสผู้ใช้หลัก
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // DbSet สำหรับ Identity (Users, Roles ฯลฯ) จะถูกเพิ่มเข้ามาโดยอัตโนมัติ

        // --- DbSet สำหรับระบบ Chat ---
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // เรียก base.OnModelCreating(builder) เสมอ!
            // เพื่อให้ Identity ตั้งค่า Schema ของตัวเองก่อน
            base.OnModelCreating(builder);

            // --- กำหนดค่าสำหรับระบบ Chat ---

            // 1. ตารางเชื่อม ConversationParticipant (Many-to-Many)
            // กำหนด Composite Primary Key (UserId, ConversationId)
            builder.Entity<ConversationParticipant>()
                .HasKey(cp => new { cp.ApplicationUserId, cp.ConversationId });

            // 1a. ความสัมพันธ์จาก ConversationParticipant -> ApplicationUser
            builder.Entity<ConversationParticipant>()
                .HasOne(cp => cp.ApplicationUser)
                .WithMany(u => u.ConversationParticipants)
                .HasForeignKey(cp => cp.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict); // ห้ามลบ User ถ้ายังอยู่ในห้องแชท (หรือเปลี่ยนเป็น Cascade ถ้าต้องการ)

            // 1b. ความสัมพันธ์จาก ConversationParticipant -> Conversation
            builder.Entity<ConversationParticipant>()
                .HasOne(cp => cp.Conversation)
                .WithMany(c => c.Participants)
                .HasForeignKey(cp => cp.ConversationId)
                .OnDelete(DeleteBehavior.Cascade); // ถ้าลบห้องแชท ให้ลบผู้เข้าร่วมด้วย

            // 2. ตาราง Message
            // 2a. ความสัมพันธ์จาก Message -> ApplicationUser (Sender)
            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict); // ห้ามลบ User ถ้ายังส่งข้อความไว้ (ป้องกันข้อมูลกำพร้า)

            // 2b. ความสัมพันธ์จาก Message -> Conversation
            builder.Entity<Message>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade); // ถ้าลบห้องแชท ให้ลบข้อความทั้งหมดด้วย

            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany() // User ไม่จำเป็นต้องมี List<Notification> ก็ได้ (One-to-Many แบบทางเดียว) หรือจะเพิ่มก็ได้
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}