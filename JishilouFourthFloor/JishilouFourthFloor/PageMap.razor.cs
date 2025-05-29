using LeafletForBlazor;
using Microsoft.AspNetCore.Components;

namespace JishilouFourthFloor;

partial class PageMap
{
    [Parameter]
    public string? Current { get; set; }

    private RealTimeMap map;

    RealTimeMap.LoadParameters mapParameters = new()
    {
        location = new RealTimeMap.Location()
        {
            longitude = Data.Rooms.Select(x => x.Value.Longitude).Average(),
            latitude = Data.Rooms.Select(x => x.Value.Latitude).Average(),
        },
        zoomLevel = 19,
        basemap = new()
        {
            basemapLayers = [
                new ()
                {
                    url = "http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
                    attribution = "©Open Street Map",
                    title = "Open Street Map",
                    detectRetina = true,
                    maxZoom = 19
                },
            ]
        }
    };

    private void UpdateAppearance()
    {
        if (map is null)
            return;

        map.Geometric.Points.AppearanceOnType(x => x.type != Current).pattern = new RealTimeMap.PointSymbol()
        {
            color = "green",
            fillColor = "green",
            fillOpacity = 0.5,
            radius = 10
        };

        map.Geometric.Points.AppearanceOnType(x => x.type == Current).pattern = new RealTimeMap.PointSymbol()
        {
            color = "red",
            fillColor = "red",
            fillOpacity = 0.5,
            radius = 10
        };
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        this.UpdateAppearance();
    }

    public async Task AddMarks()
    {
        foreach (var room in Data.Rooms)
        {
            await map.Geometric.Points.add(new RealTimeMap.StreamPoint()
            {
                guid = Guid.NewGuid(),
                latitude = room.Value.Latitude,
                longitude = room.Value.Longitude,
                type = room.Key
            });
        }

        map.Geometric.Points.AppearanceOnType(_ => true).pattern = new RealTimeMap.PointTooltip()
        {
            content = "${type}",
            opacity = 0.8,
            permanent = true
        };

        this.UpdateAppearance();
    }

    public void OnClick(RealTimeMap.ClicksMapArgs e)
    {
        var mouse = new RealTimeMap.StreamPoint()
        {
            latitude = e.location.latitude,
            longitude = e.location.longitude
        };

        var marks =
            from mark in e.sender.Geometric.Points.getItems()
            let distance = e.sender.Geometric.Computations.distance(
                mark, mouse, RealTimeMap.UnitOfMeasure.meters)
            where distance < 10
            orderby distance
            select mark;

        var result = marks.FirstOrDefault();
        if (result is null)
            return;

        NavigationManager.NavigateTo($"/{result.type}");
    }
}