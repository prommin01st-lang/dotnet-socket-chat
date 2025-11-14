namespace ChatBackend.Entities
{
    /// <summary>
    /// Enum เพื่อระบุประเภทของห้องแชท
    /// </summary>
    public enum ConversationType
    {
        /// <summary>
        /// แชท 1 ต่อ 1
        /// </summary>
        OneToOne = 1,

        /// <summary>
        /// แชทกลุ่ม
        /// </summary>
        Group = 2
    }
}