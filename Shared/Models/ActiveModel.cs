using AnjUx.Shared.Attributes;
using AnjUx.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnjUx.Shared.Models
{
    public abstract class ActiveModel : BaseModel, IActiveModel
    {
        private bool? active;

        [DBField()]
        [Display(Name = "Ativo", ShortName = "Ativo", AutoGenerateField = false, AutoGenerateFilter = true)]
        public bool? Active
        {
            get => active;
            set => active = value;
        }
    }
}
