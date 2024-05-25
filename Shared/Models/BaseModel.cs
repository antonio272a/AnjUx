using AnjUx.Shared.Attributes;
using AnjUx.Shared.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace AnjUx.Shared.Models
{
    public abstract class BaseModel : DbModel, IDbModel, IBaseModel
    {
        private DateTime? updated;
        private string? updateUser;

        [DBField()]
        [Display(AutoGenerateField = false, AutoGenerateFilter = true, Name = "Data de Atualização", ShortName = "Atualizado")]
        public DateTime? Updated
        {
            get => updated;
            set => updated = value;
        }

        [DBField()]
        [Display(AutoGenerateField = false, AutoGenerateFilter = true, Name = "Editado por", ShortName = "Editor")]
        public string? UpdateUser
        {
            get => updateUser;
            set => updateUser = value;
        }

        public override bool HasKey()
        {
            return ID.HasValue && ID.Value > 0;
        }

        public virtual bool Prepare()
        {
            if (!ID.HasValue || ID.Value == 0)
            {
                if (!Inserted.HasValue) Inserted = DateTime.Now;
                Updated = DateTime.Now;
            }

            return true;
        }

        /// <summary>
        ///     Reseta os dados da Classe quando ela deveria ser inserida e falhou.
        /// </summary>
        /// <remarks>Permite que seja efetuada nova tentativa de Save pois os dados voltam a permitir um Insert.</remarks>
        public virtual void InsertLogicalRollback()
        {
            ID = null;
            Inserted = null;
            updated = null;
        }

        public virtual string GetDbHashCodeForDropdown()
        {
            return new BaseModelDefaultComparer<BaseModel>().GetHashCode(this).ToString();
        }

        public void CloneBase(BaseModel model)
        {
            model.ID = ID;
            model.Inserted = Inserted;
            model.Updated = Updated;
            model.UpdateUser = UpdateUser;
            model.InsertUser = InsertUser;
        }
    }

    public class BaseModelDefaultComparer<T> : IEqualityComparer<T> where T : BaseModel
    {
        #region Fields

        private bool fkRules = false;

        #endregion

        #region Properties

        /// <summary>
        ///     Utilizar regras para FK Foreign Key. Quando indicado, não compara os objetos apenas por seus IDs.
        /// </summary>
        public bool FkRules
        {
            get => fkRules;

            set => fkRules = value;
        }

        #endregion

        public bool Equals(T? x, T? y)
        {
            return GetHashCode(x) == GetHashCode(y);
        }

        public int GetHashCode(T? obj)
        {
            if (obj == null)
            {
                return 0;
            }
            else
            {
                if (fkRules)
                    return obj.ID.GetValueOrDefault().ToString().GetHashCode();
                else
                    return string.Format("{0}{1}", obj.ID.GetValueOrDefault().ToString(), obj.Updated.GetValueOrDefault().Ticks.ToString()).GetHashCode();
            }
        }

        public static bool AreEquals(T? x, T? y, bool fkRule)
        {
            BaseModelDefaultComparer<T> bmdc = new()
            {
                FkRules = fkRule
            };
            return bmdc.Equals(x, y);
        }
    }
}
