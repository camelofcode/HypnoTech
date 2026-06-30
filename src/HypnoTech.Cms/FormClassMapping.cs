using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Claims;
using Humanizer;
using HypnoTech.Cms.Extensions;
using HypnoTech.Cms.FormMemberImplementations;
using HypnoTech.Common.Extensions;
using HypnoTech.Common.Security;

namespace HypnoTech.Cms
{
    public class FormClassMapping
    {
        public static Dictionary<Type, FormClassMapping> Maps { get; } = new Dictionary<Type, FormClassMapping>();

        public ICollection<FormMemberMapping> MemberMappings = new List<FormMemberMapping>();

        public ICollection<FormChild> Children = new List<FormChild>();

        public bool HasId => MemberMappings.Any(p => p.IsId);

        public Type ClassType { get; }

        public string RequiredRole { get; private set; }

        public string ControllerName { get; private set; }

        public string DisplayName { get; private set; }

        public string SingularDisplayName => DisplayName.Singularize(false);

        public string PluralDisplayName => DisplayName.Pluralize(false);

        public string DataSource { get; private set; }

        public string DummyDataSource { get; private set; }

        public bool CanAdd { get; private set; }

        public bool CanEdit { get; private set; }

        public bool CanRemove { get; private set; }

        public bool CanDuplicate { get; private set; }

        public bool CloneFilesWhenDuplicating { get; private set; }

        public string TitleProperty { get; protected set; }

        public string OrderProperty { get; protected set; }

        public bool Ordered { get; protected set; }

        public string SortProperty { get; protected set; }

        public bool SortAscending { get; protected set; }

        public string Icon { get; private set; }

        public bool HasFileUploads => MemberMappings.Any(p => p.IsFileInput);

        public FormClassMapping(Type classType)
        {
            ClassType = classType;
            CanAdd = true;
            CanEdit = true;
            CanRemove = true;
        }

        public static FormClassMapping GetMapping(Type classType)
        {
            return Maps.FirstOrDefault(p => p.Key == classType).Value;
        }

        public static FormClassMapping<TClass> GetMapping<TClass>()
        {
            return (FormClassMapping<TClass>)Maps.FirstOrDefault(p => p.Key == typeof(TClass)).Value;
        }

        public static FormClassMapping<TClass> RegisterClassMapping<TClass>(Action<FormClassMapping<TClass>> classMappingInitializer)
        {
            var mapping = new FormClassMapping<TClass>(classMappingInitializer);
            RegisterClassMapping(mapping);

            foreach (var map in mapping.MemberMappings)
            {
                if (map is FileUploadInputMapping fileMap)
                {
                    FileUploadFolderCache.Add($"{mapping.DisplayName} Files", Path.Combine("Files", fileMap.TargetDirectory));
                }
                else if (map is ImageUploadInputMapping imageMap)
                {
                    FileUploadFolderCache.Add($"{mapping.DisplayName} Images", Path.Combine("Images", imageMap.TargetDirectory));
                }
                else if (map is PhotoGalleryInputMapping galleryMap)
                {
                    FileUploadFolderCache.Add($"{mapping.DisplayName} Galleries", Path.Combine("Images", galleryMap.TargetDirectory));
                }
            }

            return mapping;
        }

        public static void RegisterClassMapping(FormClassMapping classMapping)
        {
            Maps.Add(classMapping.ClassType, classMapping);
        }

        public string GetId<TClass>(TClass model)
        {
            return !HasId ? null : model.GetType().GetProperty("Id")?.GetValue(model).ToString();
        }

        public void SetId<TClass>(TClass model, string id)
        {
            if (HasId)
            {
                model.GetType().GetProperty("Id")?.SetValue(model, id);
            }
        }

        public string GetTitle<TClass>(TClass model)
        {
            var type = model.GetType();

            if (!string.IsNullOrEmpty(TitleProperty))
            {
                return type.GetProperty(TitleProperty)?.GetValue(model).ToString();
            }

            var title = string.Empty;

            foreach (var candidate in new[] { "Name", "Title" })
            {
                var property = type.GetProperty(candidate);
                if (property != null)
                {
                    title = property.GetValue(model).ToString();
                }
            }

            return title;
        }

        public FormMemberMapping GetMember(string name)
        {
            return MemberMappings.FirstOrDefault(p => p.MemberName == name);
        }

