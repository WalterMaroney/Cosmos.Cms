﻿using CDT.Cosmos.Cms.Models.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CDT.Cosmos.Cms.Models
{
    public class EditCodePostModel : ICodeEditorViewModel
    {
        public int ArticleNumber { get; set; }

        [DataType(DataType.Html)] public string Content { get; set; }

        [DataType(DataType.Html)] public string HeaderJavaScript { get; set; }

        [DataType(DataType.Html)] public string FooterJavaScript { get; set; }

        public EditorMode EditorMode { get; set; }

        public bool IsValid { get; set; }

        [Key] public int Id { get; set; }

        public string EditingField { get; set; }
        public string EditorTitle { get; set; }
        public IEnumerable<EditorField> EditorFields { get; set; }
        public IEnumerable<string> CustomButtons { get; set; }
    }
}