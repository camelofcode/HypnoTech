using System;
using HypnoTech.Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace HypnoTech.Common.Models
{
    public class PagerViewModel
    {
        public bool Visible => TotalPages > 1;

        public bool CanGoNext => CurrentPage < TotalPages;

        public bool CanGoPrevious => CurrentPage > 1;

        public int NextPage => CanGoNext ? CurrentPage + 1 : CurrentPage;

        public int PreviousPage => CanGoPrevious ? CurrentPage - 1 : CurrentPage;

        public string NextUrl { get; }

        public string PreviousUrl { get; }

        public int CurrentPage { get; }

        public int TotalPages { get; }

        public PagerViewModel(int currentPage, int totalPages, HttpContext context)
        {
            var currentUri = new Uri(context.Request.GetDisplayUrl());

            CurrentPage = currentPage;
            TotalPages = totalPages;

            NextUrl = currentUri.UpdateQueryParameter("page", NextPage.ToString());
            PreviousUrl = currentUri.UpdateQueryParameter("page", PreviousPage.ToString());
        }
    }
}
