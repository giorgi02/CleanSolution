﻿using Core.Application.DTOs;
using Core.Application.Interfaces.Repositories;
using Core.Domain.Models;
using Mapster;

namespace Core.Application.Interactors.Queries;
public abstract class GetMaxMinBySalaryQuery
{
    public record struct Request(bool IsRequiredMax = true) : IRequest<GetPositionDto?>;


    public sealed class Handler : IRequestHandler<Request, GetPositionDto?>
    {
        private readonly IPositionRepository _repository;
        public Handler(IPositionRepository repository) =>
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));


        public async Task<GetPositionDto?> Handle(Request request, CancellationToken cancellationToken)
        {
            var positions = await _repository.ReadAsync(cancellationToken);

            Position? position;
            if (request.IsRequiredMax)
                position = positions.MaxBy(x => x.Salary);
            else
                position = positions.MinBy(x => x.Salary);

            return position?.Adapt<GetPositionDto>();
        }
    }
}