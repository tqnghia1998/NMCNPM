﻿using System;
using System.ComponentModel.DataAnnotations;

namespace DbModel
{
    /// <summary>
    /// 
    /// </summary>
    public class SubjectDataModel
    {
        /// <summary>
        /// Mã học phần phân biệt các học phần giống nhau khác lớp 
        /// và giữa các học phần khác
        /// </summary>
        [Key]
        public string Id { get; set; }

        /// <summary>
        /// Tên khoa
        /// </summary>
        [Required]
        [MaxLength(256)]
        public string Major { get; set; }

        /// <summary>
        /// Tên học phần
        /// </summary>
        [Required]
        [MaxLength(256)]
        public string Subject { get; set; }

        /// <summary>
        /// Số tín chỉ
        /// </summary>
        public int Credit { get; set; }

        /// <summary>
        /// Giảng viên phụ trách
        /// </summary>
        public string Teacher { get; set; }

        /// <summary>
        /// Term of this subject
        /// </summary>
        public string Term { get; set; }

        /// <summary>
        /// Course of this subject
        /// </summary>
        public string Course { get; set; }

        /// <summary>
        /// Thời gian bắt đầu học phần
        /// </summary>
        public string TimeStart { get; set; }

        /// <summary>
        /// Thời gian kết thúc học phần
        /// </summary>
        public string TimeFinish { get; set; }

        /// <summary>
        /// Trạng thái
        /// </summary>
        public byte Status { get; set; } = (byte)StatusModel.NonActive;
    }
}