        public FormMemberMapping MapMember(MemberInfo info, string accessPath, InputType type)
        {
            var memberMap = MemberMappings.FirstOrDefault(p => p.MemberInfo == info);
            if (memberMap == null)
            {
                switch (type)
                {
                    case InputType.None:
                        memberMap = new NoInputMapping(info, accessPath, this);
                        break;
                    case InputType.Hidden:
                        memberMap = new HiddenInputMapping(info, accessPath, this);
                        break;
                    case InputType.Text:
                        memberMap = new TextInputMapping(info, accessPath, this);
                        break;                    
                    case InputType.TextArea:
                        memberMap = new TextAreaInputMapping(info, accessPath, this);
                        break;
                    case InputType.RichText:
                        memberMap = new RichTextInputMapping(info, accessPath, this);
                        break;
                    case InputType.Date:
                        memberMap = new DateInputMapping(info, accessPath, this);
                        break;
                    case InputType.FileUpload:
                        memberMap = new FileUploadInputMapping(info, accessPath, this);
                        break;
                    case InputType.ImageUpload:
                        memberMap = new ImageUploadInputMapping(info, accessPath, this);
                        break;
                    case InputType.SelectList:
                        memberMap = new SelectListInputMapping(info, accessPath, this);
                        break;
                    case InputType.StaticSelectList:
                        memberMap = new StaticSelectListInputMapping(info, accessPath, this);
                        break;
                    case InputType.NestedSelectList:
                        memberMap = new NestedSelectListInputMapping(info, accessPath, this);
                        break;
                    case InputType.YouTube:
                        memberMap = new YouTubeInputMapping(info, accessPath, this);
                        break;
                    case InputType.ReadOnly:
                        memberMap = new ReadOnlyMapping(info, accessPath, this);
                        break;
                    case InputType.Money:
                        memberMap = new MoneyInputMapping(info, accessPath, this);
                        break;
                    case InputType.PhotoGallery:
                        break;
                    case InputType.Custom:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }

                MemberMappings.Add(memberMap);
            }

            return memberMap;
        }

        public void RequiresRole(string role)
        {
            RequiredRole = role;
            RoleStore.RegisterCmsRole(role);
        }

        public void SetControllerName(string name)
        {
            ControllerName = name;
        }

        public void SetDisplayName(string name)
        {
            DisplayName = name;
        }

        public void AddDataSource(string collection)
        {
            DataSource = collection;
        }

        public void AddDummyDataSource(string file)
        {
            DummyDataSource = file;
        }

        public void DisableAdd()
        {
            CanAdd = false;
        }

        public void DisableEdit()
        {
            CanEdit = false;
        }

        public void DisableRemove()
        {
            CanRemove = false;
        }

        public void EnableDuplicate(bool cloneFiles = false)
        {
            CanDuplicate = true;
            CloneFilesWhenDuplicating = cloneFiles;
        }

        public void SetIcon(string iconName)
        {
            Icon = iconName;
        }
    }

    public class FormClassMapping<TClass> : FormClassMapping
    {
        public FormClassMapping() : base(typeof(TClass))
        {

        }

        public FormClassMapping(Action<FormClassMapping<TClass>> classMappingInitializer) : base(typeof(TClass))
        {
            classMappingInitializer(this);
        }

        public static FormClassMapping GetMapping()
        {
            return GetMapping(typeof(TClass));
        }

        public void RegisterChild<TMember>(Expression<Func<TClass, TMember>> typeLambda, Expression<Func<TClass, string>> idLambda, string dataSource)
        {
            Children.Add(new FormChild(GetMemberInfoFromLambda(typeLambda), GetMemberInfoFromLambda(idLambda), typeof(TMember), dataSource));
        }

        public FormMemberMapping MapMemberWithoutInput<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            return MapMember(memberLambda, InputType.None);
        }

