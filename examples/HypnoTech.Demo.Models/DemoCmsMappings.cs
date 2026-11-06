using System.Collections.Generic;
using HypnoTech.Cms;
using HypnoTech.Common.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HypnoTech.Demo.Models;

public static class DemoCmsMappings
{
    public static void Register()
    {
        RegisterArticle();
        RegisterArtist();
        RegisterEvent();
    }

    private static void RegisterArticle()
    {
        if (FormClassMapping.GetMapping<Article>() != null)
        {
            return;
        }

        FormClassMapping.RegisterClassMapping<Article>(map =>
        {
            map.SetDisplayName("Articles");
            map.SetControllerName("Articles");
            map.RequiresRole("articles");
            map.AddDummyDataSource("articles");
            map.AddDataSource("demoArticles");
            map.WithTitleProperty(article => article.Title);
            map.WithDefaultSort(article => article.PublishedAt, ascending: false);
            map.EnableDuplicate(cloneFiles: true);
            map.MapHiddenInputMember(article => article.Id);
            map.MapTextInputMember(article => article.Title).IncludeInTable(0).Searchable().Required();
            map.MapTextAreaInputMember(article => article.Summary).Required();
            map.MapRichTextInputMember(article => article.Body).Required();
            map.MapDateInputMember(article => article.PublishedAt)
                .IncludeInTable(1).WithDateStringFormatter().Required();
            map.MapImageUploadInputMember(article => article.HeroImagePath)
                .SetImageSize(1600, 1000).SetTargetDirectory("DemoArticles")
                .WithDisplayText("Hero image").Required();
            map.MapCheckboxInputMember(article => article.Featured).IncludeInTable(2);
            map.MapGeneratedMember(article => article.Slug, article => article.Title.GenerateSlug());
            map.SetIcon("fa fa-newspaper");
        });
    }

    private static void RegisterArtist()
    {
        if (FormClassMapping.GetMapping<Artist>() != null)
        {
            return;
        }

        FormClassMapping.RegisterClassMapping<Artist>(map =>
        {
            map.SetDisplayName("Artists");
            map.SetControllerName("Artists");
            map.RequiresRole("artists");
            map.AddDummyDataSource("artists");
            map.AddDataSource("demoArtists");
            map.WithTitleProperty(artist => artist.Name);
            map.CanBeOrdered(artist => artist.Order);
            map.EnableDuplicate(cloneFiles: true);
            map.MapHiddenInputMember(artist => artist.Id);
            map.MapTextInputMember(artist => artist.Name).IncludeInTable(0).Searchable().Required();
            map.MapStaticSelectListInputMember(artist => artist.Discipline)
                .SetListOptions(new List<SelectListItem>
                {
                    new("Music", "Music"),
                    new("Performance", "Performance"),
                    new("Visual art", "Visual art"),
                    new("Writing", "Writing")
                }).IncludeInTable(1).Required();
            map.MapTextAreaInputMember(artist => artist.Biography).Required();
            map.MapImageUploadInputMember(artist => artist.ImagePath)
                .SetImageSize(1200, 1200).SetTargetDirectory("DemoArtists")
                .WithDisplayText("Portrait").Required();
            map.MapTextInputMember(artist => artist.Website).WithUrlValidator();
            map.MapCheckboxInputMember(artist => artist.Featured).IncludeInTable(2);
            map.SetIcon("fa fa-users");
        });
    }

    private static void RegisterEvent()
    {
        if (FormClassMapping.GetMapping<Event>() != null)
        {
            return;
        }

        FormClassMapping.RegisterClassMapping<Event>(map =>
        {
            map.SetDisplayName("Events");
            map.SetControllerName("Events");
            map.RequiresRole("events");
            map.AddDummyDataSource("events");
            map.AddDataSource("demoEvents");
            map.WithTitleProperty(item => item.Title);
            map.WithDefaultSort(item => item.StartsAt, ascending: true);
            map.EnableDuplicate(cloneFiles: true);
            map.MapHiddenInputMember(item => item.Id);
            map.MapTextInputMember(item => item.Title).IncludeInTable(0).Searchable().Required();
            map.MapDateInputMember(item => item.StartsAt)
                .IncludeInTable(1).WithDateStringFormatter().Required();
            map.MapStaticSelectListInputMember(item => item.Type)
                .SetListOptions(new List<SelectListItem>
                {
                    new("Concert", "Concert"),
                    new("Exhibition", "Exhibition"),
                    new("Workshop", "Workshop"),
                    new("Talk", "Talk")
                }).IncludeInTable(2).Required();
            map.MapSelectListInputMember(item => item.ArtistId)
                .SetListOptions<Artist, string, string>(artist => artist.Id, artist => artist.Name)
                .WithDisplayText("Lead artist").Required();
            map.MapTextInputMember(item => item.Venue).Required();
            map.MapTextAreaInputMember(item => item.Summary).Required();
            map.MapRichTextInputMember(item => item.Description).Required();
            map.MapMoneyInputMember(item => item.TicketPrice).Required();
            map.MapImageUploadInputMember(item => item.ImagePath)
                .SetImageSize(1600, 1000).SetTargetDirectory("DemoEvents")
                .WithDisplayText("Event image").Required();
            map.MapFileUploadInputMember(item => item.ProgrammePath)
                .SetTargetDirectory("DemoProgrammes").WithDisplayText("Programme");
            map.MapCheckboxInputMember(item => item.Featured).IncludeInTable(3);
            map.MapReadOnlyMember(item => item.Reference).WithDisplayText("Reference");
            map.MapGeneratedMember(item => item.Slug, item => item.Title.GenerateSlug());
            map.MapListInputMember<ScheduleItem, List<ScheduleItem>>(item => item.Schedule)
                .MapMembers(schedule =>
                {
                    schedule.MapId(entry => entry.Id);
                    schedule.MapTextInputMember(entry => entry.Time, isNameField: true).Required();
                    schedule.MapTextInputMember(entry => entry.Title).Required();
                    schedule.MapStaticSelectListMemberMapping(entry => entry.Format)
                        .SetListOptions(new List<SelectListItem>
                        {
                            new("Performance", "Performance"),
                            new("Interval", "Interval"),
                            new("Discussion", "Discussion")
                        }).Required();
                    schedule.CanBeOrdered(entry => entry.Order);
                }).SetSingularObjectName("Schedule item").Required();
            map.SetIcon("fa fa-calendar");
        });
    }
}
