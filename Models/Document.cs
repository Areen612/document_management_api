using System;

namespace DocumentManagementAPI.Models
{
    public class Document
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public byte[] Content { get; set; }
        public string FileType { get; set; }
        public DateTime CreationDate { get; set; }
    }
}