        public FormMemberMapping MapHiddenInputMember<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            return MapMember(memberLambda, InputType.Hidden);
        }

        public FormMemberMapping MapTextInputMember<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            return MapMember(memberLambda, InputType.Text);
        }

        public TextAreaInputMapping MapTextAreaInputMember<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            return MapMember(memberLambda, InputType.TextArea) as TextAreaInputMapping;
        }

        public RichTextInputMapping MapRichTextInputMember<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            return MapMember(memberLambda, InputType.RichText) as RichTextInputMapping;
        }

        public FormMemberMapping MapDateInputMember<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            return MapMember(memberLambda, InputType.Date);
        }

        public FileUploadInputMapping MapFileUploadInputMember<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            return MapMember(memberLambda, InputType.FileUpload) as FileUploadInputMapping;
        }

        public ImageUploadInputMapping MapImageUploadInputMember<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            return MapMember(memberLambda, InputType.ImageUpload) as ImageUploadInputMapping;
        }

        public SelectListInputMapping MapSelectListInputMember<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            return MapMember(memberLambda, InputType.SelectList) as SelectListInputMapping;
        }

        public StaticSelectListInputMapping MapStaticSelectListInputMember<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            return MapMember(memberLambda, InputType.StaticSelectList) as StaticSelectListInputMapping;
        }

        public YouTubeInputMapping MapYouTubeInputMember<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            return MapMember(memberLambda, InputType.YouTube) as YouTubeInputMapping;
        }

        public MoneyInputMapping MapMoneyInputMember<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            return MapMember(memberLambda, InputType.Money) as MoneyInputMapping;
        }

        public CheckboxInputMapping<TClass> MapCheckboxInputMember<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            var memberInfo = GetMemberInfoFromLambda(memberLambda, out var accessString);

            var memberMap = new CheckboxInputMapping<TClass>(memberInfo, accessString, this);

            MemberMappings.Add(memberMap);
            return memberMap;
        }

        public ListInputMapping<TListClass> MapListInputMember<TListClass, TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            var memberInfo = GetMemberInfoFromLambda(memberLambda, out var accessPath);
            var memberMap = MemberMappings.FirstOrDefault(p => p.MemberInfo == memberInfo);
            if (memberMap == null)
            {
                var mapping = new ListInputMapping<TListClass>(memberInfo, accessPath, this);
                MemberMappings.Add(mapping);
                return mapping;
            }

            return null;
        }

        public PhotoGalleryInputMapping<TListClass> MapPhotoGalleryInputMember<TListClass, TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            var memberInfo = GetMemberInfoFromLambda(memberLambda, out var accessPath);
            var memberMap = MemberMappings.FirstOrDefault(p => p.MemberInfo == memberInfo);
            if (memberMap == null)
            {
                var mapping = new PhotoGalleryInputMapping<TListClass>(memberInfo, accessPath, this);
                MemberMappings.Add(mapping);
                return mapping;
            }

            return null;
        }

        public GeneratedMemberMapping<TClass> MapGeneratedMember<TMember>(Expression<Func<TClass, TMember>> memberLambda, Func<TClass, object> generator)
        {
            var memberInfo = GetMemberInfoFromLambda(memberLambda, out var accessPath);
            var memberMap = MemberMappings.FirstOrDefault(p => p.MemberInfo == memberInfo);
            if (memberMap == null)
            {
                var mapping = new GeneratedMemberMapping<TClass>(memberInfo, accessPath, this, generator);
                MemberMappings.Add(mapping);
                return mapping;
            }

            return null;
        }

        public NestedSelectListInputMapping MapNestedSelectListInputMember<TMember, TChildMember>(Expression<Func<TClass, TMember>> memberLambda, Expression<Func<TClass, TChildMember>> childMemberLambda)
        {
            var map = MapMember(memberLambda, InputType.NestedSelectList) as NestedSelectListInputMapping;

            if (map != null)
            {
                var childMemberInfo = GetMemberInfoFromLambda(childMemberLambda, out var childAccessString);
                map.ChildMemberName = childAccessString;
                map.ChildMemberInfo = childMemberInfo;
            }

            return map;
        }

        public ReadOnlyMapping MapReadOnlyMember<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            return MapMember(memberLambda, InputType.ReadOnly) as ReadOnlyMapping;
        }

        public FormMemberMapping MapMember<TMember>(Expression<Func<TClass, TMember>> memberLambda, InputType type)
        {
            var memberInfo = GetMemberInfoFromLambda(memberLambda, out var accessString);
            return MapMember(memberInfo, accessString, type);
        }

        public void WithTitleProperty<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            TitleProperty = GetMemberInfoFromLambda(memberLambda).Name;
        }

        public void CanBeOrdered<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            Ordered = true;
            OrderProperty = GetMemberInfoFromLambda(memberLambda).Name;
        }

        public void WithDefaultSort<TMember>(Expression<Func<TClass, TMember>> memberLambda, bool ascending)
        {
            SortAscending = ascending;
            SortProperty = GetMemberInfoFromLambda(memberLambda).Name;
        }

        private static MemberInfo GetMemberInfoFromLambda<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            MemberExpression memberExpression = memberLambda.Body.GetMemberExpression();

            var memberInfo = memberExpression.Member;
            return memberInfo;
        }

        private static MemberInfo GetMemberInfoFromLambda<TMember>(Expression<Func<TClass, TMember>> memberLambda, out string lambdaString)
        {
            MemberExpression memberExpression = memberLambda.Body.GetMemberExpression();

            lambdaString = string.Join('.', memberExpression.ToString().Split('.').Skip(1));
            var memberInfo = memberExpression.Member;
            return memberInfo;
        }
    }
}
