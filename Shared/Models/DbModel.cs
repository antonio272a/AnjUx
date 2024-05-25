using AnjUx.Shared.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AnjUx.Shared.Models
{
    public abstract class DbModel
    {
        #region Fields

        private long? iD;
        private DateTime? inserted;
        private string? insertUser;

        #endregion

        #region Properties

        [DBField()]
        [Display(AutoGenerateField = false, AutoGenerateFilter = false, Name = "Identificador Único", ShortName = "ID")]
        public long? ID
        {
            get => iD;
            set => iD = value;
        }

        [DBField()]
        [Display(AutoGenerateField = false, AutoGenerateFilter = true, Name = "Data de Cadastramento", ShortName = "Criado")]
        public DateTime? Inserted
        {
            get => inserted;
            set => inserted = value;
        }


        [DBField()]
        [StringLength(50, ErrorMessageResourceName = "PropertyStringLength")]
        [Display(AutoGenerateField = false, AutoGenerateFilter = true, Name = "Cadastrado por", ShortName = "Criador")]
        public string? InsertUser
        {
            get => insertUser;
            set => insertUser = value;
        }

        #endregion

        [JsonIgnore]
        public string? DBDataHash { get; set; }


        public abstract bool HasKey();
    }
}
