using CQRS;
using Data;
using Domain.Diary.DiaryRoot;
using KurrentDB.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infras.Services.Commands.Diary
{
    public record AddDiarySectionRequest(Guid DiaryId, Guid DayId, string Detail, bool IsPinned) : IRequest<long>;

    public class AddDiarySectionHandler : IHandler<AddDiarySectionRequest, long>
    {
        private readonly IElasticSearchContext _elasticsearchContext;
        private readonly KurrentDBClient _kurrentDBClient;

        public AddDiarySectionHandler(IElasticSearchContext elasticsearchContext, KurrentDBClient kurrentDBClient)
        {
            _elasticsearchContext = elasticsearchContext;
            _kurrentDBClient = kurrentDBClient;
        }

        async Task<long> IHandler<AddDiarySectionRequest, long>.Handle(AddDiarySectionRequest request, CancellationToken cancellationToken)
        {
            var eventData = new EventData(Uuid.NewUuid(), nameof(AddSection), JsonSerializer.SerializeToUtf8Bytes(new AddSection(request.Detail,request.IsPinned)));
            var res = await _kurrentDBClient.AppendToStreamAsync(DiaryConstants.GetStreamName(request.DayId), StreamState.StreamExists, [eventData], cancellationToken: cancellationToken);
            return res.NextExpectedStreamState.ToInt64();
        }
    }
}
