using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Hotel.Core.Dtos;
using NBomber.Plugins.Http.CSharp;
using Xunit;

namespace Hotel.Api.LoadTests;

public class RoomsControllerLoadTests : LoadTestsBase
{
    protected override string ResourceUrl => "api/rooms/";

    [Fact]
    public void TestGetAllAsync()
    {
        ExecuteLoadTest("all");
    }

    [Fact]
    public void TestGetFreeAsync()
    {
        ExecuteLoadTest("free?from=2023-01-01&to=2023-02-01");
    }

    [Fact]
    public void TestOrderRoomAsync()
    {
        var createOrderStep =
            HttpStep.Create("create order", context =>
            {
                var date = new DateTime(Random.Shared.NextInt64(DateTime.MinValue.ToBinary(),
                    DateTime.MaxValue.ToBinary()));
                var orderDetailsDto = new OrderDetailsDto()
                {
                    Room = new RoomDto() { Id = 2 }, Username = "user1", From = date,
                    Until = date.AddSeconds(1)
                };
                var request = Http
                    .CreateRequest(HttpMethod.Post.Method, LoadTestsBase.BaseUrl + "/" + ResourceUrl + "order")
                    .WithBody(JsonContent.Create(orderDetailsDto));

                return request;
            });
        var deleteOrderStep = HttpStep.Create("delete order", context =>
        {
            var response = context.GetPreviousStepResponse<HttpResponseMessage>();
            var uri = response.Headers.Location;
            var request = Http.CreateRequest(HttpMethod.Delete.Method, BaseUrl + uri);

            return request;
        });

        ExecuteLoadTests(createOrderStep, deleteOrderStep);
    }
}
