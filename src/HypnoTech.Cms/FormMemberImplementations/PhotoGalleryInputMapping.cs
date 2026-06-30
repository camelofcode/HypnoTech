using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HypnoTech.Cms.Extensions;
using HypnoTech.Cms.Forms.Controls;

namespace HypnoTech.Cms.FormMemberImplementations
{
    public class PhotoGalleryInputMapping<TClass> : PhotoGalleryInputMapping, IFileUpload
    {
        public List<string> AllowedFileTypes { get; protected set; }
        public override string TargetDirectory { get; protected set; }

        public override int MaxHeight { get; protected set; }
        public override int MaxWidth { get; protected set; }

        public override MemberInfo IdInfo { get; protected set; }
        public override string IdName { get; protected set; }
        public override MemberInfo ImagePathInfo { get; protected set; }
        public override string ImagePathName { get; protected set; }
        public override MemberInfo CaptionInfo { get; protected set; }
        public override string CaptionName { get; protected set; }

        private void SetId(string name, MemberInfo info)
        {
            IdName = name;
            IdInfo = info;
        }

        private void SetImagePath(string name, MemberInfo info)
        {
            ImagePathName = name;
            ImagePathInfo = info;
        }

        private void SetCaption(string name, MemberInfo info)
        {
            CaptionName = name;
            CaptionInfo = info;
        }

        public PhotoGalleryInputMapping(MemberInfo memberInfo, string accessPath, FormClassMapping classMapping) : base(memberInfo, accessPath, classMapping)
        {
            InputType = InputType.PhotoGallery;
            TargetDirectory = "Galleries";
            AllowedFileTypes = new List<string> { "image/png", "image/jpeg" };
        }

        public override FormControl GetControl()
        {
            return new PhotoGalleryFormControl<TClass>(this);
        }

        public override FormControl GetControl(object obj)
        {
            return new PhotoGalleryFormControl<TClass>(this, obj);
        }

        public PhotoGalleryInputMapping<TClass> MapMembers<TId, TImagePath, TCaption>(Expression<Func<TClass, TId>> idLambda, Expression<Func<TClass, TImagePath>> imagePathLambda, Expression<Func<TClass, TCaption>> captionLambda)
        {
            MapLambda(idLambda, SetId);
            MapLambda(imagePathLambda, SetImagePath);
            MapLambda(captionLambda, SetCaption);
            return this;
        }

        public IFileUpload SetAllowedFileTypes(params string[] allowedFileTypes)
        {
            AllowedFileTypes = allowedFileTypes.ToList();
            return this;
        }

        public IFileUpload SetTargetDirectory(string targetDirectory)
        {
            TargetDirectory = targetDirectory;
            return this;
        }

        public PhotoGalleryInputMapping<TClass> SetImageSize(int maxWidth, int maxHeight)
        {
            MaxHeight = maxHeight;
            MaxWidth = maxWidth;
            return this;
        }

        private void MapLambda<TListClass, TKeyMember>(Expression<Func<TListClass, TKeyMember>> memberLambda, Action<string, MemberInfo> setter)
        {
            MemberExpression memberExpression = memberLambda.Body.GetMemberExpression();

            var str = memberExpression.ToString().Split('.').Last();
            var info = memberExpression.Member;
            setter(str, info);
        }
    }

    public abstract class PhotoGalleryInputMapping : FormMemberMapping
    {
        public abstract string TargetDirectory { get; protected set; }

        public abstract int MaxHeight { get; protected set; }
        public abstract int MaxWidth { get; protected set; }

        public abstract MemberInfo IdInfo { get; protected set; }
        public abstract string IdName { get; protected set; }

        public abstract MemberInfo ImagePathInfo { get; protected set; }
        public abstract string ImagePathName { get; protected set; }

        public abstract MemberInfo CaptionInfo { get; protected set; }
        public abstract string CaptionName { get; protected set; }

        protected PhotoGalleryInputMapping(MemberInfo memberInfo, string accessPath, FormClassMapping classMap) : base(memberInfo, accessPath, classMap)
        {
        }

        public void SetIds(object model, Func<string> idGenerator)
        {
            if (model != null)
            {
                var list = (IList)model;

                foreach (var item in list)
                {
                    if (!string.IsNullOrEmpty(IdName))
                    {
                        var idProperty = item.GetType().GetProperty(IdName);
                        if (idProperty != null && idProperty.GetValue(item) == null)
                        {
                            idProperty.SetValue(item, idGenerator());
                        }
                    }
                }
            }
        }

        public void SetImage(object model, int index, string imagePath)
        {
            if (model != null)
            {
                var list = (IList)model;
                var item = list[index];

                if (!string.IsNullOrEmpty(ImagePathName))
                {
                    var idProperty = item?.GetType().GetProperty(ImagePathName);
                    if (idProperty != null && idProperty.GetValue(item) == null)
                    {
                        idProperty.SetValue(item, imagePath);
                    }
                }
            }
        }
    }
}
