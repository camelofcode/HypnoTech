using System.Collections.Generic;
using System.Linq;
using HypnoTech.Cms.FormMemberImplementations;
using HypnoTech.Cms.Forms.Controls;
using HypnoTech.Cms.Validation;

namespace HypnoTech.Cms.Forms
{
    public abstract class Form
    {
        private readonly FormClassMapping _map;

        public string DisplayName => _map.DisplayName;

        public string BackUrl => $"/Cms/{_map.ControllerName}";

        public string PostUrl => $"/Cms/{_map.ControllerName}/{Action}";

        public bool HasFileUploads { get; }

        protected abstract string Action { get; }

        public List<string> HiddenFields => Controls.OfType<IProvidesState>().SelectMany(p => p.HiddenFields).ToList();

        public List<FormControl> Controls { get; }

        public bool HasBeenValidated { get; private set; }

        public bool Validate(List<string> addedFiles)
        {
            bool allValid = true;

            foreach (var control in Controls)
            {
                allValid = control.Validate(addedFiles) && allValid;
            }

            HasBeenValidated = true;
            return allValid;
        }

        protected Form(FormClassMapping classMapping)
        {
            Controls = new List<FormControl>();
            HasFileUploads = classMapping.HasFileUploads;
            _map = classMapping;
        }
    }
}
