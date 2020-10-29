using System;
using System.Collections.Generic;
using System.Text;

namespace NRTA.Core.Models
{
    public class ExamQuestion
    {
        public string Id { get; set; }
        /// <summary>
        /// 题干
        /// </summary>
        public string Subject { get; set; }
        public string A { get; set; }
        public string B { get; set; }
        public string C { get; set; }
        public string D { get; set; }
        public string E { get; set; }
        public string F { get; set; }
        public string G { get; set; }
        public string H { get; set; }
        public string Answer { get; set; }
    }
}